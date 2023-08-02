using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Snaker.Content;

internal static class ExplosionHelper
{
    public static void Fire(Vector2 position, int repeats, float scale, (float min, float max) magnitudeRange, float rotationRange = MathHelper.TwoPi)
    {
        for (int i = 0; i < repeats; ++i)
        {
            Vector2 velocity = new Vector2(Main.rand.NextFloat(magnitudeRange.min, magnitudeRange.max), 0).RotatedByRandom(rotationRange);
            Dust.NewDustDirect(position, 0, 0, DustID.Torch, 0, 0, Scale: scale).velocity = velocity;
        }
    }

    public static void Smoke(IEntitySource source, Vector2 position, int repeats, (float min, float max) magnitudeRange, float rotationRange = MathHelper.TwoPi)
    {
        for (int i = 0; i < repeats; ++i)
        {
            Vector2 velocity = new Vector2(Main.rand.NextFloat(magnitudeRange.min, magnitudeRange.max), 0).RotatedByRandom(rotationRange);
            Gore.NewGore(source, position, velocity, GoreID.Smoke1 + Main.rand.Next(3));
        }
    }
}
