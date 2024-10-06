using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            else if (value < 0.70f)
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
                    return eSprite.CapitalF;
                case < -40.0f:
                    return eSprite.CapitalE;
                case < -30.0f:
                    return eSprite.CapitalD;
                case < -20.0f:
                    return eSprite.CapitalC;
                case < -10.0f:
                    return eSprite.CapitalB;
                case < 1.0f:
                    return eSprite.CapitalA;
                case < 10.0f:
                    return eSprite.Number1;
                case < 20.0f:
                    return eSprite.Number2;
                case < 30.0f:
                    return eSprite.Number3;
                case < 40.0f:
                    return eSprite.Number4;
                case < 50.0f:
                    return eSprite.Number5;
                case < 60.0f:
                    return eSprite.Number6;
                case < 70.0f:
                    return eSprite.Number7;
                case < 80.0f:
                    return eSprite.Number8;
                case < 90.0f:
                    return eSprite.Number9;
                case < 100.0f:
                    return eSprite.CapitalW;
                case < 110.0f:
                    return eSprite.CapitalX;
                case < 120.0f:
                    return eSprite.CapitalY;
                default:
                    return eSprite.CapitalZ;
            }
        }
        public static Color DetermineHumidityColor(float value) {
            switch (value) {
                case < 10.0f:
                    return Color.SaddleBrown;      // Very Dry (Desert-like)
                case < 20.0f:
                    return Color.Tan;              // Low Humidity
                case < 30.0f:
                    return Color.Goldenrod;        // Moderately Low
                case < 40.0f:
                    return Color.YellowGreen;      // Slightly Humid
                case < 50.0f:
                    return Color.Green;            // Balanced Humidity
                case < 60.0f:
                    return Color.MediumSeaGreen;   // Moderately Humid
                case < 70.0f:
                    return Color.LightSeaGreen;    // High Humidity
                case < 80.0f:
                    return Color.SkyBlue;          // Very High Humidity
                case < 90.0f:
                    return Color.CornflowerBlue;   // Saturated
                case <= 100.0f:
                    return Color.DarkBlue;         // Excessive Moisture
                default:
                    return Color.White;            // Out of Range
            }
        }


        public static Color DetermineTemperatureColor(float value) {
            switch (value) {
                case < -50.0f:
                    return Color.DarkBlue;      // Very Cold
                case < -40.0f:
                    return Color.Blue;          // Cold
                case < -30.0f:
                    return Color.CornflowerBlue; // Slightly Cold
                case < -20.0f:
                    return Color.LightSkyBlue;  // Chilly
                case < -10.0f:
                    return Color.Aquamarine;    // Cool
                case < 1.0f:
                    return Color.LightGreen;    // Mild
                case < 10.0f:
                    return Color.Green;         // Warm
                case < 20.0f:
                    return Color.YellowGreen;   // Warmer
                case < 30.0f:
                    return Color.Yellow;        // Moderate Heat
                case < 40.0f:
                    return Color.Gold;          // Hotter
                case < 50.0f:
                    return Color.Orange;        // Very Hot
                case < 60.0f:
                    return Color.OrangeRed;     // Scorching
                case < 70.0f:
                    return Color.Red;           // Extremely Hot
                case < 80.0f:
                    return Color.DarkRed;       // Dangerously Hot
                case < 90.0f:
                    return Color.Maroon;        // Intense Heat
                case < 100.0f:
                    return Color.Purple;        // Extreme Heat
                case < 110.0f:
                    return Color.Magenta;       // Superheated
                case < 120.0f:
                    return Color.HotPink;       // Overheated
                default:
                    return Color.White;         // Max Heat
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
