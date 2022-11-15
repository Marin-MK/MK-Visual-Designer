using amethyst.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner;

public class DesignTextBox : DesignWidget
{
    public string Text { get; protected set; } = "";
    public int TextX { get; protected set; } = 0;
    public int TextY { get; protected set; } = 0;
    public int CaretY { get; protected set; } = 2;
    public int CaretHeight { get; protected set; } = 13;
    public Font Font { get; protected set; }
    public Color TextColor { get; protected set; } = Color.WHITE;
    public Color DisabledTextColor { get; protected set; } = new Color(141, 151, 163);
    public Color CaretColor { get; protected set; } = Color.WHITE;
    public Color FillerColor { get; protected set; } = new Color(0, 120, 215);
    public bool ReadOnly { get; protected set; } = false;
    public bool Enabled { get; protected set; } = true;
    public bool NumericOnly { get; protected set; } = false;
    public int DefaultNumericValue { get; protected set; } = 0;
    public bool AllowMinusSigns { get; protected set; } = true;
    public bool ShowDisabledText { get; protected set; } = false;
    public bool DeselectOnEnterPressed { get; protected set; } = true;
    public bool PopupStyle { get; protected set; } = true;

    protected DesignTextArea TextArea;

    public DesignTextBox(IContainer Parent) : base(Parent, "UnnamedTextBox")
	{
        Sprites["box"] = new Sprite(this.Viewport);
        Sprites["box"].X = WidgetPadding;
        Sprites["box"].Y = WidgetPadding;
        TextArea = new DesignTextArea(this);
        TextArea.SetPosition(6 + WidgetPadding, 4 + WidgetPadding);
        TextArea.SetFont(Fonts.Paragraph);
        TextArea.SetCaretColor(Color.WHITE);

        MinimumSize.Height += 27;
        MaximumSize.Height = MinimumSize.Height;
    }

    public void SetText(string Text)
    {
        TextArea.SetText(Text);
    }

    public void SetTextX(int TextX)
    {
        TextArea.SetTextX(TextX);
    }

    public void SetTextY(int TextY)
    {
        TextArea.SetTextY(TextY);
    }

    public void SetCaretY(int CaretY)
    {
        TextArea.SetCaretY(CaretY);
    }

    public void SetCaretHeight(int CaretHeight)
    {
        TextArea.SetCaretHeight(CaretHeight);
    }

    public void SetFont(Font Font)
    {
        TextArea.SetFont(Font);
    }

    public void SetTextColor(Color TextColor)
    {
        TextArea.SetTextColor(TextColor);
    }

    public void SetDisabledTextColor(Color DisabledTextColor)
    {
        TextArea.SetDisabledTextColor(DisabledTextColor);
    }

    public void SetCaretColor(Color CaretColor)
    {
        TextArea.SetCaretColor(CaretColor);
    }

    public void SetFillerColor(Color FillerColor)
    {
        TextArea.SetFillerColor(FillerColor);
    }

    public void SetReadOnly(bool ReadOnly)
    {
        TextArea.SetReadOnly(ReadOnly);
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

    public void SetNumericOnly(bool NumericOnly)
    {
        TextArea.SetNumericOnly(NumericOnly);
    }

    public void SetDefaultNumericValue(int DefaultNumericValue)
    {
        TextArea.SetDefaultNumericValue(DefaultNumericValue);
    }

    public void SetAllowMinusSigns(bool AllowMinusSigns)
    {
        TextArea.SetAllowMinusSigns(AllowMinusSigns);
    }

    public void SetShowDisabledText(bool ShowDisabledText)
    {
        TextArea.SetShowDisabledText(ShowDisabledText);
    }

    public void SetDeselectOnEnterPressed(bool DeselectOnEnterPressed)
    {
        TextArea.SetDeselectOnEnterPress(DeselectOnEnterPressed);
    }

    public void SetPopupStyle(bool PopupStyle)
    {
        if (this.PopupStyle != PopupStyle)
        {
            this.PopupStyle = PopupStyle;
            this.Redraw();
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.SetSize(Size.Width - WidgetPadding * 2 - 12, Size.Height - WidgetPadding * 2 - 8);
        Redraw();
    }

    protected override void Draw()
    {
        Sprites["box"].Bitmap?.Dispose();
        Sprites["box"].Bitmap = new Bitmap(Size.Width - WidgetPadding * 2, Size.Height - WidgetPadding * 2);
        Sprites["box"].Bitmap.Unlock();
        if (this.PopupStyle)
        {
            Color Edge = new Color(86, 108, 134);
            Color Outline = new Color(36, 34, 36);
            Color Filler = this.Enabled ? new Color(86, 108, 134) : new Color(40, 62, 84);
            Sprites["box"].Bitmap.DrawRect(1, 1, Size.Width - WidgetPadding * 2 - 2, Size.Height - WidgetPadding * 2 - 2, Outline);
            Sprites["box"].Bitmap.SetPixel(1, 1, Edge);
            Sprites["box"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 2, 1, Edge);
            Sprites["box"].Bitmap.SetPixel(1, Size.Height - WidgetPadding * 2 - 2, Edge);
            Sprites["box"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 2, Size.Height - WidgetPadding * 2 - 2, Edge);
            Sprites["box"].Bitmap.DrawLine(2, 0, Size.Width - WidgetPadding * 2 - 3, 0, Edge);
            Sprites["box"].Bitmap.DrawLine(0, 2, 0, Size.Height - WidgetPadding * 2 - 3, Edge);
            Sprites["box"].Bitmap.DrawLine(Size.Width - WidgetPadding * 2 - 1, 2, Size.Width - WidgetPadding * 2 - 1, Size.Height - WidgetPadding * 2 - 3, Edge);
            Sprites["box"].Bitmap.DrawLine(2, Size.Height - WidgetPadding * 2 - 1, Size.Width - WidgetPadding * 2 - 3, Size.Height - WidgetPadding * 2 - 1, Edge);
            Sprites["box"].Bitmap.FillRect(2, 2, Size.Width - WidgetPadding * 2 - 4, Size.Height - WidgetPadding * 2 - 4, Filler);
        }
        else
        {
            Color Edge = new Color(121, 121, 122);
            Color Detail = new Color(96, 100, 100);
            Color Filler = new Color(10, 23, 37);
            Sprites["box"].Bitmap.DrawRect(Size.Width - WidgetPadding * 2, Size.Height - WidgetPadding * 2, Edge);
            Sprites["box"].Bitmap.DrawRect(1, 1, Size.Width - WidgetPadding * 2 - 2, Size.Height - WidgetPadding * 2 - 2, Edge);
            Sprites["box"].Bitmap.FillRect(2, 2, Size.Width - WidgetPadding * 2 - 4, Size.Height - WidgetPadding * 2 - 4, Filler);
            Sprites["box"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 1, 0, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 1, Size.Height - WidgetPadding * 2 - 1, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(0, Size.Height - WidgetPadding * 2 - 1, Color.ALPHA);
            Sprites["box"].Bitmap.SetPixel(0, 1, Detail);
            Sprites["box"].Bitmap.SetPixel(1, 0, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 1, 1, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 2, 0, Detail);
            Sprites["box"].Bitmap.SetPixel(0, Size.Height - WidgetPadding * 2 - 2, Detail);
            Sprites["box"].Bitmap.SetPixel(1, Size.Height - WidgetPadding * 2 - 1, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 1, Size.Height - WidgetPadding * 2 - 2, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 2, Size.Height - WidgetPadding * 2 - 1, Detail);
            Sprites["box"].Bitmap.SetPixel(2, 2, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 3, 2, Detail);
            Sprites["box"].Bitmap.SetPixel(2, Size.Height - WidgetPadding * 2 - 3, Detail);
            Sprites["box"].Bitmap.SetPixel(Size.Width - WidgetPadding * 2 - 3, Size.Height - WidgetPadding * 2 - 3, Detail);
        }
        Sprites["box"].Bitmap.Lock();
        base.Draw();
    }
}

public class DesignTextArea : Widget
{
    public string Text { get; protected set; } = "";
    public int TextX { get; protected set; } = 0;
    public int TextY { get; protected set; } = 0;
    public int CaretY { get; protected set; } = 2;
    public int CaretHeight { get; protected set; } = 13;
    public Font Font { get; protected set; }
    public Color TextColor { get; protected set; } = Color.WHITE;
    public Color DisabledTextColor { get; protected set; } = new Color(141, 151, 163);
    public Color CaretColor { get; protected set; } = Color.WHITE;
    public Color FillerColor { get; protected set; } = new Color(0, 120, 215);
    public bool ReadOnly { get; protected set; } = false;
    public bool Enabled { get; protected set; } = true;
    public bool NumericOnly { get; protected set; } = false;
    public int DefaultNumericValue { get; protected set; } = 0;
    public bool AllowMinusSigns { get; protected set; } = true;
    public bool ShowDisabledText { get; protected set; } = false;
    public bool DeselectOnEnterPressed { get; protected set; } = true;

    int FrameNum;
    int FrameCount;
    int QuarterWidth;

    public DesignTextArea(IContainer Parent) : base(Parent)
    {
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].X = TextX;
        Sprites["text"].Y = TextY;
        Sprites["text"].Z = 2;
        Sprites["filler"] = new Sprite(this.Viewport, new SolidBitmap(1, 16, FillerColor));
        Sprites["filler"].Visible = false;
        Sprites["filler"].Y = 2;
        Sprites["caret"] = new Sprite(this.Viewport, new SolidBitmap(1, 16, CaretColor));
        Sprites["caret"].Y = 2;
        Sprites["caret"].Z = 1;
        Sprites["caret"].Visible = false;
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        StopCaretAnimation();
    }

    private void ShowCaretAnimation()
    {
        StopCaretAnimation();
        Sprites["caret"].Visible = true;
        Sprites["filler"].Visible = false;
        Sprites["filler"].X = TextX + (int) Math.Round(0.25 * Size.Width);
        IAnimation Animation = new LinearAnimation("show_caret", 4000, f =>
        {
            Sprites["caret"].X = TextX + (int) Math.Round(f * Size.Width);
            if (f >= 0.2 && f < 0.8)
            {
                Sprites["filler"].Visible = true;
                ((SolidBitmap) Sprites["filler"].Bitmap).SetSize(
                    // (f - 0.2) * 5 / 3d evaluates to 0 for f=0.2 and 1 for f=0.8
                    (int) Math.Round((f - 0.2) * 5 / 3d * Size.Width / 2),
                    CaretHeight
                );
            }
            else if (Sprites["filler"].Visible && f > 0.75)
            {
                Sprites["filler"].Visible = false;
            }
        });
        Animation.OnFinished += () =>
        {
            Sprites["caret"].Visible = false;
            Sprites["filler"].Visible = false;
        };
        StartAnimation(Animation);
    }

    private void StopCaretAnimation()
    {
        if (AnimationExists("move")) StopAnimation("move");
    }

    public void SetCaretColor(Color CaretColor)
    {
        if (this.CaretColor != CaretColor)
        {
            this.CaretColor = CaretColor;
            ((SolidBitmap) Sprites["caret"].Bitmap).SetColor(CaretColor);
            ShowCaretAnimation();
        }
    }

    public void SetDisabledTextColor(Color DisabledTextColor)
    {
        if (this.DisabledTextColor != DisabledTextColor)
        {
            this.DisabledTextColor = DisabledTextColor;
            this.DrawText();
        }
    }

    public void SetFillerColor(Color FillerColor)
    {
        if (this.FillerColor != FillerColor)
        {
            this.FillerColor = FillerColor;
            ((SolidBitmap) Sprites["filler"].Bitmap).SetColor(FillerColor);
            ShowCaretAnimation();
        }
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            this.DrawText();
            if (this.SelectedWidget) Window.UI.SetSelectedWidget(null);
        }
    }

    public void SetText(string Text)
    {
        if (this.Text != Text)
        {
            this.Text = Text ?? "";
            DrawText();
        }
    }

    public void SetFont(Font f)
    {
        this.Font = f;
        SetCaretHeight(this.Font.Size + 5);
        DrawText();
    }

    public void SetTextX(int TextX)
    {
        this.TextX = TextX;
        Sprites["text"].X = TextX;
    }

    public void SetTextY(int TextY)
    {
        this.TextY = TextY;
        Sprites["text"].Y = TextY;
    }

    public void SetCaretY(int CaretY)
    {
        this.CaretY = CaretY;
        Sprites["caret"].Y = Sprites["filler"].Y = CaretY;
        ShowCaretAnimation();
    }

    public void SetCaretHeight(int CaretHeight)
    {
        this.CaretHeight = CaretHeight;
        SolidBitmap caret = (SolidBitmap) Sprites["caret"].Bitmap;
        caret.SetSize(1, CaretHeight);
        SolidBitmap filler = (SolidBitmap) Sprites["filler"].Bitmap;
        filler.SetSize(filler.BitmapWidth, CaretHeight);
        ShowCaretAnimation();
    }

    public void SetTextColor(Color TextColor)
    {
        if (this.TextColor != TextColor)
        {
            this.TextColor = TextColor;
            DrawText();
        }
    }

    public void SetReadOnly(bool ReadOnly)
    {
        if (this.ReadOnly != ReadOnly)
        {
            this.ReadOnly = ReadOnly;
        }
    }

    public void SetNumericOnly(bool NumericOnly)
    {
        if (this.NumericOnly != NumericOnly)
        {
            this.NumericOnly = NumericOnly;
        }
    }

    public void SetDefaultNumericValue(int DefaultNumericValue)
    {
        if (this.DefaultNumericValue != DefaultNumericValue)
        {
            this.DefaultNumericValue = DefaultNumericValue;
        }
    }

    public void SetAllowMinusSigns(bool AllowMinusSigns)
    {
        if (this.AllowMinusSigns != AllowMinusSigns)
        {
            this.AllowMinusSigns = AllowMinusSigns;
        }
    }

    public void SetShowDisabledText(bool ShowDisabledText)
    {
        if (this.ShowDisabledText != ShowDisabledText)
        {
            this.ShowDisabledText = ShowDisabledText;
            if (!this.Enabled) DrawText();
        }
    }

    public void SetDeselectOnEnterPress(bool DeselectOnEnterPressed)
    {
        if (this.DeselectOnEnterPressed != DeselectOnEnterPressed)
        {
            this.DeselectOnEnterPressed = DeselectOnEnterPressed;
        }
    }

    /// <summary>
    /// Redraws the text bitmap.
    /// </summary>
    public void DrawText()
    {
        Sprites["text"].Bitmap?.Dispose();
        Sprites["text"].Bitmap = null;
        if (!this.Enabled && !this.ShowDisabledText || string.IsNullOrEmpty(this.Text)) return;
        Size s = Font.TextSize(this.Text);
        if (s.Width < 1 || s.Height < 1) return;
        Sprites["text"].Bitmap = new Bitmap(s);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = this.Font;
        Sprites["text"].Bitmap.DrawText(this.Text, this.Enabled ? this.TextColor : DisabledTextColor);
        Sprites["text"].Bitmap.Lock();
    }
}