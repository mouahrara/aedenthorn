using StardewModdingAPI;

namespace AllChestsMenu
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool ModToOpen { get; set; } = false;
		public bool LimitToCurrentLocation { get; set; } = false;
		public bool IncludeShippingBin { get; set; } = true;
		public bool UnrestrictedShippingBin { get; set; } = false;
		public AllChestsMenu.Sort CurrentSort { get; set; } = AllChestsMenu.Sort.NA;
		public SButton ModKey { get; set; } = SButton.LeftShift;
		public SButton ModKey2 { get; set; } = SButton.LeftControl;
		public SButton SwitchButton { get; set; } = SButton.ControllerBack;
		public SButton MenuKey { get; set; } = SButton.F2;
	}
}
