using Terraria;
using Terraria.ModLoader;

namespace Snaker.Content.Enemies;

internal class FireAntPotato : ModProjectile
{
    public const int MaxTimeLeft = 300;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 20;
        Projectile.aiStyle = 0;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MaxTimeLeft;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.alpha = 80;
    }

    public override void AI()
    {
        Projectile.rotation += Projectile.velocity.X * 0.05f;

        if (Projectile.timeLeft < MaxTimeLeft - 60)
            Projectile.velocity.Y += 0.02f;
    }
}
