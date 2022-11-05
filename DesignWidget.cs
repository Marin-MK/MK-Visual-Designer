using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace VisualDesigner;

public class DesignWidget : Widget
{
	public static int MousePadding = 3;
	public static int WidgetPadding = 10;
	public static int WidthAdd = WidgetPadding * 2;
	public static int HeightAdd = WidgetPadding * 2;

	public string Name { get; protected set; }
	public bool Selected { get; protected set; }
	public List<Property> Properties { get; init; }

	bool Hovering = false;
	bool Pressing = false;
	bool LeftPressCounts = true;
	bool WithinResizeRegion = false;
	bool WithinMoveRegion = false;
	bool Moving = false;
	bool Resizing = false;
	bool ResizeMoveY = false;
	bool ResizeMoveX = false;
	bool ResizeHorizontalOnly = false;
	bool ResizeVerticalOnly = false;
	bool ChildrenHidden = false;
	bool MayRefresh = true;

	Point MouseOrigin;
	Point PositionOrigin;
	Size SizeOrigin;
	Padding PaddingOrigin;

	public DesignWidget(IContainer Parent) : base(Parent)
	{
		Sprites["box"] = new Sprite(this.Viewport);
		Sprites["box"].X = MousePadding;
		Sprites["box"].Y = MousePadding;
		Sprites["box"].Z = 10;
		MinimumSize = new Size(WidthAdd, HeightAdd);
		SetPadding(WidgetPadding);

		this.Properties = new List<Property>()
		{
			new Property("Name", PropertyType.Text, () => Name, e => this.Name = (string) e),

			new Property("X", PropertyType.Numeric, () => Position.X, e => SetPosition((int) e, Position.Y)),

			new Property("Y", PropertyType.Numeric, () => Position.Y, e => SetPosition(Position.X, (int) e)),

			new Property("Width", PropertyType.Numeric, () => Size.Width - WidgetPadding * 2, e => {
                int w = (int) e + WidgetPadding * 2;
                if (w < MinimumSize.Width) return;
				SetWidth(w);
				if (RightDocked) UpdatePositionAndSizeIfDocked();
				if (this is DesignWindow) ((DesignWindow) this).Center();
			}),

			new Property("Height", PropertyType.Numeric, () => Size.Height - WidgetPadding * 2, e => {
				int h = (int) e + WidgetPadding * 2;
				if (h < MinimumSize.Height) return;
                SetHeight(h);
                if (BottomDocked) UpdatePositionAndSizeIfDocked();
                if (this is DesignWindow) ((DesignWindow) this).Center();
			}),

			new Property("Docking", PropertyType.Dropdown, () => HDocked ? VDocked ? 3 : 1 : VDocked ? 2 : 0, e =>
			{
				int idx = (int) e;
				bool WasHDocked = HDocked;
				bool WasVDocked = VDocked;
				MayRefresh = false;
				SetHDocked(idx == 1 || idx == 3);
				SetVDocked(idx == 2 || idx == 3);
				MayRefresh = true;
				int oldpadu = Padding.Up;
				if (WasHDocked && !HDocked)
				{
					SetWidth(MinimumSize.Width * 4);
					if (Padding.Left != WidgetPadding) SetPosition(Padding.Left - WidgetPadding, Position.Y);
					else SetPosition(Math.Min(Parent.Size.Width - 20, 50), Position.Y);
					SetPadding(WidgetPadding, Padding.Up, Padding.Right, Padding.Down);
				}
				if (WasVDocked && !VDocked)
				{
					SetHeight(MinimumSize.Height * 4);
					if (oldpadu != WidgetPadding) SetPosition(Position.X, Padding.Up - WidgetPadding);
					else SetPosition(Position.X, Math.Min(Parent.Size.Height - 20, 50));
					SetPadding(Padding.Left, WidgetPadding, Padding.Right, WidgetPadding);
				}
				if (!WasHDocked && HDocked)
				{
					SetPadding(Position.X + WidgetPadding, VDocked ? Padding.Up : WidgetPadding, WidgetPadding, WidgetPadding);
					SetPosition(0, Position.Y);
				}
				if (!WasVDocked && VDocked)
				{
					SetPadding(HDocked ? Padding.Left : WidgetPadding, Position.Y + WidgetPadding, WidgetPadding, WidgetPadding);
					SetPosition(Position.X, 0);
				}
				if (HDocked && RightDocked) SetRightDocked(false);
				if (VDocked && BottomDocked) SetBottomDocked(false);
			}, new List<string>() { "None", "Horizontal", "Vertical", "Full" }),

			new Property("Dock to Right", PropertyType.Boolean, () => RightDocked, e => 
			{
				if (HDocked) return;
				SetRightDocked((bool) e);
				if (!RightDocked)
				{
					if (HDocked) SetPadding(Parent.Size.Width / 2 - Size.Width / 2, Padding.Up, Padding.Right, Padding.Down);
					else SetPosition(Parent.Size.Width / 2 - Size.Width / 2, Position.Y);
				}
			}, null, () => !HDocked),

            new Property("Dock to Bottom", PropertyType.Boolean, () => BottomDocked, e =>
			{
				if (VDocked) return;
				SetBottomDocked((bool) e);
				if (!BottomDocked) 
				{
					if (VDocked) SetPadding(Padding.Left, Parent.Size.Height / 2 - Size.Height / 2, Padding.Right, Padding.Down);
					else SetPosition(Position.X, Parent.Size.Height / 2 - Size.Height / 2);
				}
			}, null, () => !VDocked),

			new Property("Padding", PropertyType.Padding, () =>
			{
				return new Padding(Padding.Left - WidgetPadding, Padding.Up - WidgetPadding, Padding.Right - WidgetPadding, Padding.Down - WidgetPadding);
			}, e => {
				MayRefresh = false;
				Padding p = (Padding) e;
				SetPadding(p.Left + WidgetPadding, p.Up + WidgetPadding, p.Right + WidgetPadding, p.Down + WidgetPadding);
				MayRefresh = true;
			})
		};

		SetContextMenuList(new List<IMenuItem>()
		{
			new MenuItem("Create")
			{
				Items = new List<IMenuItem>()
				{
					new MenuItem("Sibling")
					{
						IsClickable = e => e.Value = this is not DesignWindow,
						Items = new List<IMenuItem>()
						{
							new MenuItem("Button")
							{
								OnClicked = _ => CreateSibling("button")
							}
						}
					},
					new MenuItem("Child")
					{
						Items = new List<IMenuItem>()
						{
							new MenuItem("Button")
							{
								OnClicked = _ => CreateChild("button")
							}
						}
					}
				}
			},
			new MenuItem("Copy")
			{

			},
			new MenuItem("Cut")
			{

			},
			new MenuItem("Paste")
			{

			},
			new MenuSeparator(),
			new MenuItem("Delete")
			{

			}
		});

		OnContextMenuOpening += e => e.Value = Selected;
		OnSizeChanged += _ => { if (MayRefresh) Program.ParameterPanel.Refresh(); };
		OnPositionChanged += _ => { if (MayRefresh) Program.ParameterPanel.Refresh(); };
		OnPaddingChanged += _ => { if (MayRefresh) Program.ParameterPanel.Refresh(); };
    }

	public void Select()
	{
		this.Selected = true;
		UpdateBox(true);
	}

	public void Deselect()
	{
		this.Selected = false;
		UpdateBox(true);
	}

	public void Create(DesignWidget Parent, string Type)
	{
		DesignWidget w = null;
		if (Type == "button")
		{
			w = new DesignButton(Parent);
			((DesignButton) w).SetText("Unnamed Button");
			w.SetSize(140 + WidgetPadding * 2, 33 + WidgetPadding * 2);
		}
		int x = ContextMenuMouseOrigin.X - Parent.Viewport.X - w.Size.Width / 2;
        int y = ContextMenuMouseOrigin.Y - Parent.Viewport.Y - w.Size.Height / 2;
        w.SetPosition(x, y);
    }

	public void CreateSibling(string Type)
	{
		Create((DesignWidget) Parent, Type);
	}

	public void CreateChild(string Type)
	{
		Create(this, Type);
	}

	public override void LeftMouseDownInside(MouseEventArgs e)
	{
		base.LeftMouseDownInside(e);
		if (HoveringChild())
		{
			LeftPressCounts = false;
			return;
		}
		LeftPressCounts = true;
		Pressing = true;
		MouseOrigin = new Point(e.X, e.Y);
		PositionOrigin = new Point(Position.X, Position.Y);
		SizeOrigin = new Size(Size.Width, Size.Height);
		PaddingOrigin = new Padding(Padding.Left, Padding.Up, Padding.Right, Padding.Down);
		Resizing = WithinResizeRegion;
		Moving = WithinMoveRegion && this is not DesignWindow;
		Program.DesignWindow.MakeSelectedWidget(this);
		UpdateBox(true);
	}

	public override void RightMouseDownInside(MouseEventArgs e)
	{
		base.RightMouseDownInside(e);
		if (!HoveringChild()) Program.DesignWindow.MakeSelectedWidget(this);
	}

	public override void MouseMoving(MouseEventArgs e)
	{
		base.MouseMoving(e);
		WithinResizeRegion = false;
		bool OldHover = Hovering;
		Hovering = Mouse.Inside && !HoveringChild();
		if (OldHover != Hovering) UpdateBox(false);
		if (Resizing)
		{
            int diffY = e.Y - MouseOrigin.Y;
            int diffX = e.X - MouseOrigin.X;
            if (ResizeVerticalOnly) diffX = 0;
            if (ResizeHorizontalOnly) diffY = 0;
            if (diffX == 0 && diffY == 0) return;
			if (this is DesignWindow && !ChildrenHidden)
			{
				Widgets.ForEach(w => w.SetVisible(false));
				ChildrenHidden = true;
			}
			int padl = PaddingOrigin.Left;
			int padu = PaddingOrigin.Up;
			int padr = PaddingOrigin.Right;
			int padd = PaddingOrigin.Down;
			if (diffX != 0)
			{
				if (ResizeMoveX)
				{
					if (HDocked) padl += diffX;
					else SetPosition(PositionOrigin.X + diffX, Position.Y);
					diffX = -diffX;
				}
				if (HDocked)
				{
					if (!ResizeMoveX) padr -= diffX;
				}
				else SetWidth(SizeOrigin.Width + diffX);
			}
			if (diffY != 0)
			{
				if (ResizeMoveY)
				{
					if (VDocked) padu += diffY;
					else SetPosition(Position.X, PositionOrigin.Y + diffY);
					diffY = -diffY;
				}
				if (VDocked)
				{
					if (!ResizeMoveY) padd -= diffY;
				}
				else SetHeight(SizeOrigin.Height + diffY);
			}
			SetPadding(padl, padu, padr, padd);
			if (this is DesignWindow) Drawn = true;
			UpdateBox(true);
			e.Handled = true;
        }
		else if (Moving)
		{
			int diffX = e.X - MouseOrigin.X;
			int diffY = e.Y - MouseOrigin.Y;
			if (HDocked) diffX = 0;
			if (VDocked) diffY = 0;
			if (diffX == 0 && diffY == 0) return;
			Input.SetCursor(CursorType.SizeAll);
			int padl = PaddingOrigin.Left;
			SetPosition(PositionOrigin.X + diffX, PositionOrigin.Y + diffY);
			e.Handled = true;
		}
        else if (Hovering)
        {
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            if (rx >= WidgetPadding && rx < Size.Width - WidgetPadding && ry >= WidgetPadding && ry < Size.Height - WidgetPadding)
			{
				// Inside widget, move the widget.
				Input.SetCursor(CursorType.Arrow);
				WithinMoveRegion = true;
			}
			else
			{
				// Around the edge, resize the widget.
				bool Horizontal = false;
				bool Vertical = false;
				bool TopLeftOrBottomRight = false;
				bool TopRightOrBottomLeft = false;
				ResizeMoveX = false;
				ResizeMoveY = false;
				if (rx <= WidgetPadding * 2)
				{
					ResizeMoveX = true;
					// Top Left
					if (ry <= WidgetPadding * 2)
					{
						TopLeftOrBottomRight = true;
						ResizeMoveY = true;
					}
					// Bottom Left
					else if (ry >= Size.Height - WidgetPadding * 2)
					{
						TopRightOrBottomLeft = true;
					}
					// Left
					else if (rx <= WidgetPadding) Horizontal = true;
				}
				else if (rx >= Size.Width - WidgetPadding * 2)
				{
					// Top Right
					if (ry <= WidgetPadding * 2)
					{
						TopRightOrBottomLeft = true;
						ResizeMoveY = true;
					}
					// Bottom Right
					else if (ry >= Size.Height - WidgetPadding * 2) TopLeftOrBottomRight = true;
					// Right
					else if (rx >= Size.Width - WidgetPadding) Horizontal = true;
				}
				// Up & Down
				else if (ry <= WidgetPadding || ry >= Size.Height - WidgetPadding)
				{
					ResizeMoveY = ry <= WidgetPadding;
					Vertical = true;
				}
				if (Horizontal) Input.SetCursor(CursorType.SizeWE);
				else if (Vertical) Input.SetCursor(CursorType.SizeNS);
				else if (TopLeftOrBottomRight) Input.SetCursor(CursorType.SizeNWSE);
				else if (TopRightOrBottomLeft) Input.SetCursor(CursorType.SizeNESW);
				else Input.SetCursor(CursorType.Arrow);
				ResizeHorizontalOnly = Horizontal;
				ResizeVerticalOnly = Vertical;
				WithinResizeRegion = true;
				e.Handled = true;
			}
		}
		else if (!Pressing && !HoveringChild() && Program.DesignContainer.Mouse.Inside) Input.SetCursor(CursorType.Arrow);
	}

	bool HoveringChild()
	{
		return Widgets.Any(w => w.Mouse.Inside || ((DesignWidget) w).HoveringChild());
	}

	public override void LeftMouseUp(MouseEventArgs e)
	{
		base.LeftMouseUp(e);
		if (Mouse.LeftStartedInside && LeftPressCounts)
        {
            if (Resizing)
			{
				Redraw();
			}
			Pressing = false;
			MouseOrigin = null;
			PositionOrigin = null;
			SizeOrigin = null;
			Resizing = false;
            ResizeMoveX = false;
            ResizeMoveY = false;
			WithinResizeRegion = false;
			WithinMoveRegion = false;
			ResizeHorizontalOnly = false;
			ResizeVerticalOnly = false;
			Moving = false;
			if (ChildrenHidden)
			{
				Widgets.ForEach(w => w.SetVisible(true));
				ChildrenHidden = false;
			}
            Input.SetCursor(CursorType.Arrow);
			UpdateBox(true);
		}
	}

	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
		UpdateBox(true);
	}

	void UpdateBox(bool Redraw)
	{
		Color CornerColor = Color.WHITE;
		Color LineColor = Color.BLACK;

		if (Pressing || Selected)
		{
			CornerColor = new Color(128, 128, 255);
			LineColor = CornerColor;
		}

		int BoxSize = WidgetPadding - MousePadding;
		int LinePos = BoxSize / 2;

		Sprites["box"].Visible = Selected || Pressing || Hovering;

		if (!Redraw && !((Hovering || Pressing) && Sprites["box"].Bitmap == null)) return;
		Sprites["box"].Bitmap?.Dispose();
		Sprites["box"].Bitmap = new Bitmap(Size.Width - MousePadding * 2, Size.Height - MousePadding * 2);
		Sprites["box"].Bitmap.Unlock();
		Sprites["box"].Bitmap.DrawRect(LinePos, LinePos, Size.Width - MousePadding * 2 - LinePos * 2 - 1, Size.Height - MousePadding * 2 - LinePos * 2 - 1, LineColor);
		Sprites["box"].Bitmap.DrawRect(0, 0, BoxSize, BoxSize, Color.BLACK);
		Sprites["box"].Bitmap.FillRect(1, 1, BoxSize - 2, BoxSize - 2, CornerColor);
        Sprites["box"].Bitmap.DrawRect(Size.Width - MousePadding * 2 - BoxSize - 1, 0, BoxSize, BoxSize, Color.BLACK);
        Sprites["box"].Bitmap.FillRect(Size.Width - MousePadding * 2 - BoxSize, 1, BoxSize - 2, BoxSize - 2, CornerColor);
        Sprites["box"].Bitmap.DrawRect(0, Size.Height - MousePadding * 2 - BoxSize - 1, BoxSize, BoxSize, Color.BLACK);
        Sprites["box"].Bitmap.FillRect(1, Size.Height - MousePadding * 2 - BoxSize, BoxSize - 2, BoxSize - 2, CornerColor);
        Sprites["box"].Bitmap.DrawRect(Size.Width - MousePadding * 2 - BoxSize - 1, Size.Height - MousePadding * 2 - BoxSize - 1, BoxSize, BoxSize, Color.BLACK);
		Sprites["box"].Bitmap.FillRect(Size.Width - MousePadding * 2 - BoxSize, Size.Height - MousePadding * 2 - BoxSize, BoxSize - 2, BoxSize - 2, CornerColor);
		Sprites["box"].Bitmap.Lock();
	}
}
