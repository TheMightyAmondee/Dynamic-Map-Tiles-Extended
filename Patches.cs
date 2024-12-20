using DMT.Data;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;
using System.Globalization;
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
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), [typeof(Rectangle), typeof(xRectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character)]),
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
                original: AccessTools.Method(typeof(Monster), nameof(Monster.takeDamage), [typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer)]),
                postfix: new(typeof(Patches), nameof(Monster_TakeDamage_Postfix))
            );
        }

        internal static void GameLocation_Explode_Prefix(Farmer who)
        {
            if (!Enabled)
                return;
            ExplodingFarmer.Value = who;
        }

        internal static void GameLocation_ExplosionAt_Postfix(GameLocation __instance, float x, float y)
        {
            if (!Enabled || !__instance.isTileOnMap(new Vector2(x, y)) || (!Context.Config.TriggerDuringEvents && Game1.eventUp))
                return;

            foreach (var layer in __instance.map.Layers)
            {
                var tile = layer.Tiles[(int)x, (int)y];
                if (tile is null || !tile.HasProperty(Keys.ExplodeKey, out var prop))
                    continue;
                if (ExplodingFarmer.Value is not null && ExplodingFarmer.Value.currentLocation.Name == __instance.Name)
                {
                    if (!string.IsNullOrEmpty(prop) && !ExplodingFarmer.Value.mailReceived.Contains(prop))
                        ExplodingFarmer.Value.mailReceived.Add(prop);
                    TriggerActions([tile.Layer], ExplodingFarmer.Value, new((int)x, (int)y), ["Explode"]);
                }
                layer.Tiles[(int)x, (int)y] = null;
            }
        }

        internal static bool GameLocation_IsCollidingPosition_Prefix(GameLocation __instance, Rectangle position, ref bool __result)
        {
            if (!Enabled || !Context.PushTileDict.TryGetValue(__instance.Name, out var tiles))
                return true;

            foreach (var tile in tiles)
            {
                if (!position.Intersects(new(tile.Position, new(64))))
                    continue;
                __result = true;
                return false;
            }

            return true;
        }

        internal static void GameLocation_Draw_Postfix(GameLocation __instance)
        {
            if (!Enabled || !Context.PushTileDict.TryGetValue(__instance.Name, out var tiles))
                return;

            foreach (var tile in tiles)
                Game1.mapDisplayDevice.DrawTile(tile.Tile, new(tile.Position.X - Game1.viewport.X, tile.Position.Y - Game1.viewport.Y), (tile.Position.Y + 64 + (tile.Tile.Layer.Id.Contains("Front") ? 16 : 0)) / 10000f);
        }

        internal static bool GameLocation_PerformToolAction_Prefix(GameLocation __instance, Tool t, int tileX, int tileY, ref bool __result)
        {
            if (!Enabled || t is null || t.getLastFarmerToUse() is null || !__instance.isTileOnMap(new Vector2(tileX, tileY)))
                return true;
            if (!TriggerActions([.. __instance.Map.Layers], t.getLastFarmerToUse(), new(tileX, tileY), [string.Format(Triggers.UseTool, Utils.BuildFormattedTrigger(t.GetType().Name))]))
                return true;
            __result = true;
            return false;
        }

        internal static bool GameLocation_CheckAction_Prefix(GameLocation __instance, xLocation tileLocation, Farmer who, ref bool __result)
        {
            if (!Enabled || !__instance.isTileOnMap(new Vector2(tileLocation.X, tileLocation.Y)))
                return true;
            if (who.ActiveItem is not Item item || !TriggerActions([.. __instance.Map.Layers], who, new(tileLocation.X, tileLocation.Y),
                [
                    string.Format(Triggers.UseItem, BuildFormattedTrigger(item.Name, '-', item.Stack, '-', item.Quality)),
                    string.Format(Triggers.UseItem, BuildFormattedTrigger(item.QualifiedItemId, '-', item.Stack, '-', item.Quality)),
                    "Action"
                ]))
                return true;

            __result = true; 
            return false;
        }

        internal static void Farmer_GetMovementSpeed_Postfix(Farmer __instance, ref float __result)
        {
            if (!Enabled || (!Context.Config.TriggerDuringEvents && Game1.eventUp) || __instance.currentLocation is null)
                return;

            var tilePos = __instance.TilePoint;
            if (!__instance.currentLocation.isTileOnMap(tilePos))
                return;

            var tile = __instance.currentLocation.Map.GetLayer("Back").Tiles[tilePos.X, tilePos.Y];
            if (tile is not null && tile.HasProperty(Keys.SpeedKey, out var prop) && float.TryParse(prop, NumberStyles.Any, CultureInfo.InvariantCulture, out var multiplier))
                __result *= multiplier;
        }

        internal static void Farmer_MovePosition_Prefix(Farmer __instance, ref Vector2[] __state)
        {
            if (!Enabled || (!Context.Config.TriggerDuringEvents && Game1.eventUp) || __instance.currentLocation is null)
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
            __state = [__instance.Position, tileLoc];
        }

        internal static void Farmer_MovePosition_Postfix(Farmer __instance, ref Vector2[] __state)
        {
            if (!Enabled || (!Context.Config.TriggerDuringEvents && Game1.eventUp) || __state is null || __instance.currentLocation is null)
                return;
            var f = __instance;
            var tilePos = f.TilePoint;
            var oldTilePos = Utility.Vector2ToPoint(__state[1]);
            var layer = f.currentLocation.Map.GetLayer("Back");
            if (oldTilePos != tilePos)
            {
                TriggerActions([layer], f, oldTilePos, ["Off"]);
                TriggerActions([layer], f, tilePos, ["On"]);
            }

            if (f.currentLocation.isTileOnMap(tilePos))
            {
                var tile = layer.Tiles[tilePos.X, tilePos.Y];
                var oldTile = layer.Tiles[oldTilePos.X, oldTilePos.Y];
                
                if ((oldTile?.HasProperty(Keys.SlipperyKey, out var prop) ?? false) && float.TryParse(prop, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    f.xVelocity = 0f;
                    f.yVelocity = 0f;
                }
                if ((tile?.HasProperty(Keys.SlipperyKey, out prop) ?? false) && float.TryParse(prop, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                {
                    //Cap off with Math.Max (determine max allowed speed)
                    if (f.movementDirections.Contains(0))
                        f.yVelocity = Math.Max(f.yVelocity + amount, .16f);
                    if (f.movementDirections.Contains(1))
                        f.xVelocity = Math.Max(f.xVelocity + amount, .16f);
                    if (f.movementDirections.Contains(2))
                        f.yVelocity = -Math.Max(Math.Abs(f.yVelocity) + amount, .16f);
                    if (f.movementDirections.Contains(3))
                        f.xVelocity = -Math.Max(Math.Abs(f.xVelocity) + amount, .16f);
                }
            }

            if (f.movementDirections.Any() && __state[0] == f.Position)
            {
                Point startTile = new(f.GetBoundingBox().Center.X / 64, f.GetBoundingBox().Center.Y / 64);
                startTile += GetNextTile(f.FacingDirection);
                Point start = new(startTile.X * 64, startTile.Y * 64);
                xLocation startLoc = new(startTile.X, startTile.Y);

                var buildings = f.currentLocation.Map.GetLayer("Buildings");
                var tile = buildings.PickTile(startLoc, Game1.viewport.Size);

                if (!(tile?.HasProperty(Keys.PushKey, out var prop) ?? false) && !(tile?.HasProperty(Keys.PushableKey, out prop) ?? false))
                    return;
                var destination = startTile + GetNextTile(f.FacingDirection);
                foreach (var item in prop.ToString().Split(','))
                {
                    var split = item.Split(' ');
                    if (split.Length != 2 || !int.TryParse(split[0], out int x) || !int.TryParse(split[1], out int y) || destination.X != x || destination.Y != y)
                        continue;
                    PushTilesWithOthers(f, tile, startTile);
                    break;
                }
            }
        }

        internal static void NPC_CheckAction_Postfix(NPC __instance, Farmer who)
        {
            if (!Enabled || (!Context.Config.TriggerDuringEvents && Game1.eventUp) || !Game1.dialogueUp || __instance.currentLocation?.Name != who.currentLocation.Name)
                return;
            TriggerActions([.. who.currentLocation.Map.Layers], who, __instance.TilePoint, [string.Format(Triggers.TalkToNPC, Utils.BuildFormattedTrigger(__instance.Name))]);
        }

        internal static void Monster_TakeDamage_Postfix(Monster __instance, Farmer who)
        {
            if (!Enabled || (!Context.Config.TriggerDuringEvents && Game1.eventUp) || __instance.Health > 0 || __instance.currentLocation?.Name != who.currentLocation.Name)
                return;
            TriggerActions([.. who.currentLocation.Map.Layers], who, __instance.TilePoint, [string.Format(Triggers.MonsterSlain, Utils.BuildFormattedTrigger(__instance.Name))]);
        }
    }
}
