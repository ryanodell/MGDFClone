using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGDFClone.Features.MapGen
{
    public class MapGeneratorV3
    {
        private float[] m_elevationMap;
        private float[] m_temperatureMap;
        private int m_width;
        private int m_height;


        public MapGeneratorV3(int  width, int height) {
            m_width = width;
            m_height = height;
            m_elevationMap = new float[width * height];
            m_temperatureMap = new float[width * height];
        }

    }

    public struct TileMapDetailsV1 {
        public float Elevation;
        public float Temperature;
    }

}
