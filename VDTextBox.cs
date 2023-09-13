namespace VisualDesigner;

public class VDTextBox : Widget
{
    TextArea TextArea;

    public string Text => TextArea.Text;
    public bool NumericOnly => TextArea.NumericOnly;
    public float DefaultNumericValue => TextArea.DefaultNumericValue;
    public bool Enabled => TextArea.Enabled;
    public bool ShowDisabledText => TextArea.ShowDisabledText;
    public bool DeselectOnEnterPressed => TextArea.DeselectOnEnterPressed;
    public TextEvent OnTextChanged { get => TextArea.OnTextChanged; set => TextArea.OnTextChanged = value; }
    public BaseEvent OnWidgetDeselected { get => TextArea.OnWidgetDeselected; set => TextArea.OnWidgetDeselected = value; }
    public BaseEvent OnEnterPressed { get => TextArea.OnEnterPressed; set => TextArea.OnEnterPressed = value; }

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

    public void SetShowDisabledText(bool ShowDisabledText)
    {
        TextArea.SetShowDisabledText(ShowDisabledText);
    }

    public void SetDeselectOnEnterPressed(bool DeselectOnEnterPressed)
    {
        TextArea.SetDeselectOnEnterPress(DeselectOnEnterPressed);
    }
}
