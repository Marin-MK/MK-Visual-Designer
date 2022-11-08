using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner.Undo;

public class DockingUndoAction : BaseUndoAction
{
    bool OldHDocked;
    bool OldVDocked;
    bool NewHDocked;
    bool NewVDocked;

    public DockingUndoAction(DesignWidget Widget, List<BaseUndoAction>? OtherActions) : base(Widget, OtherActions) { }

    public static DockingUndoAction Create(DesignWidget Widget, bool OldHDocked, bool OldVDocked, bool NewHDocked, bool NewVDocked, List<BaseUndoAction>? OtherActions = null)
    {
        DockingUndoAction a = new DockingUndoAction(Widget, OtherActions);
        a.OldHDocked = OldHDocked;
        a.OldVDocked = OldVDocked;
        a.NewHDocked = NewHDocked;
        a.NewVDocked = NewVDocked;
        return a;
    }

    public static void Register(DesignWidget Widget, bool OldHDocked, bool OldVDocked, bool NewHDocked, bool NewVDocked, List<BaseUndoAction>? OtherActions = null)
    {
        DockingUndoAction a = Create(Widget, OldHDocked, OldVDocked, NewHDocked, NewVDocked, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Widget.SetHDocked(IsRedo ? NewHDocked : OldHDocked);
        Widget.SetVDocked(IsRedo ? NewVDocked : OldVDocked);
        return true;
    }
}
