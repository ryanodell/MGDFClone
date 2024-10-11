using MGDFClone.Features.WorldGen;

namespace MGDFClone.Models; 
public class ElevationParameters {
    public float WaterElevation { get; set; }
    public float MaxElevationInMeters { get; set; }
    public int PerlinOctaves { get; set; }

    public static ElevationParameters Default = new ElevationParameters {
        WaterElevation = 0.300f,
        MaxElevationInMeters = 7000.0f,
        PerlinOctaves = 3
    };
}
