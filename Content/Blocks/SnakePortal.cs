using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Snaker.Common.Effects;
using Snaker.Common.Helpers;
using Snaker.Content.World;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Snaker.Content.Blocks;

internal class SnakePortal : ModItem
{
    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<SnakePortalTile>());
}

public class SnakePortalTile : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.Width = 5;
		TileObjectData.newTile.Height = 5;
		TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16 };
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
		TileObjectData.addTile(Type);

		DustType = DustID.Obsidian;
		MinPick = 101;

		LocalizedText name = CreateMapEntryName();
		AddMapEntry(new Color(51, 50, 69), name);
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
	public override void KillMultiTile(int i, int j, int frameX, int frameY) => 
		Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 80, ModContent.ItemType<SnakePortal>());

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
		Tile tile = Main.tile[i, j];
		Rectangle source = new(tile.TileFrameX, tile.TileFrameY, 16, 16);

		if (Main.hardMode)
			source.Y += 90 * 2;
		else if (NPC.downedBoss2)
			source.Y += 90;

		spriteBatch.Draw(TextureAssets.Tile[Type].Value, TileHelper.TileCustomPosition(i, j), source, Lighting.GetColor(i, j));
		return false;
    }

    public override bool RightClick(int i, int j)
    {
		if (Main.hardMode)
		{
			SubworldSystem.Enter<SnakerSubworld>();
			return true;
		}
		return false;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
		if (!Main.hardMode)
			return;

		Tile tile = Main.tile[i, j];

		if (tile.TileFrameX != 72 || tile.TileFrameY != 72)
			return;

		i -= 4; //Move to top-left of the tile
		j -= 4;

		Main.spriteBatch.End();

		Texture2D palette = ModContent.Request<Texture2D>("Snaker/Content/Blocks/SnakePortalTileShaderPalette").Value;
		Texture2D tex = ModContent.Request<Texture2D>("Snaker/Content/Blocks/SnakePortalTileMask").Value;

		EffectAssets.portalShader.Parameters["timer"].SetValue(MathF.Sin(Main.GlobalTimeWrappedHourly) * 4.2f);
		EffectAssets.portalShader.Parameters["maskSize"].SetValue(tex.Size());
		EffectAssets.portalShader.Parameters["texSize"].SetValue(palette.Size());
		EffectAssets.portalShader.Parameters["palette"].SetValue(palette);
		EffectAssets.portalShader.Parameters["paletteSize"].SetValue(4);
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, EffectAssets.portalShader, Main.GameViewMatrix.EffectMatrix);

		Color col = Color.White;
		Vector2 drawPos = TileHelper.TileCustomPosition(i, j);

		Main.spriteBatch.Draw(tex, drawPos, null, col, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
		Main.spriteBatch.Draw(tex, drawPos, null, col * 0.2f, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.EffectMatrix);
	}
}