using MGDFClone.Features.MapGen;
using MGDFClone.Features.PerlinNoise;
using MGDFClone.Models;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;
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
            _initializeVegitation();
        }
    }

    public eTileMapType DetermineTerrainTile(float value) {
        if (value < m_WorlGenerationParameters.ElevationParameters.WaterElevation - 0.10f)
            return eTileMapType.DeepWater; // Low elevation = Water
        if (value < m_WorlGenerationParameters.ElevationParameters.WaterElevation)
            return eTileMapType.Water; // Low elevation = Water
        else if (value < m_WorlGenerationParameters.ElevationParameters.WaterElevation + m_WorlGenerationParameters.ElevationParameters.WaterToSandOffset)
            return eTileMapType.Sand; // Slightly higher = Sand (beach)
        //else if (value < 0.70f)
        else if (value < m_WorlGenerationParameters.ElevationParameters.WaterElevation + m_WorlGenerationParameters.ElevationParameters.WaterToSandOffset
                + m_WorlGenerationParameters.ElevationParameters.SandToGrassOffet)
            return eTileMapType.Grass; // Middle = Grasslands
        //else if (value < 0.85f)
        else if (value < m_WorlGenerationParameters.ElevationParameters.WaterElevation + m_WorlGenerationParameters.ElevationParameters.WaterToSandOffset
                + m_WorlGenerationParameters.ElevationParameters.SandToGrassOffet + m_WorlGenerationParameters.ElevationParameters.GrassToHillOffset)
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

    /// <summary>
    /// This does not accept polar region parameter - treats polar region as "SOUTH" by default
    /// </summary>
    private void _applyTemperatureV1() {
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
            if (elevation < m_WorlGenerationParameters.ElevationParameters.WaterElevation + m_WorlGenerationParameters.ElevationParameters.WaterToSandOffset
                    + m_WorlGenerationParameters.ElevationParameters.SandToGrassOffet) {
                float waterBlendFactor = (elevation / m_WorlGenerationParameters.ElevationParameters.WaterElevation);
                adjustedTemperature = MathHelper.Lerp(m_WorlGenerationParameters.WorldTemperatureParameters.WaterTemperature, adjustedTemperature, waterBlendFactor);
            }
            adjustedTemperature += seasonalOffset;
            temperatureMap[i] = adjustedTemperature;
        }
        //Log.Logger.Information($"Min Temp: {temperatureMap.Min()}");
        WorldMap.SetTemperature(temperatureMap);
    }
    private void _applyTemperatureV2() {
        WorldTemperatureParameters worldTemperatureParameters = m_WorlGenerationParameters.WorldTemperatureParameters;
        float[] temperatureMap = new float[WorldMap.Width * WorldMap.Height];
        float seasonalOffset = SeasonalTemperatureOffsets[m_WorlGenerationParameters.WorldTemperatureParameters.Season];

        for (int i = 0; i < WorldMap.Width * WorldMap.Height; i++) {
            int row = i / WorldMap.Width;
            int col = i % WorldMap.Height;
            float normalizedRow = (float)row / (WorldMap.Height - 1);

            float distanceFromPole = 0.0f;
            switch (worldTemperatureParameters.PolarRegion) {
                case ePolarRegion.NorthPole:
                    distanceFromPole = normalizedRow; // Closer to North Pole
                    break;
                case ePolarRegion.SouthPole:
                    distanceFromPole = 1.0f - normalizedRow; // Closer to South Pole
                    break;
                case ePolarRegion.NorthAndSouthPole:
                    distanceFromPole = Math.Min(normalizedRow, 1.0f - normalizedRow); // Closer to either pole
                    break;
                case ePolarRegion.NoPole:
                    distanceFromPole = 0.5f; // Uniform temperature, no pole influence
                    break;
            }

            // Cosine gradient to get values between -1 (cold) to 1 (hot)
            float temperatureFactor = (float)Math.Cos(distanceFromPole * Math.PI);

            // Normalize to a range between 0 (cold) and 1 (hot)
            float baseTemperature = (temperatureFactor + 1f) / 2f;

            // Add seasonal offset, then scale and clamp the result to a realistic temperature range
            float finalTemperature = baseTemperature + seasonalOffset;

            // Example: Clamp to a range like -30°C to +50°C
            finalTemperature = Math.Clamp(finalTemperature * 80f - 30f, -30f, 50f);

            temperatureMap[i] = finalTemperature;
        }

        WorldMap.SetTemperature(temperatureMap);
    }

    //private void _applyTemperatureV2() {
    //    WorldTemperatureParameters worldTemperatureParameters = m_WorlGenerationParameters.WorldTemperatureParameters;
    //    float[] temperatureMap = new float[WorldMap.Width * WorldMap.Height];
    //    float seasonalOffset = SeasonalTemperatureOffsets[m_WorlGenerationParameters.WorldTemperatureParameters.Season];
    //    for (int i = 0; i < WorldMap.Width * WorldMap.Height; i++) {
    //        int row = i / WorldMap.Width;
    //        int col = i % WorldMap.Height;
    //        float normalizedRow = (float)row / (WorldMap.Height - 1);
    //        float distanceFromPole = 0.0f;
    //        switch(worldTemperatureParameters.PolarRegion) {
    //            case ePolarRegion.NorthPole:
    //                distanceFromPole = normalizedRow;
    //                break;
    //            case ePolarRegion.SouthPole:
    //                distanceFromPole = 1.0f - normalizedRow;
    //                break;
    //            case ePolarRegion.NorthAndSouthPole:
    //                distanceFromPole = Math.Min(normalizedRow, 1.0f - normalizedRow);
    //                break;
    //            case ePolarRegion.NoPole:
    //                distanceFromPole = 0.5f;
    //                break;
    //        }
    //        float temperatureFactor = (float)Math.Cos(distanceFromPole * Math.PI); // Values from -1 (pole) to 1 (equator)
    //        // Normalize to 0 (cold) to 1 (hot)
    //        float baseTemperature = (temperatureFactor + 1f) / 2f;
    //        temperatureMap[i] = baseTemperature; //+ seasonalOffset;
    //        //Apply elevation but leave this for now to see how things "Arc"
    //    }
    //    WorldMap.SetTemperature(temperatureMap);
    //}

    public void ApplyTemperature() {
        if (WorldMap != null) {
            _applyTemperatureV2();
        }
    }

    private void _initializeVegitation() {
        float[] initVegitation = PerlinNoiseV4.GeneratePerlinNoise(WorldMap.Width, WorldMap.Height, 2);
        WorldMap.SetVegitation(initVegitation);
    }

    public void ApplyVegitation() {
        for (int i = 0; i < WorldMap.Width * WorldMap.Height; i++) {
            var regionTile = WorldMap.RegionTiles[i];
            var temperature = regionTile.Temperature;
            var humidity = regionTile.Humidity;
            var vegitation = regionTile.Vegitation;
            eBiome biome = BiomeManagerV1.GetBiome(temperature, humidity);
            WorldMap.RegionTiles[i].Biome = biome;
        }
    }

    private void _initializeHumidity() {
        ClimateParameters climateParameters = m_WorlGenerationParameters.ClimateParameters;
        float[] initHumidty = PerlinNoiseV4.GeneratePerlinNoise(WorldMap.Width, WorldMap.Height, climateParameters.PerlinOctaves);
        WorldMap.SetInitialHumidty(initHumidty);
    }

    public void ApplyHumidity() {
        ClimateParameters climateParameters = m_WorlGenerationParameters.ClimateParameters;
        ElevationParameters elevationParameters = m_WorlGenerationParameters.ElevationParameters;
        float mountainThreshold = m_WorlGenerationParameters.ElevationParameters.WaterElevation + m_WorlGenerationParameters.ElevationParameters.WaterToSandOffset
                + m_WorlGenerationParameters.ElevationParameters.SandToGrassOffet + m_WorlGenerationParameters.ElevationParameters.GrassToHillOffset;
        float[] finalHumidity = new float[WorldMap.Width * WorldMap.Height];
        //Left to right pass - the West to East progression of weather
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
                if (elevation <= elevationParameters.WaterElevation) {
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
        //Not doing right to left pass for now..

        //Top to bottom pass - push moisture to the tile below
        for (int i = 0; i < WorldMap.Width * WorldMap.Height; i++) {
            int row = i / WorldMap.Width;
            int col = i % WorldMap.Height;
            int currentIndex = row * WorldMap.Width + col;
            float currentHumidity = finalHumidity[currentIndex];
            if (row > 0) {
                int aboveIndex = (row - 1) * WorldMap.Width + col;
                float northHumiditySpread = currentHumidity * climateParameters.NortwardDissipation;
                finalHumidity[aboveIndex] += northHumiditySpread;  // Add to the tile above
                //finalHumidity[currentIndex] -= northHumiditySpread; // Reduce from current tile
            }

            // Spread moisture to the tile below (South)
            if (row < WorldMap.Height - 1) {
                int belowIndex = (row + 1) * WorldMap.Width + col;
                float southHumiditySpread = currentHumidity * climateParameters.SouthwardDissipation;
                finalHumidity[belowIndex] += southHumiditySpread;  // Add to the tile below
                //finalHumidity[currentIndex] -= southHumiditySpread; // Reduce from current tile
            }
            //Don't need this, just for reference
            //if(col > 0) {
            //    int leftIndex = row * WorldMap.Width + (col - 1);
            //}
            //Don't need this, just for reference
            //if(col < WorldMap.Height - 1) {
            //    int rightIndex = row * WorldMap.Width + (col + 1);
            //}
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
    private float[] m_vegationMap;
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
        m_vegationMap = new float[m_width * m_height];
        m_regionTiles = new RegionTile1[m_width * m_height];
        for (int i = 0; i < m_regionTiles.Length; i++) {
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
        for (int i = 0; i < m_regionTiles.Length; i++) {
            m_regionTiles[i].InitHumidy = initHumidity[i];
        }
    }

    public void SetVegitation(float[] vegitationMap) {
        for (int i = 0; i < m_regionTiles.Length; i++) {
            m_regionTiles[i].Vegitation = vegitationMap[i];
        }
    }

    public void SetFinalHumidity(float[] finalHumidty) {
        for (int i = 0; i < m_regionTiles.Length; i++) {
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
    public float Vegitation;
    public eBiome Biome;
}

public enum ePolarRegion {
    SouthPole,
    NorthPole,
    NorthAndSouthPole,
    NoPole
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
