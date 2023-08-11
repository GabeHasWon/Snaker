using Microsoft.Xna.Framework;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.DataStructures;
using Terraria.Chat;
using Snaker.Content.Enemies;
using Snaker.Content;
using Terraria.ModLoader.IO;

namespace Snaker.Common.EventSystem;

internal class EventManagerSystem : ModSystem
{
    public static bool downedSnakeEvent = false;

    public enum EventStage
    {
        First,
        Second,
        Third,
        Fourth,
        Boss
    }

    public static bool Active => ModContent.GetInstance<EventManagerSystem>()._active;
    public static EventStage Wave => ModContent.GetInstance<EventManagerSystem>()._wave;
    public static float WaveProgress => ModContent.GetInstance<EventManagerSystem>()._waveProgress;


    private EventStage _wave;
    private bool _active;
    private float _waveProgress;

    private WeightedRandom<int> _spawnChoices = new();

    public override void ClearWorld()
    {
        if (SubworldSystem.Current is not null) 
            downedSnakeEvent = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (downedSnakeEvent)
            tag.Add(nameof(downedSnakeEvent), downedSnakeEvent);
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.ContainsKey(nameof(downedSnakeEvent)))
            downedSnakeEvent = true;
    }

    public void StartEvent()
    {
        _wave = EventStage.First;
        _waveProgress = 0;
        _active = true;

        SetSpawnChoices();
        AnnounceWave();
    }

    public void EndEvent(bool announce = false)
    {
        _active = false;

        if (announce)
        {
            downedSnakeEvent = true;
            ChatHelper.BroadcastChatMessage(Terraria.Localization.NetworkText.FromKey("Mods.Snaker.DevilishSnakeEventDefeat"), Color.White);
        }
    }

    private void SetSpawnChoices()
    {
        _spawnChoices = new(Main.rand);

        switch (_wave)
        {
            case EventStage.First:
                _spawnChoices.Add(NPCID.FireImp, 0.6f);
                _spawnChoices.Add(NPCID.Lavabat, 0.8f);
                break;
            case EventStage.Second:
                _spawnChoices.Add(NPCID.FireImp, 0.6f);
                _spawnChoices.Add(NPCID.Lavabat, 0.8f);
                _spawnChoices.Add(NPCID.Demon, 0.9f);
                break;
            case EventStage.Third:
                _spawnChoices.Add(NPCID.FireImp, 0.5f);
                _spawnChoices.Add(NPCID.Lavabat, 0.5f);
                _spawnChoices.Add(NPCID.Demon, 0.8f);
                _spawnChoices.Add(NPCID.RedDevil, 0.1f);
                break;
            case EventStage.Fourth:
                _spawnChoices.Add(NPCID.Demon, 0.8f);
                _spawnChoices.Add(NPCID.RedDevil, 0.2f);
                break;
            default:
                break;
        }
    }

    public void ProgressEvent(float weight)
    {
        _waveProgress += weight;

        if (_waveProgress > 1)
        {
            _waveProgress = 0;
            _wave++;

            SetSpawnChoices();

            if (_wave <= EventStage.Boss)
                AnnounceWave();
            else
                EndEvent(true);
        }

        if (_wave == EventStage.Boss)
        {
            for (int i = 0; i < Main.maxNPCs; ++i) //Despawn every add before boss
            {
                NPC npc = Main.npc[i];

                if (npc.active && npc.GetGlobalNPC<InstancedEventNPC>().eventEnemy)
                {
                    npc.active = false;

                    ExplosionHelper.Fire(npc.position - npc.Size / 2f, 40, Main.rand.NextFloat(1.5f, 2.5f), (7f, 12f));
                    ExplosionHelper.Smoke(npc.GetSource_Death(), npc.position, 8, (2f, 4f));
                }
            }

            int x = SubworldSystem.Current.Width * 16 + (14 * 40);
            int y = SubworldSystem.Current.Height * 10;
            int spawn = NPC.NewNPC(new EntitySource_SpawnNPC("Event"), x, y, ModContent.NPCType<DevilishSnake>());
            Main.npc[spawn].GetGlobalNPC<InstancedEventNPC>().eventEnemy = true;
        }
    }

    private void AnnounceWave()
    {
        string spawns = "";

        if (_wave == EventStage.Boss)
            spawns = "Devilish Snake  ";
        else
            foreach (var item in _spawnChoices.elements)
                spawns += Lang.GetNPCName(item.Item1) + ", ";

        string text = $"{_wave} Wave: {spawns[..(spawns.Length - 2)]}";
        ChatHelper.BroadcastChatMessage(Terraria.Localization.NetworkText.FromLiteral(text), Color.White);
    }

    public override void PreUpdateNPCs()
    {
        int count = 0;

        if (_wave == EventStage.Boss) //Set progress to boss's health
        {
            int whoAmI = NPC.FindFirstNPC(ModContent.NPCType<DevilishSnake>());

            if (whoAmI != -1) 
            {
                NPC npc = Main.npc[whoAmI];

                if (npc.active && npc.life > 0)
                    _waveProgress = 1 - (npc.life / (float)npc.lifeMax);
            }
        }

        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC npc = Main.npc[i];
            if ((npc.active && npc.type == ModContent.NPCType<EventNPCSpawner>()) || (npc.CanBeChasedBy() && npc.GetGlobalNPC<InstancedEventNPC>().eventEnemy))
                count++;
        }

        int chance = (int)Math.Pow(count + 3, 2);
        int maxSpawns = 20 + (int)_wave * 5;
        if (_active && _wave < EventStage.Boss && !Main.gamePaused && Main.rand.NextBool(chance) && count < maxSpawns)
        {
            int npcType = _spawnChoices;
            Vector2 randomPosition;
            NPC sample = ContentSamples.NpcsByNetId[npcType];
            Subworld world = SubworldSystem.Current;
            
            const int Offset = 44 * 16;

            do
            {
                randomPosition = new Vector2(Main.rand.Next(Offset, world.Width * 16 - Offset), Main.rand.Next(Offset, world.Height * 16 - Offset));
            } while (Collision.SolidCollision(randomPosition, sample.width, sample.height));

            NPC npc = NPC.NewNPCDirect(new EntitySource_SpawnNPC("SnakerEvent"), (int)randomPosition.X, (int)randomPosition.Y, ModContent.NPCType<EventNPCSpawner>());
            npc.ai[1] = npcType;
        }
    }
}
