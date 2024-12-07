using DMT.Data;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Objects;
using System.Globalization;
using xTile.Layers;
using xTile.Tiles;

namespace DMT
{
    public static class Actions
    {
        public static readonly Dictionary<string, Action<Farmer, string, Tile, Point>> ModActions = [];

        public static void DoAddLayer(Farmer who, string value) => AddLayer(who.currentLocation.map, value);

        public static void DoAddTileSheet(Farmer who, string value)
        {
            var split = value.Split(',');
            if (split.Length == 2)
                AddTileSheet(who.currentLocation.map, split[0], split[1]);
        }

        public static void DoChangeIndex(Farmer who, string value, Tile tile, Point tilePos)
        {
            if (string.IsNullOrEmpty(value))
            {
                tile.Layer.Tiles[tilePos.X, tilePos.Y] = null;
                return;
            }
            if (int.TryParse(value, out int num))
            {
                if (tile.Layer.Tiles[tilePos.X, tilePos.Y] is null)
                    tile.Layer.Tiles[tilePos.X, tilePos.Y] = new StaticTile(tile.Layer, tile.TileSheet, BlendMode.Alpha, num);
                else
                    tile.Layer.Tiles[tilePos.X, tilePos.Y].TileIndex = num;
                return;
            }
            if (value.Contains(','))
            {
                var indecesDuration = value.Split(',');
                var indeces = indecesDuration[0].Split(' ');
                List<StaticTile> tiles = [];
                foreach (var index in indeces)
                {
                    if (int.TryParse(index, out num))
                        tiles.Add(new StaticTile(tile.Layer, tile.TileSheet, BlendMode.Alpha, num));
                    else if (index.ToString().Contains('/'))
                    {
                        var sheetIndex = index.ToString().Split('/');
                        tiles.Add(new StaticTile(tile.Layer, who.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1])));
                    }

                }
                tile.Layer.Tiles[tilePos.X, tilePos.Y] = new AnimatedTile(tile.Layer, tiles.ToArray(), int.Parse(indecesDuration[1]));
                return;
            }
            if (value.Contains('/'))
            {
                var sheetIndex = value.Split('/');
                tile.Layer.Tiles[tilePos.X, tilePos.Y] = new StaticTile(tile.Layer, who.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1]));
            }
        }

        public static void DoChangeMultipleIndexes(Farmer who, string value, Tile tile, Point tilePos)
        {
            var tileInfos = value.Split('|');
            foreach (var tileInfo in tileInfos)
            {
                var pair = tileInfo.Split('=');
                var layerXY = pair[0].Split(' ');
                var l = who.currentLocation.Map.GetLayer(layerXY[0]);
                l ??= AddLayer(who.currentLocation.Map, layerXY[0]);
                if (string.IsNullOrEmpty(pair[1]))
                {
                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = null;
                    return;
                }
                if (int.TryParse(pair[1], out int num))
                {
                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = new StaticTile(l, tile.TileSheet, BlendMode.Alpha, num);
                    return;
                }
                if (pair[1].Contains(','))
                {
                    var indexesDuration = pair[1].Split(',');
                    var indexes = indexesDuration[0].Split(' ');
                    List<StaticTile> tiles = [];
                    foreach (var index in indexes)
                    {
                        if (int.TryParse(index, out num))
                            tiles.Add(new StaticTile(l, tile.TileSheet, BlendMode.Alpha, num));
                        else if (index.Contains('/'))
                        {
                            var sheetIndex = index.Split('/');
                            tiles.Add(new StaticTile(l, who.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1])));
                        }

                    }
                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = new AnimatedTile(l, [.. tiles], int.Parse(indexesDuration[1]));
                    return;
                }
                if (pair[1].Contains('/'))
                {
                    var sheetIndex = pair[1].Split('/');
                    l.Tiles[int.Parse(layerXY[1]), int.Parse(layerXY[2])] = new StaticTile(tile.Layer, who.currentLocation.Map.GetTileSheet(sheetIndex[0]), BlendMode.Alpha, int.Parse(sheetIndex[1]));
                    return;
                }
            }
        }

        public static void DoChangeProperties(string value, Tile tile)
        {
            var props = value.Split('|');
            foreach (var prop in props)
            {
                var pair = prop.Split('=');
                if (pair.Length == 2)
                    tile.Properties[pair[0]] = pair[1];
                else
                    tile.Properties.Remove(pair[0]);
            }
        }

        public static void DoChangeMultipleProperties(Farmer who, string value, Tile tile)
        {
            var tiles = value.Split('|');
            foreach (var prop in tiles)
            {
                var pair = prop.Split('=');
                if (pair.Length == 2)
                {
                    var tileInfo = pair[0].Split(',');
                    if (tileInfo.Length == 3)
                    {
                        tile.Layer.Tiles[int.Parse(tileInfo[0]), int.Parse(tileInfo[1])].Properties[tileInfo[2]] = pair[1];
                        continue;
                    }
                    if (tileInfo.Length == 4)
                    {
                        var l = who.currentLocation.Map.GetLayer(tileInfo[0]);
                        l ??= AddLayer(who.currentLocation.Map, tileInfo[0]);
                        l.Tiles[int.Parse(tileInfo[1]), int.Parse(tileInfo[2])].Properties[tileInfo[3]] = pair[1];
                    }
                }
            }
        }

        public static void DoPlaySound(Farmer who, string value)
        {
            var split = value.Split('|');
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Contains(','))
                {
                    var split2 = split[i].Split(',');
                    if (int.TryParse(split2[1], out int delay))
                        DelayedAction.playSoundAfterDelay(split2[0], delay, who.currentLocation);
                    continue;
                }
                if (i == 0)
                    who.currentLocation.playSound(split[i]);
                else
                    DelayedAction.playSoundAfterDelay(split[i], i * 300, who.currentLocation);
            }
        }

        public static void DoShowMessage(string value)
        {
            if (value.Contains('|'))
            {
                var split = value.Split('|');
                var letter = bool.Parse(split[0]);
                value = split[1];
                if (letter)
                    Game1.drawLetterMessage(value);
                return;
            }

            Game1.drawObjectDialogue(value);
        }

        public static void DoPlayEvent(string value)
        {
            if (!value.Contains('|'))
            {
                Game1.currentLocation.currentEvent = new(value, null, null);
                Game1.currentLocation.checkForEvents();
                return;
            }
            var split = value.Split('|');
            var eventStr = split[0];
            var assetName = split[1];
            string assetKey = "";
            if (assetName.Contains('#'))
            {
                var split2 = assetName.Split('#');
                assetName = split2[0];
                assetKey = split2[1];
            }
            var eventId = split[2];

            if (!string.IsNullOrWhiteSpace(eventStr) && Context.Helper.GameContent.Load<Dictionary<string, string>>(assetName).TryGetValue(assetKey, out eventStr))
            {
                Game1.currentLocation.currentEvent = new(eventStr, assetName, eventId);
                Game1.currentLocation.checkForEvents();
            }
        }

        public static void DoPlayMusic(string value) => Game1.changeMusicTrack(value);

        public static void DoAddMailflag(Farmer who, string value) => who.mailReceived.Add(value);

        public static void DoRemoveMailflag(Farmer who, string value) => who.mailReceived.Remove(value);

        public static void DoAddMailForTomorrow(Farmer who, string value)
        {
            if (!who.mailbox.Contains(value))
                who.mailbox.Add(value);
        }

        public static void DoInvalidateAsset(string value)
        {
            foreach (var asset in value.Split('|'))
                Context.Helper.GameContent.InvalidateCache(asset);
        }

        public static void DoTeleport(Farmer who, string value)
        {
            var split = value.Split(' ');
            if (split.Length != 2 || !int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                return;
            who.Position = new Vector2(x, y);
        }

        public static void DoTeleportTile(Farmer who, string value)
        {
            var split = value.Split(' ');
            if (split.Length != 2 || !int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y))
                return;
            who.Position = new Vector2(x * 64, y * 64);
        }

        public static void DoGive(Farmer who, string value)
        {
            Item? item = null;
            if (value.StartsWith("Money/"))
            {
                if (int.TryParse(value.Split('/')[1], out int number))
                    who.addUnearnedMoney(number);
            }
            else
                item = ParseItemString(value);
            if (item is null)
                return;
            who.holdUpItemThenMessage(item, false);
            if (!who.addItemToInventoryBool(item, false))
                Game1.createItemDebris(item, who.getStandingPosition(), who.FacingDirection);
        }

        public static void DoTake(Farmer who, string value)
        {
            Item? item = null;
            if (value.StartsWith("Money/"))
            {
                if (int.TryParse(value.Split('/')[1], out int number))
                    who.Money -= number;
            }
            else
                item = ParseItemString(value);
            if (item is null || !who.Items.ContainsId(item.ItemId))
                return;
            who.Items.ReduceId(item.ItemId, item.Stack);
        }

        public static void DoSpawnChest(Farmer who, string value)
        {
            var split = value.Split('=');
            var split2 = split[0].Split(' ');
            Vector2 tilePos = new(int.Parse(split2[0]), int.Parse(split2[1]));
            if (who.currentLocation.Objects.TryGetValue(tilePos, out var obj) && obj is Chest)
                return;
            int coins = 0;
            string? chestId = null;
            List<Item> items = [];
            if (split[1].Contains('|'))
            {
                split = split[1].Split('|');
                chestId = split[0];
            }
            split2 = split[1].Split(' ');
            foreach (var str in split2)
            {
                if (str.StartsWith("Money/"))
                    _ = int.TryParse(str.Split('/')[1], out coins);
                else
                {
                    var item = ParseItemString(str);
                    if (item is not null)
                        items.Add(item);
                }
            }
            if (coins > 0)
                items.Add(ItemRegistry.Create("(O)GoldCoin", coins));
            Chest chest = new(items, tilePos);

            if (!string.IsNullOrWhiteSpace(chestId))
            {
                var itemData = ItemRegistry.GetData("(BC)" + chestId);
                chest.SetBigCraftableSpriteIndex(itemData.SpriteIndex, lid_frame_count: 5);
            }

            /*Chest chest = new(true, tilePos, chestId)
            {
                Type = "interactive",
                CanBeSetDown = true
            };
            chest.bigCraftable.Value = true;
            chest.GetItemsForPlayer(who.UniqueMultiplayerID).AddRange(items);
            if (coins > 0)
                chest.GetItemsForPlayer(who.UniqueMultiplayerID).Add(ItemRegistry.Create("(O)GoldCoin", coins));*/
            who.currentLocation.overlayObjects[tilePos] = chest;
        }

        public static void DoUpdateHealth(Farmer who, string value)
        {
            if (!int.TryParse(value, out int number))
                return;
            if (number > 0)
            {
                who.health = Math.Min(who.health + number, who.maxHealth);
                who.currentLocation.debris.Add(new(number, new(who.getStandingPosition().X + 8, who.getStandingPosition().Y), Color.LimeGreen, 1f, who));
                return;
            }
            who.takeDamage(Math.Abs(number), false, null);
        }

        public static void DoUpdateHealthPerSecond(Farmer who, string value)
        {
            if (!value.Contains('|'))
                return;
            var split = value.Split('|');
            if (!int.TryParse(split[0], out int loops) || !int.TryParse(split[1], out int number))
                return;
            Context.SecondUpdateLoops.Value = new() { Loops = loops, Value = number, IsHealth = true, Who = who };
        }

        public static void DoUpdateStamina(Farmer who, string value)
        {
            if (!float.TryParse(value, out var number))
                return;
            who.Stamina += number;
        }

        public static void DoUpdateStaminaPerSecond(Farmer who, string value)
        {
            if (!value.Contains('|'))
                return;
            var split = value.Split('|');
            if (!int.TryParse(split[0], out int loops) || !float.TryParse(split[1], out var number))
                return;
            Context.SecondUpdateLoops.Value = new() { Loops = loops, FloatValue = number, IsHealth = false, Who = who };
        }

        public static void DoAddBuff(Farmer who, string value)
        {
            foreach (var item in value.Split('|'))
            {
                if (!item.Contains(','))
                {
                    who.applyBuff(item);
                    continue;
                }
                var opt = item.Split(',');
                who.applyBuff(new Buff(opt[0], displaySource: opt[1]));
            }
        }

        public static void DoEmote(Farmer who, string value)
        {
            if (!int.TryParse(value, out int id))
                return;
            who.doEmote(id);
        }

        public static void DoExplode(Farmer who, string value, Point tilePos)
        {
            var split = value.Split(' ');
            string explodeSound = "explosion";
            bool damagesFarmer = true;
            int damageRadius = 1;
            bool destroyObjects = true;
            int radius = 1;
            Vector2 pos = new(tilePos.X, tilePos.Y);

            if (split.Length == 7 && !string.IsNullOrWhiteSpace(split[6]))
                explodeSound = split[6];
            if (split.Length >= 6 && !string.IsNullOrWhiteSpace(split[5]))
                destroyObjects = bool.Parse(split[5]);
            if (split.Length >= 5 && !string.IsNullOrWhiteSpace(split[4]))
                damageRadius = int.Parse(split[4]);
            if (split.Length >= 4 && !string.IsNullOrWhiteSpace(split[3]))
                damagesFarmer = bool.Parse(split[3]);
            if (split.Length >= 3 && !string.IsNullOrWhiteSpace(split[2]))
                radius = int.Parse(split[2]);
            if (split.Length > 1 && !string.IsNullOrWhiteSpace(split[0]) && !string.IsNullOrWhiteSpace(split[1]))
                pos = new(int.Parse(split[0]), int.Parse(split[1]));


            who.currentLocation.playSound(explodeSound);
            who.currentLocation.explode(pos, radius, who, damagesFarmer, damageRadius, destroyObjects);
        }

        public static void DoAnimate(Farmer who, string value)
        {
            if (!value.Contains(','))
                return;
            Animation? anim;
            var opt = value.Split(',');
            if (opt.Length == 2)
            {
                if (!Context.AnimationsDict.TryGetValue(opt[0], out var anims))
                    return;
                anim = anims?.Find(x => x.Name.Equals(opt[1], StringComparison.OrdinalIgnoreCase));
            }
            else
                anim = JsonConvert.DeserializeObject<Animation>(value);
            if (anim?.ToSAnim() is not TemporaryAnimatedSprite sprite)
                return;

            who.currentLocation.removeTemporarySpritesWithIDLocal(sprite.id);
            who.currentLocation.TemporarySprites.Add(sprite);
        }

        public static void DoPushTiles(Farmer who, Tile tile, Point tilePos) => PushTilesWithOthers(who, tile, tilePos);

        public static void DoPushOtherTiles(Farmer who, string value, Tile tile, Point tilePos)
        {
            var split = value.Split(',');
            foreach (var item in split)
            {
                Layer l = tile.Layer;
                var pushed = item.Split(' ');
                if (pushed.Length == 3)
                {
                    l = who.currentLocation.Map.GetLayer(pushed[0]);
                    PushTilesWithOthers(who, l.Tiles[int.Parse(pushed[1]), int.Parse(pushed[2])], tilePos);
                    continue;
                }
                PushTilesWithOthers(who, l.Tiles[int.Parse(pushed[0]), int.Parse(pushed[1])], tilePos);
            }
        }

        public static void DoWarp(Farmer who, string value)
        {
            var split = value.Split(',');
            int x, y;
            if (split.Length == 2 || string.IsNullOrWhiteSpace(split[0]))
            {
                x = int.Parse(split[0]);
                y = int.Parse(split[1]);
                who.warpFarmer(new(who.TilePoint.X, who.TilePoint.Y, who.currentLocation.Name, x, y, false));
                return;
            }
            string location = split[0];
            if (Game1.getLocationFromName(location) is null)
            {
                Context.Monitor.Log($"[{nameof(Actions)}.{nameof(DoWarp)}] Location {location} could not be found", LogLevel.Error);
                return;
            }
            x = int.Parse(split[1]);
            y = int.Parse(split[2]);

            who.warpFarmer(new(who.TilePoint.X, who.TilePoint.Y, location, x, y, false));
        }
    }
}
