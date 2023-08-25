namespace VisualDesigner;

[WidgetTypeAndName(typeof(RadioBoxData), "radiobox")]
public class DesignRadioBox : DesignWidget
{
    public string Text { get; protected set; }
    public bool Checked { get; protected set; } = false;
    public Font Font { get; protected set; }
    public bool Enabled { get; protected set; } = true;

    public DesignRadioBox(IContainer Parent) : base(Parent, "UnnamedRadioBox")
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
            if (!this.Enabled) this.SetChecked(false);
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
            if (Checked)
            {
                foreach (Widget w in Parent.Widgets)
                {
                    if (w is DesignRadioBox && w != this && ((DesignRadioBox) w).Checked) ((DesignRadioBox) w).SetChecked(false);
                }
            }
            this.Checked = Checked;
            Redraw();
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
        Sprites["text"].Bitmap.DrawText(this.Text, this.Enabled ? Color.WHITE : new Color(72, 72, 72));
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

        Sprites["box"].Bitmap.FillRect(2, 2, 12, 12, Filler);
        Sprites["box"].Bitmap.DrawLine(5, 0, 10, 0, Edges);
        Sprites["box"].Bitmap.SetPixel(3, 1, Edges);
        Sprites["box"].Bitmap.SetPixel(4, 1, Edges);
        Sprites["box"].Bitmap.SetPixel(2, 2, Edges);
        Sprites["box"].Bitmap.SetPixel(1, 3, Edges);
        Sprites["box"].Bitmap.SetPixel(1, 4, Edges);
        Sprites["box"].Bitmap.DrawLine(0, 5, 0, 10, Edges);
        Sprites["box"].Bitmap.SetPixel(11, 1, Edges);
        Sprites["box"].Bitmap.SetPixel(12, 1, Edges);
        Sprites["box"].Bitmap.SetPixel(13, 2, Edges);
        Sprites["box"].Bitmap.SetPixel(14, 3, Edges);
        Sprites["box"].Bitmap.SetPixel(14, 4, Edges);
        Sprites["box"].Bitmap.DrawLine(15, 5, 15, 10, Edges);
        Sprites["box"].Bitmap.SetPixel(1, 11, Edges);
        Sprites["box"].Bitmap.SetPixel(1, 12, Edges);
        Sprites["box"].Bitmap.SetPixel(2, 13, Edges);
        Sprites["box"].Bitmap.SetPixel(3, 14, Edges);
        Sprites["box"].Bitmap.SetPixel(4, 14, Edges);
        Sprites["box"].Bitmap.DrawLine(5, 15, 10, 15, Edges);
        Sprites["box"].Bitmap.SetPixel(11, 14, Edges);
        Sprites["box"].Bitmap.SetPixel(12, 14, Edges);
        Sprites["box"].Bitmap.SetPixel(13, 13, Edges);
        Sprites["box"].Bitmap.SetPixel(14, 11, Edges);
        Sprites["box"].Bitmap.SetPixel(14, 12, Edges);
        Sprites["box"].Bitmap.SetPixel(2, 3, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(2, 4, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(3, 2, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(4, 2, DarkOutline);
        Sprites["box"].Bitmap.DrawLine(1, 5, 1, 10, DarkOutline);
        Sprites["box"].Bitmap.DrawLine(5, 1, 10, 1, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(2, 11, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(2, 12, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(3, 13, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(4, 13, DarkOutline);
        Sprites["box"].Bitmap.DrawLine(5, 14, 10, 14, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(11, 2, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(12, 2, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(13, 3, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(13, 4, DarkOutline);
        Sprites["box"].Bitmap.DrawLine(14, 5, 14, 10, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(11, 13, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(12, 13, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(13, 11, DarkOutline);
        Sprites["box"].Bitmap.SetPixel(13, 12, DarkOutline);

        if (Lock) Sprites["box"].Bitmap.Lock();
    }

    protected override void Draw()
    {
        Sprites["box"].Bitmap.Unlock();
        RedrawBox(false);
        if (this.Checked && this.Enabled)
        {
            Color checkcolor = this.Enabled ? Color.WHITE : new Color(120, 120, 120);
            Sprites["box"].Bitmap.FillRect(5, 5, 6, 6, checkcolor);
            Sprites["box"].Bitmap.DrawLine(6, 4, 9, 4, checkcolor);
            Sprites["box"].Bitmap.DrawLine(4, 6, 4, 9, checkcolor);
            Sprites["box"].Bitmap.DrawLine(11, 6, 11, 9, checkcolor);
            Sprites["box"].Bitmap.DrawLine(6, 11, 9, 11, checkcolor);
        }
        Sprites["box"].Bitmap.Lock();
        base.Draw();
    }
}
