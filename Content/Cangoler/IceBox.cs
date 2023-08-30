using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Snaker.Content.Cangoler;

internal class IceBox : ModItem
{
	public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<IceBoxTile>());
}

public class IceBoxTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.Origin = new Point16(0, 0);
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.RandomStyleRange = 2;
        TileObjectData.addTile(Type);

        DustType = DustID.Grass;

        AddMapEntry(new Color(43, 105, 155), CreateMapEntryName());
    }

    public override bool RightClick(int i, int j)
    {
        if (!NPC.AnyNPCs(ModContent.NPCType<IceMonster>()))
        {
            NPC.NewNPC(new EntitySource_TileInteraction(Main.LocalPlayer, i, j), i * 16, j * 16, ModContent.NPCType<IceMonster>(), 1);
            return true;
        }
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 2 : 6;
}