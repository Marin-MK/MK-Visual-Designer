using RPGStudioMK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner;

public class DesignWindow : DesignWidget
{
    public string Title { get; protected set; } = "Unnamed Window";
    public bool Fullscreen { get; protected set; } = false;
    public bool IsPopup { get; protected set; } = true;
    public int WindowEdges = 7;

    public DesignWidget HoveringWidget;
    public List<DesignWidget> SelectedWidgets = new List<DesignWidget>();

    Property TitleProperty;
    Container OverlayContainer;

	public DesignWindow(IContainer Parent) : base(Parent, "UnnamedWindow")
	{
        Program.CreateCaches();
        Program.DesignWindow = this;

        SetPadding(0);

        Sprites["shadow"] = new Sprite(this.Viewport);
        Sprites["shadow"].X = WidgetPadding;
        Sprites["shadow"].Y = WidgetPadding;
        Sprites["window"] = new Sprite(this.Viewport);
        Sprites["window"].X = WidgetPadding + WindowEdges;
        Sprites["window"].Y = WidgetPadding + WindowEdges;
        Sprites["title"] = new Sprite(this.Viewport);
        Sprites["title"].X = WidgetPadding + 5 + WindowEdges;
        Sprites["title"].Y = WidgetPadding + 3 + WindowEdges;

        OverlayContainer = new Container(this);
        OverlayContainer.SetDocked(true);
        OverlayContainer.SetZIndex(10);
        OverlayContainer.Sprites["snaps"] = new Sprite(OverlayContainer.Viewport);
        MinimumSize.Width += WindowEdges * 2 + 2;
        MinimumSize.Height += WindowEdges * 2 + 2;

        OverlayContainer.Sprites["selection"] = new Sprite(OverlayContainer.Viewport);

        // Remove the Name property
        this.Properties.RemoveAll(p => p.Name == "X" || p.Name == "Y" || p.Name == "Docking" || p.Name == "Dock to Right" || p.Name == "Dock to Bottom" || p.Name == "Padding" || p.Name == "Auto-Resize" || p.Name == "BG Color");
        TitleProperty = new Property("Title", PropertyType.Text, () => Title, e =>
        {
            string OldTitle = this.Title;
            SetTitle((string) e);
            if (OldTitle != this.Title) Undo.GenericUndoAction<string>.Register(this, "SetTitle", OldTitle, Title, true);
        });
        this.Properties.AddRange(new List<Property>()
        {
            TitleProperty,
            new Property("Fullscreen", PropertyType.Boolean, () => this.Fullscreen, e => SetFullscreen((bool) e)),
            new Property("Is Popup", PropertyType.Boolean, () => this.IsPopup, e => SetIsPopup((bool) e))
        });
    }

    public void SetTitle(string Title)
    {
        if (this.Title != Title)
        {
            this.Title = Title;
            RedrawTitle();
        }
    }

    public void SetFullscreen(bool Fullscreen)
    {
        if (this.Fullscreen != Fullscreen)
        {
            this.Fullscreen = Fullscreen;
            if (this.Fullscreen)
            {
                Point OldPosition = this.Position;
                Size OldSize = this.Size;
                this.SetDocked(true);
                this.SetPosition(0, 0);
                List<Undo.BaseUndoAction> Actions = new List<Undo.BaseUndoAction>()
                {
                    Undo.DockingUndoAction.Create(this, false, false, true, true, false),
                    Undo.GenericUndoAction<Point>.Create(this, "SetPosition", OldPosition, this.Position, false),
                    Undo.GenericUndoAction<Size>.Create(this, "SetSize", OldSize, this.Size, false)
                };
                Undo.CallbackUndoAction.Register(this, IsRedo =>
                {
                    this.Fullscreen = IsRedo;
                }, true, Actions);
            }
            else
            {
                Point OldPosition = this.Position;
                Size OldSize = this.Size;
                this.SetDocked(false);
                this.SetSize(640 + WidthAdd, 480 + HeightAdd);
                Center();
                List<Undo.BaseUndoAction> Actions = new List<Undo.BaseUndoAction>()
                {
                    Undo.DockingUndoAction.Create(this, true, true, false, false, false),
                    Undo.GenericUndoAction<Point>.Create(this, "SetPosition", OldPosition, this.Position, false),
                    Undo.GenericUndoAction<Size>.Create(this, "SetSize", OldSize, this.Size, false)
                };
                Undo.CallbackUndoAction.Register(this, IsRedo =>
                {
                    this.Fullscreen = !IsRedo;
                }, true, Actions);
            }
        }
    }

    public void SetIsPopup(bool IsPopup)
    {
        if (this.IsPopup != IsPopup)
        {
            this.IsPopup = IsPopup;
            if (this.IsPopup && !Properties.Contains(TitleProperty))
            {
                int Index = Properties.FindIndex(p => p == null);
                Properties[Index] = TitleProperty;
            }
            else if (!this.IsPopup && Properties.Contains(TitleProperty))
            {
                int Index = Properties.IndexOf(TitleProperty);
                Properties[Index] = null;
            }
            Program.ParameterPanel.Redraw();
            Redraw();
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        List<DesignWidget> HoveringWidgets = GetHoveredWidgets(e.X, e.Y);
        int max = int.MinValue;
        DesignWidget curr = null;
        for (int i = 0; i < HoveringWidgets.Count; i++)
        {
            DesignWidget w = HoveringWidgets[i];
            if (w.Viewport.CreationTime > max)
            {
                max = w.Viewport.CreationTime;
                curr = w;
            }
        }
        if (curr == null && Mouse.Inside) curr = this;
        foreach (DesignWidget dw in SelectedWidgets)
        {
            if (dw.Mouse.Inside && dw.WithinResizeRegion)
            {
                curr = dw;
                break;
            }
        }
        HoveringWidget = curr;
        if (!Mouse.Inside && Program.DesignContainer.Mouse.Inside && !Mouse.LeftMousePressed) Input.SetCursor(CursorType.Arrow);
    }

    public void Center()
    {
        Program.MainWindow.CenterDesignWindow();
    }

    public void DeselectAll(DesignWidget? Exception = null)
    {
        int i = 0;
        while (SelectedWidgets.Count > i)
        {
            if (SelectedWidgets[i] == Exception)
            {
                SelectedWidgets[i].Widgets.FindAll(w => w is DesignWidget).ForEach(w => ((DesignWidget) w).Deselect());
                i++;
                continue;
            }
            SelectedWidgets[i].Deselect();
            SelectedWidgets.RemoveAt(i);
        }
        Program.ParameterPanel.SetWidget(null);
    }
    
    public void DeselectChildWidgets(DesignWidget Widget)
    {
        int i = 0;
        while (i < SelectedWidgets.Count)
        {
            if (Widget.ContainsChild(SelectedWidgets[i]))
            {
                SelectedWidgets[i].Deselect();
                SelectedWidgets.RemoveAt(i);
            }
            else i++;
        }
    }

    public void DeselectWidgetsWithoutSharedParents(DesignWidget Widget)
    {
        int i = 0;
        while (i < SelectedWidgets.Count)
        {
            if (SelectedWidgets[i].Parent != Widget.Parent)
            {
                SelectedWidgets[i].Deselect();
                SelectedWidgets.RemoveAt(i);
            }
            else i++;
        }
    }

    public override void LeftMouseDown(MouseEventArgs e)
    {
        base.LeftMouseDown(e);
        if (!Mouse.Inside && Program.DesignContainer.Mouse.Inside) Program.DesignWindow.DeselectAll();
    }

    public override void LeftMouseUp(MouseEventArgs e)
    {
        base.LeftMouseUp(e);
        if (SelectedWidgets.Count > 1)
        {
            if (SelectedWidgets[0].Moving && SelectedWidgets[0].MovingMultiple)
            {
                List<Undo.GenericUndoAction<Point>> Actions = new List<Undo.GenericUndoAction<Point>>();
                Program.DesignWindow.SelectedWidgets.ForEach(s =>
                {
                    Actions.Add(Undo.GenericUndoAction<Point>.Create(s, "SetPosition", s.PositionOrigin, s.Position, false));
                });
                Actions[0].OtherActions.AddRange(Actions.GetRange(1, Actions.Count - 1));
                Actions[0].Register();
            }
        }
    }

    private void RedrawTitle()
    {
        Sprites["title"].Bitmap?.Dispose();
        Size s = Fonts.Header.TextSize(this.Title);
        Sprites["title"].Bitmap = new Bitmap(s);
        Sprites["title"].Bitmap.Font = Fonts.Header;
        Sprites["title"].Bitmap.Unlock();
        Sprites["title"].Bitmap.DrawText(this.Title, Color.WHITE);
        Sprites["title"].Bitmap.Lock();
    }

    public void DrawSnaps(DesignWidget MovingWidget, bool SizeSnapsOnly, bool ResizeMoveX, bool ResizeMoveY, bool NoRealSnapping = false)
    {
        if (!MovingWidget.MovingMultiple) OverlayContainer.Sprites["snaps"].Bitmap?.Dispose();
        List<Rect> Snaps = new List<Rect>();
        List<DesignWidget> DesignWidgets = new List<DesignWidget>();
        Widgets.FindAll(w => w is DesignWidget).ForEach(w =>
        {
            DesignWidgets.Add((DesignWidget) w);
            DesignWidgets.AddRange(((DesignWidget) w).GetChildWidgets());
        });
        foreach (DesignWidget other in DesignWidgets)
        {
            if (other == MovingWidget || MovingWidget.ContainsChild(other) || other.Selected) continue;
            Snaps.AddRange(FindSnaps(MovingWidget, MovingWidget, other, SizeSnapsOnly, ResizeMoveX, ResizeMoveY, NoRealSnapping));
            Snaps.AddRange(FindSnaps(MovingWidget, other, MovingWidget, SizeSnapsOnly, ResizeMoveX, ResizeMoveY, NoRealSnapping));
        }
        if (Snaps.Count > 0)
        {
            if (OverlayContainer.Sprites["snaps"].Bitmap == null || OverlayContainer.Sprites["snaps"].Bitmap.Disposed) OverlayContainer.Sprites["snaps"].Bitmap = new Bitmap(Size);
            OverlayContainer.Sprites["snaps"].Bitmap.Unlock();
            DrawSnaps(Snaps);
            OverlayContainer.Sprites["snaps"].Bitmap.Lock();
        }
    }

    private void DrawSnaps(List<Rect> Snaps)
    {
        foreach (Rect Snap in Snaps)
        {
            int minx = Snap.X;
            int miny = Snap.Y;
            int maxx = Snap.Width;
            int maxy = Snap.Height;
            bool xeql = minx == maxx;

            for (int e = xeql ? miny : minx; e < (xeql ? maxy : maxx); e++)
            {
                int x = xeql ? minx : e;
                int y = xeql ? e : miny;
                int rem = e - (xeql ? miny : minx);
                if ((rem / 6) % 2 == 0) OverlayContainer.Sprites["snaps"].Bitmap.SetPixel(x, y, Color.GREEN);
            }
        }
    }

    private List<Rect> FindSnaps(DesignWidget MovingWidget, DesignWidget w1, DesignWidget w2, bool SizeSnapsOnly, bool ResizeMoveX, bool ResizeMoveY, bool NoRealSnapping)
    {
        List<Rect> Snaps = new List<Rect>();
        Rect r1 = new Rect(w1.LocalPosition, w1.Size);
        Rect r2 = new Rect(w2.LocalPosition, w2.Size);
        if (r1.X == r2.X)
        {
            int min = Math.Min(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int max = Math.Max(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int x = r1.X - 4;
            Snaps.Add(DrawSnap(x, min, x, max));
            if (!NoRealSnapping && !(SizeSnapsOnly && !ResizeMoveX))
                MovingWidget.SetHorizontallySnapped();
        }
        if (r1.X + r1.Width == r2.X + r2.Width || r1.X + r1.Width - WidthAdd + 6 == r2.X)
        {
            int min = Math.Min(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int max = Math.Max(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int x = r1.X + r1.Width - WidthAdd + 2;
            Snaps.Add(DrawSnap(x, min, x, max));
            if (!NoRealSnapping && !(r1.X + r1.Width - WidthAdd + 6 == r2.X && SizeSnapsOnly && ResizeMoveX && w1 == MovingWidget ||
                r1.X + r1.Width - WidthAdd + 6 == r2.X && SizeSnapsOnly && !ResizeMoveX && w2 == MovingWidget ||
                  r1.X + r1.Width == r2.X + r2.Width && SizeSnapsOnly && ResizeMoveX))
                MovingWidget.SetHorizontallySnapped();
        }
        if (r1.X + r1.Width / 2d == r2.X + r2.Width / 2d || r1.Width % 2 != r2.Width % 2 && r1.X + r1.Width / 2 == r2.X + r2.Width / 2 && Math.Abs(r1.X + r1.Width - (r2.X + r2.Width)) > 1)
        {
            int min = Math.Min(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int max = Math.Max(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int x = r1.X + (r1.Width - WidthAdd) / 2;
            Snaps.Add(DrawSnap(x, min, x, max));
            if (!NoRealSnapping) MovingWidget.SetHorizontallySnapped();
        }
        if (r1.Y == r2.Y)
        {
            int min = Math.Min(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int max = Math.Max(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int y = r1.Y - 4;
            Snaps.Add(DrawSnap(min, y, max, y));
            if (!NoRealSnapping && !(SizeSnapsOnly && !ResizeMoveY))
                MovingWidget.SetVerticallySnapped();
        }
        if (r1.Y + r1.Height == r2.Y + r2.Height || r1.Y + r1.Height - HeightAdd + 6 == r2.Y)
        {
            int min = Math.Min(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int max = Math.Max(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int y = r1.Y + r1.Height - HeightAdd + 2;
            Snaps.Add(DrawSnap(min, y, max, y));
            if (!NoRealSnapping && !(r1.Y + r1.Height - HeightAdd + 6 == r2.Y && SizeSnapsOnly && ResizeMoveY && w1 == MovingWidget ||
                  r1.Y + r1.Height - HeightAdd + 6 == r2.Y && SizeSnapsOnly && !ResizeMoveY && w2 == MovingWidget ||
                  r1.Y + r1.Height == r2.Y + r2.Height && SizeSnapsOnly && ResizeMoveY))
                MovingWidget.SetVerticallySnapped();
        }
        if (r1.Y + r1.Height / 2d == r2.Y + r2.Height / 2d || r1.Height % 2 != r2.Height % 2 && r1.Y + r1.Height / 2 == r2.Y + r2.Height / 2 && Math.Abs(r1.Y + r1.Height - (r2.Y + r2.Height)) > 1)
        {
            int min = Math.Min(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int max = Math.Max(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int y = r1.Y + (r1.Height - HeightAdd) / 2;
            Snaps.Add(DrawSnap(min, y, max, y));
            if (!NoRealSnapping) MovingWidget.SetVerticallySnapped();
        }
        return Snaps;
    }

    private Rect DrawSnap(int x1, int y1, int x2, int y2)
    {
        if (x1 < 0) x1 = 0;
        if (y1 < 0) y1 = 0;
        if (x1 >= Size.Width) x1 = Size.Width - 1;
        if (y1 >= Size.Height) y1 = Size.Height - 1;
        if (x2 < 0) x2 = 0;
        if (y2 < 0) y2 = 0;
        if (x2 >= Size.Width) x2 = Size.Width - 1;
        if (y2 >= Size.Height) y2 = Size.Height - 1;

        int minx = Math.Min(x1, x2);
        int miny = Math.Min(y1, y2);
        int maxx = Math.Max(x1, x2);
        int maxy = Math.Max(y1, y2);

        return new Rect(minx, miny, maxx, maxy);
    }

    public void DisposeSnaps()
    {
        OverlayContainer.Sprites["snaps"].Bitmap?.Dispose();
    }

    public override void Redraw()
    {
        base.Redraw();
    }

    protected override void Draw()
	{
		base.Draw();
        Sprites["window"].Bitmap?.Dispose();
        Sprites["shadow"].Bitmap?.Dispose();

        Sprites["title"].Visible = IsPopup;

        if (!this.IsPopup)
        {
            Sprites["window"].Bitmap = new Bitmap(Size.Width - WidgetPadding * 2, Size.Height - WidgetPadding * 2);
            Sprites["window"].Bitmap.Unlock();
            Sprites["window"].Bitmap.DrawRect(0, 0, Size.Width - WidgetPadding * 2, Size.Height - WidgetPadding * 2, new Color(59, 227, 255));
            Sprites["window"].Bitmap.FillRect(1, 1, Size.Width - WidgetPadding * 2 - 2, Size.Height - WidgetPadding * 2 - 2, new Color(40, 62, 84));
            Sprites["window"].Bitmap.Lock();
            Sprites["window"].X = WidgetPadding;
            Sprites["window"].Y = WidgetPadding;
            return;
        }

        Sprites["window"].X = WidgetPadding + WindowEdges;
        Sprites["window"].Y = WidgetPadding + WindowEdges;
        Sprites["window"].Bitmap = new Bitmap(Size.Width - WindowEdges * 2 - WidgetPadding * 2, Size.Height - WindowEdges * 2 - WidgetPadding * 2);
        Sprites["window"].Bitmap.Unlock();
        Sprites["window"].Bitmap.DrawRect(0, 0, Sprites["window"].Bitmap.Width, Sprites["window"].Bitmap.Height, new Color(59, 227, 255));
        Sprites["window"].Bitmap.FillRect(1, 1, Sprites["window"].Bitmap.Width - 2, Sprites["window"].Bitmap.Height - 2, new Color(40, 62, 84));
        Sprites["window"].Bitmap.Lock();

        Sprites["shadow"].Bitmap = new Bitmap(Size.Width - WidgetPadding * 2, Size.Height - WidgetPadding * 2);
        Sprites["shadow"].Bitmap.Unlock();
        Sprites["shadow"].Bitmap.FillGradientRectOutside(
            new Rect(Size.Width - WidgetPadding * 2, Size.Height - WidgetPadding * 2),
            new Rect(WindowEdges, WindowEdges, Size.Width - WindowEdges * 2 - WidgetPadding * 2, Size.Height - WindowEdges * 2 - WidgetPadding * 2),
            new Color(0, 0, 0, 64),
            Color.ALPHA,
            false
        );
        Sprites["shadow"].Bitmap.Lock();
    }
}
