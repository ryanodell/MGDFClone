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
    public static float[][] GenerateElevation(int width, int height) {
        //Width in this contex is how many overworld tiles will we have?
        //Each overworld tile contains 16x16 tiles. We won't go into any more "detail" as in layers at this point
        int totalWidth = width * _regionTilesize;
        int totalHeight = height * _regionTilesize;
        return PerlinNoiseV2.GeneratePerlinNoise(totalWidth, totalHeight, _perlinOctaves);
    }

}
