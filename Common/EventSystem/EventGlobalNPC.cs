using Snaker.Content.Enemies;
using Snaker.Content.World;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Common.EventSystem;

internal class EventGlobalNPC : GlobalNPC
{
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if (SubworldLibrary.SubworldSystem.Current is SnakerSubworld)
            maxSpawns = 0;
    }
}

internal class InstancedEventNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    internal bool eventEnemy = false;

    public override bool SpecialOnKill(NPC npc)
    {
        if (eventEnemy && SnakeArenaSystem.Active)
        {
            float eventWeight = npc.type switch
            {
                NPCID.Hellbat => 0.02f,
                NPCID.Lavabat => 0.06f,
                NPCID.RedDevil => 0.2f,
                NPCID.Demon => 0.1f,
                NPCID.FireImp => 0.07f,
                _ => 0.05f //Covers PotatoBeeFireAnt
            };

            eventWeight *= SnakeArenaSystem.Wave switch
            {
                SnakeArenaSystem.EventStage.First => 1f,
                SnakeArenaSystem.EventStage.Second => 0.85f,
                SnakeArenaSystem.EventStage.Third => 0.6f,
                SnakeArenaSystem.EventStage.Fourth => 0.4f,
                _ => 1f
            };

            if (Main.masterMode) //Master mode sucks and I ain't making it better
                eventWeight *= 0.8f;

            //if (npc.type == ModContent.NPCType<DevilishSnake>())
                eventWeight = 1.1f;

            ModContent.GetInstance<SnakeArenaSystem>().ProgressEvent(eventWeight);
        }
        return eventEnemy && npc.type != ModContent.NPCType<DevilishSnake>();
    }

    public override void OnSpawn(NPC npc, IEntitySource source) => eventEnemy = source is EntitySource_SpawnNPC { Context: "SnakerEvent" };
}