using Snaker.Common.EventSystem;
using Snaker.Content.Enemies;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Snaker;

public class Snaker : Mod
{
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        string type = reader.ReadString();

		if (type == "ProgressSnakeEvent")
		{
			if (Main.netMode == NetmodeID.Server)
			{
				var packet = GetPacket(2);
				packet.Write(type);
				packet.Write(reader.ReadHalf());
				packet.Send();
			}
			else
				SnakeArenaSystem.WaveProgress = (float)reader.ReadHalf();
		}
        else if (type == "EndSnakeEvent")
        {
			if (Main.netMode == NetmodeID.Server)
			{
				var packet = GetPacket(1);
				packet.Write(type);
				packet.Send();
			}
			else
				ModContent.GetInstance<SnakeArenaSystem>().EndEvent(true);
        }
    }

    public override void PostSetupContent()
    {
		if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist))
			return;

		bossChecklist.Call(
			"LogBoss",
			this,
			nameof(DevilishSnake),
			7.2f,
			() => SnakeArenaSystem.downedSnakeEvent,
			ModContent.NPCType<DevilishSnake>(),
			new Dictionary<string, object>()
			{
				["spawnInfo"] = Language.GetText("Mods.Snaker.DevilishSnakeSpawnInfo")
			}
		);
	}
}