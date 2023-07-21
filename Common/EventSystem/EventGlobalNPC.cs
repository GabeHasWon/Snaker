using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Common.EventSystem;

internal class EventGlobalNPC : GlobalNPC
{
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if (EventManagerSystem.Active)
            maxSpawns = 0;
    }
}

internal class InstancedEventNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    internal bool eventEnemy = false;

    public override bool SpecialOnKill(NPC npc)
    {
        if (eventEnemy)
        {
            float eventWeight = npc.type switch
            {
                NPCID.Lavabat => 0.0025f,
                NPCID.RedDevil => 0.01f,
                NPCID.Demon => 0.005f,
                NPCID.FireImp => 0.003f,
                _ => 0.05f
            };

            eventWeight *= 15;
            ModContent.GetInstance<EventManagerSystem>().ProgressEvent(eventWeight);
        }
        return eventEnemy;
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        eventEnemy = source is EntitySource_SpawnNPC { Context: "SnakerEvent" };
    }
}