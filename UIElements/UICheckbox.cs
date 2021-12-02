using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ItemChecklist.UIElements
{
	class UICheckbox : UIText
	{
		public static Texture2D checkboxTexture;
		public static Texture2D checkmarkTexture;
		public event EventHandler SelectedChanged;
		float order;

		private bool selected;
		public bool Selected
		{
			get { return selected; }
			set
			{
				if (value != selected)
				{
					selected = value;
					SelectedChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public UICheckbox(float order, string text, float textScale = 1, bool large = false) : base(text, textScale, large)
		{
			this.order = order;
			Left.Pixels += 20;
			//TextColor = Color.Blue;
			//OnClick += UICheckbox_onLeftClick;
			Recalculate();
		}

		//private void UICheckbox_onLeftClick(UIMouseEvent evt, UIElement listeningElement)
		//{
		//	this.Selected = !Selected;
		//}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle innerDimensions = GetInnerDimensions();
			Vector2 pos = new(innerDimensions.X - 20, innerDimensions.Y - 5);

			spriteBatch.Draw(checkboxTexture, pos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			if (Selected)
				spriteBatch.Draw(checkmarkTexture, pos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			base.DrawSelf(spriteBatch);
		}

		public override int CompareTo(object obj)
		{
			UICheckbox other = obj as UICheckbox;
			return order.CompareTo(other.order);
		}
	}
}

//public string Text
//{
//    get { return label.Text; }
//    set { label.Text = value; }
//}
