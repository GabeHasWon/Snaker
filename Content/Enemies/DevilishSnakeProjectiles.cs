using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Snaker.Common.Additive;
using Snaker.Content.World;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Content.Enemies;

internal class SnakeFireball : ModProjectile
{
    private ref float Timer => ref Projectile.ai[0];

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
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi;

        Timer++;

        if (Timer % 15 == 0)
        {
            var pos = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width * 0.5f, Projectile.height * 0.5f);
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<SnakeFireballTrail>(), 40, 3f, Main.myPlayer);
        }

        if (!Main.rand.NextBool(3))
            Dust.NewDust(Projectile.position + new Vector2(20), Projectile.width - 40, Projectile.height - 40, DustID.Torch, Scale: Main.rand.NextFloat(1f, 3f));
    }

    public override void Kill(int timeLeft)
    {
        
    }
}

internal class SnakeFireballTrail : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 34;
        Projectile.height = 48;
        Projectile.aiStyle = 0;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
    }

    public override void OnSpawn(IEntitySource source) => Projectile.rotation = Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);

    public override void AI()
    {
        Projectile.Opacity = Projectile.timeLeft / 60f;

        if (Main.rand.NextBool(5))
            Dust.NewDust(Projectile.position + new Vector2(4), Projectile.width - 8, Projectile.height - 8, DustID.Torch);
    }

    public override void Kill(int timeLeft)
    {

    }
}

internal class SnakeMine : ModProjectile, IDrawAdditive
{
    public const float Gravity = 0.2f;
    public const int MaxTimeLeft = 540;
    public const int SetupTime = MaxTimeLeft - 40;
    public const float FadeTime = 30;

    private static Asset<Texture2D> _aura;

    private ref float FallDepth => ref Projectile.ai[0];

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 3;
        _aura = ModContent.Request<Texture2D>(Texture + "Aura");
    }

    public override void Unload() => _aura = null;

    public override void SetDefaults()
    {
        Projectile.width = 26;
        Projectile.height = 22;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MaxTimeLeft;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
    }

    public override void AI()
    {
        Projectile.velocity.Y += Gravity;
        Projectile.tileCollide = Projectile.Center.Y >= FallDepth;

        if (Projectile.velocity != new Vector2(0, Gravity))
        {
            Projectile.rotation += 0.02f * Projectile.velocity.X;
            Projectile.timeLeft++;
            return;
        }

        Projectile.rotation = 0;

        if (Projectile.timeLeft > SetupTime)
            return;

        if (Projectile.timeLeft <= FadeTime)
            Projectile.Opacity = Projectile.timeLeft / FadeTime;

        if (Projectile.timeLeft == SetupTime)
            SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.25f, PitchVariance = 0.5f }, Projectile.Center);

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player player = Main.player[i];

            if (player.active && !player.dead && player.DistanceSQ(Projectile.Center) < 140 * 140)
                Projectile.Kill();
        }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => !(fallThrough = false);

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity.X = 0;
        return false;
    }

    public override void Kill(int timeLeft)
    {
        if (timeLeft <= FadeTime)
            return;

        ExplosionHelper.Smoke(Projectile.GetSource_Death(), Projectile.Center, 10, (2f, 4f));
        ExplosionHelper.Fire(Projectile.Center, 40, Main.rand.NextFloat(1f, 2f), (4f, 8f));

        Projectile.position = Projectile.Center;
        Projectile.width = Projectile.height = 280;
        Projectile.position -= Projectile.Size / 2f;
        Projectile.hostile = true;
        Projectile.hide = true;
        Projectile.Damage();
        Projectile.Kill();

        Collision.HitTiles(Projectile.position, new Vector2(0, -Main.rand.NextFloat(6, 12)), Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { PitchVariance = 0.25f, Pitch = -0.75f }, Projectile.Center);
        PunchCameraModifier modifier = new(Projectile.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 4, 3f, 10, 4000, "SnakeMine");
        Main.instance.CameraModifiers.Add(modifier);
    }

    public void DrawAdditive()
    {
        var pos = Projectile.Center - Main.screenPosition;
        float factor = MathHelper.Min(1 - (Projectile.timeLeft - SetupTime) / 40f, 1f);
        float scale = 320f / _aura.Width() * factor;
        float rotation = MathF.Sin(Projectile.timeLeft * 0.02f) * scale;
        var color = Color.Lerp(Color.White, Color.Red, factor) * Projectile.Opacity;
        Main.spriteBatch.Draw(_aura.Value, pos, null, color, rotation, _aura.Size() / 2f, scale, SpriteEffects.None, 0);
    }
}

internal class SnakePotato : ModProjectile
{
    public const float Gravity = 0.02f;

    public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

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

internal class SnakeTongue : ModProjectile
{
    public Vector2 OriginLocation => GetOriginLocation(Parent);

    private float _originalVelMagnitude;

    private ref float Timer => ref Projectile.ai[0];
    
    private int ParentWhoAmI
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    private NPC Parent => Main.npc[ParentWhoAmI];

    public static Vector2 GetOriginLocation(NPC npc) => npc.Center + new Vector2(0, 220).RotatedBy(npc.rotation + MathHelper.PiOver2 - 0.4f);

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;

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
        Projectile.extraUpdates = 1;
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (source is EntitySource_Parent parent && parent.Entity is NPC npc)
            ParentWhoAmI = npc.whoAmI;

        _originalVelMagnitude = Projectile.velocity.Length();
    }

    public override void AI()
    {
        Timer++;

        Projectile.rotation = Projectile.AngleTo(OriginLocation) + MathHelper.PiOver2;

        if (Timer > (60 * 62 / _originalVelMagnitude))
        {
            float length = Projectile.velocity.LengthSquared();

            if (length <= _originalVelMagnitude * _originalVelMagnitude)
                Projectile.velocity += Projectile.DirectionTo(OriginLocation);
            else if (length > _originalVelMagnitude * _originalVelMagnitude)
                Projectile.velocity = Projectile.DirectionTo(OriginLocation) * _originalVelMagnitude;

            if (Projectile.DistanceSQ(OriginLocation) < _originalVelMagnitude * _originalVelMagnitude * 3)
                Projectile.Kill();
        }
    }


    public override bool PreDraw(ref Color lightColor)
    {
        const float TongueFadeDistance = 50;
        const int TongueBodyHeight = 14;

        Texture2D tex = TextureAssets.Projectile[Type].Value;
        float distance = Vector2.Distance(Projectile.position, OriginLocation);
        Vector2 drawPos = Projectile.position - Main.screenPosition;
        Vector2 norm = Projectile.DirectionTo(OriginLocation);

        for (int i = TongueBodyHeight; i < distance; i += TongueBodyHeight)
        {
            var adjPos = drawPos + (norm * i);
            var realPos = adjPos + Main.screenPosition;
            var col = Lighting.GetColor(realPos.ToTileCoordinates());

            float distanceToAnchor = Vector2.DistanceSquared(OriginLocation, realPos);
            if (distanceToAnchor < TongueFadeDistance * TongueFadeDistance)
                col *= distanceToAnchor / (TongueFadeDistance * TongueFadeDistance);

            Main.EntitySpriteDraw(tex, adjPos, new Rectangle(6, 14, 6, 14), col, Projectile.rotation, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(tex, drawPos, new Rectangle(0, 0, 18, 12), Color.White, Projectile.rotation, new Vector2(9, 6), 1f, SpriteEffects.None, 0);
        return false;
    }
}