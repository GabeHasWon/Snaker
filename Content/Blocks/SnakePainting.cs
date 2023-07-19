using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Snaker.Content.Blocks;

internal class SnakePainting : ModItem
{
    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<SnakePaintingTile>());
}

public class SnakePaintingTile : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.Width = 6;
		TileObjectData.newTile.Height = 6;
		TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16 };
		TileObjectData.newTile.AnchorBottom = default;
		TileObjectData.newTile.AnchorTop = default;
		TileObjectData.newTile.AnchorWall = true;
		TileObjectData.addTile(Type);

		DustType -= 1;

		ModTranslation name = CreateMapEntryName();
		name.SetDefault("Painting");
		AddMapEntry(new Color(99, 68, 51), name);
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
	public override void KillMultiTile(int i, int j, int frameX, int frameY) => 
		Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 96, 96, ModContent.ItemType<SnakePainting>());
}