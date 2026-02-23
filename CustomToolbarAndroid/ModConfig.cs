using System.Reflection;
using StardewValley;

namespace CustomToolbarAndroid
{
	public class ModConfig
	{
		public bool EnableMod { get; set; } = true;
		public bool VerticalToolbar { get; set; } = false;
		public string PinnedPosition { get; set; } = "bottom";
		public float OpacityPercentage { get; set; } = 0.33f;
		public int MaxVisibleItems { get; set; } = 36;
		public int OffsetX { get; set; } = (int)typeof(Game1).GetField("toolbarPaddingX", BindingFlags.Public | BindingFlags.Static).GetValue(null);
		public int OffsetY { get; set; } = 12;
	}
}
