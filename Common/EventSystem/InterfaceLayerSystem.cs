using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Snaker.Content.World;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

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
				if (!SubworldLibrary.SubworldSystem.IsActive<SnakerSubworld>())
					return true;

				Texture2D tex = ModContent.Request<Texture2D>("Snaker/Assets/Images/UI/EventBar").Value;
				var pos = new Vector2(Main.screenWidth / 2 - (tex.Width / 2f), 12);
				DrawProgressBarProgress(pos);

				Main.spriteBatch.Draw(tex, pos, Color.White);
				return true;
			},
			InterfaceScaleType.UI)
		);
	}

    private void DrawProgressBarProgress(Vector2 pos)
    {
		float progress = (float)EventManagerSystem.Wave / 5f + (EventManagerSystem.WaveProgress / 5f);
		float width = 184 * progress;
		Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos + new Vector2(10, 50), new Rectangle(0, 0, (int)width, 12), Color.Orange);

		float emptyWidth = 184 * (1 - progress);
		Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos + new Vector2(10 + (int)width, 50), new Rectangle(0, 0, (int)emptyWidth, 12), Color.Gray);
    }
}
