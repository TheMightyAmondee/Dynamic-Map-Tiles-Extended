using DMT.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewValley;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace DMT
{
    public static class Utils
    {
        private static bool Enabled => Context.Config.Enabled;

        public static bool CheckTrigger(DynamicTileProperty property, IEnumerable<string> triggers)
        {
            foreach (var trigger in triggers)
            {
                if (trigger.Equals(property.Trigger, StringComparison.OrdinalIgnoreCase) == false && trigger.Contains(property.Trigger, StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        public static Point GetNextTile(int dir)
        {
            return dir switch
            {
                0 => new(0, -1),
                1 => new(1, 0),
                2 => new(0, 1),
                3 => new(-1, 0),
                _ => new(0) //Default
            };
        }

        public static bool PushTiles(GameLocation l, List<(Point p, Tile t)> tiles, int dir, Farmer f)
        {
            string[] stringtrigger = { "Push" };
            for (int i = 0; i < tiles.Count; i++)
            {
                var destination = tiles[i].p + GetNextTile(dir);
                if (l.isTileOnMap(tiles[i].p) == false || l.isTileOnMap(destination) == false ||
                    (
                        tiles[i].t.Layer.Tiles[destination.X, destination.Y] is Tile t &&
                        (t.HasProperty(Keys.PushKey, out _) == true || t.HasProperty(Keys.PushableKey, out _) == true) &&
                        tiles.Any(x => x.p == destination && x.t.Layer.Id == tiles[i].t.Layer.Id) == false
                    ))
                {
                    return false;
                }

            }

            if (Context.PushTileDict.TryGetValue(l.Name, out var pushed) == false)
            {
                Context.PushTileDict[l.Name] = pushed;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                var (p, t) = tiles[i];
                if (t.Layer.Id == "Buildings") { TriggerActions(new List<Layer>{ t.Layer }, f, p, stringtrigger); }                  
                pushed.Add(new() { Tile = t, Position = new(p.X * 64, p.Y * 64), Direction = dir, Farmer = f });
                tiles[i].t.Layer.Tiles[p.X, p.Y] = null;
            }
            
            return true;
        }

        public static void PushTilesWithOthers(Farmer f, Tile t, Point start)
        {
            List<(Point, Tile)> tiles = new List<(Point, Tile)> { (start, t) };
            if (t.HasProperty(Keys.PushAlsoKey, out var others) == false)
            {
                _ = PushTiles(f.currentLocation, tiles, f.FacingDirection, f);
                return;
            }
            var split = others.Split(',');
            foreach (var tile in split)
            {
                try
                {
                    var other = tile.Split(' ');
                    if (split.Length != 3 || int.TryParse(other[1], out int x) == false || int.TryParse(other[2], out int y) == false)
                    {
                        continue;
                    }
                    x += start.X;
                    y += start.Y;
                    var layer = f.currentLocation.Map.GetLayer(other[0]);
                    if (layer is null || f.currentLocation.isTileOnMap(x, y) == false || layer.Tiles[x, y] is not Tile otherTile)
                    {
                        continue;
                    }  
                    tiles.Add((new(x, y), otherTile));
                }
                catch(Exception ex) 
                { 
                    monitor.Log($"An error occureed trying to push a tile.\n{ex}", LogLevel.Error); 
                }
            }
            PushTiles(f.currentLocation, tiles, f.FacingDirection, f);
        }

        internal static Layer AddLayer(Map map, string id)
        {
            var layer = new Layer(id, map, map.Layers[0].LayerSize, Layer.m_tileSize);
            map.AddLayer(layer);
            return layer;
        }

        internal static TileSheet AddTileSheet(Map map, string id, string texturePath)
        {
            var texture = Context.Helper.GameContent.Load<Texture2D>(texturePath);
            if (texture == null)
            {
                return null;
            }                
            var tilesheet = new TileSheet(id, map, texturePath, new xSize(texture.Width / 16, texture.Height / 16), new xSize(16, 16));
            map.AddTileSheet(tilesheet);
            return tilesheet;
        }

        internal static Item? ParseItemString(string value)
        {
            Item item = null;
            string[] split;

            try
            {
                item = ItemRegistry.Create(value, 1, 0, true) ?? throw new();
            }
            catch
            {
                int amount = 1;
                int quality = 0;
                if (value.Contains(',') == true)
                {
                    split = value.Split(',');
                    value = split[0];
                    amount = int.Parse(split[1]);
                    if (split.Length == 3)
                    {
                        quality = int.Parse(split[2]);
                    }                        
                }
                item = ItemRegistry.Create(value, amount, quality);
            }

            return item;
        }

        internal static DynamicTileProperty? GetInternalProperty(string id)
        {
            foreach (var prop in Context.InternalProperties.Value)
            {
                if (prop.Key == id) 
                {
                    return prop.Value;
                }                
            }
                
            return null;
        }

        internal static DynamicTileProperty ParseOldProperty(KeyValuePair<string, string> old)
        {
            DynamicTileProperty prop = new() { Value = old.Value };
            string key = old.Key;
            foreach (var item in Keys.AllKeys.Union(Keys.ModKeys).OrderByDescending(x => x.Length))
            {
                if (key.StartsWith(item, StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }                    
                prop.Key = item;
                key = key.Replace(item, "");
                break;
            }

            if (string.IsNullOrWhiteSpace(prop.Key) == true)
            {
                //Context.Monitor.Log($"Found unknown key {key} when parsing old property format, this property will be ignored", LogLevel.Warn);
                return null;
            }

            key = key.Trim('_');
            if (key.StartsWith("Once", StringComparison.OrdinalIgnoreCase) == true)
            {
                prop.Once = true;
                key = key[4..];
            }
            key = key.Trim('_');
            foreach (var item in Triggers.Regexes)
            {
                if (Regex.IsMatch(key, item) == false)
                {
                    continue;
                }                   
                prop.Trigger = key;
                break;
            }
            if (string.IsNullOrWhiteSpace(prop.Trigger) == true)
            {
                Context.Monitor.Log($"Found unknown trigger {key} when parsing old property format, this property will be ignored", LogLevel.Warn);
                return null;
            }
            return prop;
        }

        internal static List<DynamicTileProperty> FilterProperties(List<DynamicTileProperty> properties)
        {
            List<string> keys = new List<string>();
            List<DynamicTileProperty> filtered = new List<DynamicTileProperty>();
            foreach (var prop in properties)
            {
                if (keys.Contains(prop.Key) == true)
                {
                    Context.Monitor.Log($"Found duplicate key {prop.Key}{(string.IsNullOrWhiteSpace(prop.LogName) ? "" : $" For named property {prop.LogName}")}, This property was automatically filtered out. Please avoid adding duplicate actions for tiles", LogLevel.Warn);
                    continue;
                }
                keys.Add(prop.Key);
                filtered.Add(prop);
            }

            return filtered;
        }

        internal static string BuildFormattedTrigger(params object[]? parts)
        {
            if ((parts?.Any() ?? false) == false)
            {
                return "";
            }
                
            StringBuilder sb = new();
            sb.Append('(');
            foreach (var part in parts)
            {
                sb.Append(part);
            }
                
            sb.Append(')');
            return sb.ToString();
        }

        internal static bool IsGlobalTrigger(string trigger)
        {
            foreach (var item in Triggers.GlobalTriggers)
            {
                if (Regex.IsMatch(trigger, item))
                {
                    return true;
                }
                    
            }
               
            return false;
        }

        //All of the methods below this line cost me a lot (mainly sanity wise), continue at your own risk

        public static bool TriggerActions(List<Layer> layers, Farmer who, Point tilePosition, string[] triggers)
        {
            if (Enabled == false || who.currentLocation.isTileOnMap(tilePosition) == false || (Context.Config.TriggerDuringEvents == false && Game1.eventUp == true))
            {
                return false;
            }
                
            bool triggered = false;
            List<(DynamicTileProperty prop, Tile tile)> properties = GetPropertiesForTriggers(layers, who, tilePosition, triggers);
            /*foreach (var layer in layers)
            {
                var tile = layer.Tiles[tilePosition.X, tilePosition.Y];

                if (tile is null)
                    continue;
                string[] props = [.. tile.Properties.Keys];

                foreach (var key in props)
                {
                    DynamicTileProperty? prop = !key.StartsWith("DMT_PROP_INTERNAL") ? ParseOldProperty(new(key, tile.Properties[key])) : GetInternalProperty(tile.Properties[key]);

                    if (prop is null || !CheckTrigger(prop, triggers))
                        continue;

                    if (prop.Once)
                        tile.Properties.Remove(key);

                    properties.Add(new(prop, tile));
                }
            }*/

            foreach (var item in properties)
            {
                try
                {
                    bool found = false;
                    var value = item.prop.Value;
                    var tile = item.tile;
                    switch (item.prop.Key) //Generally not a fan of goto's, but it is DRY this way
                    {
                        case Keys.AddLayerKey:
                            Actions.DoAddLayer(who, value);
                            goto case "Completed";
                        case Keys.AddTilesheetKey:
                            Actions.DoAddTileSheet(who, value);
                            goto case "Completed";
                        case Keys.ChangeIndexKey:
                            Actions.DoChangeIndex(who, value, tile, tilePosition);
                            goto case "Completed";
                        case Keys.ChangeMultipleIndexKey:
                            Actions.DoChangeMultipleIndexes(who, value, tile, tilePosition);
                            goto case "Completed";
                        case Keys.ChangePropertiesKey:
                            Actions.DoChangeProperties(value, tile);
                            goto case "Completed";
                        case Keys.ChangeMultiplePropertiesKey:
                            Actions.DoChangeMultipleProperties(who, value, tile);
                            goto case "Completed";
                        case Keys.SoundKey:
                            Actions.DoPlaySound(who, value);
                            goto case "Completed";
                        case Keys.TeleportKey:
                            Actions.DoTeleport(who, value);
                            goto case "Completed";
                        case Keys.TeleportTileKey:
                            Actions.DoTeleportTile(who, value);
                            goto case "Completed";
                        case Keys.GiveKey:
                            Actions.DoGive(who, value);
                            goto case "Completed";
                        case Keys.TakeKey:
                            Actions.DoTake(who, value);
                            goto case "Completed";
                        case Keys.ChestKey:
                            Actions.DoSpawnChest(who, value);
                            goto case "Completed";
                        case Keys.MessageKey:
                            Actions.DoShowMessage(value);
                            goto case "Completed";
                        case Keys.EventKey:
                            Actions.DoPlayEvent(value);
                            goto case "Completed";
                        case Keys.MailKey:
                            Actions.DoAddMailflag(who, value);
                            goto case "Completed";
                        case Keys.MailRemoveKey:
                            Actions.DoRemoveMailflag(who, value);
                            goto case "Completed";
                        case Keys.MailBoxKey:
                            Actions.DoAddMailForTomorrow(who, value);
                            goto case "Completed";
                        case Keys.InvalidateKey:
                            Actions.DoInvalidateAsset(value);
                            goto case "Completed";
                        case Keys.MusicKey:
                            Actions.DoPlayMusic(value);
                            goto case "Completed";
                        case Keys.HealthKey:
                            Actions.DoUpdateHealth(who, value);
                            goto case "Completed";
                        case Keys.StaminaKey:
                            Actions.DoUpdateStamina(who, value);
                            goto case "Completed";
                        case Keys.HealthPerSecondKey:
                            Actions.DoUpdateHealthPerSecond(who, value);
                            goto case "Completed";
                        case Keys.StaminaPerSecondKey:
                            Actions.DoUpdateStaminaPerSecond(who, value);
                            goto case "Completed";
                        case Keys.BuffKey:
                            Actions.DoAddBuff(who, value);
                            goto case "Completed";
                        case Keys.EmoteKey:
                            Actions.DoEmote(who, value);
                            goto case "Completed";
                        case Keys.ExplosionKey:
                            Actions.DoExplode(who, value, tilePosition);
                            goto case "Completed";
                        case Keys.AnimationKey:
                            Actions.DoAnimate(who, value);
                            goto case "Completed";
                        case Keys.PushKey:
                            Actions.DoPushTiles(who, tile, tilePosition);
                            goto case "Completed";
                        case Keys.PushOthersKey:
                            Actions.DoPushOtherTiles(who, value, tile, tilePosition);
                            goto case "Completed";
                        case Keys.WarpKey:
                            Actions.DoWarp(who, value);
                            goto case "Completed";
                        case "Completed":
                            triggered = true;
                            found = true;
                            break;
                    }

                    if (found == false && Keys.ModKeys.Contains(item.prop.key) == true)
                    {
                        Actions.ModActions[item.prop.Key]?.Invoke(who, value, tile, tilePosition);
                        triggered = true;
                        found = true;
                    }
                }
                catch (Exception ex)
                {
                    Context.Monitor.Log($"Error while trying to run action {item.prop.Key}", LogLevel.Error);
                    Context.Monitor.Log($"[{ex.GetType().Name}] {ex.Message}\n{ex.Message}", LogLevel.Error);
                    //Ah fuck I need to write logging for this as well don't I...
                }
            }

            return triggered;
        }

        internal static List<(DynamicTileProperty prop, Tile tile)> GetPropertiesForTriggers(List<Layer> layers, Farmer who, Point tilePosition, IEnumerable<string> triggers)
        {
            List<string> localTriggers = new List<string>();
            List<string> globalTriggers = new List<string>();
            List<(DynamicTileProperty prop, Tile tile)> properties = new List<(DynamicTileProperty prop, Tile tile)>();
            foreach (var trigger in (string[])triggers)
            {
                if (IsGlobalTrigger(trigger) == true)
                {
                    globalTriggers.Add(trigger);
                }
                else
                {
                    localTriggers.Add(trigger);
                }                  
            }

            if (globalTriggers.Count > 0)
            {
                properties.AddRange(GetPropertiesForGlobalTriggers(layers, who, globalTriggers));
            }
                
            if (localTriggers.Count > 0)
            {
                properties.AddRange(GetPropertiesForLocalTriggers(layers, who, tilePosition, localTriggers));
            }
                
            return properties;
        }

        internal static List<(DynamicTileProperty prop, Tile tile)> GetPropertiesForLocalTriggers(List<Layer> layers, Farmer who, Point tilePosition, IEnumerable<string> triggers)
        {
            List<(DynamicTileProperty prop, Tile tile)> properties = new List<(DynamicTileProperty prop, Tile tile)>();
            foreach (var layer in layers)
            {
                var tile = layer.Tiles[tilePosition.X, tilePosition.Y];

                if (tile is null)
                {
                    continue;
                }
                    
                string[] props = tile.Properties.Keys.ToArray();

                foreach (var key in props)
                {
                    DynamicTileProperty? prop = key.StartsWith("DMT_PROP_INTERNAL") == false ? ParseOldProperty(new(key, tile.Properties[key])) : GetInternalProperty(tile.Properties[key]);

                    if (prop is null || CheckTrigger(prop, triggers) == false)
                    {
                        continue;
                    }

                    if (prop.Once == true)
                    {
                        tile.Properties.Remove(key);
                    }
                        
                    properties.Add(new(prop, tile));
                }
            }
            return properties;
        }

        internal static List<(DynamicTileProperty prop, Tile tile)> GetPropertiesForGlobalTriggers(List<Layer> layers, Farmer who, IEnumerable<string> triggers)
        {
            List<(DynamicTileProperty prop, Tile tile)> properties = new List<(DynamicTileProperty prop, Tile tile)>();
            int width = layers[0].Tiles.Array.GetLength(0);
            int height = layers[0].Tiles.Array.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    foreach (var layer in layers)
                    {
                        var tile = layer.Tiles[x, y];

                        if (tile is null)
                        {
                            continue;
                        }

                        string[] props = tile.Properties.Keys.ToArray();

                        foreach (var key in props)
                        {
                            DynamicTileProperty? prop = key.StartsWith("DMT_PROP_INTERNAL") == false ? ParseOldProperty(new(key, tile.Properties[key])) : GetInternalProperty(tile.Properties[key]);

                            if (prop is null || CheckTrigger(prop, triggers) == false)
                            {
                                continue;
                            }

                            if (prop.Once == true)
                            {
                                tile.Properties.Remove(key);
                            }
                                
                            properties.Add(new(prop, tile));
                        }
                    }
                }
            }
            return properties;
        }

        /// <summary>
        /// Load the tile properties to a location from the dictionary asset
        /// </summary>
        /// <param name="l">The location to load properties to</param>
        /// <remarks>
        /// I really hope this works because I do not want to re-write this monstrosity
        /// </remarks>
        internal static void LoadLocation(GameLocation l)
        {
            if (Context.Config.Enabled == false)
            {
                return;
            }

            var dict = Context.Helper.GameContent.Load<Dictionary<string, DynamicTile>>(Context.TileDataDictPath);
            int internalPropCounter = 0;
            foreach (var item in dict)
            {
                int count = 0;
                int count2 = 0;
                var value = item.Value;
                var tileSheets = l.Map.TileSheets.ToList();

                if (IsValidLocation(value, l) == false)
                {
                    continue;
                }

                foreach (var layer in l.Map.Layers)
                {
                    if ((value.Layers?.Contains(layer.Id) ?? true) == false)
                    {
                        continue;
                    }
                    int width = layer.Tiles.Array.GetLength(0);
                    int height = layer.Tiles.Array.GetLength(1);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            var tile = layer.Tiles[x, y];
                            if (tile is null)
                            {
                                continue;
                            }
                            Point tilePoint = new(x, y);
                            if (IsValidTile(value, tile, tilePoint) == false)
                            {
                                continue;
                            }
                            count++;
                            foreach (var prop in value.Properties)
                            {
                                layer.Tiles[x, y].Properties[prop.Key] = prop.Value;
                            }                                
                            foreach (var action in value.Actions)
                            {
                                ++count2;
                                layer.Tiles[x, y].Properties[$"DMT_PROP_INTERNAL_{++internalPropCounter}"] = internalPropCounter.ToString();
                                Context.InternalProperties.Value[internalPropCounter.ToString()] = action;
                            }
                        }
                    }
                }

                Context.Monitor.Log($"Added {count} properties from {item.Key}{(count2 > 0 ? $" (Including {count2} new format properties)" : "")} to {l.Name}");
            }
        }

        internal static bool HasProperty(this Tile t, string key, [NotNullWhen(true)] out string? propertyValue)
        {
            propertyValue = null;
            if (t is null || t.Properties is null || t.Properties.Count == 0)
            {
                return false;
            }
                
            if (t.Properties.TryGetValue(key, out var prop) == true)
            {
                propertyValue = prop;
                return true;
            }

            foreach (var item in t.Properties)
            {
                if (item.Key.StartsWith("DMT_PROP_INTERNAL") == false)
                {
                    continue;
                }
                    
                if (Context.InternalProperties.Value[item.Value].Key.Equals(key, StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }
                propertyValue = item.Value;
                return true;
            }

            return false;
        }

        internal static bool IsValidLocation(DynamicTile tileData, GameLocation l)
        {
            var tileSheets = l.Map.TileSheets.ToList();
            if ((tileData.Properties?.Any() ?? false) == false && (tileData.Actions?.Any() ?? false) == false)
            {
                return false;
            }

            if (tileData.Locations is not null && 
                tileData.Locations.Count > 0 && 
                tileData.Locations.Contains(l.Name) == false)
            {
                return false;
            }

            if (tileData.TileSheets is not null && 
                tileData.TileSheets.Count > 0 && 
                tileData.TileSheets.Exists(x => tileSheets.Exists(y => y.Id == x) == true) == false)
            {
                return false;
            }

            if (tileData.TileSheetsPaths is not null && 
                tileData.TileSheetsPaths.Count > 0 && 
                tileData.TileSheetsPaths.Exists(x => tileSheets.Exists(y => y.ImageSource.Contains(x) == true)) == false)
            {
                return false;
            }

            return true;
        }

        internal static bool IsValidTile(DynamicTile tileData, Tile tile, Point tilePoint)
        {
            if (tileData.TileSheets is not null && tileData.TileSheets.Count > 0 && tileData.TileSheets.Contains(tile.TileSheet.Id) == false)
            {
                return false;
            }

            if (tileData.TileSheetsPaths is not null && tileData.TileSheetsPaths.Count > 0 && tileData.TileSheetsPaths.Exists(tile.TileSheet.ImageSource.Contains) == false)
            {
                return false;
            }

            if (tileData.Indexes is not null && tileData.Indexes.Count > 0 && tileData.Indexes.Contains(tile.TileIndex) == false)
            {
                return false;
            }

            if (tileData.Rectangles is not null && tileData.Rectangles.Count > 0 && tileData.Rectangles.Exists(x => x.Contains(tilePoint) == true) == false)
            {
                return false;
            }

            if (tileData.Tiles is not null && tileData.Tiles.Count > 0 && tileData.Tiles.Contains(new(tilePoint.X, tilePoint.Y)) == false)
            {
                return false;
            }

            return true;
        }
    }
}
