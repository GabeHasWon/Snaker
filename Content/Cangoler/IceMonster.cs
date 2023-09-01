using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Snaker.Common.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Localization;

namespace Snaker.Content.Cangoler;

public class IceMonster : ModNPC
{
	public const int SurvivalTime = 1 * 60 * 60;
	public const int MaxSpawnTime = 100;

	private static Asset<Texture2D> _hats;
	private static Asset<Texture2D> _eyes;

	private ref float Timer => ref NPC.ai[0];
	private ref float SpawnTimer => ref NPC.ai[1];
	internal ref float DespawnTimer => ref NPC.ai[2];

	private bool IsSecondStage
	{
		get => NPC.ai[3] != 0;
		set => NPC.ai[3] = value.ToInt();
	}

	private bool IsFading => DespawnTimer > SurvivalTime - (2 * 60);

	private Player Target => Main.player[NPC.target];

	private Vector2 _eyesDirection = Vector2.Zero;
	private Vector2 _spawnPosition = Vector2.Zero;

	public override void SetStaticDefaults()
	{
		NPCHelper.BuffImmune(Type);

		_hats = ModContent.Request<Texture2D>(Texture + "_Hats");
		_eyes = ModContent.Request<Texture2D>(Texture + "_Eyes");
	}

	public override void Unload() => _hats = _eyes = null;

    public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 1;
		NPC.defense = 1;
		NPC.value = Item.buyPrice(0, 0, 0, 0);
		NPC.knockBackResist = 0f;
		NPC.width = 64;
		NPC.height = 64;
		NPC.damage = 50;
		NPC.alpha = 40;
		NPC.lavaImmune = true;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.HitSound = SoundID.DD2_SkeletonHurt;
		NPC.DeathSound = SoundID.DD2_SkeletonDeath;
		NPC.dontTakeDamage = true;
		NPC.immortal = true;
	}

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundSnow,
			new FlavorTextBestiaryInfoElement("Mods.Snaker.NPCs.IceMonster.Bestiary"),
		});
	}

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => SpawnTimer >= MaxSpawnTime;

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
		string deathText = Language.GetText("Mods.Snaker.UnfathomableColdDeath." + Main.rand.Next(3)).WithFormatArgs(target.name).Value;
        target.KillMe(PlayerDeathReason.ByCustomReason(deathText), hurtInfo.Damage, hurtInfo.HitDirection);
    }

    public override void AI()
    {
		if (_spawnPosition == Vector2.Zero)
			_spawnPosition = NPC.Center + new Vector2(0, 20);

		NPC.TargetClosest(true);
		NPC.direction = NPC.spriteDirection = Target.Center.X > NPC.Center.X ? 1 : -1;
		NPC.rotation = NPC.velocity.X * 0.02f;

        PlayerTrapCollision();

        SpawnTimer++;

        if (SpawnTimer < MaxSpawnTime)
		{
			var factor = SpawnTimer / MaxSpawnTime;

            NPC.Opacity = factor;
			NPC.velocity.Y = -3f * factor;
			return;
		}

		if (Main.rand.NextBool(20))
			Dust.NewDust(NPC.position, NPC.width, NPC.height, Main.rand.NextBool() ? DustID.Ice : DustID.IceRod, 0, 0);

		Lighting.AddLight(NPC.Center, new Vector3(0.01f, 0.05f, 0.15f) * 2);

		Timer++;
		DespawnTimer++;

		if (Timer < 300)
		{
			const float MoveSpeed = 0.1f;
			const float MaxSpeed = 3f;

			if (!IsFading)
			{
				NPC.velocity += NPC.DirectionTo(Target.Center) * MoveSpeed;

				if (NPC.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
					NPC.velocity = Vector2.Normalize(NPC.velocity) * MaxSpeed;
			}
			else
				NPC.velocity *= 0.99f;

			if (IsSecondStage && !IsFading && Timer % 58 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				var vel = NPC.DirectionTo(Target.Center + Target.velocity) * 8;
                var proj = Main.projectile[Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ProjectileID.IceBolt, 30, 1f, Main.myPlayer)];
				proj.timeLeft = 90;
				proj.tileCollide = false;
				proj.friendly = false;
				proj.hostile = true;
			}
		}
		else
		{
			if (Timer < 400)
				NPC.velocity *= 0.99f;
			else if (Timer >= 400)
			{
				const float Cutoff = 10;
				const float MaxWait = 70;

				if (Timer < 400 + Cutoff)
				{
					float adj = Timer - 400;
					NPC.velocity = NPC.DirectionTo(Target.Center) * (adj / Cutoff) * 10;
				}
				else if (Timer >= 400 + MaxWait)
					Timer = 0;
			}
		}

		if (!IsSecondStage && DespawnTimer >= SurvivalTime / 2)
			IsSecondStage = true;

		if (IsSecondStage && !IsFading)
			NPC.scale = MathHelper.Lerp(NPC.scale, 1.5f, 0.02f);
		else if (IsFading)
		{
			NPC.scale = MathHelper.Lerp(NPC.scale, 0f, 0.03f);
			NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0f, 0.02f);
		}

        NPC.position = NPC.Center;
        NPC.width = (int)(ContentSamples.NpcsByNetId[Type].width * NPC.scale);
        NPC.height = (int)(ContentSamples.NpcsByNetId[Type].height * NPC.scale);
        NPC.position -= NPC.Size / 2f;

        if (DespawnTimer > SurvivalTime)
		{
			NPC.active = false;

			if (Main.netMode != NetmodeID.MultiplayerClient)
				Item.NewItem(NPC.GetSource_Death(), NPC.Hitbox, ModContent.ItemType<CangolerItem>());

			SoundEngine.PlaySound(SoundID.Shatter, NPC.Center);
		}
	}

    private void PlayerTrapCollision()
    {
		bool kill = true;

        for (int i = 0; i < Main.maxPlayers; ++i)
		{
			Player plr = Main.player[i];

			if (plr.active && !plr.dead) 
			{ 
				float dist = plr.Distance(_spawnPosition);

				if (dist > 17 * 16 && dist < 18 * 16)
					plr.velocity = plr.DirectionTo(_spawnPosition) * 8;

				if (dist < 18 * 16)
					kill = false;
			}
		}

		if (kill)
			NPC.active = false;

		if (DespawnTimer % 14 != 0)
			return;

		const int Intervals = 40;

		Vector2 offset = new(18.5f * 16, 0);

		for (int i = 0; i < Intervals; ++i)
		{
			Vector2 spawnPos = _spawnPosition + offset.RotatedBy(MathHelper.Lerp(0, MathHelper.TwoPi, i / (float)Intervals));
			Dust.NewDust(spawnPos, 1, 1, DustID.SnowflakeIce, Scale: 0.5f);
		}
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
		var pos = NPC.Center - screenPos - new Vector2(0, 16);
		var origin = NPC.frame.Size() / 2f;
		var effect = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

		spriteBatch.Draw(_hats.Value, pos, null, drawColor * NPC.Opacity, NPC.rotation, origin, NPC.scale, effect, 0);

        _eyesDirection = Vector2.Lerp(_eyesDirection, NPC.DirectionTo(Target.Center) * 12 + new Vector2(2, 10), 0.2f);
        spriteBatch.Draw(_eyes.Value, pos + _eyesDirection, null, Color.White * NPC.Opacity, 0f, _eyes.Size() / 2f, NPC.scale, effect, 0);

        spriteBatch.Draw(TextureAssets.Npc[Type].Value, pos, null, drawColor * NPC.Opacity * 0.75f, NPC.rotation, origin, NPC.scale, effect, 0);
        spriteBatch.Draw(_eyes.Value, pos + _eyesDirection, null, Color.White * NPC.Opacity * 0.5f, 0f, _eyes.Size() / 2f, NPC.scale, effect, 0);
        return false;
    }
}
