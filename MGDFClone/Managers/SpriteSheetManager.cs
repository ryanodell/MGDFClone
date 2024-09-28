using Microsoft.Xna.Framework;

namespace MGDFClone.Managers {
    public static class SpriteSheetManager {
        public static Rectangle GetSourceRectForSprite(eSprite sprite) {
            int index = (int)sprite;
            var columns = Globals.TEXTURE.Width / Globals.TILE_SIZE;
            int col = index / columns;
            int row = index % columns;
            return new Rectangle(col * Globals.TILE_SIZE, row * Globals.TILE_SIZE, Globals.TILE_SIZE, Globals.TILE_SIZE);
        }
    }
}
