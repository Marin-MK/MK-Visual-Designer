using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner.Undo;

public class DockingPositionUndoAction : BaseUndoAction
{
    bool OldBottomDocked;
    bool OldRightDocked;
    bool NewBottomDocked;
    bool NewRightDocked;

    public DockingPositionUndoAction(DesignWidget Widget, List<BaseUndoAction>? OtherActions) : base(Widget, OtherActions) { }

    public static DockingPositionUndoAction Create(DesignWidget Widget, bool OldBottomDocked, bool OldRightDocked, bool NewBottomDocked, bool NewRightDocked, List<BaseUndoAction>? OtherActions = null)
    {
        DockingPositionUndoAction a = new DockingPositionUndoAction(Widget, OtherActions);
        a.OldBottomDocked = OldBottomDocked;
        a.OldRightDocked = OldRightDocked;
        a.NewBottomDocked = NewBottomDocked;
        a.NewRightDocked = NewRightDocked;
        return a;
    }

    public static void Register(DesignWidget Widget, bool OldBottomDocked, bool OldRightDocked, bool NewBottomDocked, bool NewRightDocked, List<BaseUndoAction>? OtherActions = null)
    {
        DockingPositionUndoAction a = Create(Widget, OldBottomDocked, OldRightDocked, NewBottomDocked, NewRightDocked, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Widget.SetBottomDocked(IsRedo ? NewBottomDocked : OldBottomDocked);
        Widget.SetRightDocked(IsRedo ? NewRightDocked : OldRightDocked);
        return true;
    }
}
