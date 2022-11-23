using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner;

[WidgetTypeAndName(typeof(BrowserBoxData), "browserbox")]
public class DesignBrowserBox : DesignWidget
{
    public string Text => TextArea.Text;
    public Font Font => TextArea.Font;
    public Color TextColor => TextArea.TextColor;
    public bool ReadOnly => TextArea.ReadOnly;
    public bool Enabled { get; protected set; } = true;

    DesignTextArea TextArea;

    public DesignBrowserBox(IContainer Parent) : base(Parent, "UnnamedBrowserBox")
    {
        Sprites["bg"] = new Sprite(this.Viewport);
        Sprites["bg"].X = WidgetPadding;
        Sprites["bg"].Y = WidgetPadding;
        TextArea = new DesignTextArea(this);
        TextArea.SetPosition(3 + WidgetPadding, 3 + WidgetPadding);
        TextArea.SetZIndex(1);
        TextArea.SetReadOnly(true);
        MinimumSize.Height += 25;
        MaximumSize.Height = MinimumSize.Height;
        SetFont(Fonts.Paragraph);

        this.Properties.AddRange(new List<Property>()
        {
            new Property("Text", PropertyType.Text, () => Text, e =>
            {
                string OldText = Text;
                SetText((string) e);
                if (OldText != Text) Undo.GenericUndoAction<string>.Register(this, "SetText", OldText, Text, true);
            }),

            new Property("Font", PropertyType.Font, () => Font, e =>
            {
                Font OldFont = Font;
                SetFont((Font) e);
                if (!OldFont.Equals(Font)) Undo.GenericUndoAction<Font>.Register(this, "SetFont", OldFont, Font, true);
            }),

            new Property("Text Color", PropertyType.Color, () => TextColor, e =>
            {
                Color OldTextColor = TextColor;
                SetTextColor((Color)e);
                if (!OldTextColor.Equals(TextColor)) Undo.GenericUndoAction<Color>.Register(this, "SetTextColor", OldTextColor, TextColor, true);
            }),

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
            this.Redraw();
            this.TextArea.SetEnabled(Enabled);
        }
    }

    public void SetText(string Text)
    {
        TextArea.SetText(Text);
    }

    public void SetFont(Font f)
    {
        TextArea.SetFont(f);
    }

    public void SetTextColor(Color c)
    {
        TextArea.SetTextColor(c);
    }

    public void SetReadOnly(bool ReadOnly)
    {
        TextArea.SetReadOnly(ReadOnly);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.SetSize(this.Size.Width - 29 - WidthAdd, this.Size.Height - 3 - HeightAdd);
    }

    protected override void Draw()
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size.Width - WidthAdd, Size.Height - HeightAdd);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawRect(0, 0, Size.Width - WidthAdd, Size.Height - HeightAdd, 86, 108, 134);
        Color FillerColor = this.Enabled ? new Color(10, 23, 37) : new Color(24, 38, 53);
        Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2 - WidthAdd, Size.Height - 2 - HeightAdd, FillerColor);
        Color OutlineColor = new Color(86, 108, 134);
        Sprites["bg"].Bitmap.DrawLine(Size.Width - 25 - WidthAdd, 1, Size.Width - 25 - WidthAdd, Size.Height - 2 - HeightAdd, OutlineColor);
        Color ArrowColor = OutlineColor;
        
        Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1 - WidthAdd, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1 - HeightAdd, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1 - WidthAdd, Size.Height - 1 - HeightAdd, Color.ALPHA);

        int x = Size.Width - 18 - WidthAdd;
        int y = 7;
        Sprites["bg"].Bitmap.FillRect(x + 2, y, 2, 11, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 4, y + 1, x + 4, y + 9, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 5, y + 2, x + 5, y + 8, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 6, y + 3, x + 6, y + 7, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 7, y + 4, x + 7, y + 6, ArrowColor);
        Sprites["bg"].Bitmap.SetPixel(x + 8, y + 5, ArrowColor);

        Sprites["bg"].Bitmap.Lock();

        base.Draw();
    }
}
