

namespace VisualDesigner;

[WidgetTypeAndName(typeof(LabelData), "label")]
public class DesignLabel : DesignWidget
{
	public override bool PasteAsChildren => false;

	public string Text => Label.Text;
	public Font Font => Label.Font;
	public Color TextColor => Label.TextColor;
	public int WidthLimit => Label.WidthLimit;
	public string LimitReplacementText => Label.LimitReplacementText;
	public DrawOptions DrawOptions => Label.DrawOptions;

	Label Label;

	public DesignLabel(IContainer Parent) : base(Parent, "UnnamedLabel")
	{
		Label = new Label(this);
		Label.SetPosition(WidgetPadding, WidgetPadding);
		Label.OnSizeChanged += _ => SetSize(Label.Size.Width + WidgetPadding * 2, Label.Size.Height + WidgetPadding * 2);
		OnSizeChanged += _ => Label.SetSize(Size.Width - WidgetPadding * 2, Size.Height - WidgetPadding * 2);

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

			new Property("Width Limit", PropertyType.Numeric, () => WidthLimit, e =>
			{
				int OldWidthLimit = WidthLimit;
				SetWidthLimit((int) e);
				if (WidthLimit != OldWidthLimit) Undo.GenericUndoAction<int>.Register(this, "SetWidthLimit", OldWidthLimit, WidthLimit, true);
			}),

			new Property("Limit Text", PropertyType.Text, () => LimitReplacementText, e =>
			{
				string OldLimitReplacementText = LimitReplacementText;
				SetLimitReplacementText((string) e);
				if (LimitReplacementText != OldLimitReplacementText) Undo.GenericUndoAction<string>.Register(this, "SetLimitReplacementText", OldLimitReplacementText, LimitReplacementText, true);
			}),

			new Property("Bold", PropertyType.Boolean, () => (DrawOptions & DrawOptions.Bold) != 0, e =>
			{
				DrawOptions OldDrawOptions = DrawOptions;
				DrawOptions ops = DrawOptions;
				if ((bool) e) ops |= DrawOptions.Bold;
				else ops &= ~DrawOptions.Bold;
				SetDrawOptions(ops);
				if (ops != OldDrawOptions) Undo.GenericUndoAction<DrawOptions>.Register(this, "SetDrawOptions", OldDrawOptions, ops, true);
			}),

            new Property("Italic", PropertyType.Boolean, () => (DrawOptions & DrawOptions.Italic) != 0, e =>
            {
				DrawOptions OldDrawOptions = DrawOptions;
                DrawOptions ops = DrawOptions;
                if ((bool) e) ops |= DrawOptions.Italic;
                else ops &= ~DrawOptions.Italic;
                SetDrawOptions(ops);
                if (ops != OldDrawOptions) Undo.GenericUndoAction<DrawOptions>.Register(this, "SetDrawOptions", OldDrawOptions, ops, true);
            }),

            new Property("Underlined", PropertyType.Boolean, () => (DrawOptions & DrawOptions.Underlined) != 0, e =>
            {
				DrawOptions OldDrawOptions = DrawOptions;
                DrawOptions ops = DrawOptions;
                if ((bool) e) ops |= DrawOptions.Underlined;
                else ops &= ~DrawOptions.Underlined;
                SetDrawOptions(ops);
                if (ops != OldDrawOptions) Undo.GenericUndoAction<DrawOptions>.Register(this, "SetDrawOptions", OldDrawOptions, ops, true);
            }),

            new Property("Strikethrough", PropertyType.Boolean, () => (DrawOptions & DrawOptions.Strikethrough) != 0, e =>
            {
				DrawOptions OldDrawOptions = DrawOptions;
                DrawOptions ops = DrawOptions;
                if ((bool) e) ops |= DrawOptions.Strikethrough;
                else ops &= ~DrawOptions.Strikethrough;
                SetDrawOptions(ops);
                if (ops != OldDrawOptions) Undo.GenericUndoAction<DrawOptions>.Register(this, "SetDrawOptions", OldDrawOptions, ops, true);
            })
        });
	}

	public void SetText(string Text)
	{
		Label.SetText(Text);
	}

	public void SetFont(Font Font)
	{
		Label.SetFont(Font);
	}

	public void SetTextColor(Color TextColor)
	{
		Label.SetTextColor(TextColor);
	}

	public void SetWidthLimit(int WidthLimit)
	{
		Label.SetWidthLimit(WidthLimit);
	}

	public void SetLimitReplacementText(string LimitReplacementText)
	{
		Label.SetLimitReplacementText(LimitReplacementText);
	}

	public void SetDrawOptions(DrawOptions DrawOptions)
	{
		Label.SetDrawOptions(DrawOptions);
	}
}
