using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Snaker.Content.Enemies;

public partial class DevilishSnake : ModNPC
{
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex = _bodyTexture.Value;
        Vector2 direction = new Vector2(0, 106).RotatedBy(0.2f) * 0.12f;
        Vector2 basePos = NPC.Center - screenPos + direction;
        float rotation = NPC.rotation;
        float scale = 0.85f;

        for (int i = 0; i < 20; ++i)
        {
            rotation = MathHelper.Lerp(rotation, -MathHelper.PiOver2, 0.1f);
            scale = MathHelper.Lerp(scale, 1f, 0.2f);

            var realDirection = direction.RotatedBy(rotation);
            var pos = basePos + (realDirection * i * 3f);
            var col = Lighting.GetColor((pos + Main.screenPosition).ToTileCoordinates());
            spriteBatch.Draw(tex, pos, null, col * (i > 6 ? 1 - ((i - 6) / 14f) : 1f), rotation, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
        return true;
    }
}
