namespace CustomToolbarAndroid
{
	public class ModConfig
	{
		public bool EnableMod { get; set; } = true;
		public bool VerticalToolbar { get; set; } = false;
		public string PinnedPosition { get; set; } = "bottom";
		public float OpacityPercentage { get; set; } = 0.33f;
		public int MaxVisibleItems { get; set; } = 36;
	}
}
