using MGDFClone.Features.WorldGen;

namespace MGDFClone.Models; 
public class ElevationParameters {
    public float WaterElevation = 0.60f;
    public float MaxElevationInMeters = 7000.0f;

    public static ElevationParameters Default = new ElevationParameters {
        WaterElevation = 0.60f,
        MaxElevationInMeters = 7000.0f
    };
}
