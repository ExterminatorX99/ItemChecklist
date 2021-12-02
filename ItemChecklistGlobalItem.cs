using Terraria;
using Terraria.ModLoader;

namespace ItemChecklist
{
	internal class ItemChecklistGlobalItem : GlobalItem
	{
		// OnPIckup only called on LocalPlayer: I think
		public override void OnCreate(Item item, ItemCreationContext context)
		{
			ItemReceived(item);
		}

		// OnPIckup only called on LocalPlayer: i == Main.myPlayer
		public override bool OnPickup(Item item, Player player)
		{
			ItemReceived(item);
			return true;
		}

		// TODO, unloaded items, check against??
		internal static void ItemReceived(Item item)
		{
			var itemChecklistPlayer = Main.LocalPlayer.GetModPlayer<ItemChecklistPlayer>();
			if (itemChecklistPlayer.foundItem[item.type] || !itemChecklistPlayer.findableItems[item.type])
				return;

			Item newItem = new(item.type);
			itemChecklistPlayer.foundItems.Add(newItem);
			itemChecklistPlayer.totalItemsFound++;
			itemChecklistPlayer.foundItem[item.type] = true;
			ItemChecklistUI.instance.UpdateNeeded(item.type);
			if (ItemChecklistUI.announce)
			{
				Main.NewText($"You found your first {item.Name}.     {itemChecklistPlayer.totalItemsFound}/{itemChecklistPlayer.totalItemsToFind}   {(100f*itemChecklistPlayer.totalItemsFound/itemChecklistPlayer.totalItemsToFind).ToString("0.00")}%");
			}
			ItemChecklist.instance.NewItem(item.type);
		}
	}
}
