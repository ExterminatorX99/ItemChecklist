using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.UI;

namespace ItemChecklist.UIElements
{
	public class UIToggleHoverImageButton : UIImageButton
	{
		//private Texture2D _texture;
		private Texture2D overlay;
		private float _visibilityActive = 1f;
		private float _visibilityInactive = 0.4f;
		bool enabled;
		internal string hoverText;

		public UIToggleHoverImageButton(Texture2D texture, Texture2D overlay, string hoverText, bool enabled = false) : base(texture)
		{
			_texture = texture;
			this.overlay = overlay;
			Width.Set(_texture.Width, 0f);
			Height.Set(_texture.Height, 0f);
			this.hoverText = hoverText;
			this.enabled = enabled;
		}

		public void SetEnabled(bool enabled)
		{
			this.enabled = enabled;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			spriteBatch.Draw(_texture, dimensions.Position(), Color.White * (IsMouseHovering ? _visibilityActive : _visibilityInactive));
			if (!enabled)
			{
				// 32x32, overlay is 24x24.
				spriteBatch.Draw(overlay, dimensions.Position() + new Vector2(4), Color.White * (IsMouseHovering ? _visibilityActive : _visibilityInactive));
			}
			if (IsMouseHovering)
			{
				ItemChecklistUI.hoverText = hoverText;
			}
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			SoundEngine.PlaySound(12);
		}
	}

	public class UIImageButton : UIElement
	{
		protected Texture2D _texture;
		private float _visibilityActive = 1f;
		private float _visibilityInactive = 0.4f;

		public UIImageButton(Texture2D texture)
		{
			_texture = texture;
			Width.Set(_texture.Width, 0f);
			Height.Set(_texture.Height, 0f);
		}

		public void SetImage(Texture2D texture)
		{
			_texture = texture;
			Width.Set(_texture.Width, 0f);
			Height.Set(_texture.Height, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			spriteBatch.Draw(_texture, dimensions.Position(), Color.White * (IsMouseHovering ? _visibilityActive : _visibilityInactive));
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			SoundEngine.PlaySound(12);
		}

		public void SetVisibility(float whenActive, float whenInactive)
		{
			_visibilityActive = MathHelper.Clamp(whenActive, 0f, 1f);
			_visibilityInactive = MathHelper.Clamp(whenInactive, 0f, 1f);
		}
	}
}