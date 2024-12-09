using Microsoft.Xna.Framework;

namespace DMT.Data
{
    public class DynamicTile
    {
        public List<string>? locations;
        public List<string> Locations
        {
            get => locations ??= new List<string>();
            set => locations = value;
        }

        public List<string>? layers;
        public List<string> Layers
        {
            get => layers ??= new List<string>();
            set => layers = value;
        }

        public List<string>? tileSheets;
        public List<string> TileSheets
        {
            get => tileSheets ??= new List<string>();
            set => tileSheets = value;
        }

        public List<string>? tileSheetPaths;
        public List<string> TileSheetsPaths
        {
            get => tileSheetPaths ??= new List<string>();
            set => tileSheetPaths = value;
        }

        public List<int>? indexes;
        public List<int> Indexes
        {
            get => indexes ??= new List<int>();
            set => indexes = value;
        }

        public List<Rectangle>? rectangles;
        public List<Rectangle> Rectangles
        {
            get => rectangles ??= new List<Rectangle>();
            set => rectangles = value;
        }

        public List<Vector2>? tiles;
        public List<Vector2> Tiles
        {
            get => tiles ??= new List<Vector2>();
            set => tiles = value;
        }

        public Dictionary<string, string>? properties; //<- Converted to DynamicTileProperty at runtime
        public Dictionary<string, string> Properties
        {
            get => properties ??= new Dictionary<string, string>();
            set => properties = value;
        }

        public List<DynamicTileProperty>? actions;
        public List<DynamicTileProperty> Actions
        {
            get => actions ??= new List<DynamicTileProperty>();
            set => actions = value;
        }
    }
}
