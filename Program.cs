﻿global using RPGStudioMK;
global using RPGStudioMK.Widgets;
global using odl;
global using amethyst;
using System.Reflection;
using System.Text;


namespace VisualDesigner;

public class Program
{
    public static string? ProjectFile;
    public static bool UnsavedChanges = false;
    public static bool AnySavedStateExists = false;
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
        string? ProjectFile = Args.Length > 0 ? Args[0] : null;
        if (ProjectFile != null) Path.GetFullPath(ProjectFile);

        Directory.SetCurrentDirectory(Path.GetDirectoryName(Environment.ProcessPath));
        Config.Setup();
        Console.WriteLine();
        Amethyst.Start(Config.PathInfo, false, true);

        Widget.DefaultContextMenuFont = Fonts.Paragraph;

        MainWindow win = new MainWindow();
        win.OnClosing += e =>
        {
            if (UnsavedChanges)
            {
                e.Value = true;
                EnsureSaved(() => Amethyst.Stop());
            }
        };
        Utilities.Initialize(true);
        Program.MainWindow = win;

        if (ProjectFile != null)
        {
            OpenProject(ProjectFile);
        }
        else if (File.Exists("example.png")) OpenProject("example.png");

        Graphics.Update();
        win.UI.Widgets.ForEach(e => e.UpdateBounds());
        Graphics.Update(false, true);
        win.Show();
        Graphics.Update(false, true);

        Amethyst.Run();
    }

    public static string WidgetsToJSON(List<DesignWidget> Widgets, bool Indented = false)
    {
        List<Dictionary<string, object>> RawData = new List<Dictionary<string, object>>();
        Widgets.ForEach(w =>
        {
            WidgetData data = WidgetToData(w);
            RawData.Add(data.ConvertToDict());
        });
        string JSONData = JSONParser.JSONParser.ToString(RawData, Indented);
        return JSONData;
    }

    public static string WidgetToJSON(DesignWidget Widget, bool Indented = false)
    {
        Dictionary<string, object> RawData = WidgetToData(Widget).ConvertToDict();
        string JSONData = JSONParser.JSONParser.ToString(RawData, Indented);
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

    public static WindowData JSONToData(string JSON)
    {
        object? o = JSONParser.JSONParser.FromString(JSON);
        if (o is not Dictionary<string, object>) throw new Exception("Invalid JSON");
        return (WindowData) DictToData((Dictionary<string, object>) o);
    }

    public static List<DesignWidget> DictToWidgets(DesignWidget Parent, List<Dictionary<string, object>> Data)
    {
        List<WidgetData> DataList = Data.Select(dict => DictToData(dict)).ToList();
        List<DesignWidget> Widgets = new List<DesignWidget>();
        for (int i = 0; i < DataList.Count; i++)
        {
            Widgets.Add(WidgetFromData(Parent, DataList[i]));
            Data[i]["name"] = Widgets[i].Name;
        }
        return Widgets;
    }

    static System.Type? DataTypeToWidgetType(System.Type DataType)
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .ToList()
            .Find(t =>
            {
                WidgetTypeAndName? attrib = t.GetCustomAttribute<WidgetTypeAndName>();
                if (attrib == null) return false;
                if (attrib.DataType == DataType) return true;
                return false;
            });
    }

    public static string DataTypeToDataName(System.Type DataType)
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .ToList()
            .Find(t =>
            {
                WidgetTypeAndName? attrib = t.GetCustomAttribute<WidgetTypeAndName>();
                if (attrib == null) return false;
                if (attrib.DataType == DataType) return true;
                return false;
            })
            .GetCustomAttribute<WidgetTypeAndName>()
            .DataName;
    }

    static System.Type? TypeNameToDataType(string TypeName)
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .ToList()
            .Find(t =>
            {
                WidgetTypeAndName? attrib = t.GetCustomAttribute<WidgetTypeAndName>();
                if (attrib == null) return false;
                if (attrib.DataName == TypeName) return true;
                return false;
            })
            .GetCustomAttribute<WidgetTypeAndName>()
            .DataType;
    }

    public static WidgetData DictToData(Dictionary<string, object> Dict)
    {
        WidgetData? dat = null;
        System.Type? type = TypeNameToDataType((string) Dict["type"]);
        if (type == null) throw new Exception($"Unknown data type '{type}'.");
        dat = (WidgetData?) Activator.CreateInstance(type, Dict);
        if (dat == null) throw new Exception($"Failed to instantiate object of type '{type}'");
        return dat;
    }

    public static WidgetData WidgetToData(DesignWidget Widget)
    {
        WidgetData? dat = null;
        WidgetTypeAndName? attrib = Widget.GetType().GetCustomAttribute<WidgetTypeAndName>();
        System.Type? type = attrib?.DataType;
        if (type == null) throw new Exception($"Unknown widget type '{Widget.GetType().Name}'.");
        dat = (WidgetData?) Activator.CreateInstance(type, Widget);
        if (dat == null) throw new Exception($"Failed to instantiate object of type '{type}'");
        return dat;
    }

    public static DesignWidget WidgetFromData(DesignWidget Parent, WidgetData Data)
    {
        DesignWidget? w = null;
        System.Type? type = DataTypeToWidgetType(Data.GetType());
        if (type == null) throw new Exception($"Unknown data type '{Data.GetType()}'.");
        if (type == typeof(DesignWidget)) w = (DesignWidget?) Activator.CreateInstance(type, Parent, null);
        else w = (DesignWidget?) Activator.CreateInstance(type, Parent);
        if (w == null) throw new Exception($"Failed to instantiate object of type '{type}'");
        Data.SetWidget(w);
        return w;
    }

    public static void CreateCaches()
    {
        DesignButton.CreateFadeCache();
    }

    public static void Undo()
    {
        if (UndoList.Count > 0 && !Input.TextInputActive())
        {
            Undo.BaseUndoAction action = UndoList[UndoList.Count - 1];
            action.RevertTo(false);
            Undo.BaseUndoAction? prior = UndoList.Count > 0 ? UndoList[UndoList.Count - 1] : null;
            if (prior?.IsSavedState ?? false || prior == null && !AnySavedStateExists)
            {
                UnsavedChanges = false;
                if (MainWindow.Text.EndsWith("*")) MainWindow.SetText(MainWindow.Text.Substring(0, MainWindow.Text.Length - 1));
            }
            else
            {
                UnsavedChanges = true;
                if (!MainWindow.Text.EndsWith("*")) MainWindow.SetText(MainWindow.Text + "*");
            }
        }
    }

    public static void Redo()
    {
        if (RedoList.Count > 0 && !Input.TextInputActive())
        {
            Undo.BaseUndoAction action = RedoList[RedoList.Count - 1];
            action.RevertTo(true);
            if (action.IsSavedState)
            {
                UnsavedChanges = false;
                if (MainWindow.Text.EndsWith("*")) MainWindow.SetText(MainWindow.Text.Substring(0, MainWindow.Text.Length - 1));
            }
            else
            {
                UnsavedChanges = true;
                if (!MainWindow.Text.EndsWith("*")) MainWindow.SetText(MainWindow.Text + "*");
            }
        }
    }

    public static void NewProject()
    {
        EnsureSaved(() => ClearProject());
    }

    public static void ClearProject()
    {
        ProjectFile = null;
        DesignWindow.DeselectAll();
        int i = 0;
        while (DesignWindow.Widgets.Count > i)
        {
            if (DesignWindow.Widgets[i] is DesignWidget) DesignWindow.Widgets[i].Dispose();
            else i++;
        }
        DesignWindow.Name = "UnnamedWindow";
        DesignWindow.SetSize(640 + DesignWidget.WidthAdd, 480 + DesignWidget.HeightAdd);
        DesignWindow.SetFullscreen(false);
        DesignWindow.SetIsPopup(true);
    }

    public static void OpenProject()
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.SetTitle("Open Project File");
        ofd.SetFilter(new FileFilter("MK UI Design", "png"));
        string? Filename = ofd.ChooseFile();
        if (!string.IsNullOrEmpty(Filename))
        {
            if (!File.Exists(Filename)) return;
            EnsureSaved(() => OpenProject(Filename));
        }
    }

    public static void OpenProject(string ProjectFile)
    {
        if (!File.Exists(ProjectFile))
        {
            MessageBox win = new MessageBox("Error", $"No file exists at the path '{ProjectFile}'.", new List<string>() { "New Project", "Quit" }, IconType.Error);
            win.OnClosed += _ =>
            {
                if (win.Result == 1) Exit(false);
            };
            return;
        }
        string json = null;
        if (ProjectFile.EndsWith(".png"))
        {
            Program.ProjectFile = ProjectFile;
            decodl.PNGDecoder decoder = new decodl.PNGDecoder(ProjectFile);
            if (decoder.HasChunk("mKUI"))
            {
                byte[] Data = decoder.GetChunk("mKUI", true);
                json = Encoding.Default.GetString(Data);
            }
            else
            {
                new MessageBox("Error", $"This image was not exported by the Visual Designer and does not have any usable data.", ButtonType.OK, IconType.Error);
                return;
            }
        }
        else
        {
            StreamReader sr = new StreamReader(File.OpenRead(ProjectFile));
            json = sr.ReadToEnd();
            sr.Close();
        }
        try
        {
            WindowData WindowData = JSONToData(json);
            ClearProject();
            DesignWindow.Name = WindowData.Name;
            DesignWindow.SetSize(WindowData.Size);
            WindowData.SetWidget(DesignWindow);
            WindowData.Widgets.ForEach(data =>
            {
                WidgetFromData(DesignWindow, data);
            });
            DesignWindow.Center();
        }
        catch (Exception ex)
        {
            MessageBox win = new MessageBox("Error", $"Unknown error occurred.\n\n{ex}: {ex.Message}\n{ex.StackTrace}", ButtonType.OK, IconType.Error);
            win.OnClosed += _ =>
            {
                Exit(false);
            };
        }
    }

    public static unsafe void SaveProject()
    {
        if (string.IsNullOrEmpty(ProjectFile)) return;
        string json = WidgetToJSON(DesignWindow);
        Widget ActiveWidget = MainWindow.UI.SelectedWidget;
        List<DesignWidget> SelectedWidgets = new List<DesignWidget>(DesignWindow.SelectedWidgets);
        DesignWindow.DeselectAll();
        Bitmap Bitmap = DesignWindow.ToBitmap(-DesignWindow.WindowEdges, -DesignWindow.WindowEdges);
        decodl.PNGEncoder encoder = new decodl.PNGEncoder(Bitmap.PixelPointer, (uint)Bitmap.Width, (uint)Bitmap.Height);
        encoder.ColorType = decodl.ColorTypes.RGBA;
        encoder.AddCustomChunk("mKUI", json, true);
        encoder.Encode(ProjectFile);
        DesignWindow.UpdateBounds();
        SelectedWidgets.ForEach(w => w.Select(true));
        MainWindow.UI.SetSelectedWidget(ActiveWidget);
        if (MainWindow.Text.EndsWith("*")) MainWindow.SetText(MainWindow.Text.Substring(0, MainWindow.Text.Length - 1));
        UnsavedChanges = false;
        UndoList.ForEach(u => u.IsSavedState = false);
        RedoList.ForEach(r => r.IsSavedState = false);
        if (UndoList.Count > 0)
        {
            UndoList[^1].IsSavedState = true;
            AnySavedStateExists = true;
        }
        else
        {
            AnySavedStateExists = false;
        }
    }

    public static unsafe void SaveProjectAs()
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.SetTitle("Save Project File");
        ofd.SetFilter(new FileFilter("MK UI Design", "png"));
        ofd.SetInitialDirectory(ofd.DefaultFolder + "/" + DesignWindow.Name);
        string? Filename = ofd.SaveFile();
        if (!string.IsNullOrEmpty(Filename))
        {
            ProjectFile = Filename;
            SaveProject();
        }
    }

    public static void EnsureSaved(Action Callback)
    {
        if (UnsavedChanges)
        {
            MessageBox win = new MessageBox("Warning", "Are you sure you want to continue? All unsaved changes will be lost.", ButtonType.YesNoCancel, IconType.Warning);
            win.OnClosed += _ =>
            {
                if (win.Result == 0) Callback();
            };
        }
        else Callback();
    }

    public static string GeneratePseudoCode()
    {
        WindowData WindowData = (WindowData) WidgetToData(DesignWindow);
        CodeExporter ce = new CodeExporter();
        ce.ExportWindow(WindowData);
        return ce.ToString();
    }

    public static void ExportAsPseudoCode(string? Filename = null)
    {
        string code = GeneratePseudoCode();
        //Filename = "C:/Users/m3rei/Desktop/TestWindow.cs";
        if (string.IsNullOrEmpty(Filename))
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.SetTitle("Save Code File");
            ofd.SetFilter(new FileFilter("C# Script", "cs"));
            ofd.SetInitialDirectory(ofd.DefaultFolder + "/" + DesignWindow.Name);
            Filename = ofd.SaveFile();
        }
        if (!string.IsNullOrEmpty(Filename))
        {
            File.WriteAllText(Filename, code);
        }
    }

    public static void Exit(bool PromptSave)
    {
        if (PromptSave) EnsureSaved(() => Amethyst.Stop());
        else Amethyst.Stop();
    }
}