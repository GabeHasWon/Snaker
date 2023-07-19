using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Snaker.Content.World;

internal class SnakerGenSystem : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
    {
        int index = tasks.FindIndex(x => x.Name == "Settle Liquids Again");

        if (index != -1)
            tasks.Insert(index + 1, new PassLegacy("Snaker Pillar", SpawnPillar));
    }

    private void SpawnPillar(GenerationProgress progress, GameConfiguration configuration)
    {
        int x = WorldGen.genRand.Next(200, Main.maxTilesX / 3);

        if (WorldGen.genRand.NextBool(2))
            x = WorldGen.genRand.Next((int)(Main.maxTilesX / 1.5f), Main.maxTilesX - 200);

        Point16 size = Point16.Zero;
        StructureHelper.Generator.GetDimensions("Content/World/Structures/SnakeTower", ModContent.GetInstance<Snaker>(), ref size);
        StructureHelper.Generator.GenerateStructure("Content/World/Structures/SnakeTower", new Point16(x, Main.maxTilesY - 20 - size.Y), ModContent.GetInstance<Snaker>());
    }
}
