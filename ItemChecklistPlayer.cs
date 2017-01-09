﻿using ItemChecklist.UI;
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
		internal SortModes sortModePreference = SortModes.TerrariaSort;
		internal bool announcePreference;
		internal int showCompletedPreference;

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (ItemChecklist.ToggleChecklistHotKey.JustPressed)
			{
				if (!ItemChecklistUI.visible)
				{
					ItemChecklist.instance.ItemChecklistUI.UpdateNeeded();
				}
				ItemChecklistUI.visible = !ItemChecklistUI.visible;
			}
			if (ItemChecklist.TestResizeHotKey.JustPressed)
			{
				ItemChecklist.instance.ItemChecklistUI.checklistGrid.cols = Main.rand.Next(1,8);
				ItemChecklist.instance.ItemChecklistUI.Recalculate();
				//ItemChecklist.instance.ItemChecklistUI.checklistPanel.Recalculate();
				//ItemChecklist.instance.ItemChecklistUI.checklistGrid.Recalculate();
			}
		}

		public override void OnEnterWorld(Player player)
		{
			var itemChecklistPlayer = Main.LocalPlayer.GetModPlayer<ItemChecklistPlayer>(mod);
			ItemChecklistUI.visible = false;
			ItemChecklistUI.announce = announcePreference;
			ItemChecklistUI.sortMode = sortModePreference;
			ItemChecklistUI.showCompleted = showCompletedPreference;
			ItemChecklist.instance.ItemChecklistUI.RefreshPreferences();
			ItemChecklist.instance.ItemChecklistUI.UpdateNeeded();
		}

		// Do I need to use Initialize? I think so because of cloning.
		public override void Initialize()
		{
			if (!Main.dedServ)
			{
				foundItems = new List<Item>();
				foundItem = new bool[Main.itemName.Length];
				findableItems = new bool[Main.itemName.Length];
				for (int i = 0; i < Main.itemName.Length; i++)
				{
					if (i > 0 && !ItemID.Sets.Deprecated[i] && i != ItemID.Count && ItemChecklistUI.vanillaIDsInSortOrder[i] != -1) // TODO, is this guaranteed?
					{
						totalItemsToFind++;
						findableItems[i] = true;
					}
				}

				announcePreference = true;
				sortModePreference = SortModes.TerrariaSort;
				showCompletedPreference = 0;
			}
		}

		public override void PreUpdate()
		{
			if (!Main.dedServ)
			{
				var itemChecklistPlayer = Main.LocalPlayer.GetModPlayer<ItemChecklistPlayer>(mod);
				for (int i = 0; i < 59; i++)
				{
					if (!player.inventory[i].IsAir && !itemChecklistPlayer.foundItem[player.inventory[i].type] && itemChecklistPlayer.findableItems[player.inventory[i].type])
					{
						((ItemChecklistGlobalItem)mod.GetGlobalItem("ItemChecklistGlobalItem")).ItemReceived(player.inventory[i]);
					}
				}
			}
		}

		public override TagCompound Save()
		{
			// sanitize? should be possible to add item already seen.
			return new TagCompound
			{
				["FoundItems"] = foundItems.Select(ItemIO.Save).ToList(),
				["SortMode"] = (int)ItemChecklistUI.sortMode,
				["Announce"] = ItemChecklistUI.announce,
				["ShowCompleted"] = ItemChecklistUI.showCompleted,
			};
		}

		public override void Load(TagCompound tag)
		{
			foundItems = tag.GetList<TagCompound>("FoundItems").Select(ItemIO.Load).ToList();
			sortModePreference = (SortModes)tag.GetInt("SortMode");
			announcePreference = tag.GetBool("Announce");
			showCompletedPreference = tag.GetInt("ShowCompleted");

			foreach (var item in foundItems)
			{
				if (item.name != "Unloaded Item")
				{
					foundItem[item.type] = true;
					totalItemsFound++;
				}
			}
		}
	}
}
