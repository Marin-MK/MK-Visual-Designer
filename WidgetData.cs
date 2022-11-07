using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner;

public class WidgetData
{
    public virtual string Type => "widget";
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
        this.Position = new Point(0, 0);
        this.Position.X = (int) (long) ValueFromPath(Data, "position", "x");
        this.Position.Y = (int) (long) ValueFromPath(Data, "position", "y");
        this.Size = new Size(0, 0);
        this.Size.Width = (int) (long) ValueFromPath(Data, "size", "width");
        this.Size.Height = (int) (long) ValueFromPath(Data, "size", "height");
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
        List<object> objList = (List<object>) Data["widgets"];
        List<Dictionary<string, object>> WidgetData = objList.Select(o => (Dictionary<string, object>) o).ToList();
        this.Widgets = WidgetData.Select(dict => Program.DictToData(dict)).ToList();
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
        Widget.Name = this.Name;
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

    public Dictionary<string, object> ConvertToDict()
    {
        Dictionary<string, object> Dict = new Dictionary<string, object>();
        Dict.Add("type", this.Type);
        Dict.Add("name", Name);
        Dict.Add("position", CreateDict(("x", Position.X), ("y", Position.Y)));
        Dict.Add("size", CreateDict(("width", Size.Width), ("height", Size.Height)));
        Dict.Add("padding", CreateDict(("left", Padding.Left), ("up", Padding.Up), ("right", Padding.Right), ("down", Padding.Down)));
        Dict.Add("bottomdocked", BottomDocked);
        Dict.Add("rightdocked", RightDocked);
        Dict.Add("bgcolor", CreateDict(("red", BackgroundColor.Red), ("green", BackgroundColor.Green), ("blue", BackgroundColor.Blue), ("alpha", BackgroundColor.Alpha)));
        Dict.Add("widgets", Widgets.Select(w => w.ConvertToDict()));
        AddToDict(Dict);
        return Dict;
    }
}

public class ButtonWidgetData : WidgetData
{
    public override string Type => "button";
    public string Text;

    public ButtonWidgetData(DesignButton w) : base(w)
    {
        this.Text = w.Text;
    }

    public ButtonWidgetData(Dictionary<string, object> d) : base(d)
    {
        this.Text = (string) d["text"];
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("text", Text);
    }

    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignButton Button = (DesignButton) Widget;
        Button.SetText(this.Text);
    }
}

public class LabelWidgetData : WidgetData
{
    public override string Type => "label";
    public string Text;

    public LabelWidgetData(DesignLabel w) : base(w)
    {
        this.Text = w.Text;
    }

    public LabelWidgetData(Dictionary<string, object> d) : base(d)
    {
        this.Text = (string) d["text"];
    }

    public override void AddToDict(Dictionary<string, object> Dict)
    {
        Dict.Add("text", Text);
    }

    public override void SetWidget(DesignWidget Widget)
    {
        base.SetWidget(Widget);
        DesignLabel Label = (DesignLabel) Widget;
        Label.SetText(this.Text);
    }
}