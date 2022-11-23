using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner;

[WidgetTypeAndName(typeof(NumericBoxData), "numericbox")]
public class DesignNumericBox : DesignWidget
{
    public override bool PasteAsChildren => false;

    public int Value { get; protected set; } = 0;
    public int MaxValue { get; protected set; } = 999999;
    public int MinValue { get; protected set; } = -999999;
    public int Increment { get; protected set; } = 1;
    public bool Enabled { get; protected set; } = true;

    ButtonDrawing DownButton;
    ButtonDrawing UpButton;
    Container TextBG;
    DesignTextArea TextArea;

    public DesignNumericBox(IContainer Parent) : base(Parent, "UnnamedNumericBox")
	{
        DownButton = new ButtonDrawing(this);
        DownButton.SetText("-");
        DownButton.SetPadding(WidgetPadding, WidgetPadding, 0, WidgetPadding);
        DownButton.SetSize(30, 30);

        UpButton = new ButtonDrawing(this);
        UpButton.SetText("+");
        UpButton.SetPadding(0, WidgetPadding, WidgetPadding, WidgetPadding);
        UpButton.SetSize(30, 30);
        UpButton.SetRightDocked(true);

        TextBG = new Container(this);
        TextBG.SetDocked(true);
        TextBG.SetPadding(28 + WidgetPadding, 1 + WidgetPadding);
        TextBG.Sprites["box"] = new Sprite(TextBG.Viewport);

        TextArea = new DesignTextArea(TextBG);
        TextArea.SetDocked(true);
        TextArea.SetPadding(1, 4);
        TextArea.SetFont(Fonts.Paragraph);
        TextArea.SetNumericOnly(true);
        TextArea.SetDefaultNumericValue(0);
        TextArea.SetText(this.Value.ToString());

        TextBG.OnSizeChanged += _ => RepositionText();

        MinimumSize.Height += 30;
        MaximumSize.Height = MinimumSize.Height;

        this.Properties.AddRange(new List<Property>()
        {
            new Property("Value", PropertyType.Numeric, () => Value, e =>
            {
                int OldValue = Value;
                SetValue((int) e);
                if (OldValue != Value) Undo.GenericUndoAction<int>.Register(this, "SetValue", OldValue, Value, true);
            }),

            new Property("Min. Value", PropertyType.Numeric, () => MinValue, e =>
            {
                int OldMinValue = MinValue;
                int OldValue = Value;
                SetMinValue((int) e);
                if (OldValue < MinValue) Program.ParameterPanel.Refresh();
                if (OldMinValue != MinValue) Undo.GenericUndoAction<int>.Register(this, "SetMinValue", OldMinValue, MinValue, true);
            }),

            new Property("Max. Value", PropertyType.Numeric, () => MaxValue, e =>
            {
                int OldMaxValue = MaxValue;
                int OldValue = Value;
                SetMaxValue((int) e);
                if (OldValue > MaxValue) Program.ParameterPanel.Refresh();
                if (OldMaxValue != MaxValue) Undo.GenericUndoAction<int>.Register(this, "SetMaxValue", OldMaxValue, MaxValue, true);
            }),

            new Property("Increment", PropertyType.Numeric, () => Increment, e =>
            {
                int OldIncrement = Increment;
                SetIncrement((int) e);
                if (OldIncrement != Increment) Undo.GenericUndoAction<int>.Register(this, "SetIncrement", OldIncrement, Increment, true);
            }),

            new Property("Enabled", PropertyType.Boolean, () => Enabled, e =>
            {
                bool OldEnabled = Enabled;
                SetEnabled((bool) e);
                if (OldEnabled != Enabled) Undo.GenericUndoAction<bool>.Register(this, "SetEnabled", OldEnabled, Enabled, true);
            })
        });
    }

    void RepositionText()
    {
        Size s = TextArea.Font.TextSize(TextArea.Text);
        if (s.Width >= TextBG.Size.Width) TextArea.SetTextX(0);
        else TextArea.SetTextX(TextBG.Size.Width / 2 - s.Width / 2);
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            TextArea.SetEnabled(this.Enabled);
            DownButton.SetEnabled(this.Enabled);
            UpButton.SetEnabled(this.Enabled);
            this.Redraw();
        }
    }

    public void SetValue(int Value)
    {
        if (Value > MaxValue) Value = MaxValue;
        if (Value < MinValue) Value = MinValue;
        if (this.Value != Value)
        {
            this.Value = Value;
            TextArea.SetText(this.Value.ToString());
            RepositionText();
        }
    }

    public void SetMinValue(int MinValue)
    {
        if (this.MinValue != MinValue)
        {
            this.MinValue = MinValue;
            if (this.Value < this.MinValue)
            {
                SetValue(this.MinValue);
            }
            TextArea.SetAllowMinusSigns(MinValue < 0);
        }
    }

    public void SetMaxValue(int MaxValue)
    {
        if (this.MaxValue != MaxValue)
        {
            this.MaxValue = MaxValue;
            if (this.Value > this.MaxValue)
            {
                SetValue(this.MaxValue);
            }
        }
    }

    public void SetIncrement(int Increment)
    {
        if (this.Increment != Increment)
        {
            this.Increment = this.Increment;
        }
    }

    protected override void Draw()
    {
        TextBG.Sprites["box"].Bitmap?.Dispose();
        TextBG.Sprites["box"].Bitmap = new Bitmap(TextBG.Size);
        TextBG.Sprites["box"].Bitmap.Unlock();
        Color Edge = new Color(86, 108, 134);
        Color Filler = this.Enabled ? new Color(86, 108, 134) : new Color(40, 62, 84);
        int w = TextBG.Size.Width;
        int h = TextBG.Size.Height;
        TextBG.Sprites["box"].Bitmap.DrawLine(0, 0, w - 1, 0, Edge);
        TextBG.Sprites["box"].Bitmap.DrawLine(0, 1, w - 1, 1, new Color(36, 34, 36));
        TextBG.Sprites["box"].Bitmap.DrawLine(0, h - 1, w - 1, h - 1, Edge);
        TextBG.Sprites["box"].Bitmap.DrawLine(0, h - 2, w - 1, h - 2, new Color(36, 34, 36));
        TextBG.Sprites["box"].Bitmap.FillRect(0, 2, w, h - 4, Filler);
        TextBG.Sprites["box"].Bitmap.Lock();
        base.Draw();
    }
}

