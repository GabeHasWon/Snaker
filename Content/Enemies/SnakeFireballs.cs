using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Content.Enemies;

internal class SnakeFireball : ModProjectile
{
    private ref float Timer => ref Projectile.ai[0];

    public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 200;
        Projectile.aiStyle = 0;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 160000;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.alpha = 80;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, TorchID.Torch);

        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

        if (Projectile.frameCounter++ > 8)
        {
            Projectile.frameCounter = 0;

            if (++Projectile.frame >= Main.projFrames[Type])
                Projectile.frame = 0;
        }

        Timer++;

        if (Timer % 15 == 0)
        {
            var pos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width * 0.5f, Projectile.height * 0.5f);
            var vel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f, 0.5f);
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, vel, ModContent.ProjectileType<SnakeFireballTrail>(), 40, 3f, Main.myPlayer);
        }

        if (!Main.rand.NextBool(3))
            Dust.NewDust(Projectile.position + new Vector2(20), Projectile.width - 40, Projectile.height - 40, DustID.Torch, Scale: Main.rand.NextFloat(1f, 3f));

        Lighting.AddLight(Projectile.Center, TorchID.Torch);
    }

    public override void Kill(int timeLeft)
    {
        ExplosionHelper.Fire(Projectile.Center, 60, Main.rand.NextFloat(2, 4f), (3, 7));
        ExplosionHelper.Smoke(Projectile.GetSource_Death(), Projectile.Center, 10, (3, 7));

        PunchCameraModifier modifier = new(Projectile.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 10, 3, 20, 1500, "Fireball");
        Main.instance.CameraModifiers.Add(modifier);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        int frameHeight = tex.Height / Main.projFrames[Type];

        for (int i = 0; i < 2; ++i)
        {
            Rectangle frame = new(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            SpriteEffects effect = i % 2 == 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color col = Color.Lerp(Lighting.GetColor(Projectile.Center.ToTileCoordinates()), Color.Pink, 0.33f);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, col * Projectile.Opacity, Projectile.rotation, frame.Size() / 2f, 1f, effect, 0);
        }
        return false;
    }
}

internal class SnakeFireballTrail : ModProjectile
{
    public override void SetStaticDefaults() => Main.projFrames[Type] = 4;

    public override void SetDefaults()
    {
        Projectile.width = 44;
        Projectile.height = 44;
        Projectile.aiStyle = 0;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.alpha = 200;
    }

    public override void OnSpawn(IEntitySource source) => Projectile.rotation = Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);

    public override void AI()
    {
        Projectile.Opacity = Projectile.timeLeft / 60f;
        Projectile.velocity *= 0.98f;

        if (Projectile.frameCounter++ > 5)
        {
            Projectile.frameCounter = 0;

            if (++Projectile.frame >= Main.projFrames[Type])
                Projectile.frame = 0;
        }

        if (Projectile.velocity.LengthSquared() > 0)
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

        if (Main.rand.NextBool(5))
            Dust.NewDust(Projectile.position + new Vector2(4), Projectile.width - 8, Projectile.height - 8, DustID.Torch);
    }

    public override void Kill(int timeLeft)
    {
        ExplosionHelper.Fire(Projectile.Center, 8, Main.rand.NextFloat(1, 2), (2, 4));
    }
}