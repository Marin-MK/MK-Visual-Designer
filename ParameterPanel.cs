using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualDesigner;

public class ParameterPanel : Widget
{
	public float HSeperatorX { get; protected set; } = 0.33f;

    DesignWidget Widget;
	List<PropertyWidget> PropertyWidgets = new List<PropertyWidget>();

	Container PropertyContainer;
	VStackPanel PropertyStackPanel;

    public ParameterPanel(IContainer Parent) : base(Parent)
	{
		SetBackgroundColor(10, 23, 37);
		Sprites["text"] = new Sprite(this.Viewport);
		Size s = Fonts.Paragraph.TextSize("Properties");
		Sprites["text"].Bitmap = new Bitmap(s);
		Sprites["text"].Bitmap.Unlock();
		Sprites["text"].Bitmap.Font = Fonts.Paragraph;
		Sprites["text"].Bitmap.DrawText("Properties", Color.WHITE);
		Sprites["text"].Bitmap.Lock();
		Sprites["text"].X = 6;
		Sprites["text"].Y = 6;

		PropertyContainer = new Container(this);
		PropertyContainer.SetDocked(true);
		PropertyContainer.SetPadding(0, 32, 0, 0);

		PropertyStackPanel = new VStackPanel(PropertyContainer);
		PropertyStackPanel.SetHDocked(true);
	}

	public void SetHSeperatorX(float HSeperatorX)
	{
		if (this.HSeperatorX != HSeperatorX)
		{
			this.HSeperatorX = HSeperatorX;
			Redraw();
		}
	}

	public void SetWidget(DesignWidget Widget)
	{
		if (this.Widget != Widget)
		{
			this.Widget = Widget;
			Redraw();
		}
	}

	public void Refresh()
	{
		foreach (PropertyWidget pw in PropertyWidgets)
		{
			pw.Refresh();
		}
	}

	protected override void Draw()
	{
		base.Draw();
		while (PropertyWidgets.Count > 0)
		{
			PropertyWidgets[0].Dispose();
			PropertyWidgets.RemoveAt(0);
		}
		if (Widget == null) return;
		for (int i = 0; i < Widget.Properties.Count; i++)
		{
			Property p = Widget.Properties[i];
			PropertyWidget pw = null;
			Type type = null;
			if (p.Type == PropertyType.Text) type = typeof(TextPropertyWidget);
			else if (p.Type == PropertyType.Numeric) type = typeof(NumericPropertyWidget);
			else if (p.Type == PropertyType.Dropdown) type = typeof(DropdownPropertyWidget);
			else if (p.Type == PropertyType.Font) type = typeof(FontPropertyWidget);
			else if (p.Type == PropertyType.Padding) type = typeof(PaddingPropertyWidget);
			else if (p.Type == PropertyType.Boolean) type = typeof(BoolPropertyWidget);
			else if (p.Type == PropertyType.Color) type = typeof(ColorPropertyWidget);
			pw = (PropertyWidget) Activator.CreateInstance(type, PropertyStackPanel, p, HSeperatorX);
            pw.SetMargins(2);
			PropertyWidgets.Add(pw);
		}
	}
}
