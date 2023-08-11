using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Content.Blocks;

internal class SnakeBrick : ModItem
{
	public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<SnakeBrickTile>());
}

internal class SnakeBrickTile : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileBrick[Type] = true;

		AddMapEntry(new Color(70, 71, 65));

		MinPick = 100;
		DustType = -1;
		HitSound = SoundID.Tink;
	}
}