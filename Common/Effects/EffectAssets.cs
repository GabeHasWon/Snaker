using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace Snaker.Common.Effects;

internal class EffectAssets : ILoadable
{
    public static Effect portalShader;

    public void Load(Mod mod) => portalShader = ModContent.Request<Effect>("Snaker/Common/Effects/PortalShader", AssetRequestMode.ImmediateLoad).Value;
    public void Unload() => portalShader = null;
}
