namespace VisualDesigner.Undo;

public class NameUndoAction : BaseUndoAction
{
    string OldName;
    string NewName;

    public NameUndoAction(DesignWidget Widget, bool RefreshParameters, List<BaseUndoAction>? OtherActions) : base(Widget, RefreshParameters, OtherActions) { }

    public static NameUndoAction Create(DesignWidget Widget, string OldName, string NewName, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        NameUndoAction a = new NameUndoAction(Widget, RefreshParameters, OtherActions);
        a.OldName = OldName;
        a.NewName = NewName;
        return a;
    }

    public static void Register(DesignWidget Widget, string OldName, string NewName, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        NameUndoAction a = Create(Widget, OldName, NewName, RefreshParameters, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Widget.Name = IsRedo ? NewName : OldName;
        return true;
    }
}
