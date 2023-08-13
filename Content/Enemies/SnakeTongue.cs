using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Snaker.Content.Enemies;

internal class SnakeTongue : ModProjectile
{
    private const int MaxStuckTime = 300;

    public Vector2 OriginLocation => GetOriginLocation(Parent);

    private float _originalVelMagnitude;

    private ref float Timer => ref Projectile.ai[0];

    private int ParentWhoAmI
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    private NPC Parent => Main.npc[ParentWhoAmI];

    private int StuckPlayerWhoAmI
    {
        get => (int)Projectile.ai[2] - 1;
        set => Projectile.ai[2] = value + 1;
    }

    private bool HasStuckPlayer => StuckPlayerWhoAmI != -1;
    private Player StuckPlayer => Main.player[StuckPlayerWhoAmI];

    private int _stuckTimer = 0;

    public static Vector2 GetOriginLocation(NPC npc) => npc.Center + new Vector2(0, 220).RotatedBy(npc.rotation + MathHelper.PiOver2 - 0.3f);

    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;

    public override void SetDefaults()
    {
        Projectile.width = 60;
        Projectile.height = 60;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 1200;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.extraUpdates = 1;
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (source is EntitySource_Parent parent && parent.Entity is NPC npc)
            ParentWhoAmI = npc.whoAmI;

        _originalVelMagnitude = Projectile.velocity.Length();
        Timer = 0;
    }

    public override void AI()
    {
        Timer++;

        Projectile.rotation = Projectile.AngleTo(OriginLocation) - MathHelper.PiOver2;

        if (!Parent.active)
            Projectile.Kill();

        if (!HasStuckPlayer)
        {
            if (Timer > 1400 / _originalVelMagnitude)
            {
                float length = Projectile.velocity.LengthSquared();

                if (length <= _originalVelMagnitude * _originalVelMagnitude)
                    Projectile.velocity += Projectile.DirectionTo(OriginLocation);
                else if (length > _originalVelMagnitude * _originalVelMagnitude)
                    Projectile.velocity = Projectile.DirectionTo(OriginLocation) * _originalVelMagnitude;

                if (Projectile.DistanceSQ(OriginLocation) < _originalVelMagnitude * _originalVelMagnitude * 3)
                    Projectile.Kill();
            }
        }
        else
        {
            Projectile.Center = StuckPlayer.Center;
            Projectile.velocity = Vector2.Zero;
            _stuckTimer++;

            if (_stuckTimer > MaxStuckTime)
                Projectile.Kill();
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (!HasStuckPlayer)
        {
            StuckPlayerWhoAmI = target.whoAmI;
            Projectile.netUpdate = true;
        }
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write((short)_stuckTimer);
    public override void ReceiveExtraAI(BinaryReader reader) => _stuckTimer = reader.ReadInt16();

    public override bool PreDraw(ref Color lightColor)
    {
        const float TongueFadeDistance = 50;
        const int TongueBodyHeight = 14;

        Texture2D tex = TextureAssets.Projectile[Type].Value;
        float distance = Vector2.Distance(Projectile.position, OriginLocation);
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Vector2 norm = Projectile.DirectionTo(OriginLocation);

        for (int i = TongueBodyHeight * 2; i < distance - TongueBodyHeight; i += TongueBodyHeight)
        {
            var adjPos = drawPos + (norm * i);
            var realPos = adjPos + Main.screenPosition;
            var col = Lighting.GetColor(realPos.ToTileCoordinates());

            float distanceToAnchor = Vector2.DistanceSquared(OriginLocation, realPos);
            if (distanceToAnchor < TongueFadeDistance * TongueFadeDistance)
                col *= distanceToAnchor / (TongueFadeDistance * TongueFadeDistance);

            Main.EntitySpriteDraw(tex, adjPos, new Rectangle(24, 46, 14, 14), col, Projectile.rotation, new Vector2(7), 1f, SpriteEffects.None, 0);
        }

        Color endColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
        float endDistanceToAnchor = Vector2.DistanceSquared(OriginLocation, Projectile.Center);
        if (endDistanceToAnchor < TongueFadeDistance * TongueFadeDistance)
            endColor *= endDistanceToAnchor / (TongueFadeDistance * TongueFadeDistance);

        Main.EntitySpriteDraw(tex, drawPos, new Rectangle(0, 0, 62, 44), endColor, Projectile.rotation, new Vector2(31, 22), 1f, SpriteEffects.None, 0);
        return false;
    }
}
