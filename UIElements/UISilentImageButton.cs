using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace ItemChecklist.UIElements
{
	internal class UISilentImageButton : UIElement
	{
		private Texture2D _texture;
		private float _visibilityActive = 1f;
		private float _visibilityHovered = .9f;
		private float _visibilityInactive = 0.8f; // or color? same thing?

		public bool selected;
		internal string hoverText;

		public UISilentImageButton(Texture2D texture, string hoverText)
		{
			_texture = texture;
			Width.Set(_texture.Width, 0f);
			Height.Set(_texture.Height, 0f);
			this.hoverText = hoverText;
		}

		public void SetImage(Texture2D texture)
		{
			_texture = texture;
			Width.Set(_texture.Width, 0f);
			Height.Set(_texture.Height, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (selected)
			{
				var r = GetDimensions().ToRectangle();
				r.Inflate(0,0);
				//spriteBatch.Draw(UIElements.UIRecipeSlot.selectedBackgroundTexture, r, Color.White);
				spriteBatch.Draw(TextureAssets.InventoryBack14.Value, r, Color.White);
			}

			CalculatedStyle dimensions = GetDimensions();
			spriteBatch.Draw(_texture, dimensions.Position(), Color.White * (selected ? _visibilityActive : IsMouseHovering ? _visibilityHovered  : _visibilityInactive));
			if (IsMouseHovering)
			{
				Main.hoverItemName = hoverText;
			}
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			//SoundEngine.PlaySound(12, -1, -1, 1, 1f, 0f);
		}

		//public void SetVisibility(float whenActive, float whenInactive)
		//{
		//	this._visibilityActive = MathHelper.Clamp(whenActive, 0f, 1f);
		//	this._visibilityInactive = MathHelper.Clamp(whenInactive, 0f, 1f);
		//}
	}
}
