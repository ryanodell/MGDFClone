using MGDFClone.Features.WorldGen;

namespace MGDFClone.Models; 
public class ElevationParameters {
    public float WaterElevation { get; set; }
    public float MaxElevationInMeters { get; set; }
    public int PerlinOctaves { get; set; }
    public float WaterToSandOffset { get; set; }
    public float SandToGrassOffet { get; set; }
    public float GrassToHillOffset {  get; set; }

    public static ElevationParameters Default = new ElevationParameters {
        WaterElevation = 0.300f,
        MaxElevationInMeters = 7000.0f,
        PerlinOctaves = 3,
        WaterToSandOffset = 0.05f,
        SandToGrassOffet = 0.30f,
        GrassToHillOffset = 0.15f
    };
}
