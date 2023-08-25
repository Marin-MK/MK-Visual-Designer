namespace VisualDesigner;

[WidgetTypeAndName(typeof(CheckBoxData), "checkbox")]
public class DesignCheckBox : DesignWidget
{
    public string Text { get; protected set; }
    public bool Checked { get; protected set; } = false;
    public Font Font { get; protected set; }
    public bool Enabled { get; protected set; } = true;
    public bool Mirrored { get; protected set; } = false;

    public DesignCheckBox(IContainer Parent) : base(Parent, "UnnamedCheckBox")
    {
        this.Font = Fonts.Paragraph;
        Sprites["box"] = new Sprite(this.Viewport, new Bitmap(16, 16));
        Sprites["box"].X = WidgetPadding;
        Sprites["box"].Y = WidgetPadding;
        RedrawBox(true);
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].X = 20 + WidgetPadding;
        Sprites["text"].Y = -1 + WidgetPadding;
        SetText("");
        SetHeight(16 + HeightAdd);
        MinimumSize.Height += 16;
        MaximumSize.Height = MinimumSize.Height;

        this.Properties.AddRange(new List<Property>()
        {
            new Property("Text", PropertyType.Text, () => Text, e =>
            {
                string OldText = Text;
                SetText((string) e);
                if (OldText != Text) Undo.GenericUndoAction<string>.Register(this, "SetText", OldText, Text, true);
            }),

            new Property("Checked", PropertyType.Boolean, () => Checked, e =>
            {
                bool OldChecked = Checked;
                SetChecked((bool) e);
                if (OldChecked != Checked) Undo.GenericUndoAction<bool>.Register(this, "SetChecked", OldChecked, Checked, true);
            }),

            new Property("Font", PropertyType.Font, () => Font, e =>
            {
                Font OldFont = Font;
                SetFont((Font) e);
                if (!OldFont.Equals(Font)) Undo.GenericUndoAction<Font>.Register(this, "SetFont", OldFont, Font, true);
            }),

            new Property("Mirrored", PropertyType.Boolean, () => Mirrored, e =>
            {
                bool OldMirrored = Mirrored;
                SetMirrored((bool) e);
                if (OldMirrored != Mirrored) Undo.GenericUndoAction<bool>.Register(this, "SetMirrored", OldMirrored, Mirrored, true);
            }),

            new Property("Enabled", PropertyType.Boolean, () => Enabled, e =>
            {
                bool OldEnabled = Enabled;
                SetEnabled((bool) e);
                if (OldEnabled != Enabled) Undo.GenericUndoAction<bool>.Register(this, "SetEnabled", OldEnabled, Enabled, true);
            })
        });
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            this.Redraw();
            this.RedrawText();
        }
    }

    public void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            this.RedrawText();
        }
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            this.Text = Text;
            this.RedrawText();
        }
    }

    public void SetChecked(bool Checked)
    {
        if (this.Checked != Checked)
        {
            this.Checked = Checked;
            this.Redraw();
        }
    }

    public void SetMirrored(bool Mirrored)
    {
        if (this.Mirrored != Mirrored)
        {
            this.Mirrored = Mirrored;
            Sprites["text"].X = WidgetPadding + (this.Mirrored ? 0 : 20);
            Sprites["box"].X = this.Mirrored ? Size.Width - 16 - WidgetPadding : WidgetPadding;
        }
    }

    public void RedrawText()
    {
        if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
        Size s = this.Font.TextSize(this.Text);
        if (20 + s.Width >= Size.Width - WidthAdd)
        {
            MinimumSize.Width = 20 + s.Width + WidthAdd;
            MaximumSize.Width = MinimumSize.Width;
            SetWidth(20 + s.Width + WidthAdd);
        }
        Sprites["text"].Bitmap = new Bitmap(Math.Max(1, s.Width), Math.Max(1, s.Height));
        Sprites["text"].Bitmap.Font = this.Font;
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.DrawText(this.Text, this.Enabled ? Color.WHITE : new Color(147, 158, 169));
        Sprites["text"].Bitmap.Lock();
    }

    public void RedrawBox(bool Lock)
    {
        if (Lock) Sprites["box"].Bitmap.Unlock();
        Sprites["box"].Bitmap.Clear();
        Color Edges = null;
        Color DarkOutline = new Color(36, 34, 36);
        Color Filler = null;
        if (this.Enabled)
        {
            if (this.Checked) Edges = new Color(32, 170, 221);
            else Edges = new Color(86, 108, 134);
        }
        else
        {
            if (this.Checked) Edges = new Color(64, 104, 146);
            else Edges = new Color(86, 108, 134);
        }
        Filler = !this.Enabled && !this.Checked ? new Color(40, 62, 84) : Edges;

        Sprites["box"].Bitmap.DrawRect(1, 1, 14, 14, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(1, 1, Edges);
        Sprites["box"].Bitmap.DrawLine(2, 0, 13, 0, Edges);
        Sprites["box"].Bitmap.SetPixel(14, 1, Edges);
        Sprites["box"].Bitmap.DrawLine(15, 2, 15, 13, Edges);
        Sprites["box"].Bitmap.SetPixel(14, 14, Edges);
        Sprites["box"].Bitmap.DrawLine(2, 15, 13, 15, Edges);
        Sprites["box"].Bitmap.SetPixel(1, 14, Edges);
        Sprites["box"].Bitmap.DrawLine(0, 2, 0, 13, Edges);
        Sprites["box"].Bitmap.FillRect(2, 2, 12, 12, Filler);
        if (Lock) Sprites["box"].Bitmap.Lock();
    }

    protected override void Draw()
    {
        Sprites["box"].Bitmap.Unlock();
        RedrawBox(false);
        if (this.Checked)
        {
            int x = 4;
            int y = 4;
            Color Checkmark = this.Enabled ? Color.WHITE : new Color(147, 158, 169);
            Sprites["box"].Bitmap.DrawLine(x, y + 5, x + 1, y + 5, Checkmark);
            Sprites["box"].Bitmap.DrawLine(x + 1, y + 6, x + 4, y + 6, Checkmark);
            Sprites["box"].Bitmap.DrawLine(x + 2, y + 7, x + 4, y + 7, Checkmark);
            Sprites["box"].Bitmap.SetPixel(x + 3, y + 8, Checkmark);
            Sprites["box"].Bitmap.FillRect(x + 4, y + 4, 2, 2, Checkmark);
            Sprites["box"].Bitmap.FillRect(x + 5, y + 2, 2, 2, Checkmark);
            Sprites["box"].Bitmap.DrawLine(x + 6, y + 1, x + 7, y + 1, Checkmark);
            Sprites["box"].Bitmap.SetPixel(x + 7, y, Checkmark);
        }
        Sprites["box"].Bitmap.Lock();
        base.Draw();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["box"].X = this.Mirrored ? Size.Width - 16 - WidgetPadding : WidgetPadding;
    }
}
