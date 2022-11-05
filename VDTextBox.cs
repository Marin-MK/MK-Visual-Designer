using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner;

public class VDTextBox : Widget
{
    TextArea TextArea;

    public string Text => TextArea.Text;
    public bool NumericOnly => TextArea.NumericOnly;
    public int DefaultNumericValue => TextArea.DefaultNumericValue;
    public bool Enabled => TextArea.Enabled;
    public TextEvent OnTextChanged { get => TextArea.OnTextChanged; set => TextArea.OnTextChanged = value; }

	public VDTextBox(IContainer Parent) : base(Parent)
	{
        TextArea = new TextArea(this);
        TextArea.SetFont(Fonts.Paragraph);
        TextArea.SetPosition(0, 5);
        TextArea.SetHeight(22);
        TextArea.OnWidgetSelected += TextArea.WidgetSelected;
        TextArea.OnMouseDown += _ =>
        {
            if (!TextArea.Mouse.Inside && TextArea.SelectedWidget) Window.UI.SetSelectedWidget(null);
        };
        OnSizeChanged += _ => TextArea.SetSize(Size);
    }

    public void SetText(string Text)
    {
        TextArea.SetText(Text);
    }

    public void SetNumericOnly(bool NumericOnly)
    {
        TextArea.SetNumericOnly(NumericOnly);
    }

    public void SetDefaultNumericValue(int DefaultNumericValue)
    {
        TextArea.SetDefaultNumericValue(DefaultNumericValue);
    }

    public void SetEnabled(bool Enabled)
    {
        TextArea.SetEnabled(Enabled);
    }
}
