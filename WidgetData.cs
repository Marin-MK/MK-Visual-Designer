using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace VisualDesigner;

// Necessary to prevent derived classes from being trimmed out of the final assembly because they're never literally being referenced
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class WidgetData
{
    public virtual string ClassName => "Container";
    public virtual string[] Dependencies => new string[0];

    public string ParentName;
    public string Name;
    public Point Position;
    public Size? Size;
    public Padding Padding;
    public bool HDocked;
    public bool VDocked;
    public bool BottomDocked;
    public bool RightDocked;
    public bool AutoResize;
    public Color BackgroundColor;
    public List<WidgetData> Widgets = new List<WidgetData>();

    public WidgetData(DesignWidget Widget)
    {
        this.Name = Widget.Name;
        this.ParentName = Widget.Parent is DesignWidget ? ((DesignWidget) Widget.Parent).Name : null;
        this.Position = new Point(Widget.Position.X, Widget.Position.Y);
        this.Size = new Size(Widget.Size.Width, Widget.Size.Height);
        this.HDocked = Widget.HDocked;
        this.VDocked = Widget.VDocked;
        this.Padding = new Padding(Widget.Padding.Left, Widget.Padding.Up, Widget.Padding.Right, Widget.Padding.Down);
        this.BottomDocked = Widget.BottomDocked;
        this.RightDocked = Widget.RightDocked;
        this.AutoResize = Widget.AutoResize;
        this.BackgroundColor = new Color(Widget.BackgroundColor.Red, Widget.BackgroundColor.Green, Widget.BackgroundColor.Blue, Widget.BackgroundColor.Alpha);
        this.Widgets = Widget.Widgets.FindAll(w => w is DesignWidget).Select(w => Program.WidgetToData((DesignWidget) w)).ToList();
    }

    public WidgetData(Dictionary<string, object> Data)
    {
        this.Name = (string) Data["name"];
        if (Data.ContainsKey("size"))
        {
            this.Size = new Size(0, 0);
            this.Size.Width = (int) (long) ValueFromPath(Data, "size", "width");
            this.Size.Height = (int) (long) ValueFromPath(Data, "size", "height");
        }
        List<Dictionary<string, object>>? WidgetData = null;
        if (Data["widgets"] is List<object>)
        {
            List<object> objList = (List<object>) Data["widgets"];
            WidgetData = objList.Select(o => (Dictionary<string, object>)o).ToList();
        }
        else if (Data["widgets"] is List<Dictionary<string, object>>)
        {
            WidgetData = (List<Dictionary<string, object>>) Data["widgets"];
        }
        else throw new Exception("Invalid widget list data");
        this.Widgets = WidgetData.Select(dict => Program.DictToData(dict)).ToList();
        if (this is WindowData) return;
        this.ParentName = (string) Data["parentname"];
        this.Position = new Point(0, 0);
        this.Position.X = (int) (long) ValueFromPath(Data, "position", "x");
        this.Position.Y = (int) (long) ValueFromPath(Data, "position", "y");
        this.HDocked = (bool) Data["hdocked"];
        this.VDocked = (bool) Data["vdocked"];
        int l = (int) (long) ValueFromPath(Data, "padding", "left");
        int u = (int) (long) ValueFromPath(Data, "padding", "up");
        int r = (int) (long) ValueFromPath(Data, "padding", "right");
        int d = (int) (long) ValueFromPath(Data, "padding", "down");
        this.Padding = new Padding(l, u, r, d);
        this.BottomDocked = (bool) Data["bottomdocked"];
        this.RightDocked = (bool) Data["rightdocked"];
        this.AutoResize = (bool) Data["autoresize"];
        this.BackgroundColor = ColorFromPath(Data, "bgcolor");
    }

    protected object ValueFromPath(Dictionary<string, object> Dict, params string[] Path)
    {
        for (int i = 0; i < Path.Length; i++)
        {
            object o = Dict[Path[i]];
            if (i == Path.Length - 1) return o;
            if (o is not Dictionary<string, object>) throw new Exception("Part of the path not found");
            Dict = (Dictionary<string, object>) o;
        }
        throw new Exception("Value not found");
    }

    protected Color ColorFromPath(Dictionary<string, object> Dict, params string[] Path)
    {
        Dictionary<string, object> Hash = (Dictionary<string, object>) ValueFromPath(Dict, Path);
        byte rC = (byte) (long) Hash["red"];
        byte gC = (byte) (long) Hash["green"];
        byte bC = (byte) (long) Hash["blue"];
        byte aC = (byte) (long) Hash["alpha"];
        return new Color(rC, gC, bC, aC);
    }

    protected Dictionary<string, object> CreateDict(params (string Name, object Value)[] Values)
    {
        Dictionary<string, object> Dict = new Dictionary<string, object>();
        for (int i = 0; i < Values.Length; i++)
        {
            Dict.Add(Values[i].Name, Values[i].Value);
        }
        return Dict;
    }

    protected Dictionary<string, object> CreateColor(Color Color)
    {
        return CreateDict(("red", (long) Color.Red), ("green", (long) Color.Green), ("blue", (long) Color.Blue), ("alpha", (long) Color.Alpha));
    }

    protected string GetFontCode(Font Font)
    {
        int idx = Fonts.AllFonts.FindIndex(f => f.Font.Equals(Font));
        if (idx == -1) return $"Font.Get(\"{Font.Name}\", {Font.Size})";
        else
        {
            (_, _, string CodeName) = Fonts.AllFonts[idx];
            return $"Fonts.{CodeName}";
        }
    }

    protected string GetColorCode(Color Color, bool IgnoreAlphaWhenFull = true, bool AsColorObject = false)
    {
        return $"{(AsColorObject ? "new Color(" : "")}{Color.Red}, {Color.Green}, {Color.Blue}{(IgnoreAlphaWhenFull && Color.Alpha == 255 ? "" : ", " + Color.Alpha.ToString())}{(AsColorObject ? ")" : "")}";
    }

    protected string GetEnumCode<T>(int Value) where T : struct, IConvertible
    {
        System.Type EnumType = typeof(T);
        if (!EnumType.IsEnum) throw new Exception("The given type must be an enum.");
        string Code = "";
        foreach (var EnumObject in EnumType.GetEnumValues())
        {
            int EnumValue = Convert.ToInt32(EnumObject);
            if ((Value & EnumValue) != 0)
            {
                if (!string.IsNullOrEmpty(Code)) Code += " | ";
                Code += EnumType.Name + "." + EnumType.GetEnumName(EnumObject);
            }
        }
        return Code;
    }

    protected string GetDrawOptionsCode(DrawOptions DrawOptions)
    {
        if ((DrawOptions & DrawOptions.LeftAlign) != 0) DrawOptions &= ~DrawOptions.LeftAlign;
        return GetEnumCode<DrawOptions>((int) DrawOptions);
    }

    public virtual void AddToDict(Dictionary<string, object> Dict)
    {

    }

    public virtual void SetWidget(DesignWidget Widget)
    {
        if (Program.DesignWindow.GetWidgetByName(this.Name) != null)
        {
            Match m = Regex.Match(this.Name, @"^.*?(\d+)");
            string name = this.Name;
            int idx = 0;
            if (m.Success)
            {
                idx = Convert.ToInt32(m.Groups[1].Value);
                name = name.Replace(idx.ToString(), "");
            }
            Widget.Name = Program.DesignWindow.GetName(name, idx);
        }
        else Widget.Name = this.Name;
        Widget.SetPosition(this.Position);
        if (Size != null) Widget.SetSize(this.Size);
        Widget.SetHDocked(this.HDocked);
        Widget.SetVDocked(this.VDocked);
        Widget.SetPadding(this.Padding);
        Widget.SetBottomDocked(this.BottomDocked);
        Widget.SetRightDocked(this.RightDocked);
        Widget.AutoResize = this.AutoResize;
        Widget.SetBackgroundColor(this.BackgroundColor);
        this.Widgets.ForEach(data =>
        {
            Program.WidgetFromData(Widget, data);
        });
    }

    public virtual Dictionary<string, object> ConvertToDict()
    {
        Dictionary<string, object> Dict = new Dictionary<string, object>();
        Dict.Add("type", Program.DataTypeToDataName(GetType()));
        Dict.Add("name", Name);
        Dict.Add("parentname", ParentName);
        Dict.Add("position", CreateDict(("x", (long) Position.X), ("y", (long) Position.Y)));
        if (Size != null) Dict.Add("size", CreateDict(("width", (long) Size.Width), ("height", (long) Size.Height)));
        Dict.Add("hdocked", HDocked);
        Dict.Add("vdocked", VDocked);
        Dict.Add("padding", CreateDict(("left", (long) Padding.Left), ("up", (long) Padding.Up), ("right", (long) Padding.Right), ("down", (long) Padding.Down)));
        Dict.Add("bottomdocked", BottomDocked);
        Dict.Add("rightdocked", RightDocked);
        Dict.Add("autoresize", AutoResize);
        Dict.Add("bgcolor", CreateColor(BackgroundColor));
        Dict.Add("widgets", Widgets.Select(w => w.ConvertToDict()).ToList());
        AddToDict(Dict);
        return Dict;
    }

    public virtual void WriteCode(CodeExporter CE)
    {
        if (AutoResize) CE.WriteCode($"AutoResize = true");
        if (Position.X != 0 || Position.Y != 0) CE.WriteCode($"SetPosition({Position.X}, {Position.Y});");
        if (Size != null)
        {
            if (HDocked && VDocked) CE.WriteCode($"SetDocked(true);");
            else if (HDocked)
            {
                CE.WriteCode($"SetHeight({Size.Height - DesignWidget.HeightAdd});");
                CE.WriteCode($"SetHDocked(true);");
            }
            else if (VDocked)
            {
                CE.WriteCode($"SetWidth({Size.Width - DesignWidget.WidthAdd});");
                CE.WriteCode($"SetVDocked(true);");
            }
            else if (Size.Width != 0 || Size.Height != 0) CE.WriteCode($"SetSize({Size.Width - DesignWidget.WidthAdd}, {Size.Height - DesignWidget.HeightAdd});");
        }
        if (Padding.Left != 0 || Padding.Right != 0 || Padding.Up != 0 || Padding.Down != 0)
        {
            if (Padding.Left == Padding.Right && Padding.Right == Padding.Up && Padding.Up == Padding.Down) CE.WriteCode($"SetPadding({Padding.Left});");
            else if (Padding.Left == Padding.Right && Padding.Up == Padding.Down) CE.WriteCode($"SetPadding({Padding.Left}, {Padding.Up});");
            else CE.WriteCode($"SetPadding({Padding.Left}, {Padding.Up}, {Padding.Right}, {Padding.Down});");
        }
        if (BottomDocked) CE.WriteCode($"SetBottomDocked(true);");
        if (RightDocked) CE.WriteCode($"SetRightDocked(true);");
        if (BackgroundColor.Alpha != 0) CE.WriteCode($"SetBackgroundColor({GetColorCode(BackgroundColor)});");
    }
}

public class WindowData : WidgetData
{
    public string Title;
    public bool Fullscreen;
    public bool IsPopup;
    public bool HasOKButton;
    public bool HasCancelButton;
    public List<string> OtherButtons;

    public WindowData(DesignWindow w) : base(w)
    {
        this.Title = w.Title;
        this.Fullscreen = w.Fullscreen;
        this.IsPopup = w.IsPopup;
        this.HasOKButton = w.HasOKButton;
        this.HasCancelButton = w.HasCancelButton;
        this.OtherButtons = new List<string>(w.OtherButtons);
    }

    public WindowData(Dictionary<string, object> Data) : base(Data)
    {
        this.Title = (string) Data["title"];
        this.Fullscreen = (bool) Data["fullscreen"];
        this.IsPopup = (bool) Data["popup"];
        this.HasOKButton = (bool) Data["okbutton"];
        this.HasCancelButton = (bool) Data["cancelbutton"];
        if (Data["buttons"] is List<object>) this.OtherButtons = ((List<object>) Data["buttons"]).Select(o => o.ToString()).ToList();
        else if (Data["button"] is List<string>) this.OtherButtons = (List<string>) Data["buttons"];
        else throw new Exception("Buttons list not a valid list.");
    }

    public override Dictionary<string, object> ConvertToDict()
    {
        Dictionary<string, object> Dict = new Dictionary<string, object>();
        Dict.Add("type", Program.DataTypeToDataName(GetType()));
        Dict.Add("name", Name);
        Dict.Add("size", CreateDict(("width", (long) Size.Width), ("height", (long) Size.Height)));
        Dict.Add("widgets", Widgets.Select(w => w.ConvertToDict()).ToList());
        Dict.Add("title", Title);
        Dict.Add("fullscreen", Fullscreen);
        Dict.Add("popup", IsPopup);
        Dict.Add("okbutton", HasOKButton);
        Dict.Add("cancelbutton", HasCancelButton);
        Dict.Add("buttons", OtherButtons);
        return Dict;
    }

    public override void SetWidget(DesignWidget Widget)
    {
        DesignWindow Window = (DesignWindow) Widget;
        Window.SetTitle(Title);
        Window.SetFullscreen(Fullscreen);
        Window.SetIsPopup(IsPopup);
        Window.SetHasOKButton(HasOKButton);
        Window.SetHasCancelButton(HasCancelButton);
        Window.SetOtherButtons(OtherButtons);
    }
}

public class ButtonData : WidgetData
{
    public override string ClassName => "Button";
    public string Text;
    public Font Font;
    public Color TextColor;
    public bool LeftAlign;
    public int TextX;
    public bool Enabled;
    public bool Repeatable;

    public ButtonData(DesignButton w) : base(w)
    {
        this.Text = w.Text;
        this.Font = w.Font;
        this.TextColor = w.TextColor;
        this.LeftAlign = w.LeftAlign;
        this.TextX = w.TextX;
        this.Enabled = w.Enabled;
        this.Repeatable = w.Repeatable;
    }

    public ButtonData(Dictionary<string, object> Data) : base(Data)
    {
        this.Text = (string) Data["text"];
        this.Font = Font.Get((string) ValueFromPath(Data, "font", "name"), (int) (long) ValueFromPath(Data, "font", "size"));
        this.TextColor = ColorFromPath(Data, "textcolor");
        this.LeftAlign = (bool) Data["leftalign"];
        this.TextX = (int) (long) Data["textx"];
        this.Enabled = (bool) Data["enabled"];
        this.Repeatable = (bool) Data["repeatable"];
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("text", Text);
        Dict.Add("font", CreateDict(("name", Font.Name.Replace('\\', '/')), ("size", (long) Font.Size)));
        Dict.Add("textcolor", CreateColor(TextColor));
        Dict.Add("leftalign", LeftAlign);
        Dict.Add("textx", (long) TextX);
        Dict.Add("enabled", Enabled);
        Dict.Add("repeatable", Repeatable);
    }

    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignButton Button = (DesignButton) Widget;
        Button.SetText(this.Text);
        Button.SetFont(this.Font);
        Button.SetTextColor(this.TextColor);
        Button.SetLeftAlign(this.LeftAlign);
        Button.SetTextX(this.TextX);
        Button.SetEnabled(this.Enabled);
        Button.SetRepeatable(this.Repeatable);
    }

    public override void WriteCode(CodeExporter CE)
    {
        base.WriteCode(CE);
        CE.WriteCode($"SetFont({GetFontCode(Font)});");
        if (!string.IsNullOrEmpty(Text)) CE.WriteCode($"SetText(\"{Text}\");");
        if (!TextColor.Equals(Color.WHITE)) CE.WriteCode($"SetTextColor({GetColorCode(TextColor)});");
        if (LeftAlign) CE.WriteCode($"SetLeftAlign(true);");
        if (LeftAlign && TextX != 0) CE.WriteCode($"SetTextX({TextX});");
        if (!Enabled) CE.WriteCode($"SetEnabled(false);");
        if (Repeatable) CE.WriteCode($"SetRepeatable(true);");
    }
}

public class LabelData : WidgetData
{
    public override string ClassName => "Label";
    public string Text;
    public Font Font;
    public Color TextColor;
    public int WidthLimit;
    public string LimitReplacementText;
    public DrawOptions DrawOptions;

    public LabelData(DesignLabel w) : base(w)
    {
        this.Text = w.Text;
        this.Font = w.Font;
        this.TextColor = w.TextColor;
        this.WidthLimit = w.WidthLimit;
        this.LimitReplacementText = w.LimitReplacementText;
        this.DrawOptions = w.DrawOptions;
    }

    public LabelData(Dictionary<string, object> Data) : base(Data)
    {
        this.Text = (string) Data["text"];
        this.Font = Font.Get((string) ValueFromPath(Data, "font", "name"), (int) (long) ValueFromPath(Data, "font", "size"));
        this.TextColor = ColorFromPath(Data, "textcolor");
        this.WidthLimit = (int) (long) Data["widthlimit"];
        this.LimitReplacementText = (string) Data["limittext"];
        DrawOptions ops = DrawOptions.LeftAlign;
        if ((bool) ValueFromPath(Data, "drawoptions", "bold")) ops |= DrawOptions.Bold;
        if ((bool) ValueFromPath(Data, "drawoptions", "italic")) ops |= DrawOptions.Italic;
        if ((bool) ValueFromPath(Data, "drawoptions", "underlined")) ops |= DrawOptions.Underlined;
        if ((bool) ValueFromPath(Data, "drawoptions", "strikethrough")) ops |= DrawOptions.Strikethrough;
        if ((bool) ValueFromPath(Data, "drawoptions", "leftalign")) ops |= DrawOptions.LeftAlign;
        if ((bool) ValueFromPath(Data, "drawoptions", "rightalign")) ops |= DrawOptions.RightAlign;
        if ((bool) ValueFromPath(Data, "drawoptions", "centeralign")) ops |= DrawOptions.CenterAlign;
        this.DrawOptions = ops;
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("text", Text);
        Dict.Add("font", CreateDict(("name", Font.Name.Replace('\\', '/')), ("size", (long) Font.Size)));
        Dict.Add("textcolor", CreateColor(TextColor));
        Dict.Add("widthlimit", (long) WidthLimit);
        Dict.Add("limittext", LimitReplacementText);
        Dict.Add("drawoptions", CreateDict(
            ("bold", (DrawOptions & DrawOptions.Bold) != 0),
            ("italic", (DrawOptions & DrawOptions.Italic) != 0),
            ("underlined", (DrawOptions & DrawOptions.Underlined) != 0),
            ("strikethrough", (DrawOptions & DrawOptions.Strikethrough) != 0),
            ("leftalign", (DrawOptions & DrawOptions.LeftAlign) != 0),
            ("centeralign", (DrawOptions & DrawOptions.CenterAlign) != 0),
            ("rightalign", (DrawOptions & DrawOptions.RightAlign) != 0)
        ));
    }

    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignLabel Label = (DesignLabel) Widget;
        Label.SetText(this.Text);
        Label.SetFont(this.Font);
        Label.SetTextColor(this.TextColor);
        Label.SetWidthLimit(this.WidthLimit);
        Label.SetLimitReplacementText(this.LimitReplacementText);
        Label.SetDrawOptions(this.DrawOptions);
    }

    public override void WriteCode(CodeExporter CE)
    {
        base.WriteCode(CE);
        if (!string.IsNullOrEmpty(Text)) CE.WriteCode($"SetText(\"{Text}\");");
        CE.WriteCode($"SetFont({GetFontCode(Font)});");
        if (!TextColor.Equals(Color.WHITE)) CE.WriteCode($"SetTextColor({GetColorCode(TextColor)});");
        if (WidthLimit != -1) CE.WriteCode($"SetWidthLimit({WidthLimit});");
        if (LimitReplacementText != "...") CE.WriteCode($"SetLimitReplacementText(\"{LimitReplacementText}\");");
        if (DrawOptions != DrawOptions.LeftAlign) CE.WriteCode($"SetDrawOptions({GetDrawOptionsCode(DrawOptions)});");
    }
}

public class ListBoxData : WidgetData
{
    public override string ClassName => "ListBox";
    public override string[] Dependencies => new string[] { "System.Collections", "System.Collections.Generic" };

    public Font Font;
    public int LineHeight;
    public List<string> Items;
    public bool Enabled;
    public int SelectedIndex;
    public Color SelectedItemColor;

    public ListBoxData(DesignListBox w) : base(w)
    {
        this.Font = w.Font;
        this.LineHeight = w.LineHeight;
        this.Items = w.Items.Select(i => i.Name).ToList();
        this.Enabled = w.Enabled;
        this.SelectedIndex = w.SelectedIndex;
        this.SelectedItemColor = w.SelectedItemColor;
    }

    public ListBoxData(Dictionary<string, object> Data) : base(Data)
    {
        this.Font = Font.Get((string) ValueFromPath(Data, "font", "name"), (int) (long) ValueFromPath(Data, "font", "size"));
        this.LineHeight = (int) (long) Data["lineheight"];
        if (Data["items"] is List<string>) this.Items = ((List<string>) Data["items"]);
        else this.Items = ((List<object>) Data["items"]).Select(o => o.ToString()).ToList();
        this.Enabled = (bool) Data["enabled"];
        this.SelectedIndex = (int) (long) Data["selectedindex"];
        this.SelectedItemColor = ColorFromPath(Data, "selectedcolor");
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("font", CreateDict(("name", Font.Name.Replace('\\', '/')), ("size", (long) Font.Size)));
        Dict.Add("lineheight", (long) LineHeight);
        Dict.Add("items", Items);
        Dict.Add("enabled", Enabled);
        Dict.Add("selectedindex", (long) SelectedIndex);
        Dict.Add("selectedcolor", CreateColor(SelectedItemColor));
    }

    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignListBox List = (DesignListBox) Widget;
        List.SetFont(this.Font);
        List.SetLineHeight(this.LineHeight);
        List.SetItems(this.Items.Select(i => new ListItem(i)).ToList());
        List.SetEnabled(this.Enabled);
        List.SetSelectedIndex(this.SelectedIndex);
        List.SetSelectedItemColor(this.SelectedItemColor);
    }

    public override void WriteCode(CodeExporter CE)
    {
        base.WriteCode(CE);
        CE.WriteCode($"SetFont({GetFontCode(Font)});");
        if (LineHeight != 20) CE.WriteCode($"SetLineHeight({LineHeight});");
        if (Items.Count != 0)
        {
            CE.WriteCode("SetItems(new List<ListItem>()");
            CE.WriteCode("{", false);
            for (int i = 0; i < Items.Count; i++)
            {
                CE.WriteCode($"\tnew ListItem(\"{Items[i]}\"){(i == Items.Count - 1 ? "" : ",")}", false);
            }
            CE.WriteCode("});", false);
        }
        if (!Enabled) CE.WriteCode($"SetEnabled(false);");
        if (SelectedIndex != -1) CE.WriteCode($"SetSelectedIndex({SelectedIndex});");
        if (!SelectedItemColor.Equals(new Color(55, 187, 255))) CE.WriteCode($"SetSelectedItemColor({GetColorCode(SelectedItemColor)});");
    }
}

public class TextBoxData : WidgetData
{
    public override string ClassName => "TextBox";

    public string Text;
    public int TextX;
    public int TextY;
    public int CaretY;
    public int? CaretHeight;
    public Font Font;
    public Color TextColor;
    public Color DisabledTextColor;
    public Color CaretColor;
    public Color FillerColor;
    public bool ReadOnly;
    public bool NumericOnly;
    public int DefaultNumericValue;
    public bool AllowMinusSigns;
    public bool ShowDisabledText;
    public bool DeselectOnEnterPressed;
    public bool PopupStyle;
    public bool Enabled;

    public TextBoxData(DesignTextBox w) : base(w)
    {
        this.Text = w.Text;
        this.TextX = w.TextX;
        this.TextY = w.TextY;
        this.CaretY = w.CaretY;
        this.CaretHeight = w.CaretHeight;
        this.Font = w.Font;
        this.TextColor = w.TextColor;
        this.DisabledTextColor = w.DisabledTextColor;
        this.CaretColor = w.CaretColor;
        this.FillerColor = w.FillerColor;
        this.ReadOnly = w.ReadOnly;
        this.NumericOnly = w.NumericOnly;
        this.DefaultNumericValue = w.DefaultNumericValue;
        this.AllowMinusSigns = w.AllowMinusSigns;
        this.ShowDisabledText = w.ShowDisabledText;
        this.DeselectOnEnterPressed = w.DeselectOnEnterPressed;
        this.PopupStyle = w.PopupStyle;
        this.Enabled = w.Enabled;
    }

    public TextBoxData(Dictionary<string, object> Data) : base(Data)
    {
        this.Text = (string)Data["text"];
        this.TextX = (int)(long)Data["textx"];
        this.TextY = (int)(long)Data["texty"];
        this.CaretY = (int)(long)Data["carety"];
        this.CaretHeight = (int)(long)Data["caretheight"];
        if (this.CaretHeight == -1) this.CaretHeight = null;
        this.Font = Font.Get((string) ValueFromPath(Data, "font", "name"), (int) (long) ValueFromPath(Data, "font", "size"));
        this.TextColor = ColorFromPath(Data, "textcolor");
        this.DisabledTextColor = ColorFromPath(Data, "disabledtextcolor");
        this.CaretColor = ColorFromPath(Data, "caretcolor");
        this.FillerColor = ColorFromPath(Data, "fillercolor");
        this.ReadOnly = (bool)Data["readonly"];
        this.NumericOnly = (bool)Data["numericonly"];
        this.DefaultNumericValue = (int)(long)Data["defaultnumber"];
        this.AllowMinusSigns = (bool)Data["allowminus"];
        this.ShowDisabledText = (bool)Data["showdisabled"];
        this.DeselectOnEnterPressed = (bool)Data["deselectonenter"];
        this.PopupStyle = (bool)Data["popupstyle"];
        this.Enabled = (bool)Data["enabled"];
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("text", Text);
        Dict.Add("textx", (long)TextX);
        Dict.Add("texty", (long)TextY);
        Dict.Add("carety", (long)CaretY);
        Dict.Add("caretheight", (long) (CaretHeight ?? -1));
        Dict.Add("font", CreateDict(("name", Font.Name.Replace('\\', '/')), ("size", (long) Font.Size)));
        Dict.Add("textcolor", CreateColor(TextColor));
        Dict.Add("disabledtextcolor", CreateColor(DisabledTextColor));
        Dict.Add("caretcolor", CreateColor(CaretColor));
        Dict.Add("fillercolor", CreateColor(FillerColor));
        Dict.Add("readonly", ReadOnly);
        Dict.Add("numericonly", NumericOnly);
        Dict.Add("defaultnumber", (long)DefaultNumericValue);
        Dict.Add("allowminus", AllowMinusSigns);
        Dict.Add("showdisabled", ShowDisabledText);
        Dict.Add("deselectonenter", DeselectOnEnterPressed);
        Dict.Add("popupstyle", PopupStyle);
        Dict.Add("enabled", Enabled);
    }

    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignTextBox Box = (DesignTextBox) Widget;
        Box.SetText(Text);
        Box.SetTextX(TextX);
        Box.SetTextY(TextY);
        Box.SetCaretY(CaretY);
        if (CaretHeight != null) Box.SetCaretHeight(CaretHeight);
        Box.SetFont(Font);
        Box.SetTextColor(TextColor);
        Box.SetDisabledTextColor(DisabledTextColor);
        Box.SetCaretColor(CaretColor);
        Box.SetFillerColor(FillerColor);
        Box.SetReadOnly(ReadOnly);
        Box.SetNumericOnly(NumericOnly);
        Box.SetDefaultNumericValue(DefaultNumericValue);
        Box.SetAllowMinusSigns(AllowMinusSigns);
        Box.SetShowDisabledText(ShowDisabledText);
        Box.SetDeselectOnEnterPressed(DeselectOnEnterPressed);
        Box.SetPopupStyle(PopupStyle);
        Box.SetEnabled(Enabled);
    }

    public override void WriteCode(CodeExporter CE)
    {
        base.WriteCode(CE);
        if (NumericOnly) CE.WriteCode($"SetNumericOnly(true);");
        if (DefaultNumericValue != 0) CE.WriteCode($"SetDefaultNumericValue({DefaultNumericValue});");
        if (!AllowMinusSigns) CE.WriteCode($"SetAllowMinusSigns(false);");
        CE.WriteCode($"SetFont({GetFontCode(Font)});");
        if (!TextColor.Equals(Color.WHITE)) CE.WriteCode($"SetTextColor({GetColorCode(TextColor)});");
        if (ShowDisabledText) CE.WriteCode($"SetShowDisabledText(true);");
        if (ShowDisabledText && !DisabledTextColor.Equals(new Color(141, 151, 163))) CE.WriteCode($"SetDisabledTextColor({GetColorCode(DisabledTextColor)});");
        if (ReadOnly) CE.WriteCode("SetReadOnly(true);");
        if (!Enabled) CE.WriteCode("SetEnabled(false);");
        if (!PopupStyle) CE.WriteCode("SetPopupStyle(false);");
        if (!DeselectOnEnterPressed) CE.WriteCode("SetDeselectOnEnterPressed(false);");
        if (TextX != 0) CE.WriteCode($"SetTextX({TextX});");
        if (TextY != 0) CE.WriteCode($"SetTextY({TextY});");
        if (CaretY != 2) CE.WriteCode($"SetCaretY({CaretY});");
        if (CaretHeight != null) CE.WriteCode($"SetCaretHeight({CaretHeight});");
        if (!CaretColor.Equals(Color.WHITE)) CE.WriteCode($"SetCaretColor({GetColorCode(CaretColor)});");
        if (!FillerColor.Equals(new Color(0, 120, 215))) CE.WriteCode($"SetFillerColor({GetColorCode(FillerColor)});");
        if (!string.IsNullOrEmpty(Text)) CE.WriteCode($"SetText(\"{Text}\");");
    }
}

public class NumericBoxData : WidgetData
{
    public override string ClassName => "NumericBox";

    public int Value;
    public int MinValue;
    public int MaxValue;
    public int Increment;
    public bool Enabled;

    public NumericBoxData(DesignNumericBox w) : base(w)
    {
        this.Value = w.Value;
        this.MinValue = w.MinValue;
        this.MaxValue = w.MaxValue;
        this.Increment = w.Increment;
        this.Enabled = w.Enabled;
    }

    public NumericBoxData(Dictionary<string, object> Data) : base(Data)
    {
        this.Value = (int)(long)Data["value"];
        this.MinValue = (int)(long)Data["minvalue"];
        this.MaxValue = (int)(long)Data["maxvalue"];
        this.Increment = (int)(long)Data["increment"];
        this.Enabled = (bool)Data["enabled"];
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("value", (long)Value);
        Dict.Add("minvalue", (long)MinValue);
        Dict.Add("maxvalue", (long)MaxValue);
        Dict.Add("increment", (long)Increment);
        Dict.Add("enabled", Enabled);
    }

    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignNumericBox Box = (DesignNumericBox) Widget;
        Box.SetMinValue(MinValue);
        Box.SetMaxValue(MaxValue);
        Box.SetValue(Value);
        Box.SetIncrement(Increment);
        Box.SetEnabled(Enabled);
    }

    public override void WriteCode(CodeExporter CE)
    {
        base.WriteCode(CE);
        if (Value != 0) CE.WriteCode($"SetValue({Value});");
        if (MinValue != -999999) CE.WriteCode($"SetMinValue({MinValue});");
        if (MaxValue != 999999) CE.WriteCode($"SetMaxValue({MaxValue});");
        if (Increment != 1) CE.WriteCode($"SetIncrement({Increment});");
        if (!Enabled) CE.WriteCode("SetEnabled(false);");
    }
}

public class CheckBoxData : WidgetData
{
    public override string ClassName => "CheckBox";

    public string Text;
    public bool Checked;
    public Font Font;
    public bool Mirrored;
    public bool Enabled;

    public CheckBoxData(DesignCheckBox w) : base(w)
    {
        this.Size = null;
        this.Text = w.Text;
        this.Checked = w.Checked;
        this.Font = w.Font;
        this.Mirrored = w.Mirrored;
        this.Enabled = w.Enabled;
    }

    public CheckBoxData(Dictionary<string, object> Data) : base(Data)
    {
        this.Text = (string)Data["text"];
        this.Checked = (bool)Data["checked"];
        this.Font = Font.Get((string)ValueFromPath(Data, "font", "name"), (int)(long)ValueFromPath(Data, "font", "size"));
        this.Mirrored = (bool)Data["mirrored"];
        this.Enabled = (bool)Data["enabled"];
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("text", Text);
        Dict.Add("checked", Checked);
        Dict.Add("font", CreateDict(("name", Font.Name.Replace('\\', '/')), ("size", (long)Font.Size)));
        Dict.Add("mirrored", Mirrored);
        Dict.Add("enabled", Enabled);
    }

    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignCheckBox Box = (DesignCheckBox) Widget;
        Box.SetText(Text);
        Box.SetChecked(Checked);
        Box.SetFont(Font);
        Box.SetMirrored(Mirrored);
        Box.SetEnabled(Enabled);
    }

    public override void WriteCode(CodeExporter CE)
    {
        base.WriteCode(CE);
        if (!string.IsNullOrEmpty(Text)) CE.WriteCode($"SetText(\"{Text}\");");
        if (Checked) CE.WriteCode($"SetChecked(true);");
        CE.WriteCode($"SetFont({GetFontCode(Font)});");
        if (Mirrored) CE.WriteCode($"SetMirrored(true);");
        if (!Enabled) CE.WriteCode("SetEnabled(false);");
    }
}

public class RadioBoxData : WidgetData
{
    public override string ClassName => "RadioBox";

    public string Text;
    public bool Checked;
    public Font Font;
    public bool Enabled;

    public RadioBoxData(DesignRadioBox w) : base(w)
    {
        this.Size = null;
        this.Text = w.Text;
        this.Checked = w.Checked;
        this.Font = w.Font;
        this.Enabled = w.Enabled;
    }

    public RadioBoxData(Dictionary<string, object> Data) : base(Data)
    {
        this.Text = (string)Data["text"];
        this.Checked = (bool)Data["checked"];
        this.Font = Font.Get((string)ValueFromPath(Data, "font", "name"), (int)(long)ValueFromPath(Data, "font", "size"));
        this.Enabled = (bool)Data["enabled"];
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("text", Text);
        Dict.Add("checked", Checked);
        Dict.Add("font", CreateDict(("name", Font.Name.Replace('\\', '/')), ("size", (long)Font.Size)));
        Dict.Add("enabled", Enabled);
    }

    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignRadioBox Box = (DesignRadioBox) Widget;
        Box.SetText(Text);
        Box.SetChecked(Checked);
        Box.SetFont(Font);
        Box.SetEnabled(Enabled);
    }

    public override void WriteCode(CodeExporter CE)
    {
        base.WriteCode(CE);
        if (!string.IsNullOrEmpty(Text)) CE.WriteCode($"SetText(\"{Text}\");");
        if (Checked) CE.WriteCode($"SetChecked(true);");
        CE.WriteCode($"SetFont({GetFontCode(Font)});");
        if (!Enabled) CE.WriteCode("SetEnabled(false);");
    }
}

public class DropdownBoxData : WidgetData
{
    public override string ClassName => "DropdownBox";

    public List<string> Items;
    public int SelectedIndex;
    public bool ReadOnly;
    public bool Enabled;

    public DropdownBoxData(DesignDropdownBox w) : base(w)
    {
        this.Items = w.Items.Select(x => x.Name).ToList();
        this.SelectedIndex = w.SelectedIndex;
        this.ReadOnly = w.ReadOnly;
        this.Enabled = w.Enabled;
    }

    public DropdownBoxData(Dictionary<string, object> Data) : base(Data)
    {
        if (Data["items"] is List<string>) this.Items = ((List<string>) Data["items"]);
        else this.Items = ((List<object>) Data["items"]).Select(o => o.ToString()).ToList();
        this.SelectedIndex = (int)(long)Data["selectedindex"];
        this.ReadOnly = (bool)Data["readonly"];
        this.Enabled = (bool)Data["enabled"];
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("items", Items);
        Dict.Add("selectedindex", (long)SelectedIndex);
        Dict.Add("readonly", ReadOnly);
        Dict.Add("enabled", Enabled);
    }

    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignDropdownBox b = (DesignDropdownBox) Widget;
        b.SetItems(Items.Select(x => new ListItem(x)).ToList());
        b.SetSelectedIndex(SelectedIndex);
        b.SetReadOnly(ReadOnly);
        b.SetEnabled(Enabled);
    }

    public override void WriteCode(CodeExporter CE)
    {
        base.WriteCode(CE);
        if (Items.Count != 0)
        {
            CE.WriteCode("SetItems(new List<ListItem>()");
            CE.WriteCode("{", false);
            for (int i = 0; i < Items.Count; i++)
            {
                CE.WriteCode($"\tnew ListItem(\"{Items[i]}\"){(i == Items.Count - 1 ? "" : ",")}", false);
            }
            CE.WriteCode("});", false);
        }
        if (SelectedIndex != -1) CE.WriteCode($"SetSelectedIndex({SelectedIndex});");
        if (!ReadOnly) CE.WriteCode($"SetReadOnly(false);");
        if (!Enabled) CE.WriteCode("SetEnabled(false);");
    }
}

public class BrowserBoxData : WidgetData
{
    public override string ClassName => "BrowserBox";

    public string Text;
    public Font Font;
    public Color TextColor;
    public bool ReadOnly;
    public bool Enabled;

    public BrowserBoxData(DesignBrowserBox w) : base(w)
    {
        this.Text = w.Text;
        this.Font = w.Font;
        this.TextColor = w.TextColor;
        this.ReadOnly = w.ReadOnly;
        this.Enabled = w.Enabled;
    }
    
    public BrowserBoxData(Dictionary<string, object> Data) : base(Data)
    {
        this.Text = (string)Data["text"];
        this.Font = Font.Get((string)ValueFromPath(Data, "font", "name"), (int)(long)ValueFromPath(Data, "font", "size"));
        this.TextColor = ColorFromPath(Data, "textcolor");
        this.ReadOnly = (bool)Data["readonly"];
        this.Enabled = (bool)Data["enabled"];
    }
    
    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("text", Text);
        Dict.Add("font", CreateDict(("name", Font.Name.Replace('\\', '/')), ("size", (long)Font.Size)));
        Dict.Add("textcolor", CreateColor(TextColor));
        Dict.Add("readonly", ReadOnly);
        Dict.Add("enabled", Enabled);
    }
    
    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignBrowserBox b = (DesignBrowserBox) Widget;
        b.SetText(Text);
        b.SetFont(Font);
        b.SetTextColor(TextColor);
        b.SetReadOnly(ReadOnly);
        b.SetEnabled(Enabled);
    }
    
    public override void WriteCode(CodeExporter CE)
    {
        base.WriteCode(CE);
        if (!string.IsNullOrEmpty(Text)) CE.WriteCode($"SetText(\"{Text}\");");
        CE.WriteCode($"SetFont({GetFontCode(Font)});");
        if (!TextColor.Equals(Color.WHITE)) CE.WriteCode($"SetTextColor({GetColorCode(TextColor)});");
        if (!ReadOnly) CE.WriteCode($"SetReadOnly(false);");
        if (!Enabled) CE.WriteCode("SetEnabled(false);");
    }
}

public class NumericSliderData : WidgetData
{
    public override string ClassName => "NumericSlider";

    public int Value;
    public int MinValue;
    public int MaxValue;
    public List<int> SnapValues;
    public int SnapStrength;
    public bool Enabled;

    public NumericSliderData(DesignNumericSlider w) : base(w)
    {
        this.Value = w.Value;
        this.MinValue = w.MinValue;
        this.MaxValue = w.MaxValue;
        this.SnapValues = w.SnapValues.Select(e => e.Value).ToList();
        this.SnapStrength = w.SnapStrength;
        this.Enabled = w.Enabled;
    }

    public NumericSliderData(Dictionary<string, object> Data) : base(Data)
    {
        this.Value = (int)(long)Data["value"];
        this.MinValue = (int)(long)Data["minvalue"];
        this.MaxValue = (int)(long)Data["maxvalue"];
        if (Data["snapvalues"] is List<int>) this.SnapValues = ((List<int>) Data["snapvalues"]);
        else this.SnapValues = ((List<object>) Data["snapvalues"]).Select(o => Convert.ToInt32(o.ToString())).ToList();
        this.SnapStrength = (int)(long)Data["snapstrength"];
        this.Enabled = (bool)Data["enabled"];
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("value", (long)Value);
        Dict.Add("minvalue", (long)MinValue);
        Dict.Add("maxvalue", (long)MaxValue);
        Dict.Add("snapvalues", SnapValues);
        Dict.Add("snapstrength", (long)SnapStrength);
        Dict.Add("enabled", Enabled);
    }

    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignNumericSlider b = (DesignNumericSlider) Widget;
        b.SetMinimumValue(MinValue);
        b.SetMaximumValue(MaxValue);
        b.SetValue(Value);
        b.SetSnapValues(SnapValues);
        b.SetSnapStrength(SnapStrength);
        b.SetEnabled(Enabled);
    }

    public override void WriteCode(CodeExporter CE)
    {
        base.WriteCode(CE);
        if (MinValue != 0) CE.WriteCode($"SetMinimumValue({MinValue});");
        if (MaxValue != 100) CE.WriteCode($"SetMaximumValue({MaxValue});");
        if (Value != 50) CE.WriteCode($"SetValue({Value});");
        if (SnapStrength != 4) CE.WriteCode($"SetSnapStrength({SnapStrength});");
        if (SnapValues.Count != 5 || SnapValues[0] != 0 || SnapValues[1] != 25 || SnapValues[2] != 50 || SnapValues[3] != 75 || SnapValues[4] != 100)
        {
            string Code = "SetSnapValues(";
            for (int i = 0; i < SnapValues.Count; i++)
            {
                Code += SnapValues[i].ToString();
                if (i != SnapValues.Count - 1) Code += ", ";
            }
            Code += ");";
            CE.WriteCode(Code);
        }
    }
}