using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ItemChecklist
{
	// TODO: is ItemChecklistPlayer.foundItems a waste of memory? investigate and trim it down if needed.
	// TODO: World Checklist? MP shared checklist?
	// Has this item ever been seen on this world? - easy. Maintain separate bool array, on change, notify server, relay to clients.
	// send bool array as byte array?
	// WHY? I want to know everything we can craft yet
	public class ItemChecklist : Mod
	{
		internal static ItemChecklist instance => ModContent.GetInstance<ItemChecklist>();
		internal static ModKeybind ToggleChecklistHotKey;
		internal event Action<int> OnNewItem;

		public override void Load()
		{
			ToggleChecklistHotKey = KeybindLoader.RegisterKeybind(this, "Toggle Item Checklist", "I");
			MagicStorageIntegration.Load();
		}

		public override void Unload()
		{
			ItemChecklistUI.vanillaIDsInSortOrder = null;
			ToggleChecklistHotKey = null;
			MagicStorageIntegration.Unload();
		}

		internal Texture2D GetTexture(string assetName) => Assets.Request<Texture2D>(assetName, AssetRequestMode.ImmediateLoad).Value;

		// As of 0.2.1: All this
		// RequestFoundItems must be done in game since foundItem is a reference to an array that is initialized in LoadPlayer.
		public override object Call(params object[] args)
		{
			try
			{
				string message = args[0] as string;
				if (message == "RequestFoundItems")
				{
					if (Main.gameMenu)
					{
						return "NotInGame";
					}
					return Main.LocalPlayer.GetModPlayer<ItemChecklistPlayer>().foundItem;
				}

				if (message == "RegisterForNewItem")
				{
					Action<int> callback = args[1] as Action<int>;
					OnNewItem += callback;
					return "RegisterSuccess";
				}

				Logger.Error("ItemChecklist Call Error: Unknown Message: " + message);
			}
			catch (Exception e)
			{
				Logger.Error("ItemChecklist Call Error: " + e.StackTrace + e.Message);
			}

			return "Failure";
		}

		internal void NewItem(int type)
		{
			OnNewItem?.Invoke(type);
		}
	}
}
