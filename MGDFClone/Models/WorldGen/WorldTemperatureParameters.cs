using MGDFClone.Features.WorldGen;

namespace MGDFClone.Models {
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
