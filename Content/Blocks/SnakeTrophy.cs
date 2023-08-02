using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Snaker.Content.Blocks;

public class SnakeTrophy : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLavaDeath[Type] = true;

        TileID.Sets.FramesOnKillWall[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.StyleWrapLimit = 36;
		TileObjectData.addTile(Type);

		DustType = DustID.WoodFurniture;

		ModTranslation name = CreateMapEntryName();
		name.SetDefault("Devilish Snake Trophy");
		AddMapEntry(new Color(120, 85, 60), name);
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY) 
		=> Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 48, ModContent.ItemType<SnakeTrophyItem>());
}

public class SnakeTrophyItem : ModItem
{
	public override void SetStaticDefaults() => DisplayName.SetDefault("Devilish Snake Trophy");
	public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<SnakeTrophy>());
}
