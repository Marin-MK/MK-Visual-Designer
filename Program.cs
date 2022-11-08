global using RPGStudioMK;
global using RPGStudioMK.Widgets;
global using odl;
global using amethyst;

using System;
using RPGStudioMK.Game;

namespace VisualDesigner;

public class Program
{
    public static MainWindow MainWindow;
    public static Container DesignContainer;
    public static DesignWindow DesignWindow;
    public static ParameterPanel ParameterPanel;
    public static List<Font> CustomFonts = new List<Font>();
    public static List<Dictionary<string, object>> CopyData;
    public static List<Undo.BaseUndoAction> UndoList = new List<Undo.BaseUndoAction>();
    public static List<Undo.BaseUndoAction> RedoList = new List<Undo.BaseUndoAction>();

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

    public static string WidgetsToJSON(List<DesignWidget> Widgets)
    {
        List<Dictionary<string, object>> RawData = new List<Dictionary<string, object>>();
        Widgets.ForEach(w =>
        {
            WidgetData data = WidgetToData(w);
            RawData.Add(data.ConvertToDict());
        });
        string JSONData = JSONParser.JSONParser.ToString(RawData);
        Console.WriteLine(JSONData);
        return JSONData;
    }

    public static List<Dictionary<string, object>> WidgetsToDict(List<DesignWidget> Widgets)
    {
        List<Dictionary<string, object>> RawData = new List<Dictionary<string, object>>();
        Widgets.ForEach(w =>
        {
            WidgetData data = WidgetToData(w);
            RawData.Add(data.ConvertToDict());
        });
        return RawData;
    }

    public static List<DesignWidget> JSONToWidgets(DesignWidget Parent, string JSON)
    {
        object? o = JSONParser.JSONParser.FromString(JSON);
        if (o is not List<object>) throw new Exception("Invalid JSON");
        List<object> objList = (List<object>) o;
        List<Dictionary<string, object>> WidgetData = objList.Select(o => (Dictionary<string, object>) o).ToList();
        List<WidgetData> DataList = WidgetData.Select(dict => DictToData(dict)).ToList();
        List<DesignWidget> Widgets = new List<DesignWidget>();
        foreach (WidgetData wdgt in DataList)
        {
            Widgets.Add(WidgetFromData(Parent, wdgt));
        }
        return Widgets;
    }

    public static List<DesignWidget> DictToWidgets(DesignWidget Parent, List<Dictionary<string, object>> Data)
    {
        List<WidgetData> DataList = Data.Select(dict => DictToData(dict)).ToList();
        List<DesignWidget> Widgets = new List<DesignWidget>();
        foreach (WidgetData wdgt in DataList)
        {
            Widgets.Add(WidgetFromData(Parent, wdgt));
        }
        return Widgets;
    }

    public static WidgetData DictToData(Dictionary<string, object> Dict)
    {
        WidgetData dat = null;
        System.Type t = null;
        string Type = (string) Dict["type"];
        if (Type == "button") t = typeof(ButtonWidgetData);
        else if (Type == "label") t = typeof(LabelWidgetData);
        else if (Type == "widget") t = typeof(WidgetData);
        dat = (WidgetData) Activator.CreateInstance(t, Dict);
        return dat;
    }

    public static WidgetData WidgetToData(DesignWidget Widget)
    {
        WidgetData dat = null;
        System.Type t = null;
        if (Widget is DesignButton) t = typeof(ButtonWidgetData);
        else if (Widget is DesignLabel) t = typeof(LabelWidgetData);
        else if (Widget.GetType() == typeof(DesignWidget)) t = typeof(WidgetData);
        dat = (WidgetData) Activator.CreateInstance(t, Widget);
        return dat;
    }

    public static DesignWidget WidgetFromData(DesignWidget Parent, WidgetData Data)
    {
        DesignWidget w = null;
        System.Type t = null;
        if (Data.Type == "button") t = typeof(DesignButton);
        else if (Data.Type == "label") t = typeof(DesignLabel);
        else if (Data.Type == "widget") t = typeof(DesignWidget);
        w = (DesignWidget) Activator.CreateInstance(t, Parent);
        Data.SetWidget(w);
        return w;
    }

    public static void CreateCaches()
    {
        DesignButton.CreateFadeCache();
    }

    public static void Undo(bool Internal = false)
    {
        if (UndoList.Count > 0 && !Input.TextInputActive())
        {
            UndoList[UndoList.Count - 1].RevertTo(false);
        }
    }

    /// <summary>
    /// Redoes the latest change that you undid.
    /// </summary>
    public static void Redo()
    {
        if (RedoList.Count > 0 && !Input.TextInputActive())
        {
            RedoList[RedoList.Count - 1].RevertTo(true);
        }
    }
}