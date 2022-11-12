using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VisualDesigner;

public class WidgetData
{
    public virtual string Type => "container";
    public string ParentName; // Used only in copy/paste
    public string Name;
    public Point Position;
    public Size Size;
    public Padding Padding;
    public bool BottomDocked;
    public bool RightDocked;
    public Color BackgroundColor;
    public List<WidgetData> Widgets = new List<WidgetData>();

    public WidgetData(DesignWidget Widget)
    {
        this.Name = Widget.Name;
        this.ParentName = Widget.Parent is DesignWidget ? ((DesignWidget) Widget.Parent).Name : null;
        this.Position = new Point(Widget.Position.X, Widget.Position.Y);
        this.Size = new Size(Widget.Size.Width, Widget.Size.Height);
        this.Padding = new Padding(Widget.Padding.Left, Widget.Padding.Up, Widget.Padding.Right, Widget.Padding.Down);
        this.BottomDocked = Widget.BottomDocked;
        this.RightDocked = Widget.RightDocked;
        this.BackgroundColor = new Color(Widget.BackgroundColor.Red, Widget.BackgroundColor.Green, Widget.BackgroundColor.Blue, Widget.BackgroundColor.Alpha);
        this.Widgets = Widget.Widgets.FindAll(w => w is DesignWidget).Select(w => Program.WidgetToData((DesignWidget) w)).ToList();
    }

    public WidgetData(Dictionary<string, object> Data)
    {
        this.Name = (string) Data["name"];
        this.Size = new Size(0, 0);
        this.Size.Width = (int) (long) ValueFromPath(Data, "size", "width");
        this.Size.Height = (int) (long) ValueFromPath(Data, "size", "height");
        List<Dictionary<string, object>> WidgetData = null;
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
        int l = (int) (long) ValueFromPath(Data, "padding", "left");
        int u = (int) (long) ValueFromPath(Data, "padding", "up");
        int r = (int) (long) ValueFromPath(Data, "padding", "right");
        int d = (int) (long) ValueFromPath(Data, "padding", "down");
        this.Padding = new Padding(l, u, r, d);
        this.BottomDocked = (bool) Data["bottomdocked"];
        this.RightDocked = (bool) Data["rightdocked"];
        byte rC = (byte) (long) ValueFromPath(Data, "bgcolor", "red");
        byte gC = (byte) (long) ValueFromPath(Data, "bgcolor", "green");
        byte bC = (byte) (long) ValueFromPath(Data, "bgcolor", "blue");
        byte aC = (byte) (long) ValueFromPath(Data, "bgcolor", "alpha");
        this.BackgroundColor = new Color(rC, gC, bC, aC);
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

    protected Dictionary<string, object> CreateDict(params (string Name, object Value)[] Values)
    {
        Dictionary<string, object> Dict = new Dictionary<string, object>();
        for (int i = 0; i < Values.Length; i++)
        {
            Dict.Add(Values[i].Name, Values[i].Value);
        }
        return Dict;
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
        Widget.SetSize(this.Size);
        Widget.SetPadding(this.Padding);
        Widget.SetBottomDocked(this.BottomDocked);
        Widget.SetRightDocked(this.RightDocked);
        Widget.SetBackgroundColor(this.BackgroundColor);
        this.Widgets.ForEach(data =>
        {
            Program.WidgetFromData(Widget, data);
        });
    }

    public virtual Dictionary<string, object> ConvertToDict()
    {
        Dictionary<string, object> Dict = new Dictionary<string, object>();
        Dict.Add("type", this.Type);
        Dict.Add("name", Name);
        Dict.Add("parentname", ParentName);
        Dict.Add("position", CreateDict(("x", (long) Position.X), ("y", (long) Position.Y)));
        Dict.Add("size", CreateDict(("width", (long) Size.Width), ("height", (long) Size.Height)));
        Dict.Add("padding", CreateDict(("left", (long) Padding.Left), ("up", (long) Padding.Up), ("right", (long) Padding.Right), ("down", (long) Padding.Down)));
        Dict.Add("bottomdocked", BottomDocked);
        Dict.Add("rightdocked", RightDocked);
        Dict.Add("bgcolor", CreateDict(("red", (long) BackgroundColor.Red), ("green", (long) BackgroundColor.Green), ("blue", (long) BackgroundColor.Blue), ("alpha", (long) BackgroundColor.Alpha)));
        Dict.Add("widgets", Widgets.Select(w => w.ConvertToDict()).ToList());
        AddToDict(Dict);
        return Dict;
    }
}

public class WindowData : WidgetData
{
    public override string Type => "window";
    public string Title;
    public bool Fullscreen;
    public bool IsPopup;

    public WindowData(DesignWindow w) : base(w)
    {
        this.Title = w.Title;
        this.Fullscreen = w.Fullscreen;
        this.IsPopup = w.IsPopup;
    }

    public WindowData(Dictionary<string, object> Data) : base(Data)
    {
        this.Title = (string) Data["title"];
        this.Fullscreen = (bool) Data["fullscreen"];
        this.IsPopup = (bool) Data["popup"];
    }

    public override Dictionary<string, object> ConvertToDict()
    {
        Dictionary<string, object> Dict = new Dictionary<string, object>();
        Dict.Add("type", this.Type);
        Dict.Add("name", Name);
        Dict.Add("size", CreateDict(("width", (long) Size.Width), ("height", (long) Size.Height)));
        Dict.Add("widgets", Widgets.Select(w => w.ConvertToDict()).ToList());
        AddToDict(Dict);
        return Dict;
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("title", this.Title);
        Dict.Add("fullscreen", this.Fullscreen);
        Dict.Add("popup", this.IsPopup);
    }

    public override void SetWidget(DesignWidget Widget)
    {
        throw new MethodNotSupportedException(Widget);
    }
}

public class ButtonWidgetData : WidgetData
{
    public override string Type => "button";
    public string Text;
    public Font Font;
    public Color TextColor;
    public bool LeftAlign;
    public int TextX;
    public bool Enabled;


    public ButtonWidgetData(DesignButton w) : base(w)
    {
        this.Text = w.Text;
        this.Font = w.Font;
        this.TextColor = w.TextColor;
        this.LeftAlign = w.LeftAlign;
        this.TextX = w.TextX;
        this.Enabled = w.Enabled;
    }

    public ButtonWidgetData(Dictionary<string, object> Data) : base(Data)
    {
        this.Text = (string) Data["text"];
        this.Font = Font.Get((string) ValueFromPath(Data, "font", "name"), (int) (long) ValueFromPath(Data, "font", "size"));
        byte rC = (byte) (long) ValueFromPath(Data, "textcolor", "red");
        byte gC = (byte) (long) ValueFromPath(Data, "textcolor", "green");
        byte bC = (byte) (long) ValueFromPath(Data, "textcolor", "blue");
        byte aC = (byte) (long) ValueFromPath(Data, "textcolor", "alpha");
        this.TextColor = new Color(rC, gC, bC, aC);
        this.LeftAlign = (bool) Data["leftalign"];
        this.TextX = (int) (long) Data["textx"];
        this.Enabled = (bool) Data["enabled"];
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("text", Text);
        Dict.Add("font", CreateDict(("name", Font.Name.Replace('\\', '/')), ("size", (long) Font.Size)));
        Dict.Add("textcolor", CreateDict(("red", (long) TextColor.Red), ("green", (long) TextColor.Green), ("blue", (long) TextColor.Blue), ("alpha", (long) TextColor.Alpha)));
        Dict.Add("leftalign", LeftAlign);
        Dict.Add("textx", (long) TextX);
        Dict.Add("enabled", Enabled);
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
    }
}

public class LabelWidgetData : WidgetData
{
    public override string Type => "label";
    public string Text;
    public Font Font;
    public Color TextColor;
    public int WidthLimit;
    public string LimitReplacementText;
    public DrawOptions DrawOptions;

    public LabelWidgetData(DesignLabel w) : base(w)
    {
        this.Text = w.Text;
        this.Font = w.Font;
        this.TextColor = w.TextColor;
        this.WidthLimit = w.WidthLimit;
        this.LimitReplacementText = w.LimitReplacementText;
        this.DrawOptions = w.DrawOptions;
    }

    public LabelWidgetData(Dictionary<string, object> Data) : base(Data)
    {
        this.Text = (string) Data["text"];
        this.Font = Font.Get((string) ValueFromPath(Data, "font", "name"), (int) (long) ValueFromPath(Data, "font", "size"));
        byte rC = (byte) (long) ValueFromPath(Data, "textcolor", "red");
        byte gC = (byte) (long) ValueFromPath(Data, "textcolor", "green");
        byte bC = (byte) (long) ValueFromPath(Data, "textcolor", "blue");
        byte aC = (byte) (long) ValueFromPath(Data, "textcolor", "alpha");
        this.TextColor = new Color(rC, gC, bC, aC);
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
        Dict.Add("textcolor", CreateDict(("red", (long) TextColor.Red), ("green", (long) TextColor.Green), ("blue", (long) TextColor.Blue), ("alpha", (long) TextColor.Alpha)));
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
}