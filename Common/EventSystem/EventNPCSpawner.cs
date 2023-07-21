using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Common.EventSystem;

internal class EventNPCSpawner : ModNPC
{
    const int MaxTimer = 250;

    public override string Texture => "Snaker/Assets/Images/Empty";

    private ref float Timer => ref NPC.ai[0];
    private int StoredId => (int)NPC.ai[1];

    public override void SetStaticDefaults()
    {
        NPCID.Sets.NPCBestiaryDrawModifiers mods = new(0)
        {
            Hide = true,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, mods);
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.BoundGoblin);
        NPC.aiStyle = -1;
        NPC.noTileCollide = true;
        NPC.noGravity = true;
        NPC.friendly = false;
        NPC.immortal = true;
        NPC.dontTakeDamage = true;
        NPC.rarity = 0;
        NPC.damage = 0;
    }

    public override void OnSpawn(IEntitySource source) => Timer = MaxTimer;

    public override void AI()
    {
        NPC.rotation = MathF.Pow(--Timer * 0.02f, 2f);
        NPC.TargetClosest();

        var dustVel = Main.rand.NextFloat(MathHelper.Pi).ToRotationVector2() * Main.rand.NextFloat(3, 6f);
        Dust.NewDust(NPC.position - NPC.Size / 2f, NPC.width, NPC.height, DustID.Torch, dustVel.X, dustVel.Y, Scale: Main.rand.NextFloat(1f, 2f));
        
        if (Timer <= 0)
        {
            NPC.active = false;

            NPC.NewNPCDirect(new EntitySource_SpawnNPC("SnakerEvent"), (int)NPC.position.X, (int)NPC.position.Y + NPC.height / 2, StoredId);

            for (int i = 0; i < 40; ++i)
            {
                dustVel = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * Main.rand.NextFloat(7, 12f);
                Dust.NewDustPerfect(NPC.position - NPC.Size / 2f, DustID.Torch, dustVel, Scale: Main.rand.NextFloat(1.5f, 2.5f));
            }

            for (int i = 0; i < 8; ++i)
            {
                Vector2 velocity = new Vector2(Main.rand.NextFloat(2, 4), 0).RotatedByRandom(MathHelper.TwoPi);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center - NPC.Size / 2f, velocity, GoreID.Smoke1 + Main.rand.Next(3));
            }

            SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { PitchVariance = 0.5f, Volume = 0.25f }, NPC.Center);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var pos = NPC.position - screenPos;
        var tex = TextureAssets.Npc[StoredId].Value;
        var src = new Rectangle(0, 0, tex.Width, tex.Height / Main.npcFrameCount[StoredId]);
        var col = ContentSamples.NpcsByNetId[StoredId].GetAlpha(Lighting.GetColor(NPC.Center.ToTileCoordinates(), drawColor) * (1 - (Timer / (float)MaxTimer)));
        var effects = Main.player[NPC.target].Center.X < NPC.Center.X ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        spriteBatch.Draw(tex, pos, src, col, NPC.rotation, src.Size() / 2f, 1f, effects, 0);
        return false;
    }
}
