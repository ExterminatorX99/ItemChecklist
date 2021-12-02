using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace ItemChecklist.UIElements
{
	internal class UIItemSlot : UIElement
	{
		public static Texture2D backgroundTexture = TextureAssets.InventoryBack9.Value;

		private Texture2D _texture;
		//	private float _visibilityActive = 1f;
		//		private float _visibilityInactive = 0.4f;
		private float scale = 0.75f;
		internal int id;
		internal Item item;
		public string badge;

		public UIItemSlot(int id)
		{
			_texture = TextureAssets.Item[id].Value;
			this.id = id;
			item = new Item();
			item.SetDefaults(id, true);

			Width.Set(backgroundTexture.Width * scale, 0f);
			Height.Set(backgroundTexture.Height * scale, 0f);
		}

		//public override int CompareTo(object obj)
		//{
		//	UIItemSlot other = obj as UIItemSlot;
		//	int result;
		//	switch (ItemChecklistUI.sortMode)
		//	{
		//		case SortModes.ID:
		//			return id.CompareTo(other.id);
		//		case SortModes.AZ:
		//			return item.Name.CompareTo(other.item.Name);
		//		case SortModes.Value:
		//			result = item.value.CompareTo(other.item.value);
		//			if (result == 0)
		//				result = item.Name.CompareTo(other.item.Name);
		//			return result;
		//		case SortModes.Rare:
		//			result = item.rare.CompareTo(other.item.rare);
		//			if (result == 0)
		//				result = item.Name.CompareTo(other.item.Name);
		//			return result;
		//		case SortModes.TerrariaSort:
		//			return ItemChecklistUI.vanillaIDsInSortOrder[id].CompareTo(ItemChecklistUI.vanillaIDsInSortOrder[other.id]);
		//	}

		//	return id.CompareTo(other.id);
		//}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			//spriteBatch.Draw(this._texture, dimensions.Position(), Color.White * (base.IsMouseHovering ? this._visibilityActive : this._visibilityInactive));

			spriteBatch.Draw(backgroundTexture, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			//Texture2D texture2D = TextureAssets.Item[this.item.type].Value;
			Rectangle rectangle2;
			if (Main.itemAnimations[id] != null)
			{
				rectangle2 = Main.itemAnimations[id].GetFrame(_texture);
			}
			else
			{
				rectangle2 = _texture.Frame();
			}
			float num = 1f;
			float num2 = backgroundTexture.Width * scale;
			if (rectangle2.Width > num2 || rectangle2.Height > num2)
			{
				if (rectangle2.Width > rectangle2.Height)
				{
					num = num2 / rectangle2.Width;
				}
				else
				{
					num = num2 / rectangle2.Height;
				}
			}
			Vector2 drawPosition = dimensions.Position();
			drawPosition.X += backgroundTexture.Width * scale / 2f - rectangle2.Width * num / 2f;
			drawPosition.Y += backgroundTexture.Height * scale / 2f - rectangle2.Height * num / 2f;

			item.GetColor(Color.White);
			Color alphaColor = Main.LocalPlayer.GetModPlayer<ItemChecklistPlayer>().foundItem[id] ? item.GetAlpha(Color.White) : Color.Black;
			Color colorColor = Main.LocalPlayer.GetModPlayer<ItemChecklistPlayer>().foundItem[id] ? item.GetColor(Color.White) : Color.Black;
			//spriteBatch.Draw(_texture, drawPosition, new Rectangle?(rectangle2), this.item.GetAlpha(Color.White), 0f, Vector2.Zero, num, SpriteEffects.None, 0f);
			spriteBatch.Draw(_texture, drawPosition, rectangle2, alphaColor, 0f, Vector2.Zero, num, SpriteEffects.None, 0f);
			if (item.color != Color.Transparent)
			{
				spriteBatch.Draw(_texture, drawPosition, rectangle2, colorColor, 0f, Vector2.Zero, num, SpriteEffects.None, 0f);
			}
			if (ItemChecklistUI.showBadge && !string.IsNullOrEmpty(badge))
			{
				spriteBatch.DrawString(FontAssets.ItemStack.Value, badge, new Vector2(dimensions.Position().X + 10f * scale, dimensions.Position().Y + 26f * scale), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}

			if (IsMouseHovering)
			{
				ItemChecklistUI.hoverText = item.Name + (item.ModItem != null ? " [" + item.ModItem.Mod.Name + "]" : "");

				Main.HoverItem = item.Clone();
			}
		}
	}
}
