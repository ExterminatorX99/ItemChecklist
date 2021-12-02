using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.UI;

namespace ItemChecklist.UIElements
{
	public class UIHorizontalScrollbar : UIElement
	{
		private float _viewPosition;
		private float _viewSize = 1f;
		private float _maxViewSize = 20f;
		private bool _isDragging;
		private bool _isHoveringOverHandle;
		private float _dragXOffset;
		private Texture2D _texture;
		private Texture2D _innerTexture;

		public float ViewPosition
		{
			get
			{
				return _viewPosition;
			}
			set
			{
				_viewPosition = MathHelper.Clamp(value, 0f, _maxViewSize - _viewSize);
			}
		}

		public UIHorizontalScrollbar()
		{
			Height.Set(20f, 0f);
			MaxHeight.Set(20f, 0f);
			_texture = ItemChecklist.instance.GetTexture("UIElements/ScrollbarHorizontal"); //TextureManager.Load("Images/UI/Scrollbar");
			_innerTexture = ItemChecklist.instance.GetTexture("UIElements/ScrollbarInnerHorizontal"); //TextureManager.Load("Images/UI/ScrollbarInner");
			PaddingLeft = 5f;
			PaddingRight = 5f;
		}

		public void SetView(float viewSize, float maxViewSize)
		{
			viewSize = MathHelper.Clamp(viewSize, 0f, maxViewSize);
			_viewPosition = MathHelper.Clamp(_viewPosition, 0f, maxViewSize - viewSize);
			_viewSize = viewSize;
			_maxViewSize = maxViewSize;
		}

		public float GetValue()
		{
			return _viewPosition;
		}

		private Rectangle GetHandleRectangle()
		{
			CalculatedStyle innerDimensions = GetInnerDimensions();
			if (_maxViewSize == 0f && _viewSize == 0f)
			{
				_viewSize = 1f;
				_maxViewSize = 1f;
			}
			//return new Rectangle((int)innerDimensions.X, (int)(innerDimensions.Y + innerDimensions.Height * (this._viewPosition / this._maxViewSize)) - 3, 20, (int)(innerDimensions.Height * (this._viewSize / this._maxViewSize)) + 7);
			return new Rectangle((int)(innerDimensions.X + innerDimensions.Width * (_viewPosition / _maxViewSize)) - 3, (int)innerDimensions.Y, (int)(innerDimensions.Width * (_viewSize / _maxViewSize)) + 7, 20);
		}

		private static void DrawBar(SpriteBatch spriteBatch, Texture2D texture, Rectangle dimensions, Color color)
		{
			//spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y - 6, dimensions.Width, 6), new Rectangle?(new Rectangle(0, 0, texture.Width, 6)), color);
			//spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, dimensions.Height), new Rectangle?(new Rectangle(0, 6, texture.Width, 4)), color);
			//spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y + dimensions.Height, dimensions.Width, 6), new Rectangle?(new Rectangle(0, texture.Height - 6, texture.Width, 6)), color);
			spriteBatch.Draw(texture, new Rectangle(dimensions.X - 6, dimensions.Y, 6, dimensions.Height), new Rectangle(0, 0, 6, texture.Height), color);
			spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, dimensions.Height), new Rectangle(6, 0, 4, texture.Height), color);
			spriteBatch.Draw(texture, new Rectangle(dimensions.X + dimensions.Width, dimensions.Y, 6, dimensions.Height), new Rectangle(texture.Width - 6, 0, 6, texture.Height), color);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			CalculatedStyle innerDimensions = GetInnerDimensions();
			if (_isDragging)
			{
				float num = UserInterface.ActiveInstance.MousePosition.X - innerDimensions.X - _dragXOffset;
				_viewPosition = MathHelper.Clamp(num / innerDimensions.Width * _maxViewSize, 0f, _maxViewSize - _viewSize);
			}
			Rectangle handleRectangle = GetHandleRectangle();
			Vector2 mousePosition = UserInterface.ActiveInstance.MousePosition;
			bool isHoveringOverHandle = _isHoveringOverHandle;
			_isHoveringOverHandle = handleRectangle.Contains(new Point((int)mousePosition.X, (int)mousePosition.Y));
			if (!isHoveringOverHandle && _isHoveringOverHandle && Main.hasFocus)
			{
				SoundEngine.PlaySound(12);
			}
			DrawBar(spriteBatch, _texture, dimensions.ToRectangle(), Color.White);
			DrawBar(spriteBatch, _innerTexture, handleRectangle, Color.White * ((_isDragging || _isHoveringOverHandle) ? 1f : 0.85f));
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			base.MouseDown(evt);
			if (evt.Target == this)
			{
				Rectangle handleRectangle = GetHandleRectangle();
				if (handleRectangle.Contains(new Point((int)evt.MousePosition.X, (int)evt.MousePosition.Y)))
				{
					_isDragging = true;
					_dragXOffset = evt.MousePosition.X - handleRectangle.X;
					return;
				}
				CalculatedStyle innerDimensions = GetInnerDimensions();
				float num = UserInterface.ActiveInstance.MousePosition.X - innerDimensions.X - (handleRectangle.Width >> 1);
				_viewPosition = MathHelper.Clamp(num / innerDimensions.Width * _maxViewSize, 0f, _maxViewSize - _viewSize);
			}
		}

		public override void MouseUp(UIMouseEvent evt)
		{
			base.MouseUp(evt);
			_isDragging = false;
		}
	}

	public class FixedUIHorizontalScrollbar : UIHorizontalScrollbar
	{
		internal UserInterface userInterface;

		public FixedUIHorizontalScrollbar(UserInterface userInterface)
		{
			this.userInterface = userInterface;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			base.DrawSelf(spriteBatch);
			UserInterface.ActiveInstance = temp;
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			base.MouseDown(evt);
			UserInterface.ActiveInstance = temp;
		}
	}

	public class InvisibleFixedUIHorizontalScrollbar : FixedUIHorizontalScrollbar
	{
		public InvisibleFixedUIHorizontalScrollbar(UserInterface userInterface) : base(userInterface)
		{
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			//base.DrawSelf(spriteBatch);
			UserInterface.ActiveInstance = temp;
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			base.MouseDown(evt);
			UserInterface.ActiveInstance = temp;
		}
	}
}
