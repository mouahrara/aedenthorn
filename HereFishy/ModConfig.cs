namespace HereFishy
{
	public class ModConfig
	{
		public bool EnableMod { get; set; } = true;
		public bool PlaySound { get; set; } = true;
		public string VoiceGender { get; set; } = "Auto";
		public float StaminaCost { get; set; } = 7f;
		public bool AllowMovement { get; set; } = false;
	}
}
