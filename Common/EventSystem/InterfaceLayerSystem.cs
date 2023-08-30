using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Snaker.Content.Cangoler;
using Snaker.Content.Enemies;
using Snaker.Content.World;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Snaker.Common.EventSystem;

internal class InterfaceLayerSystem : ModSystem
{
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(x => x.Name == "Vanilla: Settings Button");
        layers.Insert(index - 1, new LegacyGameInterfaceLayer(
			"Snaker: Snake Event Progress",
			delegate
			{
				if (!SubworldSystem.IsActive<SnakerSubworld>() || !SnakeArenaSystem.Active)
					return true;

				Texture2D tex = ModContent.Request<Texture2D>("Snaker/Assets/Images/UI/EventBar").Value;
				var pos = new Vector2(Main.screenWidth / 2 - (tex.Width / 2f), 12);
                DrawProgressBarProgress(pos);

                Main.spriteBatch.Draw(tex, pos, Color.White); //Bar

                var font = FontAssets.DeathText.Value; //Wave number
                string progress = (SnakeArenaSystem.WaveProgress * 100).ToString("#0.#") + "%";
                string wave = Language.GetText("Mods.Snaker.DevilishSnakeEventWave")
                    .WithFormatArgs(Language.GetTextValue("Mods.Snaker.EventWaves." + SnakeArenaSystem.Wave), progress).Value;
                Vector2 origin = font.MeasureString(wave) / 2f;
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, wave, new Vector2(Main.screenWidth / 2f, 110), Color.White, 0f, origin, new(0.6f));
                return true;
			},
			InterfaceScaleType.UI)
		);

        layers.Insert(index - 1, new LegacyGameInterfaceLayer(
            "Snaker: Survival Timer",
            delegate
            {
                if (SurvivalTimerCheck(out float time, out float maxTime))
                    DrawSurvivalTimer(time, maxTime);
                return true;
            }
            ));
	}

	private static void DrawProgressBarProgress(Vector2 pos)
    {
        SetProgressToBossLife();

        float progress = (float)SnakeArenaSystem.Wave / 5f + (SnakeArenaSystem.WaveProgress / 5f);
        float width = 186 * progress;
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos + new Vector2(10, 50), new Rectangle(0, 0, (int)width, 12), Color.Orange);

        float emptyWidth = 186 * (1 - progress);
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos + new Vector2(10 + (int)width, 50), new Rectangle(0, 0, (int)emptyWidth, 12), Color.Gray);

        if (SnakeArenaSystem.Wave != SnakeArenaSystem.EventStage.Boss)
            return;
    }

    internal static void SetProgressToBossLife()
    {
        if (SnakeArenaSystem.Wave == SnakeArenaSystem.EventStage.Boss) //Set progress to boss's health
        {
            int whoAmI = NPC.FindFirstNPC(ModContent.NPCType<DevilishSnake>());

            if (whoAmI != -1)
            {
                NPC npc = Main.npc[whoAmI];

                if (npc.active && npc.life > 0)
                    SnakeArenaSystem.WaveProgress = 1 - (npc.life / (float)npc.lifeMax);
            }
        }
    }

    private static bool SurvivalTimerCheck(out float time, out float maxTime)
    {
        time = 0;
        maxTime = 0;

        if (SubworldSystem.Current is SnakerSubworld) //Uh oh if hardcoding
        {
            int npc = NPC.FindFirstNPC(ModContent.NPCType<DevilishSnake>());

            if (npc == -1)
                return false;

            DevilishSnake boss = Main.npc[npc].ModNPC as DevilishSnake;

            if (boss.State != DevilishSnake.SnakeState.Survival)
                return false;

            time = boss.Timer / 60f;
            maxTime = DevilishSnake.SurvivalTime;
            return true;
        }
        else if (NPC.AnyNPCs(ModContent.NPCType<IceMonster>()))
        {
            int npc = NPC.FindFirstNPC(ModContent.NPCType<IceMonster>());
            IceMonster iceMonster = Main.npc[npc].ModNPC as IceMonster;

            time = iceMonster.DespawnTimer / 60f;
            maxTime = IceMonster.SurvivalTime;
            return true;
        }
        else
            return false;
    }

    private static void DrawSurvivalTimer(float time, float maxTime)
    {
        string text = time.ToString("0.00") + "/" + (maxTime / 60f).ToString("00");
        var font = FontAssets.DeathText.Value;
        Vector2 origin = font.MeasureString(text) / 2f;
        Vector2 textPos = new(Main.screenWidth / 2f, Main.screenHeight - 120);
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, textPos, Color.White, 0f, origin, Vector2.One);

        text = Language.GetTextValue("Mods.Snaker.SurviveText");
        origin = font.MeasureString(text) / 2f;
        textPos = new(Main.screenWidth / 2f, Main.screenHeight - 160);
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, textPos, Color.LightGray, 0f, origin, Vector2.One * 0.75f);

        text = Language.GetTextValue("Mods.Snaker.SurviveSecondsText");
        origin = font.MeasureString(text) / 2f;
        textPos = new(Main.screenWidth / 2f, Main.screenHeight - 80);
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, textPos, Color.LightGray, 0f, origin, Vector2.One * 0.55f);
    }
}
