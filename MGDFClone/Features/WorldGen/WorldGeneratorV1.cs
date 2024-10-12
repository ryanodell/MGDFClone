﻿using MGDFClone.Features.MapGen;
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
    private float m_waterToSandOffset = 0.05f;
    private float m_sandToGrassOffet = 0.30f;
    private float m_grassToHillOffset = 0.15f;
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

    private void _initializeElevation() {
        float[] elevationMap = PerlinNoiseV4.GeneratePerlinNoise(WorldMap.Width, WorldMap.Height, m_WorlGenerationParameters.ElevationParameters.PerlinOctaves);
        WorldMap.SetElevation(elevationMap);

    }

    /// <summary>
    /// Call this to re-generate maps
    /// </summary>
    public void GenerateWorld() {
        if (WorldMap != null) {
            WorldMap = new WorldMap1(m_WorlGenerationParameters.WorldSize);
            _initializeElevation();
            ApplyTemperature();
            _initializeHumidity();
            ApplyHumidity();
        }
    }

    public eTileMapType DetermineTerrainTile(float value) {
        if (value < m_WorlGenerationParameters.ElevationParameters.WaterElevation - 0.10f)
            return eTileMapType.DeepWater; // Low elevation = Water
        if (value < m_WorlGenerationParameters.ElevationParameters.WaterElevation)
            return eTileMapType.Water; // Low elevation = Water
        else if (value < m_WorlGenerationParameters.ElevationParameters.WaterElevation + m_waterToSandOffset)
            return eTileMapType.Sand; // Slightly higher = Sand (beach)
        //else if (value < 0.70f)
        else if (value < m_WorlGenerationParameters.ElevationParameters.WaterElevation + m_waterToSandOffset + m_sandToGrassOffet)
            return eTileMapType.Grass; // Middle = Grasslands
        //else if (value < 0.85f)
        else if (value < m_WorlGenerationParameters.ElevationParameters.WaterElevation + m_waterToSandOffset + m_sandToGrassOffet + m_grassToHillOffset)
            return eTileMapType.Hill; // Middle = Grasslands
        else if (value < 0.90f)
            return eTileMapType.Mountain; // Higher = Mountain
        else
            return eTileMapType.Snow; // Highest = Snow
    }

    public eSprite GetSpriteForTerrainHeight(float value) {
        eSprite sprite = eSprite.None;
        return sprite;
    }

    [Obsolete]
    public eTileMapType DetermineBaseTerrain(float value) {

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
                if (elevation < m_WorlGenerationParameters.ElevationParameters.WaterElevation + m_waterToSandOffset + m_sandToGrassOffet) {
                    float waterBlendFactor = (elevation / m_WorlGenerationParameters.ElevationParameters.WaterElevation);
                    adjustedTemperature = MathHelper.Lerp(m_WorlGenerationParameters.WorldTemperatureParameters.WaterTemperature, adjustedTemperature, waterBlendFactor);
                }
                adjustedTemperature += seasonalOffset;
                temperatureMap[i] = adjustedTemperature;
            }
            WorldMap.SetTemperature(temperatureMap);
        }
    }

    private void _initializeHumidity() {
        ClimateParameters climateParameters = m_WorlGenerationParameters.ClimateParameters;
        float[] initHumidty = PerlinNoiseV4.GeneratePerlinNoise(WorldMap.Width, WorldMap.Height, climateParameters.PerlinOctaves);
        WorldMap.SetInitialHumidty(initHumidty);
    }

    public void ApplyHumidity() {
        ClimateParameters climateParameters = m_WorlGenerationParameters.ClimateParameters;
        WorldTemperatureParameters worldTemperatureParameters = m_WorlGenerationParameters.WorldTemperatureParameters;
        ElevationParameters elevationParameters = m_WorlGenerationParameters.ElevationParameters;
        float mountainThreshold = m_WorlGenerationParameters.ElevationParameters.WaterElevation + m_waterToSandOffset + m_sandToGrassOffet + m_grassToHillOffset;
        float[] finalHumidity = new float[WorldMap.Width * WorldMap.Height];
        //Left to right pass
        for (int y = 0; y < WorldMap.Height; y++) {
            //Start with the west tile - Weather moves West to East
            int leftmostTileIndex = y * WorldMap.Width;
            float moisture = WorldMap.RegionTiles[leftmostTileIndex].InitHumidy;
            for (int x = 0; x < WorldMap.Width; x++) {
                int index = y * WorldMap.Width + x;
                float temperature = WorldMap.RegionTiles[index].Temperature;
                float moistureCapacity = (float)Math.Exp(temperature / 200.5f) - 1.0f + climateParameters.BaseMoisture;
                float elevation = WorldMap.RegionTiles[index].Elevation;
                //Check if it's water to add extra moisture - simulate evaporation
                if(elevation <= elevationParameters.WaterElevation) {
                    moisture += climateParameters.WaterFactor;
                }
                //Handle mountains
                if (elevation > mountainThreshold) {
                    finalHumidity[index] = Math.Min(moisture * climateParameters.PercipitationFactor, moistureCapacity);
                    moisture *= climateParameters.RainShadowEffect;
                } else {
                    moisture = Math.Min(moisture, moistureCapacity);
                    finalHumidity[index] = moisture;
                }
                moisture *= climateParameters.EastwardDissipation;
            }
        }

        // Optional: Normalize or scale the humidity values between 0 and 100
        for (int i = 0; i < finalHumidity.Length; i++) {
            finalHumidity[i] = Math.Clamp(finalHumidity[i], climateParameters.MinimumHumidity, climateParameters.MaximumHunidty);
        }
        WorldMap.SetFinalHumidity(finalHumidity);
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

    public void SetInitialHumidty(float[] initHumidity) {
        for(int i = 0; i < m_regionTiles.Length; i++) {
            m_regionTiles[i].InitHumidy = initHumidity[i];
        }
    }

    public void SetFinalHumidity(float[] finalHumidty) {
        for(int i = 0; i < m_regionTiles.Length; i++) {
            m_regionTiles[i].Humidity = finalHumidty[i];
        }
    }
}

public class WorldMapTile1 {

}

public class LocalTile1 {

}

public class RegionTile1 {
    public float Elevation;
    public float Temperature;
    public float Humidity;
    public float InitHumidy;
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
