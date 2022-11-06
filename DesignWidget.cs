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
	public static int SnapDifference = 12;

	public string Name { get; protected set; }
	public bool Selected { get; protected set; }
	public List<Property> Properties { get; init; }
	public Point LocalPosition { get; protected set; } = new Point(0, 0);

	public bool Hovering = false;
	public bool Pressing = false;
	public bool LeftPressCounts = true;
	public bool WithinResizeRegion = false;
	public bool WithinMoveRegion = false;
	public bool Moving = false;
	public bool MovingMultiple = false;
	public bool Resizing = false;
	public bool CreatingSelection = false;
	public bool ResizeMoveY = false;
	public bool ResizeMoveX = false;
	public bool ResizeHorizontalOnly = false;
	public bool ResizeVerticalOnly = false;
	public bool ChildrenHidden = false;
	public bool MayRefresh = true;

	protected Point MouseOrigin;
	protected Point PositionOrigin;
	protected Size SizeOrigin;
	protected Padding PaddingOrigin;

	protected bool HSnapped;
	protected bool VSnapped;
	protected int SnapX;
	protected int SnapWidth;
	protected int SnapY;
	protected int SnapHeight;
    protected int SnapPaddingLeft;
    protected int SnapPaddingRight;
    protected int SnapPaddingUp;
    protected int SnapPaddingDown;

	protected Container SelectionContainer;

    public DesignWidget(IContainer Parent, string BaseName) : base(Parent)
	{
		this.Name = Program.DesignWindow?.GetName(BaseName);
		Sprites["box"] = new Sprite(this.Viewport);
		Sprites["box"].X = MousePadding;
		Sprites["box"].Y = MousePadding;
		Sprites["box"].Z = 10;
		MinimumSize = new Size(WidthAdd, HeightAdd);
		SetPadding(WidgetPadding);

		SelectionContainer = new Container(this);
		SelectionContainer.SetDocked(true);
		SelectionContainer.SetPadding(WidgetPadding);
		SelectionContainer.Sprites["sel"] = new Sprite(SelectionContainer.Viewport);

		OnWidgetSelected += WidgetSelected;

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

			new Property("BG Color", PropertyType.Color, () => BackgroundColor, e => SetBackgroundColor((Color) e)),

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

		RegisterShortcuts(new List<Shortcut>()
		{
			new Shortcut(this, new Key(Keycode.LEFT), _ => MoveH(-1)),
            new Shortcut(this, new Key(Keycode.LEFT, Keycode.SHIFT), _ => MoveH(-10)),
            new Shortcut(this, new Key(Keycode.LEFT, Keycode.CTRL), _ => MoveH(-25)),
            new Shortcut(this, new Key(Keycode.LEFT, Keycode.SHIFT, Keycode.CTRL), _ => MoveH(-50)),
            new Shortcut(this, new Key(Keycode.UP), _ => MoveV(-1)),
			new Shortcut(this, new Key(Keycode.UP, Keycode.SHIFT), _ => MoveV(-10)),
            new Shortcut(this, new Key(Keycode.UP, Keycode.CTRL), _ => MoveV(-25)),
            new Shortcut(this, new Key(Keycode.UP, Keycode.SHIFT, Keycode.CTRL), _ => MoveV(-50)),
            new Shortcut(this, new Key(Keycode.RIGHT), _ => MoveH(1)),
			new Shortcut(this, new Key(Keycode.RIGHT, Keycode.SHIFT), _ => MoveH(10)),
            new Shortcut(this, new Key(Keycode.RIGHT, Keycode.CTRL), _ => MoveH(25)),
            new Shortcut(this, new Key(Keycode.RIGHT, Keycode.SHIFT, Keycode.CTRL), _ => MoveH(50)),
            new Shortcut(this, new Key(Keycode.DOWN), _ => MoveV(1)),
			new Shortcut(this, new Key(Keycode.DOWN, Keycode.SHIFT), _ => MoveV(10)),
            new Shortcut(this, new Key(Keycode.DOWN, Keycode.CTRL), _ => MoveV(25)),
            new Shortcut(this, new Key(Keycode.DOWN, Keycode.SHIFT, Keycode.CTRL), _ => MoveV(50))
        });

		OnContextMenuOpening += e => e.Value = Selected;
		OnSizeChanged += _ => { if (MayRefresh) Program.ParameterPanel.Refresh(); };
		OnPositionChanged += _ =>
		{
			if (MayRefresh) Program.ParameterPanel.Refresh();
			if (this is not DesignWindow)
			{
                Point ParentPos = ((DesignWidget) Parent).LocalPosition;
                LocalPosition = new Point(ParentPos.X + Position.X + Padding.Left, ParentPos.Y + Position.Y + Padding.Up);
			}
		};
		OnPaddingChanged += _ => 
		{
			if (MayRefresh) Program.ParameterPanel.Refresh();
            if (this is not DesignWindow)
            {
                Point ParentPos = ((DesignWidget) Parent).LocalPosition;
                LocalPosition = new Point(ParentPos.X + Position.X + Padding.Left, ParentPos.Y + Position.Y + Padding.Up);
            }
        };
    }

	public void Select(bool AllowMultiple)
	{
		if (!this.Selected)
		{
			if (!AllowMultiple) Program.DesignWindow.DeselectAll();
			this.Selected = true;
			UpdateBox(true);
			Window.UI.SetSelectedWidget(this);
			Program.DesignWindow.SelectedWidgets.Add(this);
			if (Program.DesignWindow.SelectedWidgets.Count == 1) Program.ParameterPanel.SetWidget(this);
			else Program.ParameterPanel.SetWidget(null);
		}
    }

	public bool ContainsChild(DesignWidget Widget)
	{
		return Widgets.Contains(Widget) || Widgets.Any(w => w is DesignWidget && ((DesignWidget) w).ContainsChild(Widget));
	}

	public bool IsOrContainsSelectedExcept(DesignWidget Widget)
	{
		if (this.Selected && this != Widget) return true;
		return Widgets.Any(w => w is DesignWidget && ((DesignWidget) w).IsOrContainsSelectedExcept(Widget));
	}

	public void Deselect()
	{
		if (this.Selected)
		{
			this.Selected = false;
			UpdateBox(true);
		}
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

    public bool ExistsWithName(string Name)
    {
        return this.Name == Name || this.Widgets.Any(w => w is DesignWidget && ((DesignWidget) w).ExistsWithName(Name));
    }

    public string GetName(string BaseName)
    {
        string Name = BaseName;
        int i = 0;
        while (true)
        {
            Name = BaseName + i.ToString();
            if (!ExistsWithName(Name)) return Name;
            i++;
        }
    }

	protected List<DesignWidget> GetHoveredWidgets(int gx, int gy)
	{
		List<DesignWidget> HoveringWidgets = new List<DesignWidget>();
        for (int i = 0; i < Widgets.Count; i++)
        {
			if (Widgets[i] is not DesignWidget) continue;
            DesignWidget w = (DesignWidget) Widgets[i];
            if (w.Viewport.Rect.Contains(gx, gy))
            {
				// Within this widget, now check its children
				List<DesignWidget> hov = w.GetHoveredWidgets(gx, gy);
				HoveringWidgets.Add(w);
				if (hov.Count > 0) HoveringWidgets.AddRange(hov);
            }
        }
		return HoveringWidgets;
    }

	public List<DesignWidget> GetChildWidgets()
	{
		return Widgets.FindAll(w => w is DesignWidget).Select(w => (DesignWidget) w).ToList();
	}

    public override void LeftMouseDownInside(MouseEventArgs e)
	{
		base.LeftMouseDownInside(e);
		if (Program.DesignWindow.HoveringWidget != this)
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
		CreatingSelection = WithinMoveRegion && this is DesignWindow;
		if (Input.Press(Keycode.CTRL) && Moving)
		{
			Moving = false;
			CreatingSelection = true;
        }
        MovingMultiple = Program.DesignWindow.SelectedWidgets.Count > 1;
        if (!Input.Press(Keycode.SHIFT) && (!Moving || !MovingMultiple)) Program.DesignWindow.DeselectAll();
        if (Moving)
		{
			// Deselect all selected child widgets, because they will already be moved by moving the parent
			Program.DesignWindow.DeselectWidgetsWithoutSharedParents(this);
            if (MovingMultiple)
			{
				Program.DesignWindow.SelectedWidgets.FindAll(w => w != this).ForEach(dw =>
				{
					dw.LeftPressCounts = true;
					dw.Pressing = true;
					dw.MouseOrigin = MouseOrigin;
					dw.PositionOrigin = new Point(dw.Position.X, dw.Position.Y);
					dw.PaddingOrigin = new Padding(dw.Padding.Left, dw.Padding.Up, dw.Padding.Right, dw.Padding.Down);
					dw.Moving = true;
					dw.MovingMultiple = true;
				});
			}
		}
        if (!CreatingSelection) Select(Input.Press(Keycode.SHIFT));
        UpdateBox(true);
	}

	public override void RightMouseDownInside(MouseEventArgs e)
	{
		base.RightMouseDownInside(e);
		if (Program.DesignWindow.HoveringWidget == this) Select(false);
	}

	public void SetHorizontallySnapped(int xadd = 0)
	{
		HSnapped = true;
		SnapX = Position.X + xadd;
		SnapWidth = Size.Width;
		SnapPaddingLeft = Padding.Left;
		SnapPaddingRight = Padding.Right;
	}

	public void SetVerticallySnapped()
	{
		VSnapped = true;
		SnapY = Position.Y;
		SnapHeight = Size.Height;
		SnapPaddingUp = Padding.Up;
		SnapPaddingDown = Padding.Down;
	}

	public void ResetHSnap()
	{
		HSnapped = false;
		SnapX = -1;
		SnapWidth = -1;
	}

	public void ResetVSnap()
	{
		VSnapped = false;
		SnapY = -1;
		SnapHeight = -1;
	}

	public void MoveH(int Pixels, bool CanMoveOtherSelectedWidgets = true)
	{
		if (this is DesignWindow) return;
		if (!HDocked) SetPosition(Position.X + Pixels, Position.Y);
		if (CanMoveOtherSelectedWidgets) Program.DesignWindow.SelectedWidgets.FindAll(w => w != this).ForEach(w => w.MoveH(Pixels, false));
	}

	public void MoveV(int Pixels, bool CanMoveOtherSelectedWidgets = true)
    {
        if (this is DesignWindow) return;
        if (!VDocked) SetPosition(Position.X, Position.Y + Pixels);
        if (CanMoveOtherSelectedWidgets) Program.DesignWindow.SelectedWidgets.FindAll(w => w != this).ForEach(w => w.MoveV(Pixels, false));
    }

    // Move to DesignWidget.cs and make selections possible for every widget (e.g. for within a large Container widget, while not selecting random things outside the container that may overlap).
    protected Rect? DrawSelectionBox(int x, int y, int width, int height)
    {
        SelectionContainer.Sprites["sel"].Bitmap?.Dispose();
        if (x < 0)
        {
            width += x;
            x = 0;
        }
		if (x >= SelectionContainer.Size.Width) return null;
        if (x + width >= SelectionContainer.Size.Width) width -= x + width - SelectionContainer.Size.Width;
        if (y < 0)
        {
            height += y;
            y = 0;
        }
		if (y >= SelectionContainer.Size.Height) return null;
        if (y + height >= SelectionContainer.Size.Height) height -= y + height - SelectionContainer.Size.Height;
		if (width < 1 || height < 1) return null;
		SelectionContainer.Sprites["sel"].X = x;
		SelectionContainer.Sprites["sel"].Y = y;
        SelectionContainer.Sprites["sel"].Bitmap = new Bitmap(width, height);
        SelectionContainer.Sprites["sel"].Bitmap.Unlock();
		for (int dy = 0; dy < height; dy++)
		{
			for (int dx = 0; dx < width; dx++)
			{
				if (dx == 0 || dy == 0 || dx == width - 1 || dy == height - 1)
				{
					if (dx != 2 && dx != width -3 && (dx == 0 || dx == 1 || dx == width - 1 || dx == width - 2 || ((dx + 2) / 4) % 2 == 0) &&
						dy != 2 && dy != height - 3 && (dy == 0 || dy == 1 || dy == height - 1 || dy == height - 2 || ((dy + 2) / 4) % 2 == 0))
						SelectionContainer.Sprites["sel"].Bitmap.SetPixel(dx, dy, new Color(128, 128, 128));
				}
			}
		}
        SelectionContainer.Sprites["sel"].Bitmap.Lock();
		return new Rect(x, y, width, height);
    }

    void DisposeSelectionBox()
    {
        SelectionContainer.Sprites["sel"].Bitmap?.Dispose();
    }

	void SelectWidgetsInArea(Rect Area)
	{
		Program.DesignWindow.DeselectAll();
		for (int i = 0; i < Widgets.Count; i++)
		{
			if (Widgets[i] is not DesignWidget) continue;
			DesignWidget w = (DesignWidget) Widgets[i];
			if (Area.Contains(w.Position.X + w.Padding.Left + w.Size.Width / 2, w.Position.Y + w.Padding.Right + w.Size.Height / 2))
			{
				w.Select(true);
			}
		}
	}

    public override void MouseMoving(MouseEventArgs e)
	{
		base.MouseMoving(e);
		WithinResizeRegion = false;
		bool OldHover = Hovering;
		Hovering = Program.DesignWindow.HoveringWidget == this;
		bool HasSnapped = false;
		if (OldHover != Hovering) UpdateBox(false);
		if (Resizing)
        {
            int diffX = e.X - MouseOrigin.X;
            int diffY = e.Y - MouseOrigin.Y;
            if (ResizeVerticalOnly) diffX = 0;
            if (ResizeHorizontalOnly) diffY = 0;
            if (this is DesignWindow && !ChildrenHidden)
			{
				Widgets.ForEach(w => w.SetVisible(false));
				ChildrenHidden = true;
			}
			int padl = PaddingOrigin.Left;
			int padu = PaddingOrigin.Up;
			int padr = PaddingOrigin.Right;
			int padd = PaddingOrigin.Down;
			if (ResizeMoveX)
			{
				if (HDocked)
				{
                    if (HSnapped)
                    {
						if (Math.Abs(PaddingOrigin.Left + diffX - SnapPaddingLeft) < SnapDifference && !Input.Press(Keycode.ALT))
						{
							diffX = SnapPaddingLeft - PaddingOrigin.Left;
							HasSnapped = true;
						}
                    }
                    padl += diffX;
				}
				else
				{
                    if (HSnapped)
                    {
						if (Math.Abs(PositionOrigin.X + diffX - SnapX) < SnapDifference && !Input.Press(Keycode.ALT))
						{
							diffX = SnapX - PositionOrigin.X;
							HasSnapped = true;
						}
					}
                    SetPosition(PositionOrigin.X + diffX, Position.Y);
				}
				diffX = -diffX;
			}
			if (HDocked)
			{
				if (!ResizeMoveX)
				{
					if (HSnapped)
					{
						if (Math.Abs(PaddingOrigin.Right + diffX - SnapPaddingRight) < SnapDifference && !Input.Press(Keycode.ALT))
						{
							diffX = SnapPaddingRight - PaddingOrigin.Right;
                            HasSnapped = true;
                        }
					}
					padr -= diffX;
				}
			}
			else
			{
                if (HSnapped)
                {
					if (Math.Abs(SizeOrigin.Width + diffX - SnapWidth) < SnapDifference && !Input.Press(Keycode.ALT))
					{
						diffX = SnapWidth - SizeOrigin.Width;
                        HasSnapped = true;
                    }
                }
                SetWidth(SizeOrigin.Width + diffX);
			}
            if (ResizeMoveY)
			{
				if (VDocked)
				{
                    if (VSnapped)
                    {
						if (Math.Abs(PaddingOrigin.Up + diffY - SnapPaddingUp) < SnapDifference && !Input.Press(Keycode.ALT))
						{
							diffY = SnapPaddingUp - PaddingOrigin.Up;
                            HasSnapped = true;
                        }
                    }
                    padu += diffY;
				}
				else
				{
                    if (VSnapped)
                    {
						if (Math.Abs(PositionOrigin.Y + diffY - SnapY) < SnapDifference && !Input.Press(Keycode.ALT))
						{
							diffY = SnapY - PositionOrigin.Y;
                            HasSnapped = true;
                        }
                    }
                    SetPosition(Position.X, PositionOrigin.Y + diffY);
				}
				diffY = -diffY;
			}
			if (VDocked)
			{
				if (!ResizeMoveY)
				{
					if (VSnapped)
					{
						if (Math.Abs(PaddingOrigin.Down + diffY - SnapPaddingDown) < SnapDifference && !Input.Press(Keycode.ALT))
						{
							diffY = SnapPaddingDown - PaddingOrigin.Down;
                            HasSnapped = true;
                        }
					}
					padu -= diffY;
				}
			}
			else
			{
                if (VSnapped)
                {
					if (Math.Abs(SizeOrigin.Height + diffY - SnapHeight) < SnapDifference && !Input.Press(Keycode.ALT))
					{
						diffY = SnapHeight - SizeOrigin.Height;
                        HasSnapped = true;
                    }
                }
                SetHeight(SizeOrigin.Height + diffY);
			}
			SetPadding(padl, padu, padr, padd);
			if (this is DesignWindow)
			{
				Drawn = true;
				Sprites["shadow"].Visible = false;
				Sprites["window"].Visible = false;
				Sprites["title"].Visible = false;
			}
			UpdateBox(true);
			e.Handled = true;
			if (!HasSnapped) Program.DesignWindow.DisposeSnaps();
            Program.DesignWindow.DrawSnaps(this, true, ResizeMoveX, ResizeMoveY);
        }
		else if (Moving)
        {
            if (!MovingMultiple) Program.DesignWindow.DrawSnaps(this, false, false, false);
            int diffX = e.X - MouseOrigin.X;
			int diffY = e.Y - MouseOrigin.Y;
			if (HDocked) diffX = 0;
			if (VDocked) diffY = 0;
			if (HSnapped)
			{
				if (Math.Abs(PositionOrigin.X + diffX - SnapX) < SnapDifference && !Input.Press(Keycode.ALT))
				{
					diffX = SnapX - PositionOrigin.X;
                    HasSnapped = true;
                }
			}
			if (VSnapped)
			{
				if (Math.Abs(PositionOrigin.Y + diffY - SnapY) < SnapDifference && !Input.Press(Keycode.ALT))
				{
					diffY = SnapY - PositionOrigin.Y;
                    HasSnapped = true;
                }
			}
            if (diffX != 0 || diffY != 0) Input.SetCursor(CursorType.SizeAll);
			int padl = PaddingOrigin.Left;
			SetPosition(PositionOrigin.X + diffX, PositionOrigin.Y + diffY);
			if (!HasSnapped) Program.DesignWindow.DisposeSnaps();
		}
		else if (CreatingSelection)
		{
            int x1 = MouseOrigin.X - Viewport.X - WidgetPadding;
            int y1 = MouseOrigin.Y - Viewport.Y - WidgetPadding;
            int x2 = e.X - Viewport.X - WidgetPadding;
            int y2 = e.Y - Viewport.Y - WidgetPadding;
            int minx = Math.Min(x1, x2);
            int miny = Math.Min(y1, y2);
            int maxx = Math.Max(x1, x2);
            int maxy = Math.Max(y1, y2);
            Rect? SelectionBox = DrawSelectionBox(minx, miny, maxx - minx, maxy - miny);
			if (SelectionBox != null) SelectWidgetsInArea(SelectionBox);
			else Select(Input.Press(Keycode.SHIFT));
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
				WithinResizeRegion = false;
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
				WithinMoveRegion = false;
				e.Handled = true;
			}
		}
		else if (!Pressing && Program.DesignContainer.Mouse.Inside) Input.SetCursor(CursorType.Arrow);
	}

	public override void LeftMouseUp(MouseEventArgs e)
	{
		base.LeftMouseUp(e);
		if (LeftPressCounts)
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
			MovingMultiple = false;
			CreatingSelection = false;
			ResetHSnap();
			ResetVSnap();
			DisposeSelectionBox();
			if (ChildrenHidden)
			{
				Widgets.ForEach(w => w.SetVisible(true));
				ChildrenHidden = false;
                Sprites["shadow"].Visible = true;
                Sprites["window"].Visible = true;
                Sprites["title"].Visible = true;
            }
            Input.SetCursor(CursorType.Arrow);
			UpdateBox(true);
			Program.DesignWindow.DisposeSnaps();
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
