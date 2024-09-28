namespace MGDFClone.Features {
    public enum eTileMapType {
        DeepWater,
        Water,
        Sand,
        Grass,
        Hill,
        Mountain,
        Snow

    }
    public static class MapGenerator {
        public static eTileMapType[][] GenerateMap(int width, int height, int elevationOctaves) {
            float[][] elevationNoise = PerlinNoiseV2.GeneratePerlinNoise(width, height, elevationOctaves);
            eTileMapType[][] tileMap = new eTileMapType[width][];
            for (int i = 0; i < width; i++) {
                tileMap[i] = new eTileMapType[height];
                for (int j = 0; j < height; j++) {
                    // Determine base terrain based on elevation
                    tileMap[i][j] = DetermineBaseTerrain(elevationNoise[i][j]);
                }
            }
            return tileMap;
        }

        private static eTileMapType DetermineBaseTerrain(float elevationValue) {
            if (elevationValue < 0.20f)
                return eTileMapType.DeepWater; // Low elevation = Water
            if (elevationValue < 0.30f)
                return eTileMapType.Water; // Low elevation = Water
            else if (elevationValue < 0.35f)
                return eTileMapType.Sand; // Slightly higher = Sand (beach)


            else if (elevationValue < 0.50f)
                return eTileMapType.Grass; // Middle = Grasslands
            else if (elevationValue < 0.70f)
                return eTileMapType.Hill; // Middle = Grasslands
            else if (elevationValue < 0.80f)
                return eTileMapType.Mountain; // Higher = Mountain
            else
                return eTileMapType.Snow; // Highest = Snow
        }
    }
}
