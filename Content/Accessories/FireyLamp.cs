using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Content.Accessories;

public class FireyLamp : ModItem
{
	public override void SetDefaults()
	{
        Item.damage = 10;
        Item.DamageType = DamageClass.Summon;
        Item.knockBack = 1;
        Item.width = 22;
		Item.height = 40;
		Item.value = Item.buyPrice(0, 3, 0, 0);
		Item.rare = ItemRarityID.Expert;
		Item.accessory = true;
		Item.expert = true;
	}

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
		player.statLifeMax2 += 30;
		player.GetModPlayer<LampModPlayer>().active = true;
    }
}

internal class LampModPlayer : ModPlayer
{
	public bool active = false;

	public override void ResetEffects() => active = false;

	public override void OnHurt(Player.HurtInfo info)
	{
		if (!active)
			return;

		const float SnakeCount = 10;

		List<int> npcs = [];

		for (int i = 0; i < Main.maxNPCs; ++i)
        {
			if (Main.npc[i].CanBeChasedBy() && Main.npc[i].DistanceSQ(Player.Center) < 600 * 600)
				npcs.Add(i);
        }

		if (npcs.Count <= 0)
			return;

		int dmg = (int)Player.GetDamage(DamageClass.Summon).ApplyTo(10);

		for (int i = 0; i < SnakeCount; ++i)
		{
			Vector2 vel = (i / SnakeCount * MathHelper.TwoPi).ToRotationVector2() * 12;
			int proj = Projectile.NewProjectile(Player.GetSource_OnHurt(info.DamageSource), Player.Center, vel, ModContent.ProjectileType<LampSnake>(), dmg, 0f, Player.whoAmI);
			Main.projectile[proj].ai[0] = Main.rand.Next(npcs);
		}

		ExplosionHelper.Fire(Player.Center, 40, Main.rand.NextFloat(1, 2f), (4, 8));
		ExplosionHelper.Smoke(Player.GetSource_OnHurt(info.DamageSource), Player.Center, 40, (2, 4));
	}
}

internal class LampSnake : ModProjectile
{
	private int Target
    {
		get => (int)Projectile.ai[0];
		set => Projectile.ai[0] = value;
    }

	public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

    public override void SetDefaults()
	{
		Projectile.width = 20;
		Projectile.height = 18;
		Projectile.timeLeft = 2;
		Projectile.aiStyle = -1;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.tileCollide = false;

		AIType = 0;
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, Projectile.DirectionTo(Main.npc[Target].Center) * 12f, 0.1f);
		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

		if (!Main.npc[Target].CanBeChasedBy() || Main.npc[Target].DistanceSQ(Projectile.Center) > 600 * 600)
		{
			List<int> npcs = [];

			for (int i = 0; i < Main.maxNPCs; ++i)
			{
				if (Main.npc[i].CanBeChasedBy() && Main.npc[i].DistanceSQ(Projectile.Center) < 600 * 600)
					npcs.Add(i);
			}

			if (npcs.Count <= 0)
			{
				Projectile.Kill();
				return;
			}

			Target = Main.rand.Next(npcs);
			Projectile.netUpdate = true;

			if (!Main.npc[Target].CanBeChasedBy() || Main.npc[Target].DistanceSQ(Projectile.Center) > 600 * 600)
				Projectile.Kill();
		}

		if (++Projectile.frameCounter == 4)
        {
			Projectile.frameCounter = 0;

			if (++Projectile.frame >= 3)
				Projectile.frame = 0;
        }
	}

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
		if (modifiers.FinalDamage.Base < Projectile.damage + target.defense / 2)
			modifiers.FinalDamage.Base = Projectile.damage + target.defense / 2;
    }

    public override void OnKill(int timeLeft) => ExplosionHelper.Fire(Projectile.Center, 15, Main.rand.NextFloat(0.8f, 1.5f), (2, 4));
}