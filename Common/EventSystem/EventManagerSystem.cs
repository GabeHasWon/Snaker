using Microsoft.Xna.Framework;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.DataStructures;
using Terraria.Chat;

namespace Snaker.Common.EventSystem;

internal class EventManagerSystem : ModSystem
{
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

    public void StartEvent()
    {
        _wave = EventStage.First;
        _waveProgress = 0;
        _active = true;

        SetSpawnChoices();
        AnnounceWave();
    }

    public void EndEvent() => _active = false;

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
        _waveProgress += weight;prog

        if (_waveProgress > 1)
        {
            _waveProgress = 0;
            _wave++;

            SetSpawnChoices();

            if (_wave <= EventStage.Boss)
                AnnounceWave();
            else
                EndEvent();
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
