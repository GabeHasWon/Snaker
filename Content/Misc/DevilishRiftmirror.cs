using Snaker.Content.Blocks;
using Snaker.Content.World;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Content.Misc;

public class DevilishRiftmirror : ModItem
{
	public override void SetDefaults()
	{
		Item.width = 26;
		Item.height = 32;
		Item.rare = ItemRarityID.Cyan;
		Item.maxStack = 1;
        Item.useTime = Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
	}

    public override bool? UseItem(Player player)
    {
        if (SubworldSystem.Current is null)
            SubworldSystem.Enter<SnakerSubworld>();
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.ObsidianBrick, 50)
            .AddIngredient(ItemID.SoulofNight, 1)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ModContent.ItemType<SnakeBrick>(), 5)
            .AddIngredient(ItemID.SoulofNight, 1)
            .AddTile(TileID.Anvils)
            .Register();
    }
}