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
