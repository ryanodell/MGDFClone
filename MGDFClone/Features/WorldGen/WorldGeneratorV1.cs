using MGDFClone.Features.PerlinNoise;
using MGDFClone.Models;
using Microsoft.Xna.Framework;
namespace MGDFClone.Features.WorldGen; 

public class WorldGeneratorV1 {
    public static readonly int REGION_TILE_COUNT = 16;
    public static readonly int LOCAL_TILE_COUNT = 48;
    private WorldGenerationParameters m_WorlGenerationParameters;
    public WorldGenerationParameters WorldGenerationParameters => m_WorlGenerationParameters;
    //private WorldTemperatureParameters m_WorlTemperatureParameters;
    //private float m_minimumTemperature = -20.0f;
    //private float m_maximumTemperature = 120.0f;
    //private float m_waterCoolingFactor = 10.0f;
    //private float m_waterTemperature = 50.0f;
    //Offset to avoid overly hot beaches
    //private float m_waterElevation = 0.60f;
    //// Maximum elevation in meters
    //private float m_maxElevationInMeters = 7000.0f;
    // Convert lapse rate to Fahrenheit if using Fahrenheit scale: 6.5°C ≈ 11.7°F | 1000 meters (standard value).
    //private float m_lapseRateF = 11.7f;
    //private eSeason m_season = eSeason.Winter;
    public WorldMap1 WorldMap { get; private set; }
    Dictionary<eSeason, float> SeasonalTemperatureOffsets = new Dictionary<eSeason, float>() {
        { eSeason.Winter, -20.0f },
        { eSeason.Spring, 5.0f },  
        { eSeason.Summer, 20.0f }, 
        { eSeason.Autumn, 0.0f }   
    };

    public WorldGeneratorV1(WorldGenerationParameters worldGenerationParameters) {
        m_WorlGenerationParameters = worldGenerationParameters;
        WorldMap = new WorldMap1(m_WorlGenerationParameters.WorldSize);
    }

    public WorldGeneratorV1(eWorldSize worldSize, eSeason season) {
        WorldMap = new WorldMap1(worldSize);
    }

    public void ChangeSeason(eSeason season) {

    }

    public void GenerateElevation() {
        float[] elevationMap = PerlinNoiseV4.GeneratePerlinNoise(WorldMap.Width, WorldMap.Height, m_WorlGenerationParameters.ElevationParameters.PerlinOctaves);
        WorldMap.SetElevation(elevationMap);
        WorldMap.GenerateTemperature(m_WorlGenerationParameters.WorldTemperatureParameters.MinimumTemperature,
                m_WorlGenerationParameters.WorldTemperatureParameters.MaximumTemperature, m_WorlGenerationParameters.ElevationParameters.MaxElevationInMeters,
            m_WorlGenerationParameters.ElevationParameters.WaterElevation, m_WorlGenerationParameters.WorldTemperatureParameters.WaterCoolingFactor);

    }

    /// <summary>
    /// Call this to re-generate maps
    /// </summary>
    public void GenerateWorld() {
        if (WorldMap != null) {
            GenerateElevation();
            ApplyTemperature();
        }
    }

    public void ApplyTemperature() {
        if (WorldMap != null) {
            float[] temperatureMap = new float[WorldMap.Width * WorldMap.Height];
            float seasonalOffset = SeasonalTemperatureOffsets[m_WorlGenerationParameters.WorldTemperatureParameters.Season];
            for (int i = 0; i < WorldMap.Width * WorldMap.Height; i++) {
                int row = i / WorldMap.Width;
                int col = i % WorldMap.Width;
                float elevation = WorldMap.RegionTiles[i].Elevation;
                float latitudeFactor = row / (float)(WorldMap.Height - 1);
                latitudeFactor = latitudeFactor * latitudeFactor;
                latitudeFactor = 1.0f / (1.0f + MathF.Exp(-10.0f * (latitudeFactor - 0.5f)));
                float baseTemperature = m_WorlGenerationParameters.WorldTemperatureParameters.MaximumTemperature - 
                    latitudeFactor * (m_WorlGenerationParameters.WorldTemperatureParameters.MaximumTemperature - m_WorlGenerationParameters.WorldTemperatureParameters.MinimumTemperature);
                // Calculate the temperature drop due to elevation.
                float elevationInMeters = elevation * m_WorlGenerationParameters.ElevationParameters.MaxElevationInMeters;
                // Calculate the cooling effect based on the elevation.
                float elevationCooling = (elevationInMeters / 1000.0f) * m_WorlGenerationParameters.WorldTemperatureParameters.LapseRate;
                //float adjustedTemperature = baseTemperature - (elevation * _waterCoolingFactor);
                float adjustedTemperature = baseTemperature - elevationCooling;
                if (elevation < m_WorlGenerationParameters.ElevationParameters.WaterElevation) {
                    float waterBlendFactor = (elevation / m_WorlGenerationParameters.ElevationParameters.WaterElevation);
                    adjustedTemperature = MathHelper.Lerp(m_WorlGenerationParameters.WorldTemperatureParameters.WaterTemperature, adjustedTemperature, waterBlendFactor);
                }
                adjustedTemperature += seasonalOffset;
                temperatureMap[i] = adjustedTemperature;
            }
            WorldMap.SetTemperature(temperatureMap);
        }
    }

    public void Clear() {
        WorldMap = null;
    }
}

public class WorldMap1 {
    private int m_width;
    private int m_height;    
    private float[] m_temperatureMap;
    private eWorldSize m_worldSize;
    private RegionTile1[] m_regionTiles;
    public int Width => m_width;
    public int Height => m_height;
    public RegionTile1[] RegionTiles => m_regionTiles;
    public WorldMap1(eWorldSize worldSize) {
        m_width = (int)worldSize;
        m_height = (int)worldSize;
        m_worldSize = worldSize;
        m_temperatureMap = new float[m_width * m_height];
        m_regionTiles = new RegionTile1[m_width * m_height];
        for(int i = 0; i < m_regionTiles.Length; i++) {
            m_regionTiles[i] = new();
        }
    }

    public void SetElevation(float[] elevationMap) {
        for (int i = 0; i < m_regionTiles.Length; i++) {
            m_regionTiles[i].Elevation = elevationMap[i];
        }
    }

    public void SetTemperature(float[] temperatureMap) {
        for (int i = 0; i < m_regionTiles.Length; i++) {
            m_regionTiles[i].Temperature = temperatureMap[i];
        }
    }

    public void GenerateTemperature(float minimumTemperature, float maximumTemperature, float maxElevation, 
        float waterElevation, float waterCoolingFactor) {
        float[] temperatureMap = new float[m_width * m_height];

    }
}

public class WorldMapTile1 {

}

public class LocalTile1 {

}

public class RegionTile1 {
    public float Elevation;
    public float Temperature;
    public float Moisture;
}
public enum eSeason {
    Winter,
    Spring,
    Summer,
    Autumn
}


public enum eWorldSize {
    Pocket = 17,
    Smaller = 33,
    Small = 65,
    Medium = 129,
    Large = 257
}
