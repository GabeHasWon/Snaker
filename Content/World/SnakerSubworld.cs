using Snaker.Common.EventSystem;
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
    public override int Height => 340;

    public int OpenLeft => 51;
    public int OpenRight => Width - 51;
    public int OpenTop => 111;
    public int OpenBottom => Height - 51;

    public int OpenCenter => (OpenTop + OpenBottom) / 2;

    public override List<GenPass> Tasks => new() { new PassLegacy("Box", BoxGen) };

    private void BoxGen(GenerationProgress progress, GameConfiguration configuration)
    {
        Main.worldSurface = Main.maxTilesY - 42;
        Main.rockLayer = Main.maxTilesY;
        
        for (int i = 0; i < Width; ++i)
        {
            bool randomPillar = WorldGen.genRand.NextBool(6);

            for (int j = 0; j < Height; ++j)
            {
                if (i < 50 || i > Width - 50 || j < 110 || j > Height - 50)
                {
                    WorldGen.PlaceTile(i, j, ModContent.TileType<SnakeBrickTile>(), true);

                    if (j == Height - 49 && WorldGen.genRand.NextBool(16))
                        WorldGen.PlaceObject(i - 1, j - 2, ModContent.TileType<SnakeDecor>(), true, WorldGen.genRand.Next(5));
                }
                else
                {
                    if (!WorldGen.genRand.NextBool(5) && (i % 15 <= 2 || randomPillar))
                        WorldGen.PlaceWall(i, j, WallID.ObsidianBrickUnsafe, true);

                    if (j > 60 && j % 30 == 0)
                    {
                        WorldGen.PlaceTile(i, j, TileID.Platforms, true, false, -1, 13);

                        if (WorldGen.genRand.NextBool(16))
                            WorldGen.PlaceObject(i - 1, j - 2, ModContent.TileType<SnakeDecor>(), true, WorldGen.genRand.Next(5));
                    }
                }
            }
        }
    }

    public override void OnEnter() => ModContent.GetInstance<SnakeArenaSystem>().StartEvent();
    public override void OnExit() => ModContent.GetInstance<SnakeArenaSystem>().EndEvent();
}
