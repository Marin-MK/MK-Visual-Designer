namespace VisualDesigner.Undo;

public class DockingPositionUndoAction : BaseUndoAction
{
    bool OldBottomDocked;
    bool OldRightDocked;
    bool NewBottomDocked;
    bool NewRightDocked;

    public DockingPositionUndoAction(DesignWidget Widget, bool RefreshParameters, List<BaseUndoAction>? OtherActions) : base(Widget, RefreshParameters, OtherActions) { }

    public static DockingPositionUndoAction Create(DesignWidget Widget, bool OldBottomDocked, bool OldRightDocked, bool NewBottomDocked, bool NewRightDocked, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        DockingPositionUndoAction a = new DockingPositionUndoAction(Widget, RefreshParameters, OtherActions);
        a.OldBottomDocked = OldBottomDocked;
        a.OldRightDocked = OldRightDocked;
        a.NewBottomDocked = NewBottomDocked;
        a.NewRightDocked = NewRightDocked;
        return a;
    }

    public static void Register(DesignWidget Widget, bool OldBottomDocked, bool OldRightDocked, bool NewBottomDocked, bool NewRightDocked, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        DockingPositionUndoAction a = Create(Widget, OldBottomDocked, OldRightDocked, NewBottomDocked, NewRightDocked, RefreshParameters, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Widget.SetBottomDocked(IsRedo ? NewBottomDocked : OldBottomDocked);
        Widget.SetRightDocked(IsRedo ? NewRightDocked : OldRightDocked);
        return true;
    }
}
