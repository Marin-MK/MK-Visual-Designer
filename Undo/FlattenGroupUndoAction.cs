namespace VisualDesigner.Undo;

public class FlattenGroupUndoAction : BaseUndoAction
{
    string ParentName;
    Point Position;
    Size Size;

    public FlattenGroupUndoAction(DesignWidget Widget, bool RefreshParameters, List<BaseUndoAction>? OtherActions) : base(Widget, RefreshParameters, OtherActions) { }

    public static FlattenGroupUndoAction Create(DesignWidget Widget, string ParentName, Point Position, Size Size, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        FlattenGroupUndoAction a = new FlattenGroupUndoAction(Widget, RefreshParameters, OtherActions);
        a.ParentName = ParentName;
        a.Position = Position;
        a.Size = Size;
        return a;
    }

    public static void Register(DesignWidget Widget, string ParentName, Point Position, Size Size, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        FlattenGroupUndoAction a = Create(Widget, ParentName, Position, Size, RefreshParameters, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        if (IsRedo)
        {
            // Re-create container
            List<DesignWidget> WidgetList = new List<DesignWidget>();
            OtherActions.ForEach(a =>
            {
                FlattenUndoAction f = (FlattenUndoAction) a;
                WidgetList.Add(f.Widget);
            });
            DesignWidget Container = Program.DesignWindow.Flatten(WidgetList, ParentName, false);
            Container.SetPosition(Position);
            Container.SetSize(Size);
            Container.Name = WidgetName;
        }
        else
        {
            // Move widgets and change parents
            OtherActions.ForEach(a =>
            {
                FlattenUndoAction f = (FlattenUndoAction) a;
                f.Trigger(false);
            });
            // Container should no longer have any widgets, so we dispose it
            if (Widget.Selected) Widget.Deselect();
            Widget.Dispose();
        }
        return true;
    }
}
