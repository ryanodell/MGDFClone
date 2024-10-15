using MGDFClone.Features.WorldGen;

namespace MGDFClone.Models {
    public class WorldTemperatureParametersV2 {
        public float MinimumModerateTemperature { get; set; }
        public float MaximumModerateTemperature { get; set; }
        public float MinimumExtremeTemperature { get; set; }
        public float MaximumExtremeTemperature { get; set; }
        public float ModerateRegionHeightFraction { get; set; }

        public static WorldTemperatureParametersV2 Default = new WorldTemperatureParametersV2 {
            MinimumModerateTemperature = 60.0f,
            MaximumModerateTemperature = 90.0f,
            MinimumExtremeTemperature = -20.0f,
            MaximumExtremeTemperature = 110.0f,
            ModerateRegionHeightFraction = 0.50f
        };
    }


    public class WorldTemperatureParameters {
        public float MinimumTemperature {get; set;}
        public float MaximumTemperature {get; set;}
        public float WaterCoolingFactor { get; set; }
        public float WaterTemperature { get; set; }
        public float LapseRate { get; set; }
        public eSeason Season { get; set; }
        public ePolarRegion PolarRegion { get; set; }

        public static WorldTemperatureParameters Default = new WorldTemperatureParameters {
            MinimumTemperature = -20.0f,
            MaximumTemperature = 120.0f,
            WaterCoolingFactor = 10.0f,
            WaterTemperature = 50.0f,
            LapseRate = 11.7f,
            Season = eSeason.Summer,
            PolarRegion = ePolarRegion.SouthPole
        };
    }
}
