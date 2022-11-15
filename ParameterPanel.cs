using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualDesigner;

public class ParameterPanel : Widget
{
	public static int DragWidth = 8;

	public float HSeperatorX { get; protected set; } = 0.45f;

    public DesignWidget Widget { get; protected set; }
	List<PropertyWidget> PropertyWidgets = new List<PropertyWidget>();

	Container PropertyContainer;
	VStackPanel PropertyStackPanel;

	bool WithinDragArea = false;
	bool Dragging = false;
	Point? GlobalMouseOrigin;
	int WidthOrigin;

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

		MinimumSize.Width = 40;
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
			if (p == null) continue;
			PropertyWidget pw = null;
			Type type = null;
			if (p.Type == PropertyType.Text) type = typeof(TextPropertyWidget);
			else if (p.Type == PropertyType.Numeric) type = typeof(NumericPropertyWidget);
			else if (p.Type == PropertyType.Dropdown) type = typeof(DropdownPropertyWidget);
			else if (p.Type == PropertyType.Font) type = typeof(FontPropertyWidget);
			else if (p.Type == PropertyType.Padding) type = typeof(PaddingPropertyWidget);
			else if (p.Type == PropertyType.Boolean) type = typeof(BoolPropertyWidget);
			else if (p.Type == PropertyType.Color) type = typeof(ColorPropertyWidget);
			else if (p.Type == PropertyType.List) type = typeof(ListPropertyWidget);
			pw = (PropertyWidget) Activator.CreateInstance(type, PropertyStackPanel, p, HSeperatorX);
            pw.SetMargins(2);
			PropertyWidgets.Add(pw);
		}
	}

	public override void MouseMoving(MouseEventArgs e)
	{
		base.MouseMoving(e);
		int rx = e.X - Viewport.X;
		int ry = e.Y - Viewport.Y;
		if (Dragging)
		{
			if (GlobalMouseOrigin == null) return;
			int diffX = rx - GlobalMouseOrigin.X;
			int diffY = ry - GlobalMouseOrigin.Y;
			int NewWidth = WidthOrigin + diffX;
			if (NewWidth < MinimumSize.Width) NewWidth = MinimumSize.Width;
			Program.MainWindow.MainGrid.Columns[0] = new GridSize(NewWidth, Unit.Pixels);
			Program.MainWindow.MainGrid.UpdateContainers();
			Program.DesignWindow.Center();
		}
		else
		{
			if (ry < 0 || ry >= Size.Height || rx < Size.Width)
			{
				if (Input.SystemCursor == CursorType.SizeWE)
				{
					if (!e.CursorHandled) Input.SetCursor(CursorType.Arrow);
					e.CursorHandled = true;
				}
				WithinDragArea = false;
				return;
			}
			if (rx >= Size.Width + DragWidth)
			{
				WithinDragArea = false;
				return;
			}
			Input.SetCursor(CursorType.SizeWE);
			WithinDragArea = true;
		}
	}

	public override void LeftMouseUp(MouseEventArgs e)
	{
		base.LeftMouseUp(e);
		WithinDragArea = false;
		Dragging = false;
	}

	public override void LeftMouseDown(MouseEventArgs e)
	{
		base.LeftMouseDown(e);
		if (WithinDragArea)
		{
			Dragging = true;
			GlobalMouseOrigin = new Point(e.X, e.Y);
			WidthOrigin = Size.Width;
		}
	}
}
