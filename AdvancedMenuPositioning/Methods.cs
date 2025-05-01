using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AdvancedMenuPositioning
{
	public partial class ModEntry : Mod
	{
		private static readonly string[] vectorFieldsToAdjust = new string[] { "_characterEntrancePosition", "_characterNamePosition", "_heartDisplayPosition", "_birthdayHeadingDisplayPosition", "_birthdayDisplayPosition", "_statusHeadingDisplayPosition", "_statusDisplayPosition", "_giftLogHeadingDisplayPosition", "_giftLogCategoryDisplayPosition", "_errorMessagePosition", "_characterSpriteDrawPosition" };
		private static readonly string[] rectangleFieldsToAdjust = new string[] { "scrollBarRunner", "mapBounds", "characterSpriteBox", "_characterStatusDisplayBox", "_itemDisplayRect", "ScrollbarRunner", "SearchBoxBounds" };
		private static readonly string[] temporaryAnimatedSpriteListFieldsToAdjust = new string[] { "tempSprites" };

		private static void AdjustMenu(IClickableMenu menu, Point delta, bool isRootMenu = false)
		{
			if (isRootMenu)
			{
				adjustedMenus.Clear();
				adjustedComponents.Clear();
			}
			if (menu is not null && !adjustedMenus.Contains(menu))
			{
				List<FieldInfo> fields = GetMenuFields(menu);
				FieldInfo newXPositionField = menu.GetType().GetField(nameof(menu.xPositionOnScreen), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				FieldInfo newYPositionField = menu.GetType().GetField(nameof(menu.yPositionOnScreen), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

				adjustedMenus.Add(menu);
				if (newXPositionField is not null)
				{
					newXPositionField.SetValue(menu, (int)newXPositionField.GetValue(menu) + delta.X);
				}
				else
				{
					menu.xPositionOnScreen += delta.X;
				}
				if (newYPositionField is not null)
				{
					newYPositionField.SetValue(menu, (int)newYPositionField.GetValue(menu) + delta.Y);
				}
				else
				{
					menu.yPositionOnScreen += delta.Y;
				}
				foreach (FieldInfo field in fields)
				{
					AdjustField(menu, field, delta);
				}
				if (menu is GameMenu gameMenu)
				{
					foreach (IClickableMenu page in gameMenu.pages.Where((p, i) => i != gameMenu.currentTab))
					{
						AdjustMenu(page, delta);
					}
				}
				AdjustComponent(menu.upperRightCloseButton, delta);
			}
		}

		private static List<FieldInfo> GetMenuFields(IClickableMenu menu)
		{
			List<FieldInfo> types = menu.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).ToList();

			if (menu is ItemGrabMenu)
			{
				types.AddRange(typeof(MenuWithInventory).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
			}
			return types;
		}

		private static void AdjustField(IClickableMenu menu, FieldInfo field, Point delta)
		{
			object fieldValue = field.GetValue(menu);

			if (field.FieldType == typeof(ClickableComponent) || field.FieldType.IsSubclassOf(typeof(ClickableComponent)))
			{
				AdjustComponent((ClickableComponent)fieldValue, delta);
			}
			else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>) && typeof(ClickableComponent).IsAssignableFrom(field.FieldType.GetGenericArguments()[0]))
			{
				Type componentType = field.FieldType.GetGenericArguments()[0];
				MethodInfo adjustMethod = typeof(ModEntry).GetMethod(nameof(AdjustComponentList), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(componentType);

				adjustMethod.Invoke(null, new object[] { fieldValue, delta });
			}
			else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>) && field.FieldType.GetGenericArguments()[0] == typeof(int) && typeof(ClickableComponent).IsAssignableFrom(field.FieldType.GetGenericArguments()[1]))
			{
				Type valueType = field.FieldType.GetGenericArguments()[1];
				MethodInfo adjustMethod = typeof(ModEntry).GetMethod(nameof(AdjustComponentDictionary), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(valueType);

				adjustMethod.Invoke(null, new object[] { fieldValue, delta });
			}
			else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>) && field.FieldType.GetGenericArguments()[0] == typeof(int) && field.FieldType.GetGenericArguments()[1].IsGenericType && field.FieldType.GetGenericArguments()[1].GetGenericTypeDefinition() == typeof(List<>) && field.FieldType.GetGenericArguments()[1].GetGenericArguments()[0].IsGenericType && field.FieldType.GetGenericArguments()[1].GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(List<>) && typeof(ClickableComponent).IsAssignableFrom(field.FieldType.GetGenericArguments()[1].GetGenericArguments()[0].GetGenericArguments()[0]))
			{
				Type componentType = field.FieldType.GetGenericArguments()[1].GetGenericArguments()[0].GetGenericArguments()[0];
				MethodInfo adjustMethod = typeof(ModEntry).GetMethod(nameof(AdjustComponentDictionaryListList), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(componentType);

				adjustMethod.Invoke(null, new object[] { fieldValue, delta });
			}
			else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>) && field.FieldType.GetGenericArguments()[0].IsGenericType && field.FieldType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Dictionary<,>) && typeof(ClickableComponent).IsAssignableFrom(field.FieldType.GetGenericArguments()[0].GetGenericArguments()[0]) && field.FieldType.GetGenericArguments()[0].GetGenericArguments()[1] == typeof(CraftingRecipe))
			{
				Type componentType = field.FieldType.GetGenericArguments()[0].GetGenericArguments()[0];
				MethodInfo adjustMethod = typeof(ModEntry).GetMethod(nameof(AdjustComponentListDictionary), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(componentType);

				adjustMethod.Invoke(null, new object[] { fieldValue, delta });
			}
			else if (field.FieldType == typeof(IClickableMenu) || field.FieldType.IsSubclassOf(typeof(IClickableMenu)))
			{
				AdjustChildMenu((IClickableMenu)fieldValue, delta, menu);
			}
			else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>) && typeof(IClickableMenu).IsAssignableFrom(field.FieldType.GetGenericArguments()[0]))
			{
				Type menuType = field.FieldType.GetGenericArguments()[0];
				MethodInfo adjustMethod = typeof(ModEntry).GetMethod(nameof(AdjustChildMenuList), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(menuType);

				adjustMethod.Invoke(null, new object[] { fieldValue, delta, menu });
			}
			else if (field.FieldType == typeof(TextBox) || field.FieldType.IsSubclassOf(typeof(TextBox)))
			{
				AdjustTextBox((TextBox)fieldValue, delta);
			}
			else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>) && typeof(ProfileItem).IsAssignableFrom(field.FieldType.GetGenericArguments()[0]))
			{
				Type profileItemType = field.FieldType.GetGenericArguments()[0];
				MethodInfo adjustMethod = typeof(ModEntry).GetMethod(nameof(AdjustProfileItemList), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(profileItemType);

				adjustMethod.Invoke(null, new object[] { fieldValue, delta });
			}
			else if ((field.FieldType == typeof(Vector2) || field.FieldType.IsSubclassOf(typeof(Vector2))) && vectorFieldsToAdjust.Contains(field.Name))
			{
				AdjustVector(field, fieldValue, delta, menu);
			}
			else if ((field.FieldType == typeof(Rectangle) || field.FieldType.IsSubclassOf(typeof(Rectangle))) && rectangleFieldsToAdjust.Contains(field.Name))
			{
				AdjustRectangle(field, fieldValue, delta, menu);
			}
			else if ((field.FieldType == typeof(TemporaryAnimatedSpriteList) || field.FieldType.IsSubclassOf(typeof(TemporaryAnimatedSpriteList))) && temporaryAnimatedSpriteListFieldsToAdjust.Contains(field.Name))
			{
				AdjustTemporaryAnimatedSpriteList((TemporaryAnimatedSpriteList)fieldValue, delta);
			}
		}

		private static void AdjustComponent(ClickableComponent component, Point delta)
		{
			if (component is not null && !adjustedComponents.Contains(component))
			{
				component.bounds.Offset(delta);
				if (component is Bundle bundle)
				{
					bundle.sprite.position += new Vector2(delta.X, delta.Y);
				}
				if (component is ClickableAnimatedComponent animatedComponent)
				{
					animatedComponent.sprite.position += new Vector2(delta.X, delta.Y);
				}
				adjustedComponents.Add(component);
			}
		}

		private static void AdjustComponentList<T>(List<T> list, Point delta) where T : ClickableComponent
		{
			if (list is not null)
			{
				foreach (T component in list)
				{
					AdjustComponent(component, delta);
				}
			}
		}

		private static void AdjustComponentDictionary<T>(Dictionary<int, T> dictionary, Point delta) where T : ClickableComponent
		{
			if (dictionary is not null)
			{
				foreach (ClickableComponent component in dictionary.Values)
				{
					AdjustComponent(component, delta);
				}
			}
		}

		private static void AdjustComponentDictionaryListList<T>(Dictionary<int, List<List<T>>> dictionary, Point delta) where T : ClickableComponent
		{
			if (dictionary is not null)
			{
				foreach (KeyValuePair<int, List<List<T>>> pair in dictionary)
				{
					List<List<T>> listOfLists = pair.Value;

					if (listOfLists is not null)
					{
						foreach (List<T> subList in listOfLists)
						{
							if (subList is not null)
							{
								foreach (T component in subList)
								{
									AdjustComponent(component, delta);
								}
							}
						}
					}
				}
			}
		}

		private static void AdjustComponentListDictionary<T>(List<Dictionary<T, CraftingRecipe>> list, Point delta) where T : ClickableComponent
		{
			if (list is not null)
			{
				foreach (Dictionary<T, CraftingRecipe> dictionary in list)
				{
					if (dictionary is not null)
					{
						foreach (T component in dictionary.Keys)
						{
							AdjustComponent(component, delta);
						}
					}
				}
			}
		}

		private static void AdjustChildMenu(IClickableMenu menu, Point delta, IClickableMenu parentMenu)
		{
			if (menu != parentMenu)
			{
				AdjustMenu(menu, delta);
			}
		}

		private static void AdjustChildMenuList<T>(List<T> list, Point delta, IClickableMenu parentMenu) where T : IClickableMenu
		{
			if (list is not null)
			{
				foreach (T menu in list)
				{
					AdjustChildMenu(menu, delta, parentMenu);
				}
			}
		}

		private static void AdjustTextBox(TextBox textBox, Point delta)
		{
			if (textBox is not null)
			{
				textBox.X += delta.X;
				textBox.Y += delta.Y;
			}
		}

		private static void AdjustProfileItemList<T>(List<T> list, Point delta) where T : ProfileItem
		{
			if (list is not null)
			{
				foreach (T profileItem in list)
				{
					FieldInfo nameDrawPositionField = typeof(T).GetField("_nameDrawPosition", BindingFlags.NonPublic | BindingFlags.Instance);

					AdjustVector(nameDrawPositionField, nameDrawPositionField.GetValue(profileItem), delta, profileItem);
					if (profileItem is PI_ItemList piItemList)
					{
						AdjustVectorList((List<Vector2>)typeof(PI_ItemList).GetField("_emptyBoxPositions", BindingFlags.NonPublic| BindingFlags.Instance).GetValue(piItemList), delta);
					}
				}
			}
		}

		private static void AdjustVector(FieldInfo field, object fieldValue, Point delta, object obj)
		{
			if (fieldValue is Vector2 vector)
			{
				vector += new Vector2(delta.X, delta.Y);
				field.SetValue(obj, vector);
			}
		}

		private static void AdjustRectangle(FieldInfo field, object fieldValue, Point delta, object obj)
		{
			if (fieldValue is Rectangle rect)
			{
				rect.Offset(delta);
				field.SetValue(obj, rect);
			}
		}

		private static void AdjustTemporaryAnimatedSpriteList(TemporaryAnimatedSpriteList temporaryAnimatedSpriteList, Point delta)
		{
			if (temporaryAnimatedSpriteList is not null)
			{
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in temporaryAnimatedSpriteList)
				{
					temporaryAnimatedSprite.Position += new Vector2(delta.X, delta.Y);
				}
			}
		}

		private static void AdjustVectorList(List<Vector2> list, Point delta)
		{
			if (list is not null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					list[i] += new Vector2(delta.X, delta.Y);
				}
			}
		}

		private static bool IsKeybindPressed(SButton[] buttons)
		{
			if (!buttons.All(button => SHelper.Input.IsDown(button) || SHelper.Input.IsSuppressed(button)))
				return false;
			if (!Config.StrictKeybindings)
				return true;

			int numberOfButtonsPressed = 0;

			foreach (SButton button in Enum.GetValues(typeof(SButton)))
			{
				if (SHelper.Input.IsDown(button) || SHelper.Input.IsSuppressed(button))
				{
					numberOfButtonsPressed++;
				}
			}
			return numberOfButtonsPressed == buttons.Length;
		}

		private bool TryMoveActiveMenu(Point delta)
		{
			if (Game1.activeClickableMenu is not null)
			{
				if (currentlyDragging == Game1.activeClickableMenu || (currentlyDragging is null && Game1.activeClickableMenu.isWithinBounds((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY()))))
				{
					currentlyDragging = Game1.activeClickableMenu;
					AdjustMenu(Game1.activeClickableMenu, delta, true);
					Array.ForEach(Config.MoveKeys.Keybinds[0].Buttons, b => Helper.Input.Suppress(b));
					if (Game1.activeClickableMenu is ItemGrabMenu && Helper.ModRegistry.IsLoaded("Pathoschild.ChestsAnywhere"))
					{
						Game1.activeClickableMenu = Game1.activeClickableMenu.ShallowClone();
					}
					return true;
				}
			}
			return false;
		}

		private bool TryMoveMenuFromList(IList<IClickableMenu> list, Point delta)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i] is not null)
				{
					if (currentlyDragging == list[i] || (currentlyDragging is null && list[i].isWithinBounds((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY()))))
					{
						currentlyDragging = list[i];
						list.Add(list[i]);
						list.RemoveAt(i);
						AdjustMenu(list[i], delta, true);
						Array.ForEach(Config.MoveKeys.Keybinds[0].Buttons, b => Helper.Input.Suppress(b));
						return true;
					}
				}
			}
			return false;
		}

		private void HandleClickOnDetachedMenu(int mouseX, int mouseY, SButton button)
		{
			for (int i = detachedMenus.Count - 1; i >= 0; i--)
			{
				bool isMouseInsideDetachedMenu = detachedMenus[i].isWithinBounds(mouseX, mouseY);
				IClickableMenu previousActiveClickableMenu = Game1.activeClickableMenu;

				Game1.activeClickableMenu = detachedMenus[i];
				if (button == SButton.MouseLeft)
				{
					detachedMenus[i].receiveLeftClick(mouseX, mouseY);
					if (Game1.activeClickableMenu is ItemGrabMenu menuAsItemGrabMenu)
					{
						Game1.activeClickableMenu = new ItemGrabMenu(menuAsItemGrabMenu.ItemsToGrabMenu.actualInventory, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), menuAsItemGrabMenu.behaviorFunction, null, menuAsItemGrabMenu.behaviorOnItemGrab, canBeExitedWithKey: true, showOrganizeButton: true, source: menuAsItemGrabMenu.source, sourceItem: menuAsItemGrabMenu.sourceItem, whichSpecialButton: menuAsItemGrabMenu.whichSpecialButton, context: menuAsItemGrabMenu.context).setEssential(menuAsItemGrabMenu.essential);
					}
				}
				else
				{
					detachedMenus[i].receiveRightClick(mouseX, mouseY);
				}
				if (Game1.activeClickableMenu is not null)
				{
					Point delta = new((int)Utility.ModifyCoordinateForUIScale(detachedMenus[i].xPositionOnScreen - Game1.activeClickableMenu.xPositionOnScreen), (int)Utility.ModifyCoordinateForUIScale(detachedMenus[i].yPositionOnScreen - Game1.activeClickableMenu.yPositionOnScreen));

					detachedMenus[i] = Game1.activeClickableMenu;
					AdjustMenu(detachedMenus[i], delta, true);
					Game1.activeClickableMenu = previousActiveClickableMenu;
					if (isMouseInsideDetachedMenu)
					{
						detachedMenus.Add(detachedMenus[i]);
						detachedMenus.RemoveAt(i);
					}
				}
				else
				{
					detachedMenus.RemoveAt(i);
				}
				Game1.activeClickableMenu = previousActiveClickableMenu;
				if (isMouseInsideDetachedMenu)
				{
					Helper.Input.Suppress(button);
					break;
				}
			}
		}
	}
}
