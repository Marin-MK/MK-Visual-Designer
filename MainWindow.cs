using System;
using System.Collections.Generic;

namespace VisualDesigner;

public class MainWindow : UIWindow
{
	DesignWindow DesignWindow;

	public MainWindow() : base(false, true)
	{
		SetBackgroundColor(28, 50, 73);
		SetText("RPG Studio MK Window Designer");
		SetIcon("assets/img/logo.png");
		Maximize();

		Grid MainGrid = new Grid(UI);
		MainGrid.SetColumns(
			new GridSize(366, Unit.Pixels),
			new GridSize(1)
		);

		ParameterPanel Panel = new ParameterPanel(MainGrid);
		Program.ParameterPanel = Panel;

		Container DesignContainer = new Container(MainGrid);
		DesignContainer.SetGridColumn(1);
		Program.DesignContainer = DesignContainer;

		DesignWindow = new DesignWindow(DesignContainer);
		DesignWindow.SetSize(640 + DesignWidget.WidthAdd, 480 + DesignWidget.HeightAdd);
		DesignWindow.SetTitle("Unnamed");
		DesignWindow.Select(false);
	}

	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
		CenterDesignWindow();
	}

	public void CenterDesignWindow()
	{
        DesignWindow.SetPosition(DesignWindow.Parent.Size.Width / 2 - DesignWindow.Size.Width / 2, DesignWindow.Parent.Size.Height / 2 - DesignWindow.Size.Height / 2);
    }
}
