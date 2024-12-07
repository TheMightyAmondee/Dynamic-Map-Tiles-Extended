using DMT.Data;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Layers;
using xTile.Tiles;

namespace DMT.APIs
{
    public class API : IApi
    {
        public bool TriggerActions(IEnumerable<Layer> layers, Farmer who, Point tilePosition, IEnumerable<string> triggers)
        {
            return Utils.TriggerActions([.. layers], who, tilePosition, [.. triggers]);
        }

        public bool AddGlobalTrigger(string regex) => Triggers.GlobalTriggers.Add(regex);

        public bool RegisterAction(string key, Action<Farmer, string, Tile, Point> handler)
        {
            if (!Keys.ModKeys.Add(key))
                return false;
            Actions.ModActions.Add(key, handler);
            return true;
        }
    }
}
