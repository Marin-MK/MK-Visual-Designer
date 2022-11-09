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

    public DockingUndoAction(DesignWidget Widget, bool RefreshParameters, List<BaseUndoAction>? OtherActions) : base(Widget, RefreshParameters, OtherActions) { }

    public static DockingUndoAction Create(DesignWidget Widget, bool OldHDocked, bool OldVDocked, bool NewHDocked, bool NewVDocked, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        DockingUndoAction a = new DockingUndoAction(Widget, RefreshParameters, OtherActions);
        a.OldHDocked = OldHDocked;
        a.OldVDocked = OldVDocked;
        a.NewHDocked = NewHDocked;
        a.NewVDocked = NewVDocked;
        return a;
    }

    public static void Register(DesignWidget Widget, bool OldHDocked, bool OldVDocked, bool NewHDocked, bool NewVDocked, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        DockingUndoAction a = Create(Widget, OldHDocked, OldVDocked, NewHDocked, NewVDocked, RefreshParameters, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Widget.SetHDocked(IsRedo ? NewHDocked : OldHDocked);
        Widget.SetVDocked(IsRedo ? NewVDocked : OldVDocked);
        return true;
    }
}
