using StardewModdingAPI;

namespace FireBreath
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public SButton FireButton { get; set; } = SButton.Insert;
		public bool ScaleWithSkill { get; set; } = true;
		public int FireDamage { get; set; } = 50;
		public int FireDistance { get; set; } = 256;
		public string FireSound { get; set; } = "furnace";
		public bool FireAnnoysNonMonsters { get; set; } = false;
	}
}
