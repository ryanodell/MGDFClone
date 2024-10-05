using Microsoft.Xna.Framework;

namespace MGDFClone.Features.MapGen {
    public static class TileTypeHelper {

        public static eTileMapType DetermineBaseTerrain(float value) {

            if (value < 0.20f)
                return eTileMapType.DeepWater; // Low elevation = Water
            if (value < 0.30f)
                return eTileMapType.Water; // Low elevation = Water
            else if (value < 0.35f)
                return eTileMapType.Sand; // Slightly higher = Sand (beach)
            else if (value < 0.50f)
                return eTileMapType.Grass; // Middle = Grasslands
            else if (value < 0.70f)
                return eTileMapType.Hill; // Middle = Grasslands
            else if (value < 0.80f)
                return eTileMapType.Mountain; // Higher = Mountain
            else
                return eTileMapType.Snow; // Highest = Snow
        }

        public static void SetSpriteData(ref eSprite sprite, ref Color color, eTileMapType eTileMapType) {
            switch (eTileMapType) {
                case eTileMapType.DeepWater:
                    sprite = eSprite.Water2;
                    color = Color.DarkBlue;
                    break;
                case eTileMapType.Water:
                    sprite = eSprite.Water2;
                    color = Color.Blue;
                    break;
                case eTileMapType.Sand:
                    sprite = eSprite.CurhsedRocks2;
                    color = Color.Yellow;
                    break;
                case eTileMapType.Grass:
                    sprite = eSprite.TallGrass;
                    color = Color.DarkGreen;
                    break;
                case eTileMapType.SmallTree:
                    sprite = eSprite.SmallTree;
                    color = Color.DarkOliveGreen;
                    break;
                case eTileMapType.Forest:
                    sprite = eSprite.BigTree;
                    color = Color.Green;
                    break;
                case eTileMapType.Hill:
                    sprite = eSprite.Mountain;
                    color = Color.SaddleBrown;
                    break;
                case eTileMapType.Mountain:
                    sprite = eSprite.TriangleUp;
                    color = Color.Gray;
                    break;
                case eTileMapType.Snow:
                    sprite = eSprite.Tilde;
                    color = Color.White;
                    break;
                default:
                    break;
            }
        }
    }
}
