using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ItemChecklist
{
	class ItemChecklistPlayer : ModPlayer
	{
		//	internal static ItemChecklistPlayer localInstance;

		// This is a list of items...Holds clean versions of unloaded mystery and loaded real items.
		internal List<Item> foundItems;
		//
		internal bool[] foundItem;
		internal bool[] findableItems;
		//Skipping:
		// Main.itemName 0 is empty
		// Mysteryitem is to skip --> ItemID.Count
		// Deprecated  if (this.type > 0 && ItemID.Sets.Deprecated[this.type])

		internal int totalItemsToFind;
		internal int totalItemsFound;  // eh, property? dunno.

		// Because of save, these values inherit the last used setting while loading
		//internal SortModes sortModePreference = SortModes.TerrariaSort;
		internal bool announcePreference;
		internal bool findChestItemsPreference = true;
		internal int showCompletedPreference;

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (ItemChecklist.ToggleChecklistHotKey.JustPressed)
			{
				if (!ItemChecklistUI.Visible)
				{
					ItemChecklistUI.instance.UpdateNeeded();
				}
				ItemChecklistUI.Visible = !ItemChecklistUI.Visible;
				// Debug assistance, allows for reinitializing RecipeBrowserUI
				//if (!ItemChecklistUI.visible)
				//{
				//	ItemChecklistUI.instance.RemoveAllChildren();
				//	var isInitializedFieldInfo = typeof(Terraria.UI.UIElement).GetField("_isInitialized", BindingFlags.Instance | BindingFlags.NonPublic);
				//	isInitializedFieldInfo.SetValue(ItemChecklistUI.instance, false);
				//	ItemChecklistUI.instance.Activate();
				//}
			}
		}

		public override void OnEnterWorld(Player player)
		{
			ItemChecklistUI.Visible = false;
			ItemChecklistUI.announce = announcePreference;
			ItemChecklistUI.collectChestItems = findChestItemsPreference;
			//ItemChecklistUI.sortMode = sortModePreference;
			ItemChecklistUI.showCompleted = showCompletedPreference;
			ItemChecklistUI.instance.RefreshPreferences();
			ItemChecklistUI.instance.UpdateNeeded();
		}

		// Do I need to use Initialize? I think so because of cloning.
		public override void Initialize()
		{
			if (!Main.dedServ)
			{
				foundItems = new List<Item>();
				foundItem = new bool[ItemLoader.ItemCount];
				findableItems = new bool[ItemLoader.ItemCount];
				for (int i = 0; i < ItemLoader.ItemCount; i++)
					if (i > 0 &&
						!ItemID.Sets.Deprecated[i] &&
						i != ItemID.Count && // Is this to exclude UnloadedItem? If yes, should it be changed to ModContent.ItemType<UnloadedItem>()?
						ItemChecklistUI.vanillaIDsInSortOrder != null &&
						ItemChecklistUI.vanillaIDsInSortOrder[i] != -1) // TODO, is this guaranteed?
					{
						totalItemsToFind++;
						findableItems[i] = true;
					}

				announcePreference = false;
				findChestItemsPreference = true;
				//sortModePreference = SortModes.TerrariaSort;
				showCompletedPreference = 0;
			}
		}

		public override void UpdateAutopause()
		{
			ChestCheck();
		}

		public override void PreUpdate()
		{
			ChestCheck();
		}

		private void ChestCheck()
		{
			if (!Main.dedServ && Player.whoAmI == Main.myPlayer)
			{
				for (int i = 0; i < 59; i++)
				{
					if (!Player.inventory[i].IsAir && !foundItem[Player.inventory[i].type] && findableItems[Player.inventory[i].type])
					{
						ItemChecklistGlobalItem.ItemReceived(Player.inventory[i]); // TODO: Analyze performance impact? do every 60 frames only?
					}
				}
				if (Player.chest != -1 && (Player.chest != Player.lastChest || Main.autoPause && Main.gamePaused) && ItemChecklistUI.collectChestItems)
				{
					//Main.NewText(player.chest + " " + player.lastChest);
					Item[] items = Player.chest switch
					{
						-2 => Player.bank.item,
						-3 => Player.bank2.item,
						-4 => Player.bank3.item,
						_  => Main.chest[Player.chest].item
					};
					for (int i = 0; i < 40; i++)
					{
						if (!items[i].IsAir && !foundItem[items[i].type] && findableItems[items[i].type])
						{
							ItemChecklistGlobalItem.ItemReceived(items[i]);
						}
					}
				}
				if (ItemChecklistUI.collectChestItems && MagicStorageIntegration.Enabled)
					MagicStorageIntegration.FindItemsInStorage();
			}
		}

		public override void SaveData(TagCompound tag)
		{
			// sanitize? should be possible to add item already seen.
			tag["FoundItems"] = foundItems.Select(ItemIO.Save).ToList();
			//tag["SortMode"] = (int)ItemChecklistUI.sortMode;
			tag["Announce"] = ItemChecklistUI.announce; // Not saving default, saving last used....good thing?
			tag["CollectChestItems"] = ItemChecklistUI.collectChestItems;
			tag["ShowCompleted"] = ItemChecklistUI.showCompleted;
		}

		public override void LoadData(TagCompound tag)
		{
			foundItems = tag.GetList<TagCompound>("FoundItems").Select(ItemIO.Load).ToList();
			//sortModePreference = (SortModes)tag.GetInt("SortMode");
			announcePreference = tag.GetBool("Announce");
			if (tag.ContainsKey("CollectChestItems")) // Missing tags get defaultvalue, which would be false, which isn't what we want.
				findChestItemsPreference = tag.GetBool("CollectChestItems");
			showCompletedPreference = tag.GetInt("ShowCompleted");

			foreach (var item in foundItems)
			{
				if (item.Name != "Unloaded Item")
				{
					foundItem[item.type] = true;
					totalItemsFound++;
				}
			}
		}
	}
}
