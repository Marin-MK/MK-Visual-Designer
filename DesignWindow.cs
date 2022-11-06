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

    public DesignWidget CurrentlySelectedWidget;
    public DesignWidget HoveringWidget;

    Container SnapContainer;
    int WindowEdges = 7;

	public DesignWindow(IContainer Parent) : base(Parent, "UnnamedWindow")
	{
        Program.CreateCaches();
        Program.DesignWindow = this;

        Sprites["shadow"] = new Sprite(this.Viewport);
        Sprites["shadow"].X = WidgetPadding;
        Sprites["shadow"].Y = WidgetPadding;
        Sprites["window"] = new Sprite(this.Viewport);
        Sprites["window"].X = WidgetPadding + WindowEdges;
        Sprites["window"].Y = WidgetPadding + WindowEdges;
        Sprites["title"] = new Sprite(this.Viewport);
        Sprites["title"].X = WidgetPadding + 5 + WindowEdges;
        Sprites["title"].Y = WidgetPadding + 3 + WindowEdges;

        SnapContainer = new Container(this);
        SnapContainer.SetDocked(true);
        SnapContainer.SetPadding(WidgetPadding);
        SnapContainer.SetZIndex(10);
        SnapContainer.Sprites["snaps"] = new Sprite(SnapContainer.Viewport);
        MinimumSize.Width += WindowEdges * 2 + 2;
        MinimumSize.Height += WindowEdges * 2 + 2;

        DesignButton button = new DesignButton(this);
        button.SetPosition(WidgetPadding + 60, WidgetPadding + 60);
        button.SetSize(200 + WidthAdd, 100 + HeightAdd);
        button.SetText("OK");

        DesignLabel label = new DesignLabel(this);
        label.SetPosition(WidgetPadding + 80, WidgetPadding + 200);
        label.SetText("This is a text label!");
        label.SetFont(Fonts.Paragraph);

        // Remove the Name property
        this.Properties.RemoveAll(p => p.Name == "Name" || p.Name == "X" || p.Name == "Y" || p.Name == "Docking" || p.Name == "Dock to Right" || p.Name == "Dock to Bottom" || p.Name == "Padding");
        this.Properties.AddRange(new List<Property>()
        {
            new Property("Title", PropertyType.Text, () => Title, e => SetTitle((string) e))
        });
    }

    public void MakeSelectedWidget(DesignWidget Widget)
    {
        CurrentlySelectedWidget?.Deselect();
        Widget?.Select();
        CurrentlySelectedWidget = Widget;
        Program.ParameterPanel.SetWidget(Widget);
    }

    public void SetTitle(string Title)
    {
        if (this.Title != Title)
        {
            this.Title = Title;
            RedrawTitle();
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
        if (CurrentlySelectedWidget != null && CurrentlySelectedWidget.Mouse.Inside && CurrentlySelectedWidget.WithinResizeRegion) curr = CurrentlySelectedWidget;
        HoveringWidget = curr;
    }

    public void Center()
    {
        Program.MainWindow.CenterDesignWindow();
    }

    public override void LeftMouseDown(MouseEventArgs e)
    {
        base.LeftMouseDown(e);
        if (!Mouse.Inside && !Program.ParameterPanel.Mouse.Inside) MakeSelectedWidget(null);
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

    public void DrawSnaps(DesignWidget MovingWidget, bool SizeSnapsOnly, bool ResizeMoveX, bool ResizeMoveY)
    {
        SnapContainer.Sprites["snaps"].Bitmap?.Dispose();
        SnapContainer.Sprites["snaps"].Bitmap = new Bitmap(Size.Width - WidgetPadding * 2, Size.Height - WidgetPadding * 2);
        SnapContainer.Sprites["snaps"].Bitmap.Unlock();
        List<DesignWidget> DesignWidgets = new List<DesignWidget>();
        Widgets.FindAll(w => w is DesignWidget).ForEach(w =>
        {
            DesignWidgets.Add((DesignWidget) w);
            DesignWidgets.AddRange(((DesignWidget) w).GetChildWidgets());
        });
        foreach (DesignWidget other in DesignWidgets)
        {
            if (other == MovingWidget) continue;
            FindSnaps(MovingWidget, MovingWidget, other, SizeSnapsOnly, ResizeMoveX, ResizeMoveY);
            FindSnaps(MovingWidget, other, MovingWidget, SizeSnapsOnly, ResizeMoveX, ResizeMoveY);
        }
        SnapContainer.Sprites["snaps"].Bitmap.Lock();
    }

    private void FindSnaps(DesignWidget MovingWidget, DesignWidget w1, DesignWidget w2, bool SizeSnapsOnly, bool ResizeMoveX, bool ResizeMoveY)
    {
        Rect r1 = new Rect(w1.LocalPosition, w1.Size);
        Rect r2 = new Rect(w2.LocalPosition, w2.Size);
        if (r1.X == r2.X)
        {
            int min = Math.Min(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int max = Math.Max(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int x = r1.X - 4;
            DrawSnap(x, min, x, max);
            if (!(SizeSnapsOnly && !ResizeMoveX))
                MovingWidget.SetHorizontallySnapped();
        }
        if (r1.X + r1.Width == r2.X + r2.Width || r1.X + r1.Width - WidthAdd + 6 == r2.X)
        {
            int min = Math.Min(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int max = Math.Max(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int x = r1.X + r1.Width - WidthAdd + 2;
            DrawSnap(x, min, x, max);
            if (!(r1.X + r1.Width - WidthAdd + 6 == r2.X && SizeSnapsOnly && ResizeMoveX && w1 == MovingWidget ||
                r1.X + r1.Width - WidthAdd + 6 == r2.X && SizeSnapsOnly && !ResizeMoveX && w2 == MovingWidget ||
                  r1.X + r1.Width == r2.X + r2.Width && SizeSnapsOnly && ResizeMoveX))
                MovingWidget.SetHorizontallySnapped();
        }
        if (r1.X + r1.Width / 2d == r2.X + r2.Width / 2d || r1.Width % 2 != r2.Width % 2 && r1.X + r1.Width / 2 == r2.X + r2.Width / 2)
        {
            int min = Math.Min(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int max = Math.Max(r1.Y + (r1.Height - HeightAdd) / 2, r2.Y + (r2.Height - HeightAdd) / 2);
            int x = r1.X + (r1.Width - WidthAdd) / 2;
            DrawSnap(x, min, x, max);
            MovingWidget.SetHorizontallySnapped();
        }
        if (r1.Y == r2.Y)
        {
            int min = Math.Min(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int max = Math.Max(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int y = r1.Y - 4;
            DrawSnap(min, y, max, y);
            if (!(SizeSnapsOnly && !ResizeMoveY))
                MovingWidget.SetVerticallySnapped();
        }
        if (r1.Y + r1.Height == r2.Y + r2.Height || r1.Y + r1.Height - HeightAdd + 6 == r2.Y)
        {
            int min = Math.Min(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int max = Math.Max(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int y = r1.Y + r1.Height - HeightAdd + 2;
            DrawSnap(min, y, max, y);
            if (!(r1.Y + r1.Height - HeightAdd + 6 == r2.Y && SizeSnapsOnly && ResizeMoveY && w1 == MovingWidget ||
                  r1.Y + r1.Height - HeightAdd + 6 == r2.Y && SizeSnapsOnly && !ResizeMoveY && w2 == MovingWidget ||
                  r1.Y + r1.Height == r2.Y + r2.Height && SizeSnapsOnly && ResizeMoveY))
                MovingWidget.SetVerticallySnapped();
        }
        if (r1.Y + r1.Height / 2d == r2.Y + r2.Height / 2d || r1.Height % 2 != r2.Height % 2 && r1.Y + r1.Height / 2 == r2.Y + r2.Height / 2)
        {
            int min = Math.Min(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int max = Math.Max(r1.X + (r1.Width - WidthAdd) / 2, r2.X + (r2.Width - WidthAdd) / 2);
            int y = r1.Y + (r1.Height - HeightAdd) / 2;
            DrawSnap(min, y, max, y);
            MovingWidget.SetVerticallySnapped();
        }
    }

    private void DrawSnap(int x1, int y1, int x2, int y2)
    {
        if (x1 < 0) x1 = 0;
        if (y1 < 0) y1 = 0;
        if (x1 >= SnapContainer.Sprites["snaps"].Bitmap.Width) x1 = SnapContainer.Sprites["snaps"].Bitmap.Width - 1;
        if (y1 >= SnapContainer.Sprites["snaps"].Bitmap.Height) y1 = SnapContainer.Sprites["snaps"].Bitmap.Height - 1;
        if (x2 < 0) x2 = 0;
        if (y2 < 0) y2 = 0;
        if (x2 >= SnapContainer.Sprites["snaps"].Bitmap.Width) x2 = SnapContainer.Sprites["snaps"].Bitmap.Width - 1;
        if (y2 >= SnapContainer.Sprites["snaps"].Bitmap.Height) y2 = SnapContainer.Sprites["snaps"].Bitmap.Height - 1;

        int minx = Math.Min(x1, x2);
        int miny = Math.Min(y1, y2);
        int maxx = Math.Max(x1, x2);
        int maxy = Math.Max(y1, y2);
        bool xeql = minx == maxx;

        for (int e = xeql ? miny : minx; e < (xeql ? maxy : maxx);  e++)
        {
            int x = xeql ? minx : e;
            int y = xeql ? e : miny;
            int rem = e - (xeql ? miny : minx);
            if ((rem / 6) % 2 == 0) SnapContainer.Sprites["snaps"].Bitmap.SetPixel(x, y, Color.GREEN);
        }
    }

    public void DisposeSnaps()
    {
        SnapContainer.Sprites["snaps"].Bitmap?.Dispose();
    }

	protected override void Draw()
	{
		base.Draw();
        Sprites["window"].Bitmap?.Dispose();
        Sprites["window"].Bitmap = new Bitmap(Size.Width - WindowEdges * 2 - WidgetPadding * 2, Size.Height - WindowEdges * 2 - WidgetPadding * 2);
        Sprites["window"].Bitmap.Unlock();
        Sprites["window"].Bitmap.DrawRect(0, 0, Sprites["window"].Bitmap.Width, Sprites["window"].Bitmap.Height, new Color(59, 227, 255));
        Sprites["window"].Bitmap.FillRect(1, 1, Sprites["window"].Bitmap.Width - 2, Sprites["window"].Bitmap.Height - 2, new Color(40, 62, 84));
        Sprites["window"].Bitmap.Lock();

        Sprites["shadow"].Bitmap?.Dispose();
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
        //for (int i = 0; i < Buttons.Count; i++)
        //{
        //    Button b = Buttons[i];
        //    int x = i > 0 ? Buttons[i - 1].Position.X - b.Size.Width : Size.Width - b.Size.Width - 4;
        //    int y = Size.Height - b.Size.Height - 4;
        //    b.SetPosition(x - WindowEdges, y - WindowEdges);
        //}
    }
}
