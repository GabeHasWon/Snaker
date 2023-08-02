using Microsoft.Xna.Framework;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;
using Snaker.Content.World;

namespace Snaker.Content.Enemies;

public partial class DevilishSnake : ModNPC
{
    private static SnakerSubworld Subworld => SubworldSystem.Current as SnakerSubworld;

    public enum SnakeState : int 
    { 
        Intro = 0,
        MoveUpDown,
        Survival
    }

    public enum SnakeAttackState : int
    {
        Fireball,
        Mines,
        Potato
    }

    public SnakeState State
    {
        get => (SnakeState)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    public ref float Timer => ref NPC.ai[1];

    public SnakeAttackState AttackState
    {
        get => (SnakeAttackState)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }

    public ref float AttackTimer => ref NPC.ai[3];

    private Player Target => Main.player[NPC.target];

    private Vector2 _targetPosition = new();
    private bool _survivalDone = false;

    public override void AI()
    {
        switch (State)
        {
            case SnakeState.Intro:
                IntroBehaviour();
                break;
            case SnakeState.MoveUpDown:
                MoveUpDownBehaviour();
                break;
            case SnakeState.Survival:
                SurvivalStage();
                break;
            default:
                break;
        }
    }

    private void SurvivalStage()
    {
        NPC.hide = true;
        NPC.dontTakeDamage = true;

        Timer++;

        if (Timer % 5 == 0)
        {
            float x = Main.rand.NextFloat(Subworld.OpenLeft + 4, Subworld.OpenRight - 4) * 16;
            float y = Subworld.OpenTop - 4 * 16;
            var vel = new Vector2(0, Main.rand.NextFloat(5, 14f));

            int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(x, y), vel, ModContent.ProjectileType<SnakeMeteorPotato>(), 30, 3f, Main.myPlayer);
            Main.projectile[proj].frame = Main.rand.Next(3);
            Main.projectile[proj].scale = MathHelper.Lerp(vel.Y / 12f, 1f, 0.5f);
        }

        if (Timer >= 20 * 60)
        {
            State = SnakeState.MoveUpDown;
            AttackState = SnakeAttackState.Fireball;
            Timer = 0;
            NPC.hide = false;
            NPC.dontTakeDamage = false;

            _survivalDone = true;
        }
    }

    private void MoveUpDownBehaviour()
    {
        Timer++;
        AttackTimer++;

        if (!_survivalDone && NPC.life < NPC.lifeMax / 2)
        {
            State = SnakeState.Survival;
            Timer = 0;
            AttackTimer = 0;
            return;
        }

        //Positional stuff
        int x = SubworldSystem.Current.Width * 11;
        int y = (int)(SubworldSystem.Current.Height * 9.5f);

        _targetPosition = new Vector2(x + (MathF.Sin(Timer * 0.01f) * (15 * 16)), y + (MathF.Sin(Timer * 0.0075f) * (78 * 16)));

        NPC.Center = Vector2.Lerp(NPC.Center, _targetPosition, 0.04f);

        float finalRotation = NPC.Center.Y > Target.Center.Y ? NPC.AngleTo(Target.Center) + MathHelper.Pi : NPC.AngleTo(Target.Center) - MathHelper.Pi;
        finalRotation = MathHelper.Clamp(finalRotation, -MathHelper.PiOver2, MathHelper.PiOver2);

        NPC.rotation = MathHelper.Lerp(NPC.rotation, MathHelper.Lerp(finalRotation, 0, 0.5f), 0.1f);

        //Attack stuff
        int cutoff = AttackState switch
        {
            SnakeAttackState.Fireball => 160,
            SnakeAttackState.Mines => 240,
            SnakeAttackState.Potato => 320,
            _ => 300
        };

        if (!Main.expertMode)
            cutoff = (int)(cutoff * 1.25f);
        else
        {
            float mod = 0.25f;

            if (Main.masterMode)
                mod = 0.5f;

            float factor = NPC.life / (float)NPC.lifeMax * mod;
            cutoff = (int)(cutoff * factor);
        }
        SubAttack(cutoff);

        if (AttackTimer == cutoff)
        {
            AttackTimer = 0;
            AttackState++;

            if (AttackState > SnakeAttackState.Potato)
                AttackState = SnakeAttackState.Fireball;
        }
    }

    private void SubAttack(int cutoff)
    {
        switch (AttackState)
        {
            case SnakeAttackState.Fireball:
                NPC.TargetClosest();

                if (AttackTimer > cutoff - 60 && AttackTimer % 20 == 0)
                {
                    var vel = NPC.DirectionTo(Target.Center).RotatedByRandom(0.5f) * 7;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<SnakeFireball>(), 60, 3f, Main.myPlayer);
                }
                break;
            case SnakeAttackState.Mines:
                if (AttackTimer < 20) //slow down the boss to sell the effort of mines
                    Timer -= 0.8f * (1 - (AttackTimer / 20f)) + 0.2f;
                else if (AttackTimer >= 20 && AttackTimer <= cutoff - 20)
                    Timer--;
                if (AttackTimer > cutoff - 20)
                    Timer -= 0.8f * ((cutoff - AttackTimer) / 20f) + 0.2f;

                if (AttackTimer % 12 == 0)
                {
                    float x = Main.rand.NextFloat(Subworld.OpenLeft + 16, Subworld.OpenRight - 16) * 16;
                    float y = Main.rand.NextFloat(Subworld.OpenTop + 4, Subworld.OpenBottom - 4) * 16;
                    var vel = GetArcVel(NPC.Center, new Vector2(x, y), SnakeMine.Gravity, maxXvel: 14);

                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<SnakeMine>(), 60, 3f, Main.myPlayer);
                    Main.projectile[proj].ai[0] = y;
                    Main.projectile[proj].frame = Main.rand.Next(3);
                }
                break;
            case SnakeAttackState.Potato:
                if (AttackTimer == cutoff / 2)
                {
                    NPC.TargetClosest();

                    float x = Main.rand.NextFloat(Subworld.OpenLeft + 4, Subworld.OpenLeft + 8) * 16;
                    float y = Main.rand.NextFloat(Subworld.OpenTop + 10, Subworld.OpenCenter) * 16;
                    var vel = GetArcVel(NPC.Center, new Vector2(x, y), SnakePotato.Gravity, maxXvel: 8f);

                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<SnakePotato>(), 30, 3f, Main.myPlayer);
                    Main.projectile[proj].frame = Main.rand.Next(3);
                }
                break;
            default:
                break;
        }
    }

    private void IntroBehaviour()
    {
        Timer++;

        if (Timer < 120)
        {
            PunchCameraModifier modifier = new(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), (Timer / 120f) * 7f, 3f, 2, 4000, "DevilishSnake");
            Main.instance.CameraModifiers.Add(modifier);

            NPC.velocity.X = -14;
        }
        else if (Timer == 120)
        {
            PunchCameraModifier modifier = new(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 25, 6f, 20, 7000, "DevilishSnake");
            Main.instance.CameraModifiers.Add(modifier);
        }
        else if (Timer < 200)
            NPC.velocity.X *= 0.8f;
        else if (Timer < 260)
        {
            PunchCameraModifier modifier = new(NPC.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 10f, 3f, 2, 5000, "DevilishSnake");
            Main.instance.CameraModifiers.Add(modifier);
            NPC.velocity.X *= 0.8f;
        }
        else if (Timer == 260)
        {
            NPC.dontTakeDamage = false;

            Timer = 0;
            State = SnakeState.MoveUpDown;
            AttackState = SnakeAttackState.Fireball;
        }
    }

    //Adapted from Spirit
    public static Vector2 GetArcVel(Vector2 startingPos, Vector2 targetPos, float gravity, float? minArcHeight = null, float? maxArcHeight = null, float? maxXvel = null, float? heightAboveTarget = null, float downwardsYVelMult = 1f)
    {
        Vector2 travelDistance = targetPos - startingPos;
        float maxHeight = travelDistance.Y - (heightAboveTarget ?? 0);

        if (minArcHeight != null)
            maxHeight = Math.Min(maxHeight, -(float)minArcHeight);

        if (maxArcHeight != null)
            maxHeight = Math.Max(maxHeight, -(float)maxArcHeight);

        float travelTime;
        float neededYVel;

        if (maxHeight <= 0)
        {
            neededYVel = -(float)Math.Sqrt(-2 * gravity * maxHeight);
            travelTime = (float)Math.Sqrt(-2 * maxHeight / gravity) + (float)Math.Sqrt(2 * Math.Max(travelDistance.Y - maxHeight, 0) / gravity); //time up, then time down
        }
        else
        {
            neededYVel = Vector2.Normalize(travelDistance).Y * downwardsYVelMult;
            travelTime = (-neededYVel + (float)Math.Sqrt(Math.Pow(neededYVel, 2) - (4 * -travelDistance.Y * gravity / 2))) / (gravity); //time down
        }

        if (maxXvel != null)
            return new Vector2(MathHelper.Clamp(travelDistance.X / travelTime, -(float)maxXvel, (float)maxXvel), neededYVel);
        return new Vector2(travelDistance.X / travelTime, neededYVel);
    }
}
