/*
     * Some notes about how to approach this:
     * Create Local regions of 48x48 tiles These are the lowest possible level detail
     * Each "Sub Region" will have 16x16 locals - These house the 48x48 tiles
     * Each "Region" will contain 16x16 sub regions - Think of these as the "Overworld" tiles that represent the 16x16 subregions
     * When we generate the full world, we generate these on a "high level"
*/
namespace MGDFClone.Features;
public static class MapGeneratorV2 {
    private static readonly int _regionTilesize = 16;
    private static readonly int _perlinOctaves = 4;    

    public static float[][] GenerateIsland(int width, int height) {
        float[][] returnValue;
        //Width in this contex is how many overworld tiles will we have?
        //Each overworld tile contains 16x16 tiles. We won't go into any more "detail" as in layers at this point
        int totalWidth = width * _regionTilesize;
        int totalHeight = height * _regionTilesize;
        returnValue = PerlinNoiseV1.GeneratePerlinNoise(totalWidth, totalHeight, _perlinOctaves);
        ApplyMapFalloff(returnValue, 5);
        return returnValue;
    }

    public static void ApplyMapFalloff(float[][] elevationMap, float islandFactor) {
        int width = elevationMap.Length;
        int height = elevationMap[0].Length;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float distance = _distanceFromCenter(x, y, width, height);
                float falloffValue = _falloff(distance, islandFactor);
                elevationMap[x][y] *= falloffValue; // Modify the existing noise value
            }
        }
    }

    private static float _distanceFromCenter(int x, int y, int width, int height) {
        float dx = (float)(x - width / 2) / (width / 2);
        float dy = (float)(y - height / 2) / (height / 2);
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    // Falloff function: Low values near the edges, high values near the center
    private static float _falloff(float distance, float islandFactor) {
        return 1 - (float)Math.Pow(distance, islandFactor);
    }
}
