namespace MGDFClone.Features {
    public enum eTileMapType {
        None = 0,
        DeepWater,
        Water,
        Sand,
        Grass,
        SmallTree,
        Forest,
        Hill,
        Mountain,
        Snow
    }


    

    public static class MapGeneratorV1 {
        public static eTileMapType[][] GenerateMap(int width, int height, int elevationOctaves, int vegativeOctave) {
            float[][] elevationNoise = PerlinNoiseV1.GeneratePerlinNoise(width, height, elevationOctaves);
            //MapGeneratorV2.ApplyMapFalloff(elevationNoise, 5);
            //GenerateRivers(elevationNoise, 5000);
            float[][] vegatativeNoise = PerlinNoiseV1.GeneratePerlinNoise(width, height, vegativeOctave);
            eTileMapType[][] tileMap = new eTileMapType[width][];
            for (int i = 0; i < width; i++) {
                tileMap[i] = new eTileMapType[height];
                for (int j = 0; j < height; j++) {
                    tileMap[i][j] = DetermineBaseTerrain(elevationNoise[i][j]);
                }
            }
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    ApplyVegetationLayer(tileMap, elevationNoise[i][j], vegatativeNoise[i][j], i, j);
                }
            }
            return tileMap;
        }

        public static void GenerateRivers(float[][] heightMap, int riverCount) {
            int width = heightMap.Length;
            int height = heightMap[0].Length;
            bool[][] riverMap = new bool[width][];
            for(int i = 0; i < width; i++) {
                riverMap[i] = new bool[height];
            }
            List<(int x, int y)> heightPoints = FindHighElevationPoints(heightMap, 0.1f);
            Random rand = new Random();
            for (int i = 0; i < riverCount; i++) {
                var startingPoint = heightPoints[rand.Next(heightPoints.Count)];
                TraceRiver(heightMap, riverMap, startingPoint.x, startingPoint.y);
            }
        }

        private static void TraceRiver(float[][] heightMap, bool[][] riverMap, int x, int y) {
            int width = heightMap.Length;
            int height = heightMap[0].Length;

            while (true) {
                // Mark current tile as a river
                riverMap[x][y] = true;
                heightMap[x][y] = 0.2f;

                // Find the neighboring tile with the lowest elevation
                (int nx, int ny) = GetLowestNeighbor(heightMap, x, y);

                // If no lower neighbor or reached edge of the map, stop
                if (nx == -1 || ny == -1 || heightMap[nx][ny] >= heightMap[x][y]) {
                    break;
                }

                // Move to the next tile
                x = nx;
                y = ny;
                
            }
        }

        private static (int, int) GetLowestNeighbor(float[][] heightMap, int x, int y) {
            int width = heightMap.Length;
            int height = heightMap[0].Length;
            int[] dx = { -1, 1, 0, 0, -1, 1, -1, 1 };
            int[] dy = { 0, 0, -1, 1, -1, -1, 1, 1 };

            float minElevation = heightMap[x][y];
            (int, int) lowestNeighbor = (-1, -1);

            for (int i = 0; i < 8; i++) {
                int nx = x + dx[i];
                int ny = y + dy[i];
                if (nx >= 0 && nx < width && ny >= 0 && ny < height && heightMap[nx][ny] < minElevation) {
                    minElevation = heightMap[nx][ny];
                    lowestNeighbor = (nx, ny);
                }
            }

            return lowestNeighbor;
        }

        private static List<(int x, int y)> FindHighElevationPoints(float[][] heightMap, float threshold) {
            int width = heightMap.Length;
            int height = heightMap[0].Length;
            List<(int, int)> points = new List<(int, int)>();
            for (int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    if(heightMap[x][y] > threshold) {
                        points.Add((x,y));
                    }
                }
            }
            return points;
        }

        private static void ApplyVegetationLayer(eTileMapType[][] tileMap, float elevationValue, float vegetationValue, int x, int y) {
            if (tileMap[x][y] == eTileMapType.Grass) {
                if (vegetationValue > 0.50f) {
                    // High vegetation noise = Forest
                    tileMap[x][y] = eTileMapType.Forest;
                }
            }
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
