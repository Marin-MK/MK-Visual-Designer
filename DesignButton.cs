using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner;

public class DesignButton : DesignWidget
{
    public override bool PasteAsChildren => false;

    public static void CreateFadeCache()
    {
        Color black = new Color(0, 0, 0, 64);
        Bitmap c = new Bitmap(5, 5);
        c.Unlock();
        c.FillGradientRect(0, 0, 5, 5, Color.ALPHA, Color.ALPHA, Color.ALPHA, black);
        c.Lock();
        ButtonCornerFade = c;
        Bitmap h = new Bitmap(5, 1);
        h.Unlock();
        h.DrawGradientLine(0, 0, 4, 0, Color.ALPHA, black);
        h.Lock();
        ButtonHorizontalFade = h;
        Bitmap v = new Bitmap(1, 5);
        v.Unlock();
        v.DrawGradientLine(0, 0, 0, 4, Color.ALPHA, black);
        v.Lock();
        ButtonVerticalFade = v;
    }

    public static Bitmap ButtonCornerFade;
    public static Bitmap ButtonHorizontalFade;
    public static Bitmap ButtonVerticalFade;

    public string Text { get; protected set; }
    public Font Font { get; protected set; }
    public Color TextColor { get; protected set; } = Color.WHITE;
    public bool LeftAlign { get; protected set; } = false;
    public int TextX { get; protected set; } = 0;
    public bool Enabled { get; protected set; } = true;
    public bool Repeatable { get; protected set; } = false;

    int MaxWidth;

    public DesignButton(IContainer Parent) : base(Parent, "UnnamedButton")
	{
        this.Font = Fonts.ParagraphBold;

        Bitmap corner = ButtonCornerFade;
        Bitmap hor = ButtonHorizontalFade;
        Bitmap vert = ButtonVerticalFade;

        Sprites["topleft"] = new Sprite(this.Viewport, corner);
        Sprites["topleft"].DestroyBitmap = false;

        Sprites["bottomleft"] = new Sprite(this.Viewport, corner);
        Sprites["bottomleft"].MirrorY = true;
        Sprites["bottomleft"].DestroyBitmap = false;

        Sprites["topright"] = new Sprite(this.Viewport, corner);
        Sprites["topright"].MirrorX = true;
        Sprites["topright"].DestroyBitmap = false;

        Sprites["bottomright"] = new Sprite(this.Viewport, corner);
        Sprites["bottomright"].MirrorX = Sprites["bottomright"].MirrorY = true;
        Sprites["bottomright"].DestroyBitmap = false;

        Sprites["left"] = new Sprite(this.Viewport, hor);
        Sprites["left"].DestroyBitmap = false;
        Sprites["right"] = new Sprite(this.Viewport, hor);
        Sprites["right"].MirrorX = true;
        Sprites["right"].DestroyBitmap = false;

        Sprites["top"] = new Sprite(this.Viewport, vert);
        Sprites["top"].DestroyBitmap = false;
        Sprites["bottom"] = new Sprite(this.Viewport, vert);
        Sprites["bottom"].MirrorY = true;
        Sprites["bottom"].DestroyBitmap = false;

        Sprites["filler"] = new Sprite(this.Viewport);
        Sprites["filler"].X = Sprites["filler"].Y = WidgetPadding + corner.Width;

        Sprites["left"].X = WidgetPadding;
        Sprites["left"].Y = WidgetPadding + corner.Width;
        Sprites["right"].Y = Sprites["left"].Y;
        Sprites["topleft"].X = WidgetPadding;
        Sprites["topleft"].Y = WidgetPadding;
        Sprites["topright"].Y = WidgetPadding;
        Sprites["bottomleft"].X = WidgetPadding;
        Sprites["top"].X = WidgetPadding + corner.Width;
        Sprites["top"].Y = WidgetPadding;
        Sprites["bottom"].X = Sprites["top"].X;

        Sprites["text"] = new Sprite(this.Viewport);

        MinimumSize.Width += corner.Width * 2 + 1;
        MinimumSize.Height += corner.Height * 2 + 1;

        this.Properties.AddRange(new List<Property>()
        {
            new Property("Text", PropertyType.Text, () => Text, e =>
			{
				string OldText = Text;
				SetText((string) e);
				if (Text != OldText) Undo.GenericUndoAction<string>.Register(this, "SetText", OldText, Text, true);
			}),

            new Property("Text Color", PropertyType.Color, () => TextColor, e =>
			{
				Color OldTextColor = TextColor;
				SetTextColor((Color) e);
				if (!TextColor.Equals(OldTextColor)) Undo.GenericUndoAction<Color>.Register(this, "SetTextColor", OldTextColor, TextColor, true);
			}),

            new Property("Font", PropertyType.Font, () => Font, e =>
			{
				Font OldFont = Font;
				SetFont((Font) e);
				if (!Font.Equals(OldFont)) Undo.GenericUndoAction<Font>.Register(this, "SetFont", OldFont, Font, true);
			}),

            new Property("Left-Align", PropertyType.Boolean, () => LeftAlign, e =>
            {
                bool OldLeftAlign = LeftAlign;
                SetLeftAlign((bool) e);
                if (OldLeftAlign != LeftAlign) Undo.GenericUndoAction<bool>.Register(this, "SetLeftAlign", OldLeftAlign, LeftAlign, true);
                Program.ParameterPanel.Refresh();
            }),

            new Property("Text X", PropertyType.Numeric, () => TextX, e =>
            {
                int OldTextX = TextX;
                SetTextX((int) e);
                if (OldTextX != TextX) Undo.GenericUndoAction<int>.Register(this, "SetTextX", OldTextX, TextX, true);
            }, null, () => LeftAlign, "Not left-aligned"),

            new Property("Enabled", PropertyType.Boolean, () => Enabled, e =>
            {
                bool OldEnabled = Enabled;
                SetEnabled((bool) e);
                if (OldEnabled != Enabled) Undo.GenericUndoAction<bool>.Register(this, "SetEnabled", OldEnabled, Enabled, true);
            }),

            new Property("Repeatable", PropertyType.Boolean, () => Repeatable, e =>
            {
                bool OldRepeatable = Repeatable;
                SetRepeatable((bool) e);
                if (OldRepeatable != Repeatable) Undo.GenericUndoAction<bool>.Register(this, "SetRepeatable", OldRepeatable, Repeatable, true);
            })
        });
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            this.Text = Text;
            RedrawText();
        }
    }

    public void SetFont(Font Font)
    {
        if (this.Font != Font)
        {
            this.Font = Font;
            RedrawText();
        }
    }

    public void SetTextColor(Color TextColor)
    {
        if (this.TextColor != TextColor)
        {
            this.TextColor = TextColor;
            RedrawText();
        }
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            RedrawFiller();
            RedrawText();
        }
    }

    public void SetLeftAlign(bool LeftAlign)
    {
        if (this.LeftAlign != LeftAlign)
        {
            this.LeftAlign = LeftAlign;
            if (!LeftAlign)
            {
                Sprites["text"].X = WidgetPadding + (Size.Width - WidgetPadding * 2) / 2 - MaxWidth / 2;
            }
            else
            {
                Sprites["text"].X = WidgetPadding + 10 + TextX;
            }
        }
    }

    public void SetTextX(int TextX)
    {
        if (this.TextX != TextX)
        {
            this.TextX = TextX;
            if (LeftAlign) Sprites["text"].X = WidgetPadding + 10 + TextX;
        }
    }

    public void SetRepeatable(bool Repeatable)
    {
        if (this.Repeatable != Repeatable)
        {
            this.Repeatable = Repeatable;
        }
    }

    public void RedrawText()
    {
        Sprites["text"].Bitmap?.Dispose();
        if (string.IsNullOrEmpty(this.Text)) return;
        List<string> Lines = this.Text.Split('\n').ToList();
        MaxWidth = 0;
        Lines.ForEach(l => MaxWidth = Math.Max(MaxWidth, this.Font.TextSize(l).Width));
        Sprites["text"].Bitmap = new Bitmap(MaxWidth, Size.Height - WidgetPadding * 2);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = this.Font;
        Color c = this.Enabled ? this.TextColor : new Color(147, 158, 169);
        for (int i = 0; i < Lines.Count; i++)
        {
            Sprites["text"].Bitmap.DrawText(Lines[i], MaxWidth / 2, i * 18, c, DrawOptions.CenterAlign);
        }
        Sprites["text"].Bitmap.Lock();
        if (!LeftAlign)
        {
            Sprites["text"].X = WidgetPadding + (Size.Width - WidgetPadding * 2) / 2 - MaxWidth / 2;
        }
        else
        {
            Sprites["text"].X = WidgetPadding + 10 + TextX;
        }
        Sprites["text"].Y = WidgetPadding + (Size.Height - WidgetPadding * 2) / 2 - 9 * Lines.Count - Font.Size / 2 + 4;
    }

    public void RedrawFiller()
    {
        if (Sprites["filler"].Bitmap != null) Sprites["filler"].Bitmap.Dispose();
        int w = Size.Width - WidgetPadding * 2 - 5 * 2;
        int h = Size.Height - WidgetPadding * 2 - 5 * 2;
        Sprites["filler"].Bitmap = new Bitmap(w, h);
        Sprites["filler"].Bitmap.Unlock();
        if (this.Enabled) Sprites["filler"].Bitmap.FillRect(0, 0, w, h, new Color(51, 86, 121));
        else Sprites["filler"].Bitmap.FillRect(0, 0, w, h, new Color(51, 86, 121));
        Sprites["filler"].Bitmap.Lock();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        RedrawFiller();

        int o = 5;

        Sprites["bottomleft"].Y = Size.Height - WidgetPadding - o;
        Sprites["topright"].X = Size.Width - WidgetPadding - o;
        Sprites["bottomright"].X = Sprites["topright"].X;
        Sprites["bottomright"].Y = Sprites["bottomleft"].Y;

        Sprites["right"].X = Sprites["topright"].X;
        Sprites["left"].ZoomY = Size.Height - WidgetPadding * 2 - 2 * o;
        Sprites["right"].ZoomY = Sprites["left"].ZoomY;

        Sprites["top"].ZoomX = Size.Width - WidgetPadding * 2 - 2 * o;
        Sprites["bottom"].Y = Sprites["bottomleft"].Y;
        Sprites["bottom"].ZoomX = Sprites["top"].ZoomX;

        if (!string.IsNullOrEmpty(this.Text))
        {
            if (!LeftAlign)
            {
                Sprites["text"].X = WidgetPadding + (Size.Width - WidgetPadding * 2) / 2 - MaxWidth / 2;
            }
            else
            {
                Sprites["text"].X = WidgetPadding + 10 + TextX;
            }
            Sprites["text"].Y = WidgetPadding + (Size.Height - WidgetPadding * 2) / 2 - 9 * this.Text.Split('\n').Length - Font.Size / 2 + 4;
        }
    }
}
