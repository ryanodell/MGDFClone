namespace MGDFClone.Models; 
public class ClimateParameters {
    public float MountainThreshold { get; set; }
    public float PercipitationFactor { get; set; }
    public float RainShadowEffect { get; set; }
    public float EastwardDissipation { get; set; }
    public float BaseMoisture { get; set; }
    public float MinimumHumidity { get; set; }
    public float MaximumHunidty { get; set; }
    public int   PerlinOctaves { get; set; }
    public float WaterFactor { get; set; }

    public static ClimateParameters Default = new ClimateParameters {
        MountainThreshold = 0.8f,
        PercipitationFactor = 0.9f,
        RainShadowEffect = 0.2f,
        EastwardDissipation = 0.85f,
        BaseMoisture = 0.25f,
        MinimumHumidity = 0.0f,
        MaximumHunidty = 100.0f,
        PerlinOctaves = 3,
        WaterFactor = 20.0f
    };
}
