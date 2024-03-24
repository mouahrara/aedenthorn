using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewValley.Objects;

namespace AllChestsMenu
{
	public class StorageData
	{
		public string location;
		public string name;
		public string label;
		public int index;
		public InventoryMenu menu;
		public List<ClickableTextureComponent> inventoryButtons = new();
		public bool collapsed;
		public Chest chest;
		public Vector2 tile;
	}
}
