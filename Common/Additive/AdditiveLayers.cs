using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Snaker.Common.Additive;

internal class AdditiveLayers : ILoadable
{
    public void Load(Mod mod)
    {
        On_Main.DrawProjectiles += Main_DrawProjectiles;
    }

    public void Unload() { }

    private void Main_DrawProjectiles(On_Main.orig_DrawProjectiles orig, Main self)
    {
        orig(self);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        DrawAdditiveProjectiles();
        Main.spriteBatch.End();
    }

    private static void DrawAdditiveProjectiles()
    {
        for (int i = 0; i < Main.maxProjectiles; ++i)
        {
            Projectile p = Main.projectile[i];
            if (p.active && p.ModProjectile is IDrawAdditive additive)
                additive.DrawAdditive();
        }
    }
}

interface IDrawAdditive
{
    void DrawAdditive();
}