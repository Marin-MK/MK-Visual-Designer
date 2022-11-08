using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner.Undo;

public class PositionUndoAction : BaseUndoAction
{
    Point OldPoint;
    Point NewPoint;

    public PositionUndoAction(DesignWidget Widget, List<BaseUndoAction>? OtherActions) : base(Widget, OtherActions) { }

    public static PositionUndoAction Create(DesignWidget Widget, Point OldPoint, Point NewPoint, List<BaseUndoAction>? OtherActions = null)
    {
        PositionUndoAction a = new PositionUndoAction(Widget, OtherActions);
        a.OldPoint = OldPoint;
        a.NewPoint = NewPoint;
        return a;
    }

    public static void Register(DesignWidget Widget, Point OldPoint, Point NewPoint, List<BaseUndoAction>? OtherActions = null)
    {
        PositionUndoAction a = Create(Widget, OldPoint, NewPoint, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Widget.SetPosition(IsRedo ? NewPoint : OldPoint);
        return true;
    }
}
