using RPGStudioMK.Undo;
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

	public virtual bool PasteAsChildren => true;

	public string Name { get; set; }
	public bool Selected { get; protected set; }
	public List<Property> Properties { get; init; }
	public Point LocalPosition { get; protected set; } = new Point(0, 0);

	public bool Hovering = false;
	public bool Pressing = false;
	public bool LeftPressCounts = false;
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

	public Point MouseOrigin;
	public Point PositionOrigin;
	public Size SizeOrigin;
	public Padding PaddingOrigin;

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

	public DesignWidget(IContainer Parent, string? BaseName = null) : base(Parent)
	{
		if (!string.IsNullOrEmpty(BaseName)) this.Name = Program.DesignWindow?.GetName(BaseName) ?? BaseName ?? "Unknown";
		Sprites["_bg"].X = WidgetPadding;
		Sprites["_bg"].Y = WidgetPadding;
		Sprites["_box"] = new Sprite(this.Viewport);
		Sprites["_box"].X = MousePadding;
		Sprites["_box"].Y = MousePadding;
		Sprites["_box"].Z = 10;
		MinimumSize = new Size(WidthAdd, HeightAdd);

		SelectionContainer = new Container(this);
		SelectionContainer.SetDocked(true);
		SelectionContainer.Sprites["_sel"] = new Sprite(SelectionContainer.Viewport);

		OnWidgetSelected += WidgetSelected;

		this.Properties = new List<Property>()
		{
			new Property("Name", PropertyType.Text, () => Name, e =>
			{
				string OldName = Name;
				string NewName = (string) e;
				DesignWidget? dw = Program.DesignWindow.GetWidgetByName(NewName);
				if (dw != null)
				{
					new MessageBox("Error", $"A widget already exists with the name '{NewName}' (type '{dw.GetType().Name}').", ButtonType.OK, IconType.Error);
					Program.ParameterPanel.Refresh();
				}
				else
				{
					this.Name = NewName;
					if (OldName != Name) Undo.NameUndoAction.Register(this, OldName, Name, true);
				}
			}),

			new Property("X", PropertyType.Numeric, () => Position.X, e =>
			{
				Point OldPoint = this.Position;
				SetPosition((int) e, Position.Y);
				Point NewPoint = this.Position;
				if (!Moving && !OldPoint.Equals(NewPoint)) Undo.GenericUndoAction<Point>.Register(this, "SetPosition", OldPoint, NewPoint, true);
			}),

			new Property("Y", PropertyType.Numeric, () => Position.Y, e =>
			{
				Point OldPoint = this.Position;
				SetPosition(Position.X, (int) e);
				Point NewPoint = this.Position;
				if (!Moving && !OldPoint.Equals(NewPoint)) Undo.GenericUndoAction<Point>.Register(this, "SetPosition", OldPoint, NewPoint, true);
			}),

			new Property("Width", PropertyType.Numeric, () => Size.Width - WidthAdd, e => 
			{
				Size OldSize = this.Size;
                int w = (int) e + WidthAdd;
                if (w < MinimumSize.Width) return;
				SetWidth(w);
				if (RightDocked) UpdatePositionAndSizeIfDocked();
				if (this is DesignWindow) ((DesignWindow) this).Center();
				Size NewSize = this.Size;
				if (!Resizing && !OldSize.Equals(NewSize)) Undo.GenericUndoAction<Size>.Register(this, "SetSize", OldSize, NewSize, true);
			}),

			new Property("Height", PropertyType.Numeric, () => Size.Height - HeightAdd, e => 
			{
				Size OldSize = this.Size;
				int h = (int) e + HeightAdd;
				if (h < MinimumSize.Height) return;
                SetHeight(h);
                if (BottomDocked) UpdatePositionAndSizeIfDocked();
                if (this is DesignWindow) ((DesignWindow) this).Center();
				Size NewSize = this.Size;
				if (!Resizing && !OldSize.Equals(NewSize)) Undo.GenericUndoAction<Size>.Register(this, "SetSize", OldSize, NewSize, true);
			}),

			new Property("Auto-Resize", PropertyType.Boolean, () => AutoResize, e =>
			{
				bool OldAutoResize = this.AutoResize;
				this.AutoResize = (bool) e;
				this.UpdateAutoScroll();
				if (OldAutoResize != AutoResize) Undo.CallbackUndoAction.Register(this, v =>
				{
					this.AutoResize = v;
					this.UpdateAutoScroll();
				}, true);
			}),

			new Property("BG Color", PropertyType.Color, () => BackgroundColor, e =>
			{
				Color OldColor = this.BackgroundColor;
				SetBackgroundColor((Color) e);
				Color NewColor = this.BackgroundColor;
				if (!OldColor.Equals(NewColor)) Undo.GenericUndoAction<Color>.Register(this, "SetBackgroundColor", OldColor, NewColor, true);
			}),

			new Property("Docking", PropertyType.Dropdown, () => HDocked ? VDocked ? 3 : 1 : VDocked ? 2 : 0, e =>
			{
				if (this is DesignLabel) return;
				int idx = (int) e;
				bool WasHDocked = HDocked;
				bool WasVDocked = VDocked;
				bool WasRightDocked = RightDocked;
				bool WasBottomDocked = BottomDocked;
				Point OldPosition = Position;
				Padding OldPadding = Padding;
				Size OldSize = Size;
				MayRefresh = false;
                if ((idx == 1 || idx == 3) && RightDocked) SetRightDocked(false);
                if ((idx == 2 || idx == 3) && BottomDocked) SetBottomDocked(false);
                SetHDocked(idx == 1 || idx == 3);
				SetVDocked(idx == 2 || idx == 3);
				MayRefresh = true;
				int oldpadu = Padding.Up;
				if (WasHDocked && !HDocked)
				{
					SetWidth(MinimumSize.Width * 4);
					if (Padding.Left != 0) SetPosition(Padding.Left, Position.Y);
					else SetPosition(Math.Min(Parent.Size.Width - 20, 50), Position.Y);
					SetPadding(0, Padding.Up, Padding.Right, Padding.Down);
				}
				if (WasVDocked && !VDocked)
				{
					SetHeight(MinimumSize.Height * 4);
					if (oldpadu != 0) SetPosition(Position.X, Padding.Up);
					else SetPosition(Position.X, Math.Min(Parent.Size.Height - 20, 50));
					SetPadding(Padding.Left, 0, Padding.Right, 0);
				}
				if (!WasHDocked && HDocked)
				{
					SetPadding(Position.X, VDocked ? Padding.Up : 0, 0, 0);
					SetPosition(0, Position.Y);
				}
				if (!WasVDocked && VDocked)
				{
					SetPadding(HDocked ? Padding.Left : 0, Position.Y, 0, 0);
					SetPosition(Position.X, 0);
				}
				if (WasHDocked != HDocked || WasVDocked != VDocked)
				{
					List<Undo.BaseUndoAction> Actions = new List<Undo.BaseUndoAction>();
					if (!OldPosition.Equals(Position)) Actions.Add(Undo.GenericUndoAction<Point>.Create(this, "SetPosition", OldPosition, this.Position, false));
					if (!OldPadding.Equals(Padding)) Actions.Add(Undo.GenericUndoAction<Padding>.Create(this, "SetPadding", OldPadding, this.Padding, false));
					if (!OldSize.Equals(Size)) Actions.Add(Undo.GenericUndoAction<Size>.Create(this, "SetSize", OldSize, this.Size, false));
					if (WasBottomDocked != BottomDocked || WasRightDocked != RightDocked) Actions.Add(Undo.DockingPositionUndoAction.Create(this, WasBottomDocked, WasRightDocked, BottomDocked, RightDocked, false));
					Undo.DockingUndoAction.Register(this, WasHDocked, WasVDocked, HDocked, VDocked, true, Actions);
				}
			}, new List<string>() { "None", "Horizontal", "Vertical", "Full" }, () => this is not DesignLabel, "Unavailable"),

			new Property("Dock to Right", PropertyType.Boolean, () => RightDocked, e => 
			{
				if (HDocked || !MayRefresh) return;
				bool OldRightDocked = RightDocked;
				Point OldPosition = Position;
				Padding OldPadding = Padding;
				SetRightDocked((bool) e);
				if (!RightDocked)
				{
					if (HDocked) SetPadding(Parent.Size.Width / 2 - Size.Width / 2, Padding.Up, Padding.Right, Padding.Down);
					else SetPosition(Parent.Size.Width / 2 - Size.Width / 2, Position.Y);
				}
				if (RightDocked != OldRightDocked)
				{
					List<Undo.BaseUndoAction> Actions = new List<Undo.BaseUndoAction>();
					if (!OldPosition.Equals(Position)) Actions.Add(Undo.GenericUndoAction<Point>.Create(this, "SetPosition", OldPosition, Position, false));
					else if (!OldPadding.Equals(Padding)) Actions.Add(Undo.GenericUndoAction<Padding>.Create(this, "SetPadding", OldPadding, Padding, false));
					Undo.DockingPositionUndoAction.Register(this, this.BottomDocked, OldRightDocked, this.BottomDocked, this.RightDocked, true, Actions);
				}
			}, null, () => !HDocked),

            new Property("Dock to Bottom", PropertyType.Boolean, () => BottomDocked, e =>
			{
				if (VDocked || !MayRefresh) return;
                bool OldBottomDocked = BottomDocked;
                Point OldPosition = Position;
                Padding OldPadding = Padding;
                SetBottomDocked((bool) e);
				if (!BottomDocked) 
				{
					if (VDocked) SetPadding(Padding.Left, Parent.Size.Height / 2 - Size.Height / 2, Padding.Right, Padding.Down);
					else SetPosition(Position.X, Parent.Size.Height / 2 - Size.Height / 2);
				}
				if (BottomDocked != OldBottomDocked)
				{
                    List<Undo.BaseUndoAction> Actions = new List<Undo.BaseUndoAction>();
                    if (!OldPosition.Equals(Position)) Actions.Add(Undo.GenericUndoAction<Point>.Create(this, "SetPosition", OldPosition, Position, false));
                    else if (!OldPadding.Equals(Padding)) Actions.Add(Undo.GenericUndoAction<Padding>.Create(this, "SetPadding", OldPadding, Padding, false));
					Undo.DockingPositionUndoAction.Register(this, OldBottomDocked, this.RightDocked, this.BottomDocked, this.RightDocked, true, Actions);
                }
			}, null, () => !VDocked),

			new Property("Padding", PropertyType.Padding, () => Padding, e => {
				MayRefresh = false;
				Padding OldPadding = Padding;
				Padding p = (Padding) e;
				SetPadding(p.Left, p.Up, p.Right, p.Down);
				if (!OldPadding.Equals(Padding)) Undo.GenericUndoAction<Padding>.Register(this, "SetPadding", OldPadding, Padding, true);
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
							new MenuItem("Button", _ => CreateSibling("button")),
                            new MenuItem("Label", _ => CreateSibling("label")),
							new MenuItem("List Box", _ => CreateSibling("list")),
							new MenuItem("Text Box", _ => CreateSibling("textbox"))
                        }
					},
					new MenuItem("Child")
					{
						Items = new List<IMenuItem>()
						{
							new MenuItem("Button", _ => CreateChild("button")),
                            new MenuItem("Label", _ => CreateChild("label")),
							new MenuItem("List Box", _ => CreateChild("list")),
							new MenuItem("Text Box", _ => CreateChild("textbox"))
                        }
					}
				}
			},
			new MenuItem("Copy")
			{
				Shortcut = "Ctrl+C",
				OnClicked = _ => CopySelection()
			},
			new MenuItem("Cut")
			{
				Shortcut = "Ctrl+X",
				OnClicked = _ => CutSelection()
			},
			new MenuItem("Paste (smart)")
			{
				Shortcut = "Ctrl+V",
				OnClicked = _ => PasteSelection(PasteType.Smart)
			},
            new MenuItem("Paste as Child")
            {
                OnClicked = _ => PasteSelection(PasteType.Child)
            },
            new MenuItem("Paste as Sibling")
			{
				OnClicked = _ => PasteSelection(PasteType.Sibling)
			},
			new MenuItem("Duplicate")
			{
				Shortcut = "Ctrl+D",
				OnClicked = _ => DuplicateSelection()
			},
			new MenuItem("Flatten")
			{
				OnClicked = _ => Flatten(),
				IsClickable = e => e.Value = this is not DesignWindow
			},
			new MenuSeparator(),
			new MenuItem("Delete")
			{
				Shortcut = "Del",
				OnClicked = _ => DeleteSelection()
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
            new Shortcut(this, new Key(Keycode.DOWN, Keycode.SHIFT, Keycode.CTRL), _ => MoveV(50)),
			new Shortcut(this, new Key(Keycode.DELETE), _ => DeleteSelection()),
			new Shortcut(this, new Key(Keycode.C, Keycode.CTRL), _ => CopySelection()),
			new Shortcut(this, new Key(Keycode.X, Keycode.CTRL), _ => CutSelection()),
			new Shortcut(this, new Key(Keycode.V, Keycode.CTRL), _ => PasteSelection(PasteType.Smart)),
			new Shortcut(this, new Key(Keycode.D, Keycode.CTRL), _ => DuplicateSelection())
        });

		OnContextMenuOpening += e => e.Value = Selected;
		OnSizeChanged += _ =>
		{
			if (MayRefresh) Program.ParameterPanel.Refresh();
            UpdateAutoResizeParents();
        };
		OnPositionChanged += e =>
		{
			if (MayRefresh) Program.ParameterPanel.Refresh();
			if (this is not DesignWindow) RecalculateLocalPosition();
			UpdateAutoResizeParents();
		};
		OnPaddingChanged += _ => 
		{
			if (MayRefresh) Program.ParameterPanel.Refresh();
			if (this is not DesignWindow) RecalculateLocalPosition();
        };
    }

	private void RecalculateLocalPosition()
    {
        Point ParentPos = ((DesignWidget) Parent).LocalPosition;
        LocalPosition = new Point(ParentPos.X + Position.X + Padding.Left, ParentPos.Y + Position.Y + Padding.Up);
		Widgets.FindAll(w => w is DesignWidget).ForEach(w => ((DesignWidget) w).RecalculateLocalPosition());
    }

    private void UpdateAutoResizeParents()
	{
		if (this is DesignWindow) return;
		DesignWidget DesignParent = (DesignWidget) Parent;
		if (DesignParent.AutoResize) DesignParent.UpdateAutoScroll();
		if (DesignParent is DesignWindow) return;
		DesignParent.UpdateAutoResizeParents();
	}

	public void CopySelection(bool Delete = false)
	{
		if (this is DesignWindow) return;
		var data = Program.WidgetsToDict(Program.DesignWindow.SelectedWidgets);
		Program.CopyData = data;
	}

	public void CutSelection()
	{
		if (this is DesignWindow) return;
		CopySelection();
		DeleteSelection();
	}

	public void PasteSelection(PasteType PasteType)
	{
		if (Program.CopyData == null || Program.CopyData.Count == 0) return;
		var data = Program.CopyData;
		DesignWidget DesignParent = null;
		if (Program.DesignWindow.SelectedWidgets.Count > 1) PasteType = PasteType.Sibling;
		if (PasteType == PasteType.Smart && this.PasteAsChildren || PasteType == PasteType.Child) DesignParent = this;
		else DesignParent = (DesignWidget) Parent;
		List<DesignWidget> Widgets = Program.DictToWidgets(DesignParent, data);
		string PasteOriginParentName = (string) data[0]["parentname"];
		int minx = int.MaxValue;
		int miny = int.MaxValue;
		int maxw = int.MinValue;
		int maxh = int.MinValue;
		Widgets.ForEach(w =>
		{
			if (w.Position.X < minx) minx = w.Position.X;
			if (w.Position.Y < miny) miny = w.Position.Y;
			if (w.Position.X + w.Size.Width > maxw) maxw = w.Position.X + w.Size.Width;
			if (w.Position.Y + w.Size.Height > maxh) maxh = w.Position.Y + w.Size.Height;
		});
		int avgx = minx + (maxw - minx) / 2;
		int avgy = miny + (maxh - miny) / 2;
		if (DesignParent.Name != PasteOriginParentName)
			Widgets.ForEach(w =>  w.SetPosition(
				(w.Position.X - avgx) + w.Parent.Size.Width / 2,
				(w.Position.Y - avgy) + w.Parent.Size.Height / 2
			));
		Program.DesignWindow.DeselectAll();
		Widgets.ForEach(w => w.Select(true));
		Undo.CreateDeleteUndoAction.Register(this, DesignParent.Name, CloneData(data), false, false);
	}

	private List<Dictionary<string, object>> CloneData(List<Dictionary<string, object>> data)
	{
		List<Dictionary<string, object>> NewList = new List<Dictionary<string, object>>();
		foreach (var dict in data)
		{
			Dictionary<string, object> NewDict = new Dictionary<string, object>(dict);
			NewDict["widgets"] = CloneData((List<Dictionary<string, object>>) dict["widgets"]);
			NewList.Add(NewDict);
		}
		return NewList;
	}

	public void DuplicateSelection()
	{
		if (this is DesignWindow) return;
		CopySelection();
		PasteSelection(PasteType.Sibling);
	}

	public void DeleteSelection()
	{
		if (this is DesignWindow || Program.DesignWindow.SelectedWidgets.Count == 0) return;
		List<Dictionary<string, object>> WidgetData = Program.WidgetsToDict(Program.DesignWindow.SelectedWidgets);
		DesignWidget DesignParent = (DesignWidget) Program.DesignWindow.SelectedWidgets[0].Parent;
		while (Program.DesignWindow.SelectedWidgets.Count > 0)
		{
			Program.DesignWindow.SelectedWidgets[0].Dispose();
			Program.DesignWindow.SelectedWidgets.RemoveAt(0);
		}
		Undo.CreateDeleteUndoAction.Register(this, DesignParent.Name, WidgetData, true, true);
	}

	public DesignWidget Flatten(List<DesignWidget>? WidgetList = null, string? ParentName = null, bool Undoable = true)
	{
		bool WasNull = false;
		if (WidgetList == null)
		{
			WidgetList = Program.DesignWindow.SelectedWidgets;
			WasNull = true;
		}
		if (this is DesignWindow && WidgetList == null || WidgetList.Count == 0) return null;
		DesignWidget DesignParent = string.IsNullOrEmpty(ParentName) ? (DesignWidget) WidgetList[0].Parent : Program.DesignWindow.GetWidgetByName(ParentName);
		int minx = int.MaxValue;
		int miny = int.MaxValue;
		int maxwidth = int.MinValue;
		int maxheight = int.MinValue;
		for (int i = 0; i < WidgetList.Count; i++)
		{
			DesignWidget w = WidgetList[i];
			int x = w.Position.X + w.Padding.Left;
			int y = w.Position.Y + w.Padding.Up;
			int width = w.Size.Width;
			int height = w.Size.Height;
			if (x < minx) minx = x;
			if (y < miny) miny = y;
			if (x + width > maxwidth) maxwidth = x + width;
			if (y + height > maxheight) maxheight = y + height;
		}
		DesignWidget Container = new DesignWidget(DesignParent, "UnnamedContainer");
		Container.SetPosition(minx, miny);
		Container.SetSize(maxwidth - minx, maxheight - miny);
		List<Undo.BaseUndoAction> Actions = new List<Undo.BaseUndoAction>();
		for (int i = 0; i < WidgetList.Count; i++)
		{
			DesignWidget w = WidgetList[i];
			DesignWidget OldParent = (DesignWidget) w.Parent;
			Point OldPosition = w.Position;
			int OldZIndex = w.ZIndex;
			w.SetParent(Container);
			w.SetPosition(w.Position.X - minx, w.Position.Y - miny);
			w.SetZIndex(Container.ZIndex + 1);
			if (Undoable) Actions.Add(Undo.FlattenUndoAction.Create(w, OldPosition, w.Position, OldParent.Name, Container.Name, OldZIndex, w.ZIndex, false));
		}
		if (Undoable) Undo.FlattenGroupUndoAction.Register(Container, DesignParent.Name, Container.Position, Container.Size, true, Actions);
		if (WasNull || WidgetList.All(w => w.Selected))
		{
			Program.DesignWindow.DeselectAll();
			Container.Select(false);
		}
		return Container;
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
			((DesignButton) w).SetText("Untitled Button");
			w.SetSize(140 + WidthAdd, 33 + HeightAdd);
		}
		else if (Type == "label")
		{
			w = new DesignLabel(Parent);
			((DesignLabel) w).SetText("Untitled Label");
			((DesignLabel) w).SetFont(Fonts.Paragraph);
		}
		else if (Type == "list")
		{
			w = new DesignListBox(Parent);
			w.SetSize(160 + WidthAdd, 200 + HeightAdd);
			((DesignListBox) w).SetFont(Fonts.Paragraph);
			((DesignListBox) w).SetItems(new List<ListItem>()
			{
				new ListItem("Item One"),
				new ListItem("Item Two"),
				new ListItem("Item Three")
			});
		}
		else if (Type == "textbox")
		{
			w = new DesignTextBox(Parent);
			w.SetSize(100 + WidthAdd, 27 + HeightAdd);
			((DesignTextBox) w).SetText("Blank");
			((DesignTextBox) w).SetFont(Fonts.Paragraph);
			((DesignTextBox) w).SetCaretHeight(14);
		}
		else
		{
			throw new Exception($"Unsupported widget type '{Type}'.");
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

    public string GetName(string BaseName, int StartIndex = 0)
    {
        string Name = BaseName;
        int i = StartIndex;
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
        if (!Input.Press(Keycode.SHIFT))
		{
			if (!Moving || !MovingMultiple) Program.DesignWindow.DeselectAll();
            else if (Moving && !Selected) Program.DesignWindow.DeselectAll();
		} 
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
        SelectionContainer.Sprites["_sel"].Bitmap?.Dispose();
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
		SelectionContainer.Sprites["_sel"].X = x;
		SelectionContainer.Sprites["_sel"].Y = y;
        SelectionContainer.Sprites["_sel"].Bitmap = new Bitmap(width, height);
        SelectionContainer.Sprites["_sel"].Bitmap.Unlock();
		for (int dy = 0; dy < height; dy++)
		{
			for (int dx = 0; dx < width; dx++)
			{
				if (dx == 0 || dy == 0 || dx == width - 1 || dy == height - 1)
				{
					if (dx != 2 && dx != width -3 && (dx == 0 || dx == 1 || dx == width - 1 || dx == width - 2 || ((dx + 2) / 4) % 2 == 0) &&
						dy != 2 && dy != height - 3 && (dy == 0 || dy == 1 || dy == height - 1 || dy == height - 2 || ((dy + 2) / 4) % 2 == 0))
						SelectionContainer.Sprites["_sel"].Bitmap.SetPixel(dx, dy, new Color(128, 128, 128));
				}
			}
		}
        SelectionContainer.Sprites["_sel"].Bitmap.Lock();
		return new Rect(x, y, width, height);
    }

    void DisposeSelectionBox()
    {
        SelectionContainer.Sprites["_sel"].Bitmap?.Dispose();
    }

	void SelectWidgetsInArea(Rect Area, bool AllowMultiple)
	{
		if (!AllowMultiple) Program.DesignWindow.DeselectAll();
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

	public DesignWidget? GetWidgetByName(string Name)
	{
		if (this.Name == Name) return this;
		foreach (Widget w in Widgets)
		{
			if (w is DesignWidget)
			{
				DesignWidget? dw = ((DesignWidget) w).GetWidgetByName(Name);
				if (dw != null) return dw;
			}
		}
		return null;
	}

    public override void MouseMoving(MouseEventArgs e)
	{
		base.MouseMoving(e);
		if (!LeftPressCounts && Mouse.LeftMousePressed) return;
		WithinResizeRegion = false;
		bool OldHover = Hovering;
		Hovering = Program.DesignWindow.HoveringWidget == this;
		bool HasSnapped = false;
		if (OldHover != Hovering && !Resizing && !Moving) UpdateBox(false);
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
			if (MinimumSize.Width != -1 && SizeOrigin.Width + diffX < MinimumSize.Width) diffX = MinimumSize.Width - SizeOrigin.Width;
			else if (MaximumSize.Width != -1 && SizeOrigin.Width + diffX > MaximumSize.Width) diffX = MaximumSize.Width - SizeOrigin.Width;
            if (MinimumSize.Height != -1 && SizeOrigin.Height + diffY < MinimumSize.Height) diffY = MinimumSize.Height - SizeOrigin.Height;
            else if (MaximumSize.Height != -1 && SizeOrigin.Height + diffY > MaximumSize.Height) diffY = MaximumSize.Height - SizeOrigin.Height;
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
            Program.DesignWindow.DrawSnaps(this, false, false, false, MovingMultiple);
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
            int x1 = MouseOrigin.X - Viewport.X;
            int y1 = MouseOrigin.Y - Viewport.Y;
            int x2 = e.X - Viewport.X;
            int y2 = e.Y - Viewport.Y;
            int minx = Math.Min(x1, x2);
            int miny = Math.Min(y1, y2);
            int maxx = Math.Max(x1, x2);
            int maxy = Math.Max(y1, y2);
            Rect? SelectionBox = DrawSelectionBox(minx, miny, maxx - minx, maxy - miny);
			if (SelectionBox != null) SelectWidgetsInArea(SelectionBox, Input.Press(Keycode.SHIFT));
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
			else if (this is not DesignWindow || !((DesignWindow) this).Fullscreen)
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
		else if (!Pressing && Program.DesignContainer.Mouse.Inside && LeftPressCounts) Input.SetCursor(CursorType.Arrow);
	}

	public override void LeftMouseUp(MouseEventArgs e)
	{
		base.LeftMouseUp(e);
		if (LeftPressCounts)
        {
            if (Resizing)
			{
				if (!Padding.Equals(PaddingOrigin))
				{
					Undo.GenericUndoAction<Size> SizeAction = null;
                    if (!Size.Equals(SizeOrigin)) SizeAction = Undo.GenericUndoAction<Size>.Create(this, "SetSize", this.SizeOrigin, this.Size, false);
                    Undo.GenericUndoAction<Padding>.Register(this, "SetPadding", this.PaddingOrigin, this.Padding, true, new List<Undo.BaseUndoAction>() { SizeAction });
				}
                else if (!Size.Equals(SizeOrigin)) Undo.GenericUndoAction<Size>.Register(this, "SetSize", this.SizeOrigin, this.Size, true);
				Redraw();
				if (this is DesignWindow) Program.MainWindow.CenterDesignWindow();
			}
            if (Moving && !MovingMultiple && !Position.Equals(PositionOrigin))
            {
				Undo.GenericUndoAction<Point>.Register(this, "SetPosition", this.PositionOrigin, this.Position, true);
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
            if (Program.DesignContainer.Mouse.Inside) Input.SetCursor(CursorType.Arrow);
			UpdateBox(true);
			Program.DesignWindow.DisposeSnaps();
			LeftPressCounts = false;
		}
	}

	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
		UpdateBox(true);
        ((SolidBitmap) Sprites["_bg"].Bitmap).SetSize(Size.Width - WidthAdd, Size.Height - HeightAdd);
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

		Sprites["_box"].Visible = Selected || Pressing || Hovering;

		if (!Redraw && !((Hovering || Pressing) && Sprites["_box"].Bitmap == null)) return;
		Sprites["_box"].Bitmap?.Dispose();
		Sprites["_box"].Bitmap = new Bitmap(Size.Width - MousePadding * 2, Size.Height - MousePadding * 2);
		Sprites["_box"].Bitmap.Unlock();
		Sprites["_box"].Bitmap.DrawRect(LinePos, LinePos, Size.Width - MousePadding * 2 - LinePos * 2 - 1, Size.Height - MousePadding * 2 - LinePos * 2 - 1, LineColor);
		Sprites["_box"].Bitmap.DrawRect(0, 0, BoxSize, BoxSize, Color.BLACK);
		Sprites["_box"].Bitmap.FillRect(1, 1, BoxSize - 2, BoxSize - 2, CornerColor);
        Sprites["_box"].Bitmap.DrawRect(Size.Width - MousePadding * 2 - BoxSize - 1, 0, BoxSize, BoxSize, Color.BLACK);
        Sprites["_box"].Bitmap.FillRect(Size.Width - MousePadding * 2 - BoxSize, 1, BoxSize - 2, BoxSize - 2, CornerColor);
        Sprites["_box"].Bitmap.DrawRect(0, Size.Height - MousePadding * 2 - BoxSize - 1, BoxSize, BoxSize, Color.BLACK);
        Sprites["_box"].Bitmap.FillRect(1, Size.Height - MousePadding * 2 - BoxSize, BoxSize - 2, BoxSize - 2, CornerColor);
        Sprites["_box"].Bitmap.DrawRect(Size.Width - MousePadding * 2 - BoxSize - 1, Size.Height - MousePadding * 2 - BoxSize - 1, BoxSize, BoxSize, Color.BLACK);
		Sprites["_box"].Bitmap.FillRect(Size.Width - MousePadding * 2 - BoxSize, Size.Height - MousePadding * 2 - BoxSize, BoxSize - 2, BoxSize - 2, CornerColor);
		Sprites["_box"].Bitmap.Lock();
	}
}

public enum PasteType
{
    Smart,
    Child,
    Sibling
}