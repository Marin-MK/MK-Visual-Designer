using System.Diagnostics;

namespace VisualDesigner;

public class MainWindow : UIWindow
{
	public Grid MainGrid;

	DesignWindow DesignWindow;

	public MainWindow() : base(false, true)
	{
		SetBackgroundColor(28, 50, 73);
        string Version = FileVersionInfo.GetVersionInfo(Environment.ProcessPath).ProductVersion;
		SetText($"RPG Studio MK Window Designer {Version}");
        SetIcon("assets/img/logo.png");
		Maximize();

		Container MainContainer = new Container(UI);

		MainGrid = new Grid(UI);
		MainGrid.SetDocked(true);
		MainGrid.SetColumns(
			new GridSize(420, Unit.Pixels),
			new GridSize(ParameterPanel.DragWidth, Unit.Pixels),
			new GridSize(1)
		);
		MainGrid.SetRows(
			new GridSize(32, Unit.Pixels),
			new GridSize(1, Unit.Pixels),
			new GridSize(1)
		);

		MenuBar MainMenuBar = new MenuBar(MainGrid);
		MainMenuBar.SetGridColumn(0, 2);
		MainMenuBar.SetBackgroundColor(10, 23, 37);
		MainMenuBar.SetItems(MenuBarItemProvider.GetItems());
		MainMenuBar.RemoveShortcuts();

		Container Seperator = new Container(MainGrid);
		Seperator.SetGridRow(1);
		Seperator.SetGridColumn(0, 2);
		Seperator.SetBackgroundColor(79, 108, 159);

		ParameterPanel Panel = new ParameterPanel(MainGrid);
		Panel.SetGridRow(2);
		Program.ParameterPanel = Panel;

		Container DesignContainer = new Container(MainGrid);
		DesignContainer.SetGridRow(2);
		DesignContainer.SetGridColumn(2);
		Program.DesignContainer = DesignContainer;

		DesignWindow = new DesignWindow(DesignContainer);
		DesignWindow.SetSize(640 + DesignWidget.WidthAdd, 480 + DesignWidget.HeightAdd);
		DesignWindow.SetTitle("Unnamed");
		DesignWindow.Select(false);

		UI.RegisterShortcut(new Shortcut(null, new Key(Keycode.Z, Keycode.CTRL), _ => Program.Undo(), true));
        UI.RegisterShortcut(new Shortcut(null, new Key(Keycode.Y, Keycode.CTRL), _ => Program.Redo(), true));
		UI.RegisterShortcut(new Shortcut(null, new Key(Keycode.S, Keycode.CTRL) , _ => Program.SaveProject(), true));
        UI.RegisterShortcut(new Shortcut(null, new Key(Keycode.S, Keycode.SHIFT, Keycode.CTRL), _ => Program.SaveProjectAs(), true));
        UI.RegisterShortcut(new Shortcut(null, new Key(Keycode.O, Keycode.CTRL), _ => Program.OpenProject(), true));
        UI.RegisterShortcut(new Shortcut(null, new Key(Keycode.N, Keycode.CTRL), _ => Program.NewProject(), true));
        UI.RegisterShortcut(new Shortcut(null, new Key(Keycode.E, Keycode.CTRL), _ => Program.ExportAsPseudoCode(), true));
		UI.RegisterShortcut(new Shortcut(null, new Key(Keycode.A, Keycode.CTRL), _ => Program.DesignWindow.SelectAllChildren(), true, e => e.Value = !Input.TextInputActive()));
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
