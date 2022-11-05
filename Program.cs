global using RPGStudioMK;
global using RPGStudioMK.Widgets;
global using odl;
global using amethyst;

using System;

namespace VisualDesigner;

public class Program
{
    public static MainWindow MainWindow;
    public static Container DesignContainer;
    public static DesignWindow DesignWindow;
    public static ParameterPanel ParameterPanel;
    public static List<Font> CustomFonts = new List<Font>();

    public static void Main(string[] Args)
    {
        Config.Setup();
        Amethyst.Start(Config.PathInfo, false, true);

        Widget.DefaultContextMenuFont = Fonts.Paragraph;

        MainWindow win = new MainWindow();
        Utilities.Initialize(true);
        Program.MainWindow = win;
        Graphics.Update();
        win.UI.Widgets.ForEach(e => e.UpdateBounds());
        Graphics.Update(false, true);
        win.Show();
        Graphics.Update(false, true);

        Amethyst.Run();
    }

    public static void CreateCaches()
    {
        DesignButton.CreateFadeCache();
    }
}