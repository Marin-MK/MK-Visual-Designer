namespace VisualDesigner.Undo;

public class CreateDeleteUndoAction : BaseUndoAction
{
    string ParentWidgetName;
    List<Dictionary<string, object>> WidgetData;
    bool Delete;

    public CreateDeleteUndoAction(DesignWidget Widget, bool RefreshParameters, List<BaseUndoAction>? OtherActions) : base(Widget, RefreshParameters, OtherActions) { }

    public static CreateDeleteUndoAction Create(DesignWidget Widget, string ParentWidgetName, List<Dictionary<string, object>> WidgetData, bool Delete, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        CreateDeleteUndoAction a = new CreateDeleteUndoAction(Widget, RefreshParameters, OtherActions);
        a.ParentWidgetName = ParentWidgetName;
        a.WidgetData = WidgetData;
        a.Delete = Delete;
        return a;
    }

    public static void Register(DesignWidget Widget, string ParentWidgetName, List<Dictionary<string, object>> WidgetData, bool Delete, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        CreateDeleteUndoAction a = Create(Widget, ParentWidgetName, WidgetData, Delete, RefreshParameters, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        // Redo creation: create
        // Redo deletion: delete
        // Undo creation: delete
        // Undo deletion: create
        bool Creation = IsRedo != Delete;
        DesignWidget? ParentWidget = Program.DesignWindow.GetWidgetByName(ParentWidgetName);
        if (ParentWidget == null) throw new Exception("Could not get parent widget for re-creation.");
        if (Creation)
        {
            List<DesignWidget> Widgets = Program.DictToWidgets(ParentWidget, WidgetData);
            Program.DesignWindow.DeselectAll();
            Widgets.ForEach(w => w.Select(true));
        }
        else
        {
            for (int i = 0; i < WidgetData.Count; i++)
            {
                string Name = (string) WidgetData[i]["name"];
                DesignWidget? DeleteWidget = ParentWidget.GetWidgetByName(Name);
                if (DeleteWidget == null) throw new Exception("Could not get widget for re-deletion.");
                if (Program.DesignWindow.SelectedWidgets.Contains(DeleteWidget)) Program.DesignWindow.SelectedWidgets.Remove(DeleteWidget);
                DeleteWidget.Dispose();
            }
            Program.DesignWindow.DeselectAll();
        }
        return true;
    }
}
