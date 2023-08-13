using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Snaker.Common.Helpers;
using System;
using Microsoft.Xna.Framework;

namespace Snaker.Content.Enemies;

public class PotatoBeeFireAnt : ModNPC
{
	private ref float Timer => ref NPC.ai[0];

	private Player Target => Main.player[NPC.target];

    public override void SetStaticDefaults()
	{
		Main.npcFrameCount[NPC.type] = 1;
		NPCHelper.BuffImmune(Type);
	}

    public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 200;
		NPC.defense = 18;
		NPC.value = Item.buyPrice(0, 0, 0, 0);
		NPC.knockBackResist = 0f;
		NPC.width = 46;
		NPC.height = 44;
		NPC.damage = 24;
		NPC.lavaImmune = true;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.HitSound = SoundID.DD2_SkeletonHurt;
		NPC.DeathSound = SoundID.DD2_SkeletonDeath;
		NPC.dontTakeDamage = false;
	}

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
			new FlavorTextBestiaryInfoElement("Mods.Snaker.NPCs.PotatoBeeFireAnt.Bestiary"),
		});
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
		{
			for (int i = 1; i < 5; ++i)
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, GoreID.AmbientAirborneCloud1, 1f);
		}
	}

    public override void AI()
    {
		const int MaxDistance = 800;

		NPC.TargetClosest(true);
		NPC.direction = NPC.spriteDirection = Target.Center.X > NPC.Center.X ? 1 : -1;
		
		Timer++;

		if (Target.DistanceSQ(NPC.Center) < MaxDistance * MaxDistance)
        {
			float adjTimer = (int)(Timer * 0.25f) * 4;
			float sine = MathF.Sin(adjTimer * 0.25f) * MathHelper.PiOver2;

			if (NPC.velocity.LengthSquared() <= 4 * 4)
				NPC.velocity = NPC.DirectionTo(Target.Center).RotatedBy(sine) * 4;
			else
				NPC.velocity *= 0.95f;

			if (Timer % 180 == 0)
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(Target.Center) * 12, ProjectileID.Fireball, 22, 3f, Main.myPlayer);
        }
		else
        {
			const float Interval = 8;
			float adjTimer = (int)(Timer * (1 / Interval)) * Interval;
			float sine = MathF.Sin(adjTimer * 0.25f) * MathHelper.PiOver4;
			NPC.velocity = NPC.DirectionTo(Target.Center).RotatedBy(sine) * 8;
        }
    }
}
