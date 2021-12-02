using System.Collections.Generic;
using ItemChecklist.UIElements;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace ItemChecklist;

public class UISystem : ModSystem
{
	internal static UISystem instance;
	internal static UserInterface ItemChecklistInterface;

	public override void Load()
	{
		instance = this;

		if (!Main.dedServ)
		{
			UICheckbox.checkboxTexture = ItemChecklist.instance.GetTexture("UIElements/checkBox");
			UICheckbox.checkmarkTexture = ItemChecklist.instance.GetTexture("UIElements/checkMark");
			UIHorizontalGrid.moreLeftTexture = ItemChecklist.instance.GetTexture("UIElements/MoreLeft");
			UIHorizontalGrid.moreRightTexture = ItemChecklist.instance.GetTexture("UIElements/MoreRight");
		}
	}

	public override void Unload()
	{
		ItemChecklistInterface = null;
		instance = null;

		UICheckbox.checkboxTexture = null;
		UICheckbox.checkmarkTexture = null;
		UIHorizontalGrid.moreLeftTexture = null;
		UIHorizontalGrid.moreRightTexture = null;
	}

	public override void AddRecipes()
	{
		if (!Main.dedServ)
		{
			ItemChecklistInterface = new UserInterface();
			ItemChecklistUI.Visible = true;
		}
	}

	public override void UpdateUI(GameTime gameTime)
	{
		ItemChecklistInterface?.Update(gameTime);
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		if (mouseTextIndex != -1)
			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"ItemChecklist: Item Checklist",
				UIDrawMethod,
				InterfaceScaleType.UI)
			);
	}

	private static bool UIDrawMethod()
	{
		if (ItemChecklistUI.Visible)
		{
			ItemChecklistInterface?.Draw(Main.spriteBatch, new GameTime());

			if (ItemChecklistUI.hoverText != "")
			{
				float x = FontAssets.MouseText.Value.MeasureString(ItemChecklistUI.hoverText).X;
				Vector2 vector = new Vector2(Main.mouseX, Main.mouseY) + new Vector2(16f, 16f);
				if (vector.Y > Main.screenHeight - 30)
					vector.Y = Main.screenHeight - 30;

				if (vector.X > Main.screenWidth - x - 30)
					vector.X = Main.screenWidth - x - 30;

				Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value, ItemChecklistUI.hoverText, vector.X, vector.Y,
					new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), Color.Black, Vector2.Zero);
			}
		}

		return true;
	}
}
