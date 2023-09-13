

namespace VisualDesigner;

[WidgetTypeAndName(typeof(ListBoxData), "list")]
public class DesignListBox : DesignWidget
{
    public override bool PasteAsChildren => false;

    public Font Font => ListDrawer.Font;
    public int LineHeight => ListDrawer.LineHeight;
    public List<ListItem> Items => ListDrawer.Items;
    public bool Enabled { get; protected set; } = true;
    public int SelectedIndex => ListDrawer.SelectedIndex;
    public Color SelectedItemColor => ListDrawer.SelectedItemColor;

    public Container MainContainer;
    public DesignListDrawer ListDrawer;

    public DesignListBox(IContainer Parent) : base(Parent, "UnnamedListBox")
	{
        Sprites["bg"] = new Sprite(this.Viewport);
        Sprites["bg"].X = WidgetPadding;
        Sprites["bg"].Y = WidgetPadding;
        MainContainer = new Container(this);
        MainContainer.SetPosition(1 + WidgetPadding, 2 + WidgetPadding);
        MainContainer.VAutoScroll = true;
        ListDrawer = new DesignListDrawer(MainContainer);
        VScrollBar vs = new VScrollBar(this);
        vs.SetPosition(WidgetPadding, WidgetPadding);
        vs.SetPressable(false);
        MainContainer.SetVScrollBar(vs);
        SetSize(132 + WidthAdd, 174 + HeightAdd);
        Properties.AddRange(new List<Property>()
        {
            new Property("Font", PropertyType.Font, () => Font, e =>
            {
                Font OldFont = Font;
                SetFont((Font) e);
                if (!OldFont.Equals(Font)) Undo.GenericUndoAction<Font>.Register(this, "SetFont", OldFont, Font, true);
            }),

            new Property("Line Height", PropertyType.Numeric, () => LineHeight, e =>
            {
                int OldLineHeight = LineHeight;
                SetLineHeight((int) e);
                if (OldLineHeight != LineHeight) Undo.GenericUndoAction<int>.Register(this, "SetLineHeight", OldLineHeight, LineHeight, true);
            }),

            new Property("Items", PropertyType.List, () => Items.Select(i => i.Name).ToList(), e =>
            {
                List<ListItem> OldItems = Items;
                SetItems(((List<string>) e).Select(s => new ListItem(s)).ToList());
                if (OldItems != Items && !OldItems.Equals(Items)) Undo.GenericUndoAction<List<ListItem>>.Register(this, "SetItems", OldItems, Items, true);
            }),

            new Property("Selected Index", PropertyType.Numeric, () => SelectedIndex, e =>
            {
                int OldSelectedIndex = SelectedIndex;
                SetSelectedIndex((int) e);
                if (OldSelectedIndex != SelectedIndex) Undo.CallbackUndoAction.Register(this, (IsRedo, Widget) =>
                {
                    ((DesignListBox) Widget).SetSelectedIndex(IsRedo ? SelectedIndex : OldSelectedIndex);
                }, true);
            }, new List<object>() { -1 }),

            new Property("Selected Color", PropertyType.Color, () => SelectedItemColor, e =>
            {
                Color OldSelectedItemColor = SelectedItemColor;
                SetSelectedItemColor((Color) e);
                if (!OldSelectedItemColor.Equals(SelectedItemColor)) Undo.GenericUndoAction<Color>.Register(this, "SetSelectedItemColor", OldSelectedItemColor, SelectedItemColor, true);
            }),

            new Property("Enabled", PropertyType.Boolean, () => Enabled, e =>
            {
                bool OldEnabled = Enabled;
                SetEnabled((bool) e);
                if (OldEnabled != Enabled) Undo.GenericUndoAction<bool>.Register(this, "SetEnabled", OldEnabled, Enabled, true);
            })
        });
    }

    public void SetFont(Font Font)
    {
        ListDrawer.SetFont(Font);
    }

    public void SetLineHeight(int LineHeight)
    {
        ListDrawer.SetLineHeight(LineHeight);
    }

    public void SetItems(List<ListItem> Items)
    {
        ListDrawer.SetItems(Items);
    }

    public void SetSelectedIndex(int idx, bool ForceRefresh = false)
    {
        ListDrawer.SetSelectedIndex(idx, ForceRefresh);
    }

    public void SetSelectedItemColor(Color SelectedItemColor)
    {
        ListDrawer.SetSelectedItemColor(SelectedItemColor);
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            RedrawBox();
            ListDrawer.SetEnabled(Enabled);
        }
    }

    public void RedrawBox()
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size.Width - WidgetPadding * 2, Size.Height - WidgetPadding * 2);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawRect(Size.Width - WidgetPadding * 2, Size.Height - WidgetPadding * 2, this.Enabled ? new Color(86, 108, 134) : new Color(36, 34, 36));
        Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - WidgetPadding * 2 - 2, Size.Height - WidgetPadding * 2 - 2, this.Enabled ? new Color(10, 23, 37) : new Color(72, 72, 72));
        Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 1, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(0, Size.Height - WidgetPadding * 2 - 1, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 1, Size.Height - WidgetPadding * 2 - 1, Color.ALPHA);
        Color DarkOutline = this.Enabled ? new Color(40, 62, 84) : new Color(36, 34, 36);
        Sprites["bg"].Bitmap.SetPixel(1, 1, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 2, 1, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(1, Size.Height - WidgetPadding * 2 - 2, DarkOutline);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 2, Size.Height - WidgetPadding * 2 - 2, DarkOutline);
        Sprites["bg"].Bitmap.DrawLine(Size.Width - WidgetPadding * 2 - 12, 1, Size.Width - WidgetPadding * 2 - 12, Size.Height - WidgetPadding * 2 - 2, DarkOutline);
        Sprites["bg"].Bitmap.Lock();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        RedrawBox();
        MainContainer.SetSize(Size.Width - WidgetPadding * 2 - 13, Size.Height - WidgetPadding * 2 - 4);
        ListDrawer.SetWidth(MainContainer.Size.Width);
        MainContainer.VScrollBar.SetPosition(Size.Width - WidgetPadding - 10, 2 + WidgetPadding);
        MainContainer.VScrollBar.SetSize(8, Size.Height - WidgetPadding * 2 - 4);
    }

    public override void Redraw()
    {
        base.Redraw();
        ListDrawer.Redraw();
    }
}

public class DesignListDrawer : Widget
{
    public Font Font { get; protected set; }
    public int LineHeight { get; protected set; } = 20;
    public List<ListItem> Items { get; protected set; } = new List<ListItem>();
    public bool Enabled { get; protected set; } = true;
    public int SelectedIndex { get; protected set; } = -1;
    public Color SelectedItemColor { get; protected set; } = new Color(55, 187, 255);

    public DesignListDrawer(IContainer Parent) : base(Parent)
    {
        this.Font = Fonts.Paragraph;
        Sprites["selection"] = new Sprite(this.Viewport, new SolidBitmap(Size.Width, 20, new Color(28, 50, 73)));
        Sprites["selection"].Visible = false;
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, 20, new Color(59, 227, 255)));
        Sprites["hover"].Visible = false;
    }

    public void SetFont(Font f)
    {
        if (this.Font != f)
        {
            this.Font = f;
            Redraw();
        }
    }

    public void SetLineHeight(int Height)
    {
        if (this.LineHeight != Height)
        {
            this.LineHeight = Height;
            if (SelectedIndex != -1) Sprites["selection"].Y = LineHeight * SelectedIndex;
            ((SolidBitmap)Sprites["hover"].Bitmap).SetSize(2, LineHeight);
            Redraw();
        }
    }

    public void SetItems(List<ListItem> Items)
    {
        this.Items = Items;
        Redraw();
        if (SelectedIndex >= Items.Count) SetSelectedIndex(Items.Count - 1);
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            this.Redraw();
        }
    }

    public void SetSelectedIndex(int Index, bool ForceRefresh = false)
    {
        if (this.SelectedIndex != Index || ForceRefresh)
        {
            this.SelectedIndex = Index;
            if (Index == -1)
            {
                Sprites["selection"].Visible = false;
            }
            else
            {
                Sprites["selection"].Visible = true;
                Sprites["selection"].Y = LineHeight * Index;
            }
            this.Redraw();
        }
    }

    public void SetSelectedItemColor(Color SelectedItemColor)
    {
        if (this.SelectedItemColor != SelectedItemColor)
        {
            this.SelectedItemColor = SelectedItemColor;
            Redraw();
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        ((SolidBitmap) Sprites["selection"].Bitmap).SetSize(Size.Width, LineHeight);
    }

    protected override void Draw()
    {
        if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
        SetSize(Size.Width, LineHeight * Items.Count);
        Sprites["text"].Bitmap = new Bitmap(Size);
        Sprites["text"].Bitmap.Font = this.Font;
        Sprites["text"].Bitmap.Unlock();
        for (int i = 0; i < this.Items.Count; i++)
        {
            bool sel = i == SelectedIndex;
            Color c = this.Enabled ? (sel ? this.SelectedItemColor : Color.WHITE) : new Color(72, 72, 72);
            Sprites["text"].Bitmap.DrawText(this.Items[i].ToString(), 10, LineHeight * i + LineHeight / 2 - 10, c);
        }
        Sprites["text"].Bitmap.Lock();
        base.Draw();
    }
}
