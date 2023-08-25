namespace VisualDesigner;

[WidgetTypeAndName(typeof(DropdownBoxData), "dropdownbox")]
public class DesignDropdownBox : DesignWidget
{
    public override bool PasteAsChildren => false;

    public List<ListItem> Items { get; protected set; } = new List<ListItem>();
    public int SelectedIndex { get; protected set; } = -1;
    public bool ReadOnly => TextArea.ReadOnly;
    public bool Enabled { get; protected set; } = true;

    DesignTextArea TextArea;

    public DesignDropdownBox(IContainer Parent) : base(Parent, "UnnamedDropdownBox")
    {
        Sprites["bg"] = new Sprite(this.Viewport);
        Sprites["bg"].X = WidgetPadding;
        Sprites["bg"].Y = WidgetPadding;
        TextArea = new DesignTextArea(this);
        TextArea.SetPosition(6 + WidgetPadding, 2 + WidgetPadding);
        TextArea.SetFont(Fonts.Paragraph);
        TextArea.SetCaretColor(Color.WHITE);
        TextArea.SetReadOnly(true);
        TextArea.SetZIndex(1);
        MinimumSize.Height += 24;
        MaximumSize.Height = MinimumSize.Height;
        SetHeight(24 + HeightAdd);

        this.Properties.AddRange(new List<Property>()
        {
            new Property("Items", PropertyType.List, () => Items.Select(x => x.Name).ToList(), e =>
            {
                List<ListItem> OldItems = Items;
                SetItems(((List<string>) e).Select(x => new ListItem(x)).ToList());
                if (!OldItems.Equals(Items)) Undo.GenericUndoAction<List<ListItem>>.Register(this, "SetItems", OldItems, Items, true);
            }),

            new Property("Selected Index", PropertyType.Numeric, () => SelectedIndex, e =>
            {
                int OldSelectedIndex = SelectedIndex;
                SetSelectedIndex((int) e);
                if (OldSelectedIndex != SelectedIndex) Undo.GenericUndoAction<int>.Register(this, "SetSelectedIndex", OldSelectedIndex, SelectedIndex, true);
            }, new List<object>() { -1 }),

            new Property("Enabled", PropertyType.Boolean, () => Enabled, e =>
            {
                bool OldEnabled = Enabled;
                SetEnabled((bool) e);
                if (OldEnabled != Enabled) Undo.GenericUndoAction<bool>.Register(this, "SetEnabled", OldEnabled, Enabled, true);
            }),
            
            new Property("Read-only", PropertyType.Boolean, () => ReadOnly, e =>
            {
                bool OldReadOnly = ReadOnly;
                SetReadOnly((bool) e);
                if (OldReadOnly != ReadOnly) Undo.GenericUndoAction<bool>.Register(this, "SetReadOnly", OldReadOnly, ReadOnly, true);
            }),
        });
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            this.TextArea.SetEnabled(Enabled);
            this.Redraw();
        }
    }

    public void SetReadOnly(bool ReadOnly)
    {
        TextArea.SetReadOnly(ReadOnly);
        Redraw();
    }

    public void SetSelectedIndex(int Index)
    {
        if (this.SelectedIndex != Index)
        {
            this.TextArea.SetText(Index >= Items.Count || Index == -1 ? "" : Items[Index].Name);
            this.SelectedIndex = Index;
        }
    }

    public void SetItems(List<ListItem> Items)
    {
        this.Items = Items;
        this.TextArea.SetText(SelectedIndex >= Items.Count || SelectedIndex == -1 ? "" : Items[SelectedIndex].Name);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.SetSize(this.Size.Width - 28 - WidthAdd, this.Size.Height - 3 - HeightAdd);
    }

    protected override void Draw()
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size.Width - WidthAdd, Size.Height - HeightAdd);
        Sprites["bg"].Bitmap.Unlock();
        Color Filler = this.Enabled ? new Color(10, 23, 37) : new Color(24, 38, 53);
        Sprites["bg"].Bitmap.FillRect(Size.Width - WidthAdd, Size.Height - 2 - HeightAdd, Filler);
        Color ArrowColor = new Color(86, 108, 134);
        Color ArrowShadow = new Color(17, 27, 38);
        Color LineColor = ArrowColor;
        Sprites["bg"].Bitmap.FillRect(0, Size.Height - 2 - HeightAdd, Size.Width - WidthAdd, 2, LineColor);
        int x = Size.Width - 18 - WidthAdd;
        int y = Size.Height / 2 - 14;
        Sprites["bg"].Bitmap.FillRect(x, y, 11, 4, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 3, y + 4, x + 7, y + 4, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 4, y + 5, x + 6, y + 5, ArrowColor);
        Sprites["bg"].Bitmap.SetPixel(x + 5, y + 6, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x, y + 2, x + 5, y + 7, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x, y + 3, x + 5, y + 8, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x + 6, y + 6, x + 10, y + 2, ArrowShadow);
        Sprites["bg"].Bitmap.DrawLine(x + 6, y + 7, x + 10, y + 3, ArrowShadow);
        Sprites["bg"].Bitmap.Lock();
        base.Draw();
    }
}
