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
using Snaker.Content.World;
using Terraria.Localization;
using System.IO;

namespace Snaker.Common.EventSystem;

internal class SnakeArenaSystem : ModSystem
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

    public static bool Active => ModContent.GetInstance<SnakeArenaSystem>()._active;
    public static EventStage Wave => ModContent.GetInstance<SnakeArenaSystem>()._wave;

    public static float WaveProgress
    {
        get => ModContent.GetInstance<SnakeArenaSystem>()._waveProgress;
        internal set => ModContent.GetInstance<SnakeArenaSystem>()._waveProgress = value;
    }

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

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write((Half)_waveProgress);
        writer.Write((byte)_wave);
    }

    public override void NetReceive(BinaryReader reader)
    {
        _waveProgress = (float)reader.ReadHalf();
        _wave = (EventStage)reader.ReadByte();
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

            if (Main.netMode != NetmodeID.MultiplayerClient)
                ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Mods.Snaker.DevilishSnakeEventDefeat"), Color.MediumAquamarine);
        }

        if (Main.netMode == NetmodeID.Server)
        {
            var packet = Mod.GetPacket(1);
            packet.Write("EndSnakeEvent");
            packet.Send();
        }
    }

    private void SetSpawnChoices()
    {
        _spawnChoices = new(Main.rand);

        switch (_wave)
        {
            case EventStage.First:
                _spawnChoices.Add(NPCID.Hellbat, 1f);
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
                _spawnChoices.Add(ModContent.NPCType<PotatoBeeFireAnt>(), 0.2f);
                break;
            case EventStage.Fourth:
                _spawnChoices.Add(NPCID.Demon, 0.8f);
                _spawnChoices.Add(NPCID.RedDevil, 0.4f);
                _spawnChoices.Add(ModContent.NPCType<PotatoBeeFireAnt>(), 0.6f);
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

            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendData(MessageID.WorldData);
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

            for (int i = 0; i < Main.maxProjectiles; ++i)
            {
                Projectile proj = Main.projectile[i];

                if (proj.active && proj.hostile)
                {
                    proj.active = false;

                    ExplosionHelper.Fire(proj.position - proj.Size / 2f, 20, Main.rand.NextFloat(1.5f, 2.5f), (7f, 12f));
                    ExplosionHelper.Smoke(proj.GetSource_Death(), proj.position, 3, (2f, 4f));
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int x = (SubworldSystem.Current as SnakerSubworld).OpenRight * 16 + 1400;
                int y = SubworldSystem.Current.Height * 10;
                int spawn = NPC.NewNPC(new EntitySource_SpawnNPC("Event"), x, y, ModContent.NPCType<DevilishSnake>());
                Main.npc[spawn].GetGlobalNPC<InstancedEventNPC>().eventEnemy = true;
            }
        }
    }

    private void AnnounceWave()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        string spawns = "";

        if (_wave == EventStage.Boss)
            spawns = Lang.GetNPCName(ModContent.NPCType<DevilishSnake>()).Value + "  ";
        else
            foreach (var item in _spawnChoices.elements)
                spawns += Lang.GetNPCName(item.Item1) + ", ";

        string text = $"{spawns[..(spawns.Length - 2)]}";
        string chat = Language.GetText("Mods.Snaker.DevilishSnakeEventWave").WithFormatArgs(Language.GetTextValue("Mods.Snaker.EventWaves." + Wave), text).Value;
        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(chat), Color.IndianRed);
    }

    public override void PreUpdateInvasions()
    {
        if (SubworldSystem.Current is not SnakerSubworld)
            return;

        if (!Main.getGoodWorld)
        {
            Main.dayTime = true;
            Main.time = 26000;
        }
        else
        {
            Main.dayTime = false;
            Main.time = 16000;
        }

        if (Main.netMode == NetmodeID.Server) //Set server progress to boss's progress
            InterfaceLayerSystem.SetProgressToBossLife();

        int count = 0;

        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC npc = Main.npc[i];
            if ((npc.active && npc.type == ModContent.NPCType<EventNPCSpawner>()) 
                || (npc.CanBeChasedBy() && npc.TryGetGlobalNPC<InstancedEventNPC>(out var eventNPC) && eventNPC.eventEnemy))
                count++;
        }

        int chance = (int)Math.Pow(Math.Max(0, count + 3 - Main.CurrentFrameFlags.ActivePlayersCount), 2);
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

            int npc = NPC.NewNPC(new EntitySource_SpawnNPC("SnakerEvent"), (int)randomPosition.X, (int)randomPosition.Y, ModContent.NPCType<EventNPCSpawner>());
            Main.npc[npc].ai[1] = npcType;

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc);
        }
    }
}
