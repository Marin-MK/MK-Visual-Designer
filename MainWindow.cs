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

		Container MainContainer = new Container(UI);

		Grid MainGrid = new Grid(UI);
		MainGrid.SetDocked(true);
		MainGrid.SetColumns(
			new GridSize(366, Unit.Pixels),
			new GridSize(1)
		);
		MainGrid.SetRows(
			new GridSize(32, Unit.Pixels),
			new GridSize(1, Unit.Pixels),
			new GridSize(1)
		);

		MenuBar MainMenuBar = new MenuBar(MainGrid);
		MainMenuBar.SetGridColumn(0, 1);
		MainMenuBar.SetBackgroundColor(10, 23, 37);
		MainMenuBar.SetItems(MenuBarItemProvider.GetItems());

		Container Seperator = new Container(MainGrid);
		Seperator.SetGridRow(1);
		Seperator.SetGridColumn(0, 1);
		Seperator.SetBackgroundColor(79, 108, 159);

		ParameterPanel Panel = new ParameterPanel(MainGrid);
		Panel.SetGridRow(2);
		Program.ParameterPanel = Panel;

		Container DesignContainer = new Container(MainGrid);
		DesignContainer.SetGridRow(2);
		DesignContainer.SetGridColumn(1);
		Program.DesignContainer = DesignContainer;

		DesignWindow = new DesignWindow(DesignContainer);
		DesignWindow.SetSize(640 + DesignWidget.WidthAdd, 480 + DesignWidget.HeightAdd);
		DesignWindow.SetTitle("Unnamed");
		DesignWindow.Select(false);

		UI.RegisterShortcut(new Shortcut(null, new Key(Keycode.Z, Keycode.CTRL), _ => Program.Undo(), true));
        UI.RegisterShortcut(new Shortcut(null, new Key(Keycode.Y, Keycode.CTRL), _ => Program.Redo(), true));
    }

	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
		CenterDesignWindow();
	}

	public void CenterDesignWindow()
	{
        if (!DesignWindow.Fullscreen) DesignWindow.SetPosition(DesignWindow.Parent.Size.Width / 2 - DesignWindow.Size.Width / 2, DesignWindow.Parent.Size.Height / 2 - DesignWindow.Size.Height / 2);
    }
}
