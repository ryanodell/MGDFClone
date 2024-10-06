using Microsoft.Xna.Framework;
using Serilog;

namespace MGDFClone.Features.MapGen {
    public static class TileTypeHelper {

        public static eTileMapType DetermineBaseTerrain(float value) {

            if (value < 0.20f)
                return eTileMapType.DeepWater; // Low elevation = Water
            if (value < 0.30f)
                return eTileMapType.Water; // Low elevation = Water
            else if (value < 0.35f)
                return eTileMapType.Sand; // Slightly higher = Sand (beach)
            else if (value < 0.60f)
                return eTileMapType.Grass; // Middle = Grasslands
            else if (value < 0.85f)
                return eTileMapType.Hill; // Middle = Grasslands
            else if (value < 0.90f)
                return eTileMapType.Mountain; // Higher = Mountain
            else
                return eTileMapType.Snow; // Highest = Snow
        }

        public static eSprite DetermineTemperatureTile(float value) {
            switch (value) {
                case < -50.0f:
                    return eSprite.CapitalZ;
                case < -30.0f:
                    return eSprite.CapitalY;
                case < -20.0f:
                    return eSprite.CapitalX;
                case < 0.0f:
                    return eSprite.CapitalO;
                case < 20.0f:
                    return eSprite.Number2;
                case < 40.0f:
                    return eSprite.Number4;
                case < 60.0f:
                    return eSprite.Number6;
                case < 80.0f:
                    return eSprite.Number8;
                case < 100.0f:
                    return eSprite.Number9;
                case < 120.0f:
                    return eSprite.House;
                default:
                    return eSprite.CapitalA;
            }
        }
        public static Color DetermineTemperatureColor(float value) {
            switch (value) {
                case < -50.0f:
                    return Color.White;
                case < -30.0f:
                    return Color.LightGray;
                case < -20.0f:
                    return Color.Gray;
                case < 0.0f:
                    return Color.DarkGray;
                case < 20.0f:
                    return Color.Blue;
                case < 40.0f:
                    return Color.DarkBlue;
                case < 60.0f:
                    return Color.Green;
                case < 80.0f:
                    return Color.DarkGreen;
                case < 100.0f:
                    return Color.LightYellow;
                case < 120.0f:
                    return Color.Red;
                default:
                    return Color.DarkRed;
            }
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
