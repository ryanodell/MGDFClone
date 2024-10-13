using System.Diagnostics.CodeAnalysis;

namespace MGDFClone.Features.WorldGen;
public static class BiomeManagerV1 {

    public static Dictionary<eBiome, List<eTrees>> TreeCache = new Dictionary<eBiome, List<eTrees>> {
        {
            eBiome.DryArid, new List<eTrees> {
                eTrees.Joshua,
                eTrees.Baobab,
                eTrees.Olive,
                eTrees.DatePalm
            }
        },
        {
            eBiome.Temperate, new List<eTrees> {
                eTrees.Oak,
                eTrees.Maple,
                eTrees.Pine,
                eTrees.Redwood,
            }
        },
        {
            eBiome.Subtropical, new List<eTrees> {
                eTrees.Mangrove,
                eTrees.CoconutPalm,
                eTrees.Teak,
                eTrees.Rubber,
                eTrees.Kapok,
            }
        },
        {
            eBiome.Tropical, new List<eTrees> {
                eTrees.Mangrove,
                eTrees.CoconutPalm,
                eTrees.Teak,
                eTrees.Rubber,
                eTrees.Kapok,
            }
        },
        {
            eBiome.Cold, new List<eTrees> {
                eTrees.Spruce,
                eTrees.Birch,
                eTrees.Larch
            }
        }
    };

    public static eBiome GetBiome(float temperature, float humidity) {
        eBiome returnValue = eBiome.Temperate;
        if (temperature > 90 && humidity < 25) {
            returnValue = eBiome.DryArid;
        }
        if (temperature > 90 && humidity > 50) {
            returnValue = eBiome.Subtropical;
        }
        if (temperature > 90 && humidity > 75) {
            returnValue = eBiome.Tropical;
        }
        if (temperature < 40 && humidity < 25) {
            returnValue = eBiome.Cold;
        }
        return returnValue;
    }

    public static eSprite GetSpriteForBiome(eBiome biome) {
        switch (biome) {
            case eBiome.DryArid:
                return eSprite.Cactus1;
            case eBiome.Tropical:
                return eSprite.PalmTree;
            case eBiome.Subtropical:
                return eSprite.BigTree;
            case eBiome.Temperate:
                return eSprite.SmallTree;
            case eBiome.Cold:
                return eSprite.PineTree;
            default:
                return eSprite.BigTree;
        }
    }
}

public enum eBiome {
    DryArid, // Low humidity - High Temperature
    Temperate, // Moderate Humidity - Variable Temperature
    Subtropical, // Moderate to high humidity, Warm Temperature
    Tropical, //High humidity high temperature
    Cold //Low humidity cold temperature
}

public enum eTrees {
    //Dry Arid
    Joshua,
    Baobab,
    Olive,
    DatePalm,

    //Temperate
    Oak,
    Maple,
    Pine,
    Redwood,

    //Subtropical
    Cypress,
    Eucalyptus,
    Magnolia,
    LiveOak,

    //Tropical
    Mangrove,
    CoconutPalm,
    Teak,
    Rubber,
    Kapok,

    //Cold
    Spruce,
    Birch,
    Larch
}
