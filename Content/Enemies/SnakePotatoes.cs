using Microsoft.Xna.Framework;
using Snaker.Content.World;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Content.Enemies;

internal class SnakePotato : ModProjectile
{
    public const float Gravity = 0.02f;

    public override void SetDefaults()
    {
        Projectile.width = 114;
        Projectile.height = 114;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60000;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
    }

    public override void AI()
    {
        Projectile.velocity.Y += Gravity;
        Projectile.rotation += Projectile.velocity.X * 0.01f;

        if ((Projectile.timeLeft + 1) % 10 == 0)
        {
            var vel = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2();
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, vel * Main.rand.NextFloat(4, 12f), ModContent.ProjectileType<SnakeFireballTrail>(), Projectile.damage / 3, 2f, Main.myPlayer);
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            Projectile.velocity.X = -oldVelocity.X * 0.8f;

        if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            return true;
        return false;
    }

    public override void Kill(int timeLeft)
    {
        if (Main.netMode != NetmodeID.Server)
        {
            for (int i = 0; i < 3; ++i)
                Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, Mod.Find<ModGore>("BigPotato" + i).Type, 1f);
        }
    }
}

internal class SnakeMeteorPotato : ModProjectile
{
    public const float Gravity = 0.02f;

    public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

    public override void SetDefaults()
    {
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 1200;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
    }

    public override void AI()
    {
        Projectile.velocity.Y += Gravity;
        Projectile.rotation += Projectile.velocity.X * 0.01f;
        Projectile.tileCollide = ((SubworldLibrary.SubworldSystem.Current as SnakerSubworld).OpenBottom - 6) * 16 <= Projectile.Center.Y;
    }

    public override void Kill(int timeLeft)
    {
    }
}