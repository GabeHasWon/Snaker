using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Content.Weapons;

public class BurningPotato : ModItem
{
	// public override void SetStaticDefaults() => Tooltip.SetDefault("Explodes after a few seconds");

	public override void SetDefaults()
	{
		Item.damage = 70;
		Item.DamageType = DamageClass.Ranged;
		Item.width = 30;
		Item.height = 34;
		Item.useTime = Item.useAnimation = 30;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.knockBack = 6;
		Item.value = Item.buyPrice(0, 0, 1, 0);
		Item.rare = ItemRarityID.Green;
		Item.UseSound = SoundID.Item1;
		Item.autoReuse = true;
		Item.consumable = true;
		Item.shoot = ModContent.ProjectileType<BurningPotatoProjectile>();
		Item.shootSpeed = 8f;
		Item.noMelee = true;
		Item.noUseGraphic = true;
		Item.maxStack = 99;
	}
}

public class BurningPotatoProjectile : ModProjectile
{
	const int ExplodeTimeLeft = 3;

    public override void SetDefaults()
    {
		Projectile.CloneDefaults(ProjectileID.Grenade);
		Projectile.width = 20;
		Projectile.height = 18;
		Projectile.timeLeft = 4 * 60;
		Projectile.aiStyle = 0;
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.shouldFallThrough = false;

		AIType = 0;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
		fallThrough = false;
		return true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
	{
		if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
			Projectile.velocity.X = -oldVelocity.X * 0.8f;

		if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
		{
			Projectile.velocity.X *= 0.98f;
			Projectile.velocity.Y = -oldVelocity.Y * 0.2f;
		}
		return false;
	}

    public override void AI()
    {
		if (Main.rand.NextBool(5))
        {
			Vector2 velocity = new Vector2(0, -Main.rand.NextFloat(0.5f, 3f)).RotatedByRandom(MathHelper.PiOver4);
			Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, 0, 0, Scale: Main.rand.NextFloat(1f, 2f)).velocity = velocity + Projectile.velocity;
		}

		Projectile.velocity.X *= 0.997f;
		Projectile.velocity.Y += 0.2f;
		Projectile.rotation += 0.05f * Projectile.velocity.X;

		if (Projectile.timeLeft == ExplodeTimeLeft)
		{
			ExplosionHelper.Fire(Projectile.Center, 40, Main.rand.NextFloat(1f, 2f), (4f, 8f));
			ExplosionHelper.Smoke(Projectile.GetSource_Death(), Projectile.position, 8, (2, 4));
			ExplosionHelper.Smoke(Projectile.GetSource_Death(), Projectile.position, 4, (3, 6));

			Projectile.position = Projectile.Center;
			Projectile.width = Projectile.height = 180;
			Projectile.position -= Projectile.Size / 2f;

			Projectile.friendly = true;
			Projectile.hostile = true;
			Projectile.hide = true;

			SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { PitchVariance = 0.5f }, Projectile.Center);
		}
	}
}