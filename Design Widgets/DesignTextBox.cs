using amethyst.Animations;

namespace VisualDesigner;

[WidgetTypeAndName(typeof(TextBoxData), "textbox")]
public class DesignTextBox : DesignWidget
{
    public override bool PasteAsChildren => false;

    public string Text => TextArea.Text;
    public int TextX => TextArea.TextX;
    public int TextY => TextArea.TextY;
    public int CaretY => TextArea.CaretY;
    public int? CaretHeight => TextArea.CaretHeight;
    public Font Font => TextArea.Font;
    public Color TextColor => TextArea.TextColor;
    public Color DisabledTextColor => TextArea.DisabledTextColor;
    public Color CaretColor => TextArea.CaretColor;
    public Color FillerColor => TextArea.FillerColor;
    public bool ReadOnly => TextArea.ReadOnly;
    public bool NumericOnly => TextArea.NumericOnly;
    public int DefaultNumericValue => TextArea.DefaultNumericValue;
    public bool AllowMinusSigns => TextArea.AllowMinusSigns;
    public bool ShowDisabledText => TextArea.ShowDisabledText;
    public bool DeselectOnEnterPressed => TextArea.DeselectOnEnterPressed;
    public bool PopupStyle { get; protected set; } = true;
    public bool Enabled { get; protected set; } = true;

    protected DesignTextArea TextArea;

    public DesignTextBox(IContainer Parent) : base(Parent, "UnnamedTextBox")
	{
        Sprites["box"] = new Sprite(this.Viewport);
        Sprites["box"].X = WidgetPadding;
        Sprites["box"].Y = WidgetPadding;
        TextArea = new DesignTextArea(this);
        TextArea.SetPosition(6 + WidgetPadding, 4 + WidgetPadding);
        TextArea.SetFont(Fonts.Paragraph);

        MinimumSize.Height += 27;

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
                Program.ParameterPanel.Refresh();
            }),

            new Property("Enabled", PropertyType.Boolean, () => Enabled, e =>
            {
                bool OldEnabled = Enabled;
                SetEnabled((bool) e);
                if (OldEnabled != Enabled) Undo.GenericUndoAction<bool>.Register(this, "SetEnabled", OldEnabled, Enabled, true);
            }),

            new Property("Numeric-only", PropertyType.Boolean, () => NumericOnly, e =>
            {
                bool OldNumericOnly = NumericOnly;
                SetNumericOnly((bool) e);
                if (OldNumericOnly != NumericOnly) Undo.GenericUndoAction<bool>.Register(this, "SetNumericOnly", OldNumericOnly, NumericOnly, true);
            }),

            new Property("Default Number", PropertyType.Numeric, () => DefaultNumericValue, e =>
            {
                int OldDefaultNumericValue = DefaultNumericValue;
                SetDefaultNumericValue((int) e);
                if (OldDefaultNumericValue != DefaultNumericValue) Undo.GenericUndoAction<int>.Register(this, "SetDefaultNumericValue", OldDefaultNumericValue, DefaultNumericValue, true);
            }),

            new Property("Allow Minus", PropertyType.Boolean, () => AllowMinusSigns, e =>
            {
                bool OldAllowMinusSigns = AllowMinusSigns;
                SetAllowMinusSigns((bool) e);
                if (OldAllowMinusSigns != AllowMinusSigns) Undo.GenericUndoAction<bool>.Register(this, "SetAllowMinusSigns", OldAllowMinusSigns, AllowMinusSigns, true);
            }),

            new Property("Read-only", PropertyType.Boolean, () => ReadOnly, e =>
            {
                bool OldReadOnly = ReadOnly;
                SetReadOnly((bool) e);
                if (OldReadOnly != ReadOnly) Undo.GenericUndoAction<bool>.Register(this, "SetReadOnly", OldReadOnly, ReadOnly, true);
            }),

            new Property("Text Color", PropertyType.Color, () => TextColor, e =>
            {
                Color OldTextColor = TextColor;
                SetTextColor((Color) e);
                if (!OldTextColor.Equals(TextColor)) Undo.GenericUndoAction<Color>.Register(this, "SetTextColor", OldTextColor, TextColor, true);
            }),

            new Property("Show Disabled Text", PropertyType.Boolean, () => ShowDisabledText, e =>
            {
                bool OldShowDisabledText = ShowDisabledText;
                SetShowDisabledText((bool) e);
                if (OldShowDisabledText != ShowDisabledText) Undo.GenericUndoAction<bool>.Register(this, "SetShowDisabledText", OldShowDisabledText, ShowDisabledText, true);
            }),

            new Property("Disabled Text Color", PropertyType.Color, () => DisabledTextColor, e =>
            {
                Color OldDisabledTextColor = DisabledTextColor;
                SetDisabledTextColor((Color) e);
                if (!OldDisabledTextColor.Equals(DisabledTextColor)) Undo.GenericUndoAction<Color>.Register(this, "SetDisabledTextColor", OldDisabledTextColor, DisabledTextColor, true);
            }),

            new Property("Deselect on Enter", PropertyType.Boolean, () => DeselectOnEnterPressed, e =>
            {
                bool OldDeselectOnEnterPressed = DeselectOnEnterPressed;
                SetDeselectOnEnterPressed((bool) e);
                if (OldDeselectOnEnterPressed != DeselectOnEnterPressed) Undo.GenericUndoAction<bool>.Register(this, "SetDeselectOnEnterPressed", OldDeselectOnEnterPressed, DeselectOnEnterPressed, true);
            }),

            new Property("Pop-up Style", PropertyType.Boolean, () => PopupStyle, e =>
            {
                bool OldPopupStyle = PopupStyle;
                SetPopupStyle((bool) e);
                if (OldPopupStyle != PopupStyle) Undo.GenericUndoAction<bool>.Register(this, "SetPopupStyle", OldPopupStyle, PopupStyle, true);
            }),

            new Property("Text X", PropertyType.Numeric, () => TextX, e =>
            {
                int OldTextX = TextX;
                SetTextX((int) e);
                if (OldTextX != TextX) Undo.GenericUndoAction<int>.Register(this, "SetTextX", OldTextX, TextX, true);
            }),

            new Property("Text Y", PropertyType.Numeric, () => TextY, e =>
            {
                int OldTextY = TextY;
                SetTextY((int) e);
                if (OldTextY != TextY) Undo.GenericUndoAction<int>.Register(this, "SetTextY", OldTextY, TextY, true);
            }),

            new Property("Caret Y", PropertyType.Numeric, () => CaretY, e =>
            {
                int OldCaretY = CaretY;
                SetCaretY((int) e);
                if (OldCaretY != CaretY) Undo.GenericUndoAction<int>.Register(this, "SetCaretY", OldCaretY, CaretY, true);
            }),

            new Property("Caret Height", PropertyType.Numeric, () => CaretHeight ?? -1, e =>
            {
                int? OldCaretHeight = CaretHeight;
                SetCaretHeight((int) e == -1 ? null : (int) e);
                if (OldCaretHeight != CaretHeight) Undo.GenericUndoAction<int?>.Register(this, "SetCaretHeight", OldCaretHeight, CaretHeight, true);
            }),

            new Property("Caret Color", PropertyType.Color, () => CaretColor, e =>
            {
                Color OldCaretColor = CaretColor;
                SetCaretColor((Color) e);
                if (!OldCaretColor.Equals(CaretColor)) Undo.GenericUndoAction<Color>.Register(this, "SetCaretColor", OldCaretColor, CaretColor, true);
            }),

            new Property("Filler Color", PropertyType.Color, () => FillerColor, e =>
            {
                Color OldFillerColor = FillerColor;
                SetFillerColor((Color) e);
                if (!OldFillerColor.Equals(FillerColor)) Undo.GenericUndoAction<Color>.Register(this, "SetFillerColor", OldFillerColor, FillerColor, true);
            })
        });
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

    public void SetCaretHeight(int? CaretHeight)
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
            this.TextArea.SetEnabled(Enabled);
            this.Redraw();
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
    public int? CaretHeight { get; protected set; } = null;
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
                    CaretHeight ?? Font.Size + 5
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
        if (!this.CaretColor.Equals(CaretColor))
        {
            this.CaretColor = CaretColor;
            ((SolidBitmap) Sprites["caret"].Bitmap).SetColor(CaretColor);
            ShowCaretAnimation();
        }
    }

    public void SetDisabledTextColor(Color DisabledTextColor)
    {
        if (!this.DisabledTextColor.Equals(DisabledTextColor))
        {
            this.DisabledTextColor = DisabledTextColor;
            this.DrawText();
        }
    }

    public void SetFillerColor(Color FillerColor)
    {
        if (!this.FillerColor.Equals(FillerColor))
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

    public void SetFont(Font Font)
    {
        if (this.Font == null && Font != null || !this.Font.Equals(Font))
        {
            this.Font = Font;
            DrawText();
        }
    }

    public void SetTextX(int TextX)
    {
        if (this.TextX != TextX)
        {
            this.TextX = TextX;
            Sprites["text"].X = TextX;
        }
    }

    public void SetTextY(int TextY)
    {
        if (this.TextY != TextY)
        {
            this.TextY = TextY;
            Sprites["text"].Y = TextY;
        }
    }

    public void SetCaretY(int CaretY)
    {
        if (this.CaretY != CaretY)
        {
            this.CaretY = CaretY;
            Sprites["caret"].Y = Sprites["filler"].Y = CaretY;
            ShowCaretAnimation();
        }
    }

    public void SetCaretHeight(int? CaretHeight)
    {
        if (this.CaretHeight != CaretHeight)
        {
            this.CaretHeight = CaretHeight;
            SolidBitmap caret = (SolidBitmap) Sprites["caret"].Bitmap;
            caret.SetSize(1, CaretHeight ?? Font.Size + 5);
            SolidBitmap filler = (SolidBitmap) Sprites["filler"].Bitmap;
            filler.SetSize(filler.BitmapWidth, CaretHeight ?? Font.Size + 5);
            ShowCaretAnimation();
        }
    }

    public void SetTextColor(Color TextColor)
    {
        if (!this.TextColor.Equals(TextColor))
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