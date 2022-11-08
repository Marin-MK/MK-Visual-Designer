using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner;

public class DesignLabel : DesignWidget
{
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
				if (Text != OldText) Undo.GenericUndoAction<string>.Register(this, "SetText", OldText, Text);
			}),
            new Property("Text Color", PropertyType.Color, () => TextColor, e => SetTextColor((Color) e)),
            new Property("Font", PropertyType.Font, () => Font, e => SetFont((Font) e)),
			new Property("Width Limit", PropertyType.Numeric, () => WidthLimit, e => SetWidthLimit((int) e)),
			new Property("Limit Text", PropertyType.Text, () => LimitReplacementText, e => SetLimitReplacementText((string) e)),
			new Property("Bold", PropertyType.Boolean, () => (DrawOptions & DrawOptions.Bold) != 0, e =>
			{
				DrawOptions ops = DrawOptions;
				if ((bool) e) ops |= DrawOptions.Bold;
				else ops &= ~DrawOptions.Bold;
				SetDrawOptions(ops);
			}),
            new Property("Italic", PropertyType.Boolean, () => (DrawOptions & DrawOptions.Italic) != 0, e =>
            {
                DrawOptions ops = DrawOptions;
                if ((bool) e) ops |= DrawOptions.Italic;
                else ops &= ~DrawOptions.Italic;
                SetDrawOptions(ops);
            }),
            new Property("Underline", PropertyType.Boolean, () => (DrawOptions & DrawOptions.Underlined) != 0, e =>
            {
                DrawOptions ops = DrawOptions;
                if ((bool) e) ops |= DrawOptions.Underlined;
                else ops &= ~DrawOptions.Underlined;
                SetDrawOptions(ops);
            }),
            new Property("Strikethrough", PropertyType.Boolean, () => (DrawOptions & DrawOptions.Strikethrough) != 0, e =>
            {
                DrawOptions ops = DrawOptions;
                if ((bool) e) ops |= DrawOptions.Strikethrough;
                else ops &= ~DrawOptions.Strikethrough;
                SetDrawOptions(ops);
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
