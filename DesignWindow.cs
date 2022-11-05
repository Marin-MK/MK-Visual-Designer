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

    int WindowEdges = 7;

    DesignWidget CurrentlySelectedWidget;

	public DesignWindow(IContainer Parent) : base(Parent)
	{
        Program.CreateCaches();

        Sprites["shadow"] = new Sprite(this.Viewport);
        Sprites["shadow"].X = WidgetPadding;
        Sprites["shadow"].Y = WidgetPadding;
        Sprites["window"] = new Sprite(this.Viewport);
        Sprites["window"].X = WidgetPadding + WindowEdges;
        Sprites["window"].Y = WidgetPadding + WindowEdges;
        Sprites["title"] = new Sprite(this.Viewport);
        Sprites["title"].X = WidgetPadding + 5 + WindowEdges;
        Sprites["title"].Y = WidgetPadding + 3 + WindowEdges;
        MinimumSize.Width += WindowEdges * 2 + 2;
        MinimumSize.Height += WindowEdges * 2 + 2;

        DesignButton button = new DesignButton(this);
        button.SetPosition(WidgetPadding + 60, WidgetPadding + 60);
        button.SetSize(200 + WidthAdd, 100 + HeightAdd);
        button.SetText("OK");

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
