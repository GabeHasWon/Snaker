using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Content.Weapons;

public class SnakeHammer : ModItem
{
	// public override void SetStaticDefaults() => Tooltip.SetDefault("Charge to slam into the ground, hurting nearby enemies\nRight click to throw like a boomerang");

    public override void SetDefaults()
    {
        Item.width = 48;
        Item.height = 50;
        Item.knockBack = 6f;
        Item.autoReuse = false;
        Item.useTime = Item.useAnimation = 24;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item1;
        Item.DamageType = DamageClass.Melee;
        Item.damage = 40;
        Item.crit = 5;
        Item.knockBack = 6;
        Item.rare = ItemRarityID.Purple;
        Item.value = Item.buyPrice(gold: 5);
        Item.noUseGraphic = true;
        Item.shootSpeed = 0f;
        Item.shoot = ModContent.ProjectileType<SnakeHammerSwung>();
        Item.channel = true;
        Item.noMelee = true;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        if (player.altFunctionUse == 2)
        {
            Item.shoot = ModContent.ProjectileType<SnakeHammerThrown>();
            Item.shootSpeed = 9;
            Item.knockBack = 12f;
            Item.damage = 95;
        }
        else
        {
            Item.shoot = ModContent.ProjectileType<SnakeHammerSwung>();
            Item.shootSpeed = 0f;
            Item.knockBack = 6f;
            Item.damage = 40;
        }

        return player.altFunctionUse != 2 || player.ownedProjectileCounts[ModContent.ProjectileType<SnakeHammerThrown>()] < 1;
    }

    public override void ModifyShootStats(Player p, ref Vector2 pl, ref Vector2 v, ref int t, ref int damage, ref float kB) => damage = Item.damage;
}

internal class SnakeHammerSwung : ModProjectile
{
    private const int MaxCharge = 60;

    public override string Texture => base.Texture[..(base.Texture.Length - "Swung".Length)];

    protected Player Owner => Main.player[Projectile.owner];

    private ref float Timer => ref Projectile.ai[0];
    private ref float Charge => ref Projectile.ai[1];

    private readonly List<SlashEffect> Particles = new();

    private bool _letGo = false;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 50;
        Projectile.aiStyle = 0;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        DrawHeldProjInFrontOfHeldItemAndArms = true;
    }

    public override bool? CanHitNPC(NPC target) => _letGo ? null : false;
    public override bool? CanCutTiles() => _letGo ? null : false;

    public override void AI()
    {
        Owner.direction = Main.MouseWorld.X <= Owner.Center.X ? -1 : 1;
        Owner.heldProj = Projectile.whoAmI;
        Owner.itemTime = 2;

        float charge = Math.Max(MathHelper.PiOver4 - (MathF.Sqrt(Charge) * 0.08f), 0);
        float swing = SwingArc();
        float moveRotation;

        if (Owner.direction == -1)
        {
            swing *= -1;
            swing -= charge;
            moveRotation = swing;
            moveRotation -= MathHelper.PiOver4 * 3;
            swing -= MathHelper.PiOver4;
        }
        else
        {
            swing += charge;
            swing -= MathHelper.PiOver4;
            moveRotation = swing;
        }

        Projectile.timeLeft++;
        Projectile.rotation = (MathHelper.PiOver4 * Owner.direction) + swing;

        var offset = (moveRotation.ToRotationVector2() * 40) + new Vector2(-4, 4);
        Projectile.Center = Owner.Center + offset;

        var particlePos = Owner.Center + (offset * 0.66f) + offset.RotatedBy(MathHelper.PiOver2 * -Owner.direction) - Projectile.Size / 2f;
        Particles.Add(new(2, Projectile.rotation + MathHelper.PiOver4 * 3, Main.rand.NextFloat(-0.05f, 0.05f), particlePos));

        foreach (var item in Particles)
            item.Update();

        float armRot = Projectile.rotation - MathHelper.PiOver2 - MathHelper.PiOver4;
        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);

        if (!Owner.channel || _letGo)
        {
            Timer++;
            _letGo = true;
        }
        else
            Charge++;
    }

    private float SwingArc()
    {
        float useTime = (int)(Owner.HeldItem.useTime / Owner.GetAttackSpeed(DamageClass.Melee));
        float adjTimer = Timer % useTime;
        float returnValue = MathF.Pow(adjTimer, 2);

        if (Timer >= useTime - 1)
            Projectile.Kill();

        SwingEffects(useTime, adjTimer);

        return returnValue / (useTime * useTime) * MathHelper.Pi * 1.25f - MathHelper.PiOver2;
    }

    private void SwingEffects(float useTime, float adjTimer)
    {
        if (adjTimer == 1) //Play sound
            SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.8f }, Projectile.Center);

        if (Charge > MaxCharge && adjTimer >= useTime - 2)
            CheckPound();
    }

    private void CheckPound()
    {
        if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
        {
            for (int i = 0; i < 8; ++i)
            {
                Vector2 velocity = new Vector2(Main.rand.NextFloat(2, 4), 0).RotatedByRandom(MathHelper.TwoPi);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, velocity, GoreID.Smoke1 + Main.rand.Next(3));
            }

            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 320;
            Projectile.position -= Projectile.Size / 2f;
            Projectile.friendly = true;
            Projectile.hide = true;
            Projectile.Damage();
            Projectile.Kill();

            Collision.HitTiles(Projectile.position, new Vector2(0, -Main.rand.NextFloat(6, 12)), Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { PitchVariance = 0.25f, Pitch = -0.75f }, Projectile.Center);

            PunchCameraModifier modifier = new(Projectile.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 7f, 3f, 15, 1000f, "SnakeHammer");
            Main.instance.CameraModifiers.Add(modifier);
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.HitDirectionOverride = Main.MouseWorld.X <= Owner.Center.X ? -1 : 1;

    public override bool PreDraw(ref Color lightColor)
    {
        if (Projectile.hide)
            return false;

        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);

        if (_letGo)
        {
            foreach (var item in Particles) //Draws all of the slash effects
            {
                var tex = TextureAssets.Extra[89].Value;
                var scale = new Vector2(0.3f * ((item.colorBase - 230) / 25f), 0.5f * ((item.colorBase - 200) / 55f));
                var origin = new Vector2(tex.Width / 2, tex.Height - 0);
                var col = item.GetColor() * Lighting.Brightness((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
                Main.EntitySpriteDraw(tex, item.position - Main.screenPosition + item.offset, null, col, item.Rotation, origin, scale, SpriteEffects.None, 0);
            }
        }

        Vector2 drawPos = Projectile.position - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
        Color color = Projectile.GetAlpha(lightColor);
        Rectangle src = new(0, texture.Height * Projectile.frame, texture.Width, texture.Height / Main.projFrames[Type]);
        Main.EntitySpriteDraw(texture, drawPos, src, color, Projectile.rotation, drawOrigin, Projectile.scale * Math.Min(Charge / MaxCharge * 0.25f + 0.75f, 1), SpriteEffects.None, 0);
        return false;
    }

    /// <summary>
    /// Controls one instance of a little slash effect.
    /// </summary>
    private class SlashEffect
    {
        internal readonly int MaxTimeLeft = 0;
        internal readonly float Rotation = 0;

        internal int timeLeft = 0;
        internal Vector2 offset = Vector2.Zero;
        internal int colorBase = 0;
        internal Vector2 position = Vector2.Zero;

        public SlashEffect(int timeLeft, float realRotation, float rotAdjustment, Vector2 position)
        {
            this.timeLeft = timeLeft;
            this.position = position;
            Rotation = realRotation + rotAdjustment;
            MaxTimeLeft = timeLeft;
            colorBase = Main.rand.Next(230, 255);

            const float Range = 16;

            float offsetBasis = realRotation - Rotation;
            var random = new Vector2(Main.rand.NextFloat(-Range, Range), Main.rand.NextFloat(-Range, Range));
            offset = offsetBasis * Rotation.ToRotationVector2() * Main.rand.NextFloat(-16, 17) + random;
        }

        internal void Update() => timeLeft--;

        internal Color GetColor()
        {
            var col = new Color(colorBase, colorBase, colorBase, 0) * 0.6f;
            return col * (timeLeft / (float)MaxTimeLeft);
        }
    }
}

internal class SnakeHammerThrown : ModProjectile
{
    public override string Texture => base.Texture[..(base.Texture.Length - "Thrown".Length)];

    protected Player Owner => Main.player[Projectile.owner];

    private ref float Timer => ref Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 42;
        Projectile.aiStyle = 0;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.hostile = false;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 16;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.extraUpdates = 1;
        Projectile.localNPCHitCooldown = 20;
        Projectile.usesLocalNPCImmunity = true;

        DrawHeldProjInFrontOfHeldItemAndArms = true;
    }

    public override void AI()
    {
        Projectile.timeLeft++;
        Projectile.rotation += 0.25f;
        Projectile.tileCollide = Timer > 6;

        if (Timer++ >= 30)
        {
            if (Projectile.Hitbox.Intersects(Owner.Hitbox))
                Projectile.Kill();

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Owner.Center) * 9, 0.2f);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Projectile.penetrate == 1)
        {
            Projectile.penetrate++;
            Timer = 30;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Timer = 31;
        Projectile.velocity = Projectile.DirectionTo(Owner.Center) * 9;
        return false;
    }
}