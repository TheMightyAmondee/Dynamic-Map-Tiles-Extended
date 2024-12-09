using DMT.Data;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;
using System.Globalization;
using xTile.Layers;
using static StardewValley.Minigames.AbigailGame;

namespace DMT
{
    internal static class Patches
    {
        private static bool Enabled => Context.Config.Enabled;

        private static PerScreen<Farmer> ExplodingFarmer => new();

        internal static void Patch(ModEntry context)
        {
            Harmony harmony = new(context.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.explode)),
                prefix: new(typeof(Patches), nameof(GameLocation_Explode_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.explosionAt)),
                postfix: new(typeof(Patches), nameof(GameLocation_ExplosionAt_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xRectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character) }),
                prefix: new(typeof(Patches), nameof(GameLocation_IsCollidingPosition_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.draw)),
                postfix: new(typeof(Patches), nameof(GameLocation_Draw_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performToolAction)),
                prefix: new(typeof(Patches), nameof(GameLocation_PerformToolAction_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
                prefix: new(typeof(Patches), nameof(GameLocation_CheckAction_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getMovementSpeed)),
                postfix: new(typeof(Patches), nameof(Farmer_GetMovementSpeed_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.MovePosition)),
                prefix: new(typeof(Patches), nameof(Farmer_MovePosition_Prefix)),
                postfix: new(typeof(Patches), nameof(Farmer_MovePosition_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
                postfix: new(typeof(Patches), nameof(NPC_CheckAction_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Monster), nameof(Monster.takeDamage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) }),
                postfix: new(typeof(Patches), nameof(Monster_TakeDamage_Postfix))
            );
        }

        internal static void GameLocation_Explode_Prefix(Farmer who)
        {
            if (Enabled == false)
            {
                return;
            }
            ExplodingFarmer.Value = who;
        }

        internal static void GameLocation_ExplosionAt_Postfix(GameLocation __instance, float x, float y)
        {
            if (Enabled == false || __instance.isTileOnMap(new Vector2(x, y)) == false || (Context.Config.TriggerDuringEvents == false && Game1.eventUp == true == true))
            {
                return;
            }

            foreach (var layer in __instance.map.Layers)
            {
                string[] explodetrigger = { "Explode" };
                var tile = layer.Tiles[(int)x, (int)y];

                if (tile is null || tile.HasProperty(Keys.ExplodeKey, out var prop) == false)
                {
                    continue;
                }
                if (ExplodingFarmer.Value is not null && ExplodingFarmer.Value.currentLocation.Name == __instance.Name)
                {
                    if (string.IsNullOrEmpty(prop) == false && ExplodingFarmer.Value.mailReceived.Contains(prop) == false)
                    {
                        ExplodingFarmer.Value.mailReceived.Add(prop);
                    }                       
                    TriggerActions(new List<Layer> { tile.Layer }, ExplodingFarmer.Value, new((int)x, (int)y), explodetrigger);
                }
                layer.Tiles[(int)x, (int)y] = null;
            }
        }

        internal static bool GameLocation_IsCollidingPosition_Prefix(GameLocation __instance, Rectangle position, ref bool __result)
        {
            if (Enabled == false || Context.PushTileDict.TryGetValue(__instance.Name, out var tiles) == false)
            {
                return true;
            }
                
            foreach (var tile in tiles)
            {
                if (position.Intersects(new(tile.Position, new(64))) == false)
                {
                    continue;
                }                 
                __result = true;
                return false;
            }

            return true;
        }

        internal static void GameLocation_Draw_Postfix(GameLocation __instance)
        {
            if (Enabled == false || Context.PushTileDict.TryGetValue(__instance.Name, out var tiles) == false)
            {
                return;
            }               

            foreach (var tile in tiles)
            {
                Game1.mapDisplayDevice.DrawTile(tile.Tile, new(tile.Position.X - Game1.viewport.X, tile.Position.Y - Game1.viewport.Y), (tile.Position.Y + 64 + (tile.Tile.Layer.Id.Contains("Front") ? 16 : 0)) / 10000f);
            }
               
        }

        internal static bool GameLocation_PerformToolAction_Prefix(GameLocation __instance, Tool t, int tileX, int tileY, ref bool __result)
        {
            string[] trigger = { string.Format(Triggers.UseTool, Utils.BuildFormattedTrigger(t.GetType().Name)) };
            List<Layer> layers = __instance.Map.Layers.ToList();

            if (Enabled == false || t is null || t.getLastFarmerToUse() is null || __instance.isTileOnMap(new Vector2(tileX, tileY)) == false)
            {
                return true;
            }
            if (TriggerActions(layers, t.getLastFarmerToUse(), new(tileX, tileY), trigger) == false)
            {
                return true;
            }               
            __result = true;
            return false;
        }

        internal static bool GameLocation_CheckAction_Prefix(GameLocation __instance, xLocation tileLocation, Farmer who, ref bool __result)
        {
            if (Enabled == false || __instance.isTileOnMap(new Vector2(tileLocation.X, tileLocation.Y)) == false)
            {
                return true;
            }
            List<Layer> layers = __instance.Map.Layers.ToList();
            
            if (who.ActiveItem is not Item item 
                || TriggerActions(layers, 
                    who, 
                    new(tileLocation.X, tileLocation.Y), 
                    new string[3] { 
                        string.Format(
                            Triggers.UseItem, 
                            BuildFormattedTrigger(
                                item.Name, 
                                '-', 
                                item.Stack, 
                                '-', 
                                item.Quality)), 
                        string.Format(Triggers.UseItem, 
                        BuildFormattedTrigger(
                            item.QualifiedItemId, 
                            '-', 
                            item.Stack, 
                            '-', 
                            item.Quality)), 
                        "Action" }) 
                == false)
            {
                return true;
            }
                

            __result = true; 
            return false;
        }

        internal static void Farmer_GetMovementSpeed_Postfix(Farmer __instance, ref float __result)
        {
            if (Enabled == false || (Context.Config.TriggerDuringEvents == false && Game1.eventUp == true == true) || __instance.currentLocation is null)
            {
                return;
            }
                
            var tilePos = __instance.TilePoint;
            if (__instance.currentLocation.isTileOnMap(tilePos) == false)
            {
                return;
            }            

            var tile = __instance.currentLocation.Map.GetLayer("Back").Tiles[tilePos.X, tilePos.Y];
            if (tile is not null && tile.HasProperty(Keys.SpeedKey, out var prop) == true && float.TryParse(prop, NumberStyles.Any, CultureInfo.InvariantCulture, out var multiplier) == true)
            {
                __result *= multiplier;
            }               
        }

        internal static void Farmer_MovePosition_Prefix(Farmer __instance, ref Vector2[] __state)
        {
            if (Enabled == false || (Context.Config.TriggerDuringEvents == false && Game1.eventUp == true) || __instance.currentLocation is null)
                return;
            var tileLoc = __instance.Tile;
            if (__instance.currentLocation.isTileOnMap(tileLoc))
            {
                var tile = __instance.currentLocation.Map.GetLayer("Back").Tiles[(int)tileLoc.X, (int)tileLoc.Y];
                if (tile?.HasProperty(Keys.MoveKey, out var prop) ?? false)
                {
                    var split = prop.ToString().Split(' ');
                    __instance.xVelocity = float.Parse(split[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                    __instance.yVelocity = float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                }
            }
            __state = new Vector2[] { __instance.Position, tileLoc};
        }

        internal static void Farmer_MovePosition_Postfix(Farmer __instance, ref Vector2[] __state)
        {
            if (Enabled == false || (Context.Config.TriggerDuringEvents == false && Game1.eventUp == true) || __state is null || __instance.currentLocation is null)
            {
                return;
            }                
            var f = __instance;
            var tilePos = f.TilePoint;
            var oldTilePos = Utility.Vector2ToPoint(__state[1]);
            var backlayer = f.currentLocation.Map.GetLayer("Back");
            var layers = new List<Layer>() { backlayer }; 

            if (oldTilePos != tilePos)
            {
                TriggerActions(layers, f, oldTilePos, new string[1] { "Off" });
                TriggerActions(layers, f, tilePos, new string[1] { "On" });
            }

            if (f.currentLocation.isTileOnMap(tilePos) == true)
            {
                var tile = backlayer.Tiles[tilePos.X, tilePos.Y];
                var oldTile = backlayer.Tiles[oldTilePos.X, oldTilePos.Y];
                
               
                if ((tile?.HasProperty(Keys.SlipperyKey, out var prop) ?? false) && float.TryParse(prop, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) == true)
                {
                    //Cap off with Math.Max (determine max allowed speed)
                    if (f.movementDirections.Contains(0) == true)
                    {
                        f.yVelocity = Math.Max(f.yVelocity + amount, .16f);
                    }
                    if (f.movementDirections.Contains(1) == true)
                    {
                        f.xVelocity = Math.Max(f.xVelocity + amount, .16f);
                    }
                        
                    if (f.movementDirections.Contains(2) == true)
                    {
                        f.yVelocity = -Math.Max(Math.Abs(f.yVelocity) + amount, .16f);
                    }                       
                    if (f.movementDirections.Contains(3) == true)
                    {
                        f.xVelocity = -Math.Max(Math.Abs(f.xVelocity) + amount, .16f);
                    }                           
                }
                else if ((oldTile?.HasProperty(Keys.SlipperyKey, out prop) ?? false) && float.TryParse(prop, NumberStyles.Any, CultureInfo.InvariantCulture, out _) == true)
                {
                    f.xVelocity = 0f;
                    f.yVelocity = 0f;
                }
            }

            if (f.movementDirections.Any() == true && __state[0] == f.Position)
            {
                Point startTile = new(f.GetBoundingBox().Center.X / 64, f.GetBoundingBox().Center.Y / 64);
                startTile += GetNextTile(f.FacingDirection);
                Point start = new(startTile.X * 64, startTile.Y * 64);
                xLocation startLoc = new(startTile.X, startTile.Y);

                var buildings = f.currentLocation.Map.GetLayer("Buildings");
                var tile = buildings.PickTile(startLoc, Game1.viewport.Size);

                if ((tile?.HasProperty(Keys.PushKey, out var prop) ?? false) == false && (tile?.HasProperty(Keys.PushableKey, out prop) ?? false) == false)
                {
                    return;
                }                    
                var destination = startTile + GetNextTile(f.FacingDirection);
                foreach (var item in prop.ToString().Split(','))
                {
                    var split = item.Split(' ');
                    if (split.Length != 2 || int.TryParse(split[0], out int x) == false || int.TryParse(split[1], out int y) == false || destination.X != x || destination.Y != y)
                    {
                        continue;
                    }                       
                    PushTilesWithOthers(f, tile, startTile);
                    break;
                }
            }
        }

        internal static void NPC_CheckAction_Postfix(NPC __instance, Farmer who)
        {
            if (Enabled == false || (Context.Config.TriggerDuringEvents == false && Game1.eventUp == true) || Game1.dialogueUp == false || __instance.currentLocation?.Name != who.currentLocation.Name)
            {
                return;
            }               
            var layers = who.currentLocation.Map.Layers.ToList();
            TriggerActions(layers, who, __instance.TilePoint, new string[1] { string.Format(Triggers.TalkToNPC, Utils.BuildFormattedTrigger(__instance.Name))});
        }

        internal static void Monster_TakeDamage_Postfix(Monster __instance, Farmer who)
        {
            if (Enabled == false || (Context.Config.TriggerDuringEvents == false && Game1.eventUp == true) || __instance.Health > 0 || __instance.currentLocation?.Name != who.currentLocation.Name)
            {
                return;
            }
            var layers = who.currentLocation.Map.Layers.ToList();
            TriggerActions(layers, who, __instance.TilePoint, new string[1] { string.Format(Triggers.MonsterSlain, Utils.BuildFormattedTrigger(__instance.Name)) });
        }
    }
}
