using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace ItemChecklist.UIElements
{
	public class UIHorizontalGrid : UIElement
	{
		public delegate bool ElementSearchMethod(UIElement element);

		private class UIInnerList : UIElement
		{
			public override bool ContainsPoint(Vector2 point)
			{
				return true;
			}

			protected override void DrawChildren(SpriteBatch spriteBatch)
			{
				Vector2 position = Parent.GetDimensions().Position();
				Vector2 dimensions = new(Parent.GetDimensions().Width, Parent.GetDimensions().Height);
				foreach (UIElement current in Elements)
				{
					Vector2 position2 = current.GetDimensions().Position();
					Vector2 dimensions2 = new(current.GetDimensions().Width, current.GetDimensions().Height);
					if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2))
					{
						current.Draw(spriteBatch);
					}
				}
			}
		}

		public List<UIElement> _items = new();
		protected UIHorizontalScrollbar _scrollbar;
		internal UIElement _innerList = new UIInnerList();
		private float _innerListWidth;
		public float ListPadding = 5f;

		public static Texture2D moreLeftTexture;
		public static Texture2D moreRightTexture;

		public int Count
		{
			get
			{
				return _items.Count;
			}
		}

		// todo, vertical/horizontal orientation, left to right, etc?
		public UIHorizontalGrid()
		{
			_innerList.OverflowHidden = false;
			_innerList.Width.Set(0f, 1f);
			_innerList.Height.Set(0f, 1f);
			OverflowHidden = true;
			Append(_innerList);
		}

		public float GetTotalWidth()
		{
			return _innerListWidth;
		}

		public void Goto(ElementSearchMethod searchMethod, bool center = false)
		{
			for (int i = 0; i < _items.Count; i++)
			{
				if (searchMethod(_items[i]))
				{
					_scrollbar.ViewPosition = _items[i].Left.Pixels;
					if (center)
					{
						_scrollbar.ViewPosition = _items[i].Left.Pixels - GetInnerDimensions().Width / 2 + _items[i].GetOuterDimensions().Width / 2;
					}
					return;
				}
			}
		}

		public virtual void Add(UIElement item)
		{
			_items.Add(item);
			_innerList.Append(item);
			UpdateOrder();
			_innerList.Recalculate();
		}

		public virtual void AddRange(IEnumerable<UIElement> items)
		{
			foreach (var item in items)
			{
				_items.Add(item);
				_innerList.Append(item);
			}

			UpdateOrder();
			_innerList.Recalculate();
		}

		public virtual bool Remove(UIElement item)
		{
			_innerList.RemoveChild(item);
			UpdateOrder();
			return _items.Remove(item);
		}

		public virtual void Clear()
		{
			_innerList.RemoveAllChildren();
			_items.Clear();
		}

		public override void Recalculate()
		{
			base.Recalculate();
			UpdateScrollbar();
		}

		public override void ScrollWheel(UIScrollWheelEvent evt)
		{
			base.ScrollWheel(evt);
			if (_scrollbar != null)
			{
				_scrollbar.ViewPosition -= evt.ScrollWheelValue;
			}
		}

		public override void RecalculateChildren()
		{
			float availableHeight = GetInnerDimensions().Height;
			base.RecalculateChildren();
			float left = 0f;
			float top = 0f;
			float maxRowWidth = 0f;
			for (int i = 0; i < _items.Count; i++)
			{
				var item = _items[i];
				var outerDimensions = item.GetOuterDimensions();
				if (top + outerDimensions.Height > availableHeight && top > 0)
				{
					left += maxRowWidth + ListPadding;
					top = 0;
					maxRowWidth = 0;
				}
				maxRowWidth = Math.Max(maxRowWidth, outerDimensions.Width);
				item.Top.Set(top, 0f);
				top += outerDimensions.Height + ListPadding;
				item.Left.Set(left, 0f);
				item.Recalculate();
			}
			_innerListWidth = left + maxRowWidth;
		}

		private void UpdateScrollbar()
		{
			if (_scrollbar == null)
			{
				return;
			}
			_scrollbar.SetView(GetInnerDimensions().Width, _innerListWidth);
		}

		public void SetScrollbar(UIHorizontalScrollbar scrollbar)
		{
			_scrollbar = scrollbar;
			UpdateScrollbar();
		}

		public void UpdateOrder()
		{
			_items.Sort(SortMethod);
			UpdateScrollbar();
		}

		public int SortMethod(UIElement item1, UIElement item2)
		{
			return item1.CompareTo(item2);
		}

		public override List<SnapPoint> GetSnapPoints()
		{
			List<SnapPoint> list = new();
			if (GetSnapPoint(out SnapPoint item))
			{
				list.Add(item);
			}
			foreach (UIElement current in _items)
			{
				list.AddRange(current.GetSnapPoints());
			}
			return list;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			//var r = GetDimensions().ToRectangle();
			//r.Inflate(-10,-10);
			//spriteBatch.Draw(Main.magicPixel, r, Color.Yellow);
			if (_scrollbar != null)
			{
				_innerList.Left.Set(-_scrollbar.GetValue(), 0f);
			}
			Recalculate();
		}

		public bool drawArrows;
		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			base.DrawChildren(spriteBatch);
			if (drawArrows)
			{
				var inner = GetInnerDimensions().ToRectangle();
				if (_scrollbar.ViewPosition != 0)
				{
					spriteBatch.Draw(moreLeftTexture, new Vector2(inner.X, inner.Y), Color.White * .5f);
				}
				if (_scrollbar.ViewPosition < _innerListWidth - inner.Width)
				{
					spriteBatch.Draw(moreRightTexture, new Vector2(inner.Right - moreRightTexture.Width, inner.Y), Color.White * .5f);
				}
			}
		}
	}
}