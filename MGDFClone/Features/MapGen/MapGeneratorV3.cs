/*
 * Notes: A world consists of world tiles. Each world tile contains 16x16 region tiles. Each region tile contains 48x48
 */
namespace MGDFClone.Features.MapGen {
    public class MapGeneratorV3
    {
        public static readonly int REGION_TILE_COUNT = 16;
        public static readonly int LOCAL_TILE_COUNT = 48;
        private WorldMap m_worldMap;
        public WorldMap WorldMap {  get { return m_worldMap; } }

        public MapGeneratorV3(eWorldSize worldSize) {
            m_worldMap = new WorldMap(worldSize);
        }

    }

    public class WorldMap {
        private int m_width;
        private int m_height;
        private float[] m_elevationMap;
        private float[] m_temperatureMap;
        private eWorldSize m_worldSize;
        public WorldMap(eWorldSize worldSize) {
            m_width = (int)worldSize;
            m_height = (int)worldSize;
            m_worldSize = worldSize;
            m_elevationMap = new float[m_width * m_height];
            m_temperatureMap = new float[m_width * m_height];
        }
    }

    public class WorldMapTile {

    }

    public class RegionTile {

    }

    public class LocalTile {

    }

    public class RegionTileV1 {
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

}
