using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner.Undo;

public class SizeUndoAction : BaseUndoAction
{
    Size OldSize;
    Size NewSize;

    public SizeUndoAction(DesignWidget Widget, List<BaseUndoAction>? OtherActions) : base(Widget, OtherActions) { }

    public static SizeUndoAction Create(DesignWidget Widget, Size OldSize, Size NewSize, List<BaseUndoAction>? OtherActions = null)
    {
        SizeUndoAction a = new SizeUndoAction(Widget, OtherActions);
        a.OldSize = OldSize;
        a.NewSize = NewSize;
        return a;
    }

    public static void Register(DesignWidget Widget, Size OldSize, Size NewSize, List<BaseUndoAction>? OtherActions = null)
    {
        SizeUndoAction a = Create(Widget, OldSize, NewSize, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Widget.SetSize(IsRedo ? NewSize : OldSize);
        return true;
    }
}
