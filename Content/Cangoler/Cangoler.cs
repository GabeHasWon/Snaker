using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Content.Cangoler;

internal class CangolerItem : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 38;
        Item.height = 42;
        Item.knockBack = 2;
        Item.autoReuse = false;
        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item8;
        Item.DamageType = DamageClass.Summon;
        Item.damage = 30;
        Item.crit = 5;
        Item.knockBack = 6;
        Item.mana = 40;
        Item.rare = ItemRarityID.Purple;
        Item.value = Item.buyPrice(gold: 5);
        Item.shootSpeed = 8f;
        Item.shoot = ModContent.ProjectileType<CangolerMinion>();
        Item.channel = false;
        Item.noMelee = true;
        Item.buffTime = 4;
        Item.buffType = ModContent.BuffType<CangolerBuff>();
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        player.AddBuff(ModContent.BuffType<CangolerBuff>(), 4);
        return true;
    }
}

internal class CangolerMinion : ModProjectile
{
    const int MaxRange = 800;
    const float ChargeTime = 120;

    private enum CangolerState
    {
        Idle,
        Charging,
    }

    protected Player Owner => Main.player[Projectile.owner];

    private Vector2 IdleLocation => Owner.Center - new Vector2((40 + 40 * Projectile.minionPos) * Owner.direction, 
        40 + MathF.Sin((Timer * 0.02f) + (Projectile.minionPos * MathHelper.PiOver4)) * 14);

    private CangolerState State
    {
        get => (CangolerState)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }

    private ref float Timer => ref Projectile.ai[1];

    private int TargetWhoAmI
    {
        get => Projectile.OwnerMinionAttackTargetNPC is not null ? Projectile.OwnerMinionAttackTargetNPC.whoAmI : (int)Projectile.ai[2];
        set => Projectile.ai[2] = value;
    }

    private NPC Target => Projectile.OwnerMinionAttackTargetNPC ?? Main.npc[TargetWhoAmI];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.MinionSacrificable[Type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 26;
        Projectile.height = 28;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.hostile = false;
        Projectile.timeLeft = 4;
        Projectile.penetrate = -1;
        Projectile.minion = true;
        Projectile.minionSlots = 1f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.localNPCHitCooldown = 20;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override bool? CanHitNPC(NPC target) => State == CangolerState.Charging && Timer >= ChargeTime ? null : false;

    public override void AI()
    {
        if (!Owner.HasBuff<CangolerBuff>())
            Projectile.Kill();

        Owner.AddBuff(ModContent.BuffType<CangolerBuff>(), 4);

        Projectile.timeLeft++;
        Projectile.rotation = Projectile.velocity.X *= 0.08f;

        if (State == CangolerState.Idle)
        {
            float distance = Math.Min(20, Projectile.Distance(IdleLocation)) / 2;
            Projectile.velocity = (IdleLocation - Projectile.Center).SafeNormalize(Vector2.Zero) * distance;
            Projectile.direction = Projectile.spriteDirection = Owner.Center.X <= Projectile.Center.X ? 1 : -1;

            ScanNearbyEnemies();

            Timer++;

            if (State == CangolerState.Charging)
                Timer = 0;
        }
        else
        {
            Timer++;

            if (Math.Abs(Projectile.velocity.X) > 0.001f)
                Projectile.direction = Projectile.spriteDirection = -Math.Sign(Projectile.velocity.X);
            
            if (Timer < ChargeTime)
            {
                Projectile.velocity *= 0.98f;

                float factor = Timer / ChargeTime;
                Projectile.position += Main.rand.NextVector2CircularEdge(3 * factor, 3 * factor);
            }
            else if (Timer < ChargeTime * 6)
            {
                float distance = Math.Min(20, Projectile.Distance(Target.Center)) / 2;
                Projectile.velocity = (Target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * distance;

                if (!Target.CanBeChasedBy() || Target.DistanceSQ(Projectile.Center) > MaxRange * MaxRange)
                {
                    ScanNearbyEnemies();

                    if (!Target.CanBeChasedBy() || Target.DistanceSQ(Projectile.Center) > MaxRange * MaxRange) //Rescan enemies, if still no valid enemy reset
                    {
                        State = CangolerState.Idle;
                        return;
                    }
                    else
                        Timer = 0;
                }
            }
            else
            {
                ScanNearbyEnemies();

                if (!Target.CanBeChasedBy() || Target.DistanceSQ(Projectile.Center) > MaxRange * MaxRange)
                {
                    Timer = 0;
                    State = CangolerState.Idle;
                }
                else
                    Timer = 0;
            }
        }
    }

    private void ScanNearbyEnemies()
    {
        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC npc = Main.npc[i];

            if (npc.CanBeChasedBy())
            {
                float distanceToNpc = npc.DistanceSQ(Projectile.Center);

                if (distanceToNpc < MaxRange * MaxRange && Collision.CanHit(Projectile, npc))
                {
                    State = CangolerState.Charging;
                    TargetWhoAmI = i;
                }
            }
        }
    }
}

public class CangolerBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) => player.buffTime[buffIndex] = 1800;
}