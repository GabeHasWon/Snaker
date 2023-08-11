using Microsoft.Xna.Framework;
using SubworldLibrary;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Snaker.Content.World;

internal class SnakerArenaBiome : ModBiome
{
	public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/Arena");
	public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;
	public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;
	public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>("Snaker/SnakerAreaBackground");

	public override string BestiaryIcon => "Snaker/Assets/Images/SnakeArena_Icon";
	public override string BackgroundPath => MapBackground;
	public override Color? BackgroundColor => Color.AntiqueWhite;
	public override string MapBackground => "Snaker/Assets/Images/SnakeArena_MapBG";

	// public override void SetStaticDefaults() => DisplayName.SetDefault("Devilish Snake Arena");
	public override bool IsBiomeActive(Player player) => SubworldSystem.IsActive<SnakerSubworld>();
}

public class SnakerAreaBackground : ModSurfaceBackgroundStyle
{
	/// <summary>
	/// Used to move all of the backgrounds way up, subworld is too small for normal backgrounds.
	/// </summary>
	FieldInfo bgTopYField;

	public override void Load()
	{
		bgTopYField = typeof(Main).GetField("bgTopY", BindingFlags.NonPublic | BindingFlags.Instance);

        Terraria.Graphics.Effects.On_SkyManager.ProcessCloudAlpha += SkyManager_ProcessCloudAlpha;
	}

	/// <summary>
	/// I don't want to make a CustomSky just to disable clouds. Here's a fix!
	/// </summary>
    private float SkyManager_ProcessCloudAlpha(Terraria.Graphics.Effects.On_SkyManager.orig_ProcessCloudAlpha orig, SkyManager self)
    {
		if (SubworldSystem.IsActive<SnakerSubworld>())
			return 0f;

		return orig(self);
    }

    public override void Unload() => bgTopYField = null;

    public override int ChooseFarTexture()
	{
		bgTopYField.SetValue(Main.instance, (int)bgTopYField.GetValue(Main.instance) - 600);
		return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Images/Backgrounds/SnakeArenaBackground_Far");
	}

	public override int ChooseMiddleTexture()
	{
		bgTopYField.SetValue(Main.instance, (int)bgTopYField.GetValue(Main.instance) - 400);
		return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Images/Backgrounds/SnakeArenaBackground_Middle");
	}

	public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
	{
		b -= 800;
		return BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Images/Backgrounds/SnakeArenaBackground_Front");
	}

	public override void ModifyFarFades(float[] fades, float transitionSpeed)
	{
		for (int i = 0; i < fades.Length; i++)
		{
			if (i == Slot)
			{
				fades[i] += transitionSpeed;
				if (fades[i] > 1f)
					fades[i] = 1f;
			}
			else
			{
				fades[i] -= transitionSpeed;
				if (fades[i] < 0f)
					fades[i] = 0f;
			}
		}
	}
}