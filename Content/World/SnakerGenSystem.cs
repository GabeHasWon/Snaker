using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Snaker.Content.World;

internal class SnakerGenSystem : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int index = tasks.FindIndex(x => x.Name == "Settle Liquids Again");

        if (index != -1)
            tasks.Insert(index + 1, new PassLegacy("Snaker Pillar", SpawnPillar));

        index = tasks.FindIndex(x => x.Name == "Wall Variety");

        if (index != -1)
            tasks.Insert(index, new PassLegacy("Snaker Cangoler", SpawnCangoler));
    }

    private void SpawnCangoler(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Building a resting place";

        int min = GenVars.snowMinX[0]; //Gets the bounds of the very top of the biome
        int max = GenVars.snowMaxX[Array.IndexOf(GenVars.snowMaxX, 0) - 1];
        int x = (min + max) / 2;
        int y = (int)(Main.worldSurface * 0.4f);

        while (!Main.tile[x, y].HasTile || (Main.tile[x, y].TileType != TileID.IceBlock && Main.tile[x, y].TileType != TileID.SnowBlock))
        {
            if (y > Main.maxTilesY - 200)
                return;

            y++;
        }

        y += 200;

        Point16 size = Point16.Zero;
        StructureHelper.Generator.GetDimensions("Content/World/Structures/Cangoler", ModContent.GetInstance<Snaker>(), ref size);
        StructureHelper.Generator.GenerateStructure("Content/World/Structures/Cangoler", new Point16(x, y), ModContent.GetInstance<Snaker>());
    }

    private void SpawnPillar(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Building a snake's tower";

        int x = WorldGen.genRand.Next(200, Main.maxTilesX / 3);

        if (WorldGen.genRand.NextBool(2))
            x = WorldGen.genRand.Next((int)(Main.maxTilesX / 1.5f), Main.maxTilesX - 200);

        Point16 size = Point16.Zero;
        StructureHelper.Generator.GetDimensions("Content/World/Structures/SnakeTower", ModContent.GetInstance<Snaker>(), ref size);
        StructureHelper.Generator.GenerateStructure("Content/World/Structures/SnakeTower", new Point16(x, Main.maxTilesY - 40 - size.Y), ModContent.GetInstance<Snaker>());
    }
}
