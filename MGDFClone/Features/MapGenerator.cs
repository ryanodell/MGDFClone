namespace MGDFClone.Features {
    public enum eElevationTileType {
        DeepWater,
        Water,
        Sand,
        Grass,
        Hill,
        Mountain,
        Snow

    }
    public static class MapGenerator {
        public static eElevationTileType[][] GenerateElevation(int width, int height, int elevationOctaves) {
            float[][] elevationNoise = PerlinNoiseV2.GeneratePerlinNoise(width, height, elevationOctaves);
            eElevationTileType[][] tileMap = new eElevationTileType[width][];
            for (int i = 0; i < width; i++) {
                tileMap[i] = new eElevationTileType[height];
                for (int j = 0; j < height; j++) {
                    // Determine base terrain based on elevation
                    tileMap[i][j] = DetermineBaseTerrain(elevationNoise[i][j]);
                }
            }
            return tileMap;
        }

        private static eElevationTileType DetermineBaseTerrain(float elevationValue) {
            if (elevationValue < 0.20f)
                return eElevationTileType.DeepWater; // Low elevation = Water
            if (elevationValue < 0.30f)
                return eElevationTileType.Water; // Low elevation = Water
            else if (elevationValue < 0.35f)
                return eElevationTileType.Sand; // Slightly higher = Sand (beach)


            else if (elevationValue < 0.50f)
                return eElevationTileType.Grass; // Middle = Grasslands
            else if (elevationValue < 0.70f)
                return eElevationTileType.Hill; // Middle = Grasslands
            else if (elevationValue < 0.80f)
                return eElevationTileType.Mountain; // Higher = Mountain
            else
                return eElevationTileType.Snow; // Highest = Snow
        }
    }
}
