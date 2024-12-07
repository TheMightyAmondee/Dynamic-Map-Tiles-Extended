global using xRectangle = xTile.Dimensions.Rectangle;
global using xLocation = xTile.Dimensions.Location;
global using xSize = xTile.Dimensions.Size;
global using SContext = StardewModdingAPI.Context;
global using LogLevel = StardewModdingAPI.LogLevel;
global using static DMT.ModEntry;
global using static DMT.Utils;
using DMT.Data;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using DMT.APIs;
using StardewModdingAPI.Utilities;
using StardewValley;
using Microsoft.Xna.Framework;
using Netcode;

namespace DMT
{
    internal class ModEntry : Mod
    {
        public string TileDataDictPath => $"DMT/Tiles";
        public string AnimationDataDictPath => $"DMT/Animations";

        internal static ModEntry Context { get; private set; }

        public Config Config { get; private set; }

        public Dictionary<string, List<PushedTile>> PushTileDict { get; } = [];

        public Dictionary<string, List<Animation>> AnimationsDict { get; private set; } = [];

        public Dictionary<string, DynamicTile> DynamicTiles { get; private set; } = [];

        internal PerScreen<Dictionary<string, DynamicTileProperty>> InternalProperties = new(() => []);

        internal PerScreen<SecondUpdateData> SecondUpdateLoops = new(() => new());

        public override void Entry(IModHelper helper)
        {
            Context = this;

            Config = Helper.ReadConfig<Config>();

            Helper.Events.GameLoop.GameLaunched += onGameLaunched;
            Helper.Events.Player.Warped += onWarped;
            Helper.Events.Content.AssetRequested += onAssetRequested;
            Helper.Events.Content.AssetsInvalidated += onAssetInvalidated;
            Helper.Events.GameLoop.OneSecondUpdateTicked += onOneSecondUpdate;
            Helper.Events.GameLoop.SaveLoaded += onSaveLoad;

            Helper.ConsoleCommands.Add("dmt", "DMT test commands", onConsoleCommand);
        }

        private void onSaveLoad(object? sender, SaveLoadedEventArgs e)
        {
            var passOutEvent = Helper.Reflection.GetField<NetEvent0>(Game1.player, "passOutEvent", false);
            passOutEvent.GetValue().onEvent += onFarmerPassOut;
        }

        private void onOneSecondUpdate(object? sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.Enabled || !SContext.IsPlayerFree || SecondUpdateLoops.Value.Loops <= 0)
                return;
            --SecondUpdateLoops.Value.Loops;

            var who = SecondUpdateLoops.Value.Who;
            var value = SecondUpdateLoops.Value.Value;
            var value2 = SecondUpdateLoops.Value.FloatValue;

            if (SecondUpdateLoops.Value.IsHealth)
            {
                if (value > 0)
                {
                    who.health = Math.Min(who.health + value, who.maxHealth);
                    who.currentLocation.debris.Add(new(value, new(who.getStandingPosition().X + 8, who.getStandingPosition().Y), Color.LimeGreen, 1f, who));
                }
                else
                    who.takeDamage(Math.Abs(value), false, null);
                return;
            }
            who.Stamina += value2;
        }

        private void onAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo(AnimationDataDictPath)))
            {
                AnimationsDict = [];
                AnimationsDict = Helper.GameContent.Load<Dictionary<string, List<Animation>>>(AnimationDataDictPath);
            }

            if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo(TileDataDictPath)) && SContext.IsWorldReady)
                LoadLocation(Game1.player.currentLocation);
        }

        private void onAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(TileDataDictPath))
                e.LoadFrom(() => new Dictionary<string, DynamicTile>(), AssetLoadPriority.Exclusive);
            if (e.NameWithoutLocale.IsEquivalentTo(AnimationDataDictPath))
                e.LoadFrom(() => new Dictionary<string, List<Animation>>(), AssetLoadPriority.Exclusive);
        }

        private void onWarped(object? sender, WarpedEventArgs e)
        {
            GameLocation l = e.NewLocation;
            LoadLocation(l);
            TriggerActions([.. l.Map.Layers], e.Player, e.Player.TilePoint, ["Enter"]);
        }

        private void onGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            Patches.Patch(this);

            var configMenu = Helper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new(),
                save: () => Helper.WriteConfig(Config)
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.Enabled,
                setValue: value => Config.Enabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Trigger During Events",
                getValue: () => Config.TriggerDuringEvents,
                setValue: value => Config.TriggerDuringEvents = value
            );
        }

        private void onConsoleCommand(string cmd, string[] args)
        {
            if (args.Length == 0 || args.Length < 2 || !SContext.IsPlayerFree)
                return;
            var who = Game1.player;
            var l = who.currentLocation;
            l.setTileProperty(who.TilePoint.X, who.TilePoint.Y, "Back", args[0] + "_Once_On", args[1]);
            TriggerActions([l.Map.GetLayer("Back")], who, who.TilePoint, ["On"]);
        }

        private void onFarmerPassOut()
        {
            if (SecondUpdateLoops.Value.Loops <= 0)
                return;
            SecondUpdateLoops.Value.Loops = 0;
        }
    }
}
