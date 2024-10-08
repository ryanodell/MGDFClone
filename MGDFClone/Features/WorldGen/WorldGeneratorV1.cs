using MGDFClone.Features.PerlinNoise;
namespace MGDFClone.Features.WorldGen; 
public class WorldGeneratorV1 {
    public static readonly int REGION_TILE_COUNT = 16;
    public static readonly int LOCAL_TILE_COUNT = 48;
    private float m_minimumTemperature = -20.0f;
    private float m_maximumTemperature = 120.0f;
    private float m_waterCoolingFactor = 10.0f;
    private float m_waterTemperature = 50.0f;
    private float[] _temperatureMap;
    //Offset to avoid overly hot beaches
    private float m_waterElevation = 0.60f;
    // Maximum elevation in meters
    float m_maxElevationInMeters = 7000.0f;
    private eSeason m_season = eSeason.Winter;
    public WorldMap1? WorldMap { get; private set; }
    Dictionary<eSeason, float> SeasonalTemperatureOffsets = new Dictionary<eSeason, float>() {
        { eSeason.Winter, -20.0f },
        { eSeason.Spring, 5.0f },  
        { eSeason.Summer, 20.0f }, 
        { eSeason.Autumn, 0.0f }   
    };

    public WorldGeneratorV1(eWorldSize worldSize, eSeason season) {
        WorldMap = new WorldMap1(worldSize);
        m_season = season;
    }

    public void ChangeSeason(eSeason season) {

    }

    public void GenerateWorld() {
        if (WorldMap != null) {
            float[] elevationMap = PerlinNoiseV4.GeneratePerlinNoise(WorldMap.Width, WorldMap.Height, 4);
            WorldMap.SetElevation(elevationMap);
            WorldMap.GenerateTemperature(m_minimumTemperature, m_maximumTemperature, m_maxElevationInMeters,
                m_waterElevation, m_waterCoolingFactor);
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
