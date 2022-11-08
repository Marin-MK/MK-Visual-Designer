using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner.Undo;

public class BGColorUndoAction : BaseUndoAction
{
    Color OldColor;
    Color NewColor;

    public BGColorUndoAction(DesignWidget Widget, List<BaseUndoAction>? OtherActions) : base(Widget, OtherActions) { }

    public static BGColorUndoAction Create(DesignWidget Widget, Color OldColor, Color NewColor, List<BaseUndoAction>? OtherActions = null)
    {
        BGColorUndoAction a = new BGColorUndoAction(Widget, OtherActions);
        a.OldColor = OldColor;
        a.NewColor = NewColor;
        return a;
    }

    public static void Register(DesignWidget Widget, Color OldColor, Color NewColor, List<BaseUndoAction>? OtherActions = null)
    {
        BGColorUndoAction a = Create(Widget, OldColor, NewColor, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Widget.SetBackgroundColor(IsRedo ? NewColor : OldColor);
        return true;
    }
}
