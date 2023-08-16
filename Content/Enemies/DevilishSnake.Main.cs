using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Snaker.Common.Helpers;
using Snaker.Content.Weapons;
using Snaker.Content.Blocks;
using System;
using Microsoft.Xna.Framework;
using Snaker.Content.Misc;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Snaker.Content.Enemies;

[AutoloadBossHead]
public partial class DevilishSnake : ModNPC
{
	static Asset<Texture2D> _bodyTexture;

    public override void SetStaticDefaults()
	{
		Main.npcFrameCount[NPC.type] = 1;
		NPCHelper.BuffImmune(Type);

		_bodyTexture = ModContent.Request<Texture2D>(Texture + "BodySegment");
	}

	public override void Unload() => _bodyTexture = null;

    public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 12000;
		NPC.defense = 18;
		NPC.value = Item.buyPrice(0, 15, 0, 0);
		NPC.knockBackResist = 0f;
		NPC.width = 480;
		NPC.height = 296;
		NPC.damage = 0;
		NPC.lavaImmune = true;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.HitSound = SoundID.DD2_SkeletonHurt;
		NPC.DeathSound = SoundID.DD2_SkeletonDeath;
		NPC.boss = true;
		NPC.dontTakeDamage = true;
		NPC.hide = true;
	}

	public override bool CheckActive() => false;
    public override void DrawBehind(int index) => Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);

    public override bool? CanBeHitByProjectile(Projectile projectile)
    {
        if (NPC.DistanceSQ(projectile.Center - new Vector2(0, 60)) < MathF.Pow(NPC.width / 3.5f, 2))
            return null;
		return false;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
			new FlavorTextBestiaryInfoElement("Mods.Snaker.NPCs.DevilishSnake.Bestiary"),
		});
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
		{
			Vector2 Offset(int distance, float rotation) => (rotation - MathHelper.PiOver2 + NPC.rotation).ToRotationVector2() * distance;

            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + Offset(100, -MathHelper.PiOver4), NPC.velocity, Mod.Find<ModGore>("DevilishSnake0").Type, 1f);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + Offset(200, -MathHelper.PiOver2 - (MathHelper.PiOver4 / 3f)), NPC.velocity, Mod.Find<ModGore>("DevilishSnake1").Type, 1f);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + Offset(100, MathHelper.PiOver4 / 2f), NPC.velocity, Mod.Find<ModGore>("DevilishSnake2").Type, 1f);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + Offset(200, -MathHelper.PiOver4), NPC.velocity, Mod.Find<ModGore>("DevilishSnake3").Type, 1f);

            //Body gores
            Vector2 direction = new Vector2(0, 106).RotatedBy(0.2f) * 0.12f;
            Vector2 basePos = NPC.Center + direction;
            float rotation = NPC.rotation;
            float scale = 0.85f;

            for (int i = 0; i < 20; ++i)
            {
                rotation = MathHelper.Lerp(rotation, -MathHelper.PiOver2, 0.1f);
                scale = MathHelper.Lerp(scale, 1f, 0.2f);

                var realDirection = direction.RotatedBy(rotation);
                var pos = basePos + (realDirection * i * 3f);

				var gore = Gore.NewGoreDirect(NPC.GetSource_Death(), pos - new Vector2(0, 160), NPC.velocity, Mod.Find<ModGore>("DevilishSnakeBody").Type, 1f);

				if (i > 6)
					gore.alpha = (int)(((i - 6) / 14f) * 255);
            }
        }

		for (int k = 0; k < 20; k++)
		{
			const int ShrinkFactor = 100;

			Vector2 adjPos = NPC.position + new Vector2(ShrinkFactor / 2);
			Dust.NewDust(adjPos, NPC.width - ShrinkFactor, NPC.height - ShrinkFactor, DustID.Blood, 2.5f * hit.HitDirection, -2.5f, 0, default, 1.2f);
			Dust.NewDust(adjPos, NPC.width - ShrinkFactor, NPC.height - ShrinkFactor, DustID.RedMoss, 2.5f * hit.HitDirection, -2.5f, 0, default, 1.2f);
		}
	}

	public override void ModifyNPCLoot(NPCLoot npcLoot)
	{
        npcLoot.AddMasterModeCommonDrop<SnakeRelicItem>();
        npcLoot.AddBossBag<SnakeBag>();

        LeadingConditionRule notExpertRule = new(new Conditions.NotExpert());
		notExpertRule.AddCommon<SnakePainting>(7);
		notExpertRule.AddCommon<SnakeTrophyItem>(10);
		notExpertRule.AddCommon<BurningPotato>(1, 32, 43);
		notExpertRule.AddCommon<SnakeBrick>(1, 10, 20);
		notExpertRule.AddOneFromOptions<SnakeHammer, SnakeStaff>();

		npcLoot.Add(notExpertRule);
	}
}
