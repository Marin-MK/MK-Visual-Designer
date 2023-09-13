namespace VisualDesigner;

[WidgetTypeAndName(typeof(NumericSliderData), "numericslider")]
public class DesignNumericSlider : DesignWidget
{
    public int Value { get; protected set; } = 50;
    public int MinValue { get; protected set; } = 0;
    public int MaxValue { get; protected set; } = 100;
    public List<(int Value, double Factor, int X)> SnapValues { get; protected set; } = new List<(int, double, int)>();
    public int SnapStrength { get; protected set; } = 4;
    public bool Enabled { get; protected set; } = true;

    public DesignNumericSlider(IContainer Parent) : base(Parent, "UnnamedNumericSlider")
    {
        Sprites["bars"] = new Sprite(this.Viewport);
        Sprites["bars"].X = 4 + WidgetPadding;
        Sprites["bars"].Y = WidgetPadding;
        Sprites["slider"] = new Sprite(this.Viewport);
        Sprites["slider"].X = WidgetPadding;
        Sprites["slider"].Y = WidgetPadding;
        Sprites["slider"].Bitmap = new Bitmap(8, 17);
        Sprites["slider"].Bitmap.Unlock();
        Sprites["slider"].Bitmap.FillRect(0, 0, 8, 17, new Color(55, 171, 206));
        Sprites["slider"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["slider"].Bitmap.SetPixel(7, 0, Color.ALPHA);
        Sprites["slider"].Bitmap.SetPixel(0, 16, Color.ALPHA);
        Sprites["slider"].Bitmap.SetPixel(7, 16, Color.ALPHA);
        Sprites["slider"].Bitmap.Lock();
        MinimumSize.Height += 17;
        MaximumSize.Height = MinimumSize.Height;
        SetHeight(17 + HeightAdd);

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
                SetMinimumValue((int) e);
                if (OldMinValue != MinValue) Undo.GenericUndoAction<int>.Register(this, "SetMinimumValue", OldMinValue, MinValue, true);
            }),

            new Property("Max. Value", PropertyType.Numeric, () => MaxValue, e =>
            {
                int OldMaxValue = MaxValue;
                SetMaximumValue((int) e);
                if (OldMaxValue != MaxValue) Undo.GenericUndoAction<int>.Register(this, "SetMaximumValue", OldMaxValue, MaxValue, true);
            }),

            new Property("Snap Values", PropertyType.List, () => SnapValues.Select(e => e.Value).ToList(), e =>
            {
                List<int> OldSnapValues = SnapValues.Select(e => e.Value).ToList();
                SetSnapValues((List<int>) e);
                List<int> NewSnapValues = SnapValues.Select(e => e.Value).ToList();
                if (!OldSnapValues.Equals(NewSnapValues)) Undo.GenericUndoAction<List<int>>.Register(this, "SetSnapValues", OldSnapValues, NewSnapValues, true);
            }, true),

            new Property("Snap Strength", PropertyType.Numeric, () => SnapStrength, e =>
            {
                int OldSnapStrength = SnapStrength;
                SetSnapStrength((int) e);
                if (OldSnapStrength != SnapStrength) Undo.GenericUndoAction<int>.Register(this, "SetSnapStrength", OldSnapStrength, SnapStrength, true);
            }, new List<object>() { 0 }),

            new Property("Enabled", PropertyType.Boolean, () => Enabled, e =>
            {
                bool OldEnabled = Enabled;
                SetEnabled((bool) e);
                if (OldEnabled != Enabled) Undo.GenericUndoAction<bool>.Register(this, "SetEnabled", OldEnabled, Enabled, true);
            })
        });
    }

    public void SetValue(int Value)
    {
        if (this.Value != Value)
        {
            this.Value = Value;
            this.Redraw();
        }
    }

    public void SetMinimumValue(int MinValue)
    {
        if (this.MinValue != MinValue)
        {
            this.MinValue = MinValue;
            RecalculateSnapFactors();
            this.Redraw();
        }
    }

    public void SetMaximumValue(int MaxValue)
    {
        if (this.MaxValue != MaxValue)
        {
            this.MaxValue = MaxValue;
            RecalculateSnapFactors();
            this.Redraw();
        }
    }

    public void SetSnapValues(List<int> SnapValues)
    {
        SetSnapValues(SnapValues.ToArray());
    }

    public void SetSnapValues(params int[] Values)
    {
        this.SnapValues.Clear();
        foreach (int Value in Values)
        {
            double snapfactor = MaxValue == MinValue ? 0 : Math.Clamp((Value - MinValue) / (double)(MaxValue - MinValue), 0, 1);
            int x = (int)Math.Round(snapfactor * (Size.Width - WidthAdd - 9));
            this.SnapValues.Add((Value, snapfactor, x));
        }
        this.Redraw();
    }

    public void SetSnapStrength(int PixelSnapDifference)
    {
        this.SnapStrength = PixelSnapDifference;
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            Redraw();
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        RecalculateSnapFactors();
    }

    void RecalculateSnapFactors()
    {
        for (int i = 0; i < SnapValues.Count; i++)
        {
            double snapfactor = MaxValue == MinValue ? 0 : Math.Clamp((SnapValues[i].Value - MinValue) / (double)(MaxValue - MinValue), 0, 1);
            int x = (int)Math.Round(snapfactor * (Size.Width - WidthAdd - 9));
            SnapValues[i] = (SnapValues[i].Value, snapfactor, x);
        }
    }

    protected override void Draw()
    {
        base.Draw();
        int MaxX = Size.Width - WidthAdd - 8;
        double factor = MaxValue == MinValue ? 0 : Math.Clamp((Value - MinValue) / (double)(MaxValue - MinValue), 0, 1);
        Sprites["slider"].X = WidgetPadding + (int)Math.Round(factor * MaxX);
        Sprites["slider"].Visible = Enabled;

        Color PreColor = new Color(55, 171, 206);
        Color PostColor = new Color(73, 89, 109);

        if (!Enabled) PreColor = PostColor;

        Sprites["bars"].Bitmap?.Dispose();
        Sprites["bars"].Bitmap = new Bitmap(Size.Width - WidthAdd - 8, Size.Height - HeightAdd);
        Sprites["bars"].Bitmap.Unlock();
        if (Enabled)
        {
            Sprites["bars"].Bitmap.FillRect(2, 7, Sprites["slider"].X - WidgetPadding - 8, 3, PreColor);
            if (Sprites["slider"].X - WidgetPadding < Size.Width - WidthAdd - 14)
                Sprites["bars"].Bitmap.FillRect(Sprites["slider"].X - WidgetPadding + 6, 7, Size.Width - 14 - Sprites["slider"].X - WidgetPadding, 3, PostColor);
        }
        else
        {
            Sprites["bars"].Bitmap.FillRect(2, 7, Size.Width - WidthAdd - 14, 3, PreColor);
        }
        foreach ((int Value, double Factor, int X) Snap in SnapValues)
        {
            Color c = Snap.Factor > factor ? PostColor : PreColor;
            Sprites["bars"].Bitmap.DrawLine(Snap.X, 1, Snap.X, Size.Height - HeightAdd - 2, c);
            if (Snap.X > 0) Sprites["bars"].Bitmap.DrawLine(Snap.X - 1, 1, Snap.X - 1, Size.Height - HeightAdd - 2, Color.ALPHA);
            if (Snap.X < Size.Width - WidthAdd - 9) Sprites["bars"].Bitmap.DrawLine(Snap.X + 1, 1, Snap.X + 1, Size.Height - HeightAdd - 2, Color.ALPHA);
        }
        Sprites["bars"].Bitmap.Lock();
    }
}
