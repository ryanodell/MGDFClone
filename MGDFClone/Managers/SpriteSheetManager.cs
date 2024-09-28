using Microsoft.Xna.Framework;

namespace MGDFClone.Managers {
    public static class SpriteSheetManager {
        public static Rectangle GetSourceRectForSprite(eSprite sprite) {
            int index = (int)sprite;
            var columns = Globals.TEXTURE.Width / Globals.TILE_SIZE;
            int row = index / columns;
            int col = index % columns;
            return new Rectangle(col * Globals.TILE_SIZE, row * Globals.TILE_SIZE, Globals.TILE_SIZE, Globals.TILE_SIZE);
        }
    }
}
