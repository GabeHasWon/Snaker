using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
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
            var col = GetAlpha(Lighting.GetColor((pos + Main.screenPosition).ToTileCoordinates())).Value * (i > 6 ? 1 - ((i - 6) / 14f) : 1f);
            Main.EntitySpriteDraw(tex, pos, null, col, rotation, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        var headLightCol = Lighting.GetColor(NPC.Center.ToTileCoordinates());
        var headCol = GetAlpha(headLightCol).GetValueOrDefault(headLightCol);
        var npcTex = TextureAssets.Npc[Type].Value;
        Main.EntitySpriteDraw(npcTex, NPC.Center - Main.screenPosition, null, headCol, NPC.rotation, npcTex.Size() / 2f, scale, SpriteEffects.None, 0);
        return false;
    }

    public override Color? GetAlpha(Color drawColor)
    {
        if (State == SnakeState.Survival)
        {
            if (Timer < 60)
                return drawColor * Math.Max(1 - (Timer / 60f), 0.2f);
            else if (Timer >= 60 && Timer < SurvivalTime - 60)
                return drawColor * ((MathF.Sin(Timer * 0.03f) * 0.1f) + 0.2f);
            else
                return drawColor * Math.Min(Math.Max((Timer - (SurvivalTime - 60)) / 60f, 0.2f), 1f);
        }
        return drawColor;
    }
}
