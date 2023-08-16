using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Snaker.Common.Helpers;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Snaker.Content.Enemies;

public class PotatoBeeFireAnt : ModNPC
{
	private ref float Timer => ref NPC.ai[0];

	private Player Target => Main.player[NPC.target];

	static Asset<Texture2D> _glowTex;

    public override void SetStaticDefaults()
	{
		Main.npcFrameCount[NPC.type] = 4;
		NPCHelper.BuffImmune(Type);

		_glowTex = ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void Unload() => _glowTex = null;

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
			{
				var vel = NPC.DirectionTo(Target.Center) * 12;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<FireAntPotato>(), 24, 3f, Main.myPlayer);
			}
		}
		else
		{
			const float Interval = 8;
			float adjTimer = (int)(Timer * (1 / Interval)) * Interval;
			float sine = MathF.Sin(adjTimer * 0.25f) * MathHelper.PiOver4;
			NPC.velocity = NPC.DirectionTo(Target.Center).RotatedBy(sine) * 8;
		}
    }

    public override void FindFrame(int frameHeight)
    {
        if (NPC.frameCounter++ > 1)
        {
            NPC.frameCounter = 0;

			if (NPC.frame.Y < frameHeight * (Main.npcFrameCount[Type] - 1))
				NPC.frame.Y += frameHeight;
			else
				NPC.frame.Y = 0;
        }
    }

	public override void OnKill()
	{
		const int Range = 40;

		for (int i = 0; i < 3; ++i)
		{
			int x = (int)NPC.Center.X + Main.rand.Next(-Range, Range);
			int y = (int)NPC.Center.Y + Main.rand.Next(-Range, Range);
            NPC.NewNPC(NPC.GetSource_Death(), x, y, NPCID.Bee);
		}
	}

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
        {
            ExplosionHelper.Fire(NPC.Center, 25, Main.rand.NextFloat(1f, 2f), (1f, 3f));
            ExplosionHelper.Smoke(NPC.GetSource_Death(), NPC.Center, 10, (1f, 3f));
        }
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
		var tex = _glowTex.Value;
		var effect = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		var drawPos = NPC.position - screenPos - new Vector2(4, 4);

        Main.EntitySpriteDraw(tex, drawPos, NPC.frame, Color.White, NPC.rotation, Vector2.Zero, 1f, effect, 0);
    }
}
