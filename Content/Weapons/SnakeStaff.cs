using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Content.Weapons;

internal class SnakeStaff : ModItem
{
    // public override void SetStaticDefaults() => Tooltip.SetDefault("Summons little snakes to fight for you\nSnakes do rapid melee damage, and shoot fireballs at range");

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 36;
        Item.knockBack = 2;
        Item.autoReuse = false;
        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item8;
        Item.DamageType = DamageClass.Summon;
        Item.damage = 40;
        Item.crit = 5;
        Item.knockBack = 6;
        Item.rare = ItemRarityID.Purple;
        Item.value = Item.buyPrice(gold: 5);
        Item.shootSpeed = 8f;
        Item.shoot = ModContent.ProjectileType<SnakeSummon>();
        Item.channel = false;
        Item.noMelee = true;
        Item.buffTime = 4;
        Item.buffType = ModContent.BuffType<SnakeSummonBuff>();
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        player.AddBuff(ModContent.BuffType<SnakeSummonBuff>(), 2);
        return true;
    }
}

internal class SnakeSummon : ModProjectile
{
    const int MaxRangedRange = 1000;
    const int MaxMeleeRange = 400;

    private static Asset<Texture2D> _bodyTex;

    private enum SnakeState
    {
        Idle,
        AttackingRange,
        AttackingMelee
    }

    protected Player Owner => Main.player[Projectile.owner];

    private SnakeState State
    {
        get => (SnakeState)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }

    private ref float Timer => ref Projectile.ai[1];

    private ref float Sine => ref Projectile.localAI[0]; //Unsynced but deterministic so idc

    private int TargetWhoAmI
    {
        get => (int)Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    private NPC Target => Main.npc[TargetWhoAmI];

    private List<SnakeBody> bodies = new();
    private short _minionNumber = 0;

    public override void SetStaticDefaults() => _bodyTex = ModContent.Request<Texture2D>(Texture + "Body");
    public override void Unload() => _bodyTex = null;

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
        Projectile.localNPCHitCooldown = 10;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write(_minionNumber);
    public override void ReceiveExtraAI(BinaryReader reader) => _minionNumber = reader.ReadInt16();

    public override void OnSpawn(IEntitySource source)
    {
        const int BodyCount = 10;

        for (int i = 0; i < BodyCount; ++i)
            bodies.Add(new SnakeBody(Projectile.Center, i == BodyCount - 1));

        _minionNumber = 0;
        for (int i = 0; i < Main.maxProjectiles; ++i)
        {
            if (Main.projectile[i].type == Type && Main.projectile[i].owner == Projectile.owner)
                _minionNumber++;

            if (i == Projectile.whoAmI)
                break;
        }
    }

    public override void AI()
    {
        if (!Owner.HasBuff<SnakeSummonBuff>())
            Projectile.Kill();

        Owner.AddBuff(ModContent.BuffType<SnakeSummonBuff>(), 2);

        Projectile.timeLeft = 2;
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        for (int i = 0; i < bodies.Count; ++i)
            bodies[i].Update(i == 0 ? Projectile.Center : bodies[i - 1].position, 16);

        Timer++;

        if (State == SnakeState.Idle)
        {
            SnakeyMovement(Owner.Center - new Vector2(0, 60 + (_minionNumber * 50)).RotatedBy(Timer * 0.05f));
            ScanNearbyEnemies();

            if (Projectile.DistanceSQ(Owner.Center) > 1600 * 1600)
            {
                Projectile.Center = Owner.Center;
                ExplosionHelper.Fire(Projectile.Center, 20, Main.rand.NextFloat(1, 1.5f), (3, 5f));
            }
        }
        else if (State == SnakeState.AttackingRange)
        {
            SnakeyMovement(Owner.Center - new Vector2(0, 100 + (_minionNumber * 50)).RotatedBy(Timer * 0.05f), 1.2f);

            if (!Target.CanBeChasedBy() || Target.DistanceSQ(Projectile.Center) > MaxRangedRange * MaxRangedRange)
            {
                ScanNearbyEnemies();

                if (!Target.CanBeChasedBy() || Target.DistanceSQ(Projectile.Center) > MaxRangedRange * MaxRangedRange) //Rescan enemies, if still no valid enemy reset
                {
                    State = SnakeState.Idle;
                    return;
                }
            }

            if (Target.DistanceSQ(Projectile.Center) < MaxMeleeRange * MaxMeleeRange)
            {
                State = SnakeState.AttackingMelee;
                return;
            }

            if (Timer % 50 == 0 && Main.myPlayer == Projectile.owner)
            {
                Vector2 vel = Projectile.DirectionTo(Target.Center + (Target.velocity * 8)) * 12;
                var dmg = (int)Owner.GetDamage(DamageClass.Summon).ApplyTo(40);
                int type = ModContent.ProjectileType<SnakeSummonFireball>();
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, vel, type, dmg, 0f, Projectile.owner);
            }
        }
        else if (State == SnakeState.AttackingMelee)
        {
            SnakeyMovement(Target.Center, 2f);

            Projectile.damage = (int)Owner.GetDamage(DamageClass.Summon).ApplyTo(20);

            if (!Target.CanBeChasedBy() || Target.DistanceSQ(Projectile.Center) > MaxMeleeRange * MaxMeleeRange)
            {
                ScanNearbyEnemies();

                if (!Target.CanBeChasedBy() || Target.DistanceSQ(Projectile.Center) > MaxMeleeRange * MaxMeleeRange) //Rescan enemies, if still no valid enemy reset
                {
                    State = SnakeState.Idle;
                    return;
                }
            }
        }
    }

    private void ScanNearbyEnemies()
    {
        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC npc = Main.npc[i];

            if (npc.CanBeChasedBy() && npc.DistanceSQ(Projectile.Center) < MaxRangedRange * MaxRangedRange && Collision.CanHit(Projectile, npc))
            {
                State = SnakeState.AttackingRange;
                TargetWhoAmI = i;
                return;
            }
        }
    }

    private void SnakeyMovement(Vector2 target, float speedMod = 1f)
    {
        float maxSpeed = 6 * speedMod;

        Sine = MathF.Sin(Timer * 0.15f) * MathHelper.PiOver2 * 0.2f;
        Projectile.velocity += (target - Projectile.Center).SafeNormalize(Vector2.Zero).RotatedBy(Sine) * 0.8f * speedMod;

        if (Projectile.velocity.LengthSquared() > maxSpeed * maxSpeed)
            Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        foreach (var item in bodies)
            item.Draw();
        return true;
    }

    internal class SnakeBody
    {
        public Vector2 position;
        public bool tail;
        public float rotation;

        public SnakeBody(Vector2 pos, bool tail)
        {
            position = pos;
            this.tail = tail;
        }

        public void Update(Vector2 parentPosition, int parentHeight)
        {
            if (Vector2.DistanceSquared(parentPosition, position) > parentHeight * parentHeight)
            {
                Vector2 dir = Vector2.Normalize(parentPosition - position);
                position += dir * (Vector2.Distance(parentPosition, position) - parentHeight);
                rotation = dir.ToRotation() + MathHelper.PiOver2;
            }
        }

        public void Draw()
        {
            var src = new Rectangle(0, 28 * tail.ToInt(), 26, 16);
            var color = Lighting.GetColor(position.ToTileCoordinates());
            Main.EntitySpriteDraw(_bodyTex.Value, position - Main.screenPosition, src, color, rotation, src.Size() / 2f, 1f, SpriteEffects.None, 0);
        }
    }
}

public class SnakeSummonFireball : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.timeLeft = 4 * 60;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.alpha = 155;

        AIType = 0;
    }

    public override void AI()
    {
        Vector2 velocity = new Vector2(0, -Main.rand.NextFloat(0.2f, 1f)).RotatedByRandom(MathHelper.TwoPi);
        Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0, 0, Scale: Main.rand.NextFloat(1f, 2f)).velocity = velocity;

        if (Projectile.wet)
            Projectile.Kill();
    }
}

public class SnakeSummonBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("Devilish Snakeling");
        // Description.SetDefault("'A small, frail-looking devil snake follows you'");
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) => player.buffTime[buffIndex] = 18000;
}