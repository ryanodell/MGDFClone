using MGDFClone.Features.PerlinNoise;
namespace MGDFClone.Features.WorldGen; 
public class WorldGeneratorV1 {
    public static readonly int REGION_TILE_COUNT = 16;
    public static readonly int LOCAL_TILE_COUNT = 48;
    private WorldMap1 m_worldMap;
    public WorldMap1 WorldMap { get { return m_worldMap; } }

    public WorldGeneratorV1(eWorldSize worldSize) {
        m_worldMap = new WorldMap1(worldSize);
    }

    public void GenerateWorld() {
        m_worldMap.Generate();
    }
}

public class WorldMap1 {
    private int m_width;
    private int m_height;
    private float[] m_temperatureMap;
    private eWorldSize m_worldSize;
    private RegionTile1[] m_regionTiles;
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

    public void Generate() {
        float[] elevationMap = PerlinNoiseV4.GeneratePerlinNoise(m_width, m_height, 4);
        for (int i = 0; i < m_regionTiles.Length; i++) {
            m_regionTiles[i].Elevation = elevationMap[i];
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
}

public enum eWorldSize {
    Pocket = 17,
    Smaller = 33,
    Small = 65,
    Medium = 129,
    Large = 257
}
