using Snaker.Content.Blocks;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Snaker.Content.World;

internal class SnakerSubworld : Subworld
{
    public override int Width => 240;
    public override int Height => 280;

    public override List<GenPass> Tasks => new()
    {
        new PassLegacy("Box", BoxGen),
    };

    private void BoxGen(GenerationProgress progress, GameConfiguration configuration)
    {
        Main.worldSurface = Main.maxTilesY - 42;
        Main.rockLayer = Main.maxTilesY;

        for (int i = 0; i < Width; ++i)
        {
            for (int j = 0; j < Height; ++j)
            {
                if (i < 50 || i > Width - 50 || j < 50 || j > Height - 50)
                    WorldGen.PlaceTile(i, j, ModContent.TileType<SnakeBrickTile>(), true);
                else
                {
                    if (!WorldGen.genRand.NextBool(5) && i % 15 <= 2)
                        WorldGen.PlaceWall(i, j, WallID.ObsidianBrickUnsafe);

                    if (j > 60 && j % 30 == 0)
                        WorldGen.PlaceTile(i, j, TileID.Platforms, true, false, -1, 13);
                }
            }
        }
    }
}
