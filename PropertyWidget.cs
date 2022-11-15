using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VisualDesigner;

public abstract class PropertyWidget : Widget
{
	protected Label NameLabel;
	protected Property Property;
	protected float HSeparatorX;
	protected bool Available = true;

	public PropertyWidget(IContainer Parent, Property Property, float HSeparatorX) : base(Parent)
	{
		this.Property = Property;
		this.HSeparatorX = HSeparatorX;
		NameLabel = new Label(this);
		NameLabel.SetPosition(6, 6);
        NameLabel.SetFont(Fonts.ParagraphBold);
        NameLabel.SetText(Property.Name);
		Sprites["box"] = new Sprite(this.Viewport);
		Sprites["hsep"] = new Sprite(this.Viewport, new SolidBitmap(1, 28, new Color(86, 108, 134)));
		Sprites["hsep"].Y = 2;
		SetHeight(32);
	}

	public virtual void Refresh()
	{
		Available = Property.IsAvailable?.Invoke() ?? true;
        SetEnabled(Available);
	}

	public abstract void SetEnabled(bool Enabled);
	
	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
		Sprites["hsep"].X = (int) Math.Round(Size.Width * HSeparatorX);
		NameLabel.SetWidthLimit(Sprites["hsep"].X - NameLabel.Position.X - 10);
    }

	public override void Redraw()
	{
		base.Redraw();
		Sprites["box"].Bitmap?.Dispose();
		Sprites["box"].Bitmap = new Bitmap(Size);
		Sprites["box"].Bitmap.Unlock();
		Sprites["box"].Bitmap.FillRect(Size, new Color(24, 38, 53));
		Sprites["box"].Bitmap.Lock();
	}
}

public class TextPropertyWidget : PropertyWidget
{
	protected VDTextBox TextBox;

	public TextPropertyWidget(IContainer Parent, Property Property, float HSeparatorX) : base(Parent, Property, HSeparatorX)
	{
		TextBox = new VDTextBox(this);
		Refresh();
		TextBox.OnWidgetDeselected += _ =>
		{
			if (!Available) return;
			Property.OnSetValue(TextBox.Text.Replace("\\n", "\n"));
		};
    }

	public override void Refresh()
	{
		base.Refresh();
		if (!Available)
		{
			throw new NotImplementedException();
		}
		object value = Property.GetValue();
        if (value is string)
		{
			string txt = (string) value;
			txt = txt.Replace("\n", "\\n");
			TextBox.SetText(txt);
		}
	}

	public override void SetEnabled(bool Enabled)
	{
		TextBox.SetEnabled(Enabled);
	}

	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
		if (TextBox == null) return;
		TextBox.SetPosition(Sprites["hsep"].X + 8, TextBox.Position.Y);
		TextBox.SetSize(Size.Width - TextBox.Position.X - 4, TextBox.Size.Height);
	}
}

public class NumericPropertyWidget : PropertyWidget
{
    VDTextBox TextBox;

    public NumericPropertyWidget(IContainer Parent, Property Property, float HSeparatorX) : base(Parent, Property, HSeparatorX)
    {
        TextBox = new VDTextBox(this);
		TextBox.SetNumericOnly(true);
		TextBox.SetDefaultNumericValue((int) Property.GetValue());
		TextBox.SetShowDisabledText(true);
        int? MinValue = null;
		int? MaxValue = null;
		if (Property.Parameters is List<object>)
		{
			List<object> Params = (List<object>) Property.Parameters;
			if (Params.Count == 2)
			{
				MinValue = (int?) Params[0];
				MaxValue = (int?) Params[1];
			}
		}
		Refresh();
		TextBox.OnWidgetDeselected += _ =>
		{
			if (string.IsNullOrEmpty(TextBox.Text) || !Utilities.IsNumeric(TextBox.Text)) return;
			int value = Convert.ToInt32(TextBox.Text);
			if (MinValue != null && value < MinValue) value = (int) MinValue;
			if (MaxValue != null && value > MaxValue) value = (int) MaxValue;
            Property.OnSetValue(value);
		};
    }

	public override void Refresh()
	{
		base.Refresh();
		if (!Available)
		{
			TextBox.SetText(Property.UnavailableText);
			TextBox.SetEnabled(false);
			return;
		}
        TextBox.SetEnabled(true);
        TextBox.SetText(((int) Property.GetValue()).ToString());
    }

	public override void SetEnabled(bool Enabled)
	{
		TextBox.SetEnabled(Enabled);
	}

	public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (TextBox == null) return;
        TextBox.SetPosition(Sprites["hsep"].X + 8, TextBox.Position.Y);
        TextBox.SetSize(Size.Width - TextBox.Position.X - 4, TextBox.Size.Height);
    }
}

public class DropdownPropertyWidget : PropertyWidget
{
    protected VDDropdownBox DropdownBox;

    public DropdownPropertyWidget(IContainer Parent, Property Property, float HSeparatorX) : base(Parent, Property, HSeparatorX)
    {
		DropdownBox = new VDDropdownBox(this);
		DropdownBox.SetPosition(0, 4);
		DropdownBox.SetShowDisabledText(true);
		if (Property.Parameters is not List<string>) throw new Exception("Property must include a list of list items as parameters");
		List<ListItem> Items = new List<ListItem>();
		foreach (string s in (List<string>) Property.Parameters)
		{
			Items.Add(new ListItem(s));
		}
		DropdownBox.SetItems(Items);
		Refresh();
		DropdownBox.OnSelectionChanged += _ =>
		{
			if (!Available) return;
			Property.OnSetValue(DropdownBox.SelectedIndex);
		};
    }

	public override void Refresh()
	{
		base.Refresh();
		if (!Available)
		{
			DropdownBox.SetText("Unavailable");
			return;
		}
        DropdownBox.SetSelectedIndex((int) Property.GetValue());
    }

	public override void SetEnabled(bool Enabled)
	{
		DropdownBox.SetEnabled(Enabled);
	}

	public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (DropdownBox == null) return;
        DropdownBox.SetPosition(Sprites["hsep"].X + 4, DropdownBox.Position.Y);
        DropdownBox.SetSize(Size.Width - DropdownBox.Position.X - 4, DropdownBox.Size.Height);
    }
}

public class FontPropertyWidget : PropertyWidget
{
    VDDropdownBox DropdownBox;

    public FontPropertyWidget(IContainer Parent, Property Property, float HSeparatorX) : base(Parent, Property, HSeparatorX)
    {
		DropdownBox = new VDDropdownBox(this);
		DropdownBox.SetPosition(0, 4);
		SetAvailableFonts();
		Refresh();
		DropdownBox.OnSelectionChanged += _ =>
		{
			if (DropdownBox.Items[DropdownBox.SelectedIndex].Object is string) // Create new...
			{
				GenericTextNumberWindow win = new GenericTextNumberWindow("New Font", "Name:", "", "Size: ", 10, 1, null);
				win.OnClosed += _ =>
				{
					if (!win.Apply) return;
					string name = win.Value1;
					int size = win.Value2;
					if (!Font.Exists(name))
					{
						new MessageBox("Error", $"No font exists at the specified path '{name}'.", ButtonType.OK);
						DropdownBox.SetSelectedIndex(0);
					}
					else
					{
						Font f = new Font(name, size);
                        Program.CustomFonts.Add(f);
                        SetAvailableFonts();
                        int idx = DropdownBox.Items.FindIndex(i => ((Font) i.Object).Equals(f));
                        DropdownBox.SetSelectedIndex(idx);
						Property.OnSetValue((Font) DropdownBox.Items[DropdownBox.SelectedIndex].Object);
                    }
				};
			}
			else Property.OnSetValue((Font) DropdownBox.Items[DropdownBox.SelectedIndex].Object);
		};
    }

	public override void Refresh()
	{
		base.Refresh();
		if (!Available)
		{
			throw new NotImplementedException();
		}
		int idx = DropdownBox.Items.FindIndex(i => i.Object is Font && ((Font) i.Object).Equals((Font) Property.GetValue()));
		if (idx > -1) DropdownBox.SetSelectedIndex(idx);
		else
		{
			Program.CustomFonts.Add((Font) Property.GetValue());
			SetAvailableFonts();
            idx = DropdownBox.Items.FindIndex(i => i.Object is Font && ((Font) i.Object).Equals((Font) Property.GetValue()));
            DropdownBox.SetSelectedIndex(idx);
        }
	}

	public override void SetEnabled(bool Enabled)
	{
		DropdownBox.SetEnabled(Enabled);
	}

	void SetAvailableFonts()
	{
        List<ListItem> Items = new List<ListItem>();
        foreach ((string alias, Font f, _) in Fonts.AllFonts)
        {
			Items.Add(new ListItem(alias, f));
        }
		foreach (Font f in Program.CustomFonts)
		{
            string name = f.Name.Split('/', '\\').Last();
            if (name.EndsWith(".ttf")) name = name.Substring(0, name.Length - 4);
			name = $"({f.Size}, {name})";
			Items.Add(new ListItem(name, f));
        }
        Items.Add(new ListItem("Create new..."));
        DropdownBox.SetItems(Items);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (DropdownBox == null) return;
        DropdownBox.SetPosition(Sprites["hsep"].X + 4, DropdownBox.Position.Y);
        DropdownBox.SetSize(Size.Width - DropdownBox.Position.X - 4, DropdownBox.Size.Height);
    }
}

public class PaddingPropertyWidget : TextPropertyWidget
{
	public PaddingPropertyWidget(IContainer Parent, Property Property, float HSeparatorX) : base(Parent, Property, HSeparatorX)
	{
		TextBox.OnWidgetDeselected.GetInvocationList().ToList().ForEach(d => TextBox.OnWidgetDeselected -= (BaseEvent) d);
		Refresh();
		TextBox.OnWidgetDeselected += _ =>
		{
			Padding? padding = StringToPadding(TextBox.Text);
			if (padding != null) Property.SetValue(padding);
		};
	}

	public override void Refresh()
	{
		base.Refresh();
		if (!Available)
		{
			throw new NotImplementedException();
		}
		TextBox.SetText(PaddingToString((Padding) Property.GetValue()));
	}

	string PaddingToString(Padding p)
	{
		if (p.Left == p.Up & p.Up == p.Right && p.Right == p.Down) return p.Left.ToString();
		else if (p.Left == p.Right && p.Up == p.Down) return $"{p.Left}, {p.Up}";
		else return $"{p.Left}, {p.Up}, {p.Right}, {p.Down}";
	}

	Padding? StringToPadding(string Text)
	{
		Match m = Regex.Match(Text, @"^(-)*(\d+), *(-)*(\d+), *(-)*(\d+), *(-)*(\d+)$");
		if (m.Success)
		{
			return new Padding(
				(m.Groups[1].Length > 0 ? -1 : 1) * Convert.ToInt32(m.Groups[2].Value),
				(m.Groups[3].Length > 0 ? -1 : 1) * Convert.ToInt32(m.Groups[4].Value),
				(m.Groups[5].Length > 0 ? -1 : 1) * Convert.ToInt32(m.Groups[6].Value),
                (m.Groups[7].Length > 0 ? -1 : 1) * Convert.ToInt32(m.Groups[8].Value)
			);
		}
		m = Regex.Match(Text, @"^(-)*(\d+), *(-)*(\d+)$");
		if (m.Success)
		{
            return new Padding(
                (m.Groups[1].Length > 0 ? -1 : 1) * Convert.ToInt32(m.Groups[2].Value),
                (m.Groups[3].Length > 0 ? -1 : 1) * Convert.ToInt32(m.Groups[4].Value)
            );
        }
        m = Regex.Match(Text, @"^(-)*(\d+)$");
        if (m.Success)
        {
            return new Padding(
                (m.Groups[1].Length > 0 ? -1 : 1) * Convert.ToInt32(m.Groups[2].Value)
            );
        }
		return null;
    }
}

public class BoolPropertyWidget : PropertyWidget
{
    protected CheckBox CheckBox;

    public BoolPropertyWidget(IContainer Parent, Property Property, float HSeparatorX) : base(Parent, Property, HSeparatorX)
    {
        CheckBox = new CheckBox(this);
		CheckBox.SetPosition(0, 6);
        Refresh();
        CheckBox.OnCheckChanged += _ =>
        {
            Property.OnSetValue(CheckBox.Checked);
        };
    }

    public override void Refresh()
    {
        base.Refresh();
        CheckBox.SetChecked((bool) Property.GetValue());
    }

	public override void SetEnabled(bool Enabled)
	{
		CheckBox.SetEnabled(Enabled);
	}

	public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (CheckBox == null) return;
        CheckBox.SetPosition(Sprites["hsep"].X + 6, CheckBox.Position.Y);
        CheckBox.SetSize(Size.Width - CheckBox.Position.X - 4, CheckBox.Size.Height);
    }
}

public class ColorPropertyWidget : PropertyWidget
{
    protected VDDropdownBox DropdownBox;
	protected bool IncludeTransparency = true;

    public ColorPropertyWidget(IContainer Parent, Property Property, float HSeparatorX) : base(Parent, Property, HSeparatorX)
    {
        DropdownBox = new VDDropdownBox(this);
        DropdownBox.SetPosition(0, 4);
        Refresh();
		IncludeTransparency = Property.Parameters is bool ? (bool) Property.Parameters : true;
		DropdownBox.OnDropDownClicked += _ =>
		{
			ColorPickerWindow win = new ColorPickerWindow(TextToColor(DropdownBox.Text), IncludeTransparency);
			win.OnClosed += _ =>
			{
				if (!win.Apply) return;
				Property.SetValue(win.Value);
				Refresh();
			};
		};
    }

	public Color TextToColor(string Text)
	{
		Match m = Regex.Match(Text, @"\((\d+), *(\d+), *(\d+)\)");
		if (!m.Success)
		{
			if (IncludeTransparency) m = Regex.Match(Text, @"\((\d+), *(\d+), *(\d+), *(\d+)\)");
			else throw new Exception("Invalid color format.");
			if (!m.Success) throw new Exception("Invalid color format.");
        }
		int r = Convert.ToInt32(m.Groups[1].Value);
        int g = Convert.ToInt32(m.Groups[2].Value);
        int b = Convert.ToInt32(m.Groups[3].Value);
		int a = m.Groups.Count == 5 ? Convert.ToInt32(m.Groups[4].Value) : 255;
		if (r < 0 || g < 0 || b < 0 || r > 255 || g > 255 || b > 255) throw new Exception("Invalid color values.");
		return new Color((byte) r, (byte) g, (byte) b, (byte) a);
    }

	public string ColorToText(Color Color)
	{
		if (IncludeTransparency) return $"({Color.Red}, {Color.Green}, {Color.Blue}, {Color.Alpha})";
		return $"({Color.Red}, {Color.Green}, {Color.Blue})";
	}

    public override void Refresh()
    {
        base.Refresh();
		if (!Available)
		{
			throw new NotImplementedException();
		}
        DropdownBox.SetText(ColorToText((Color) Property.GetValue()));
    }

    public override void SetEnabled(bool Enabled)
    {
        DropdownBox.SetEnabled(Enabled);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (DropdownBox == null) return;
        DropdownBox.SetPosition(Sprites["hsep"].X + 4, DropdownBox.Position.Y);
        DropdownBox.SetSize(Size.Width - DropdownBox.Position.X - 4, DropdownBox.Size.Height);
    }
}

public class ListPropertyWidget : PropertyWidget
{
    protected VDDropdownBox DropdownBox;

	public ListPropertyWidget(IContainer Parent, Property Property, float HSeparatorX) : base(Parent, Property, HSeparatorX)
	{
		DropdownBox = new VDDropdownBox(this);
		DropdownBox.SetPosition(0, 4);
		DropdownBox.SetText("Edit Items");
		Refresh();
		DropdownBox.OnDropDownClicked += _ =>
		{
			GenericListWindow win = new GenericListWindow("Edit Items", (List<string>) Property.GetValue());
			win.OnClosed += _ =>
			{
				if (!win.Apply) return;
				Property.SetValue(win.Value);
				Refresh();
			};
		};
	}

    public override void Refresh()
    {
        base.Refresh();
		if (!Available)
		{
			throw new NotImplementedException();
		}
    }

    public override void SetEnabled(bool Enabled)
    {
        DropdownBox.SetEnabled(Enabled);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (DropdownBox == null) return;
        DropdownBox.SetPosition(Sprites["hsep"].X + 4, DropdownBox.Position.Y);
        DropdownBox.SetSize(Size.Width - DropdownBox.Position.X - 4, DropdownBox.Size.Height);
    }
}