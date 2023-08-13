using Snaker.Common.EventSystem;
using Snaker.Content.Enemies;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Snaker;

public class Snaker : Mod
{
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
				// Other optional arguments as needed...
			}
		);
	}
}