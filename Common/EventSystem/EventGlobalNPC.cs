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
        if (eventEnemy && EventManagerSystem.Active)
        {
            float eventWeight = npc.type switch
            {
                NPCID.Lavabat => 0.0025f,
                NPCID.RedDevil => 0.01f,
                NPCID.Demon => 0.005f,
                NPCID.FireImp => 0.003f,
                _ => 0.05f
            };

            eventWeight *= 200;
            eventWeight *= EventManagerSystem.Wave switch
            {
                EventManagerSystem.EventStage.First => 1f,
                EventManagerSystem.EventStage.Second => 0.9f,
                EventManagerSystem.EventStage.Third => 0.8f,
                EventManagerSystem.EventStage.Fourth => 0.6f,
                _ => 0.4f
            };

            if (npc.type == ModContent.NPCType<DevilishSnake>())
                eventWeight = 1.1f;

            ModContent.GetInstance<EventManagerSystem>().ProgressEvent(eventWeight);
        }
        return eventEnemy && npc.type != ModContent.NPCType<DevilishSnake>();
    }

    public override void OnSpawn(NPC npc, IEntitySource source) => eventEnemy = source is EntitySource_SpawnNPC { Context: "SnakerEvent" };
}