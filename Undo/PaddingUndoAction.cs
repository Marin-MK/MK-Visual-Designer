using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner.Undo;

public class PaddingUndoAction : BaseUndoAction
{
    Padding OldPadding;
    Padding NewPadding;

    public PaddingUndoAction(DesignWidget Widget, List<BaseUndoAction>? OtherActions) : base(Widget, OtherActions) { }

    public static PaddingUndoAction Create(DesignWidget Widget, Padding OldPadding, Padding NewPadding, List<BaseUndoAction>? OtherActions = null)
    {
        PaddingUndoAction a = new PaddingUndoAction(Widget, OtherActions);
        a.OldPadding = OldPadding;
        a.NewPadding = NewPadding;
        return a;
    }

    public static void Register(DesignWidget Widget, Padding OldPadding, Padding NewPadding, List<BaseUndoAction>? OtherActions = null)
    {
        PaddingUndoAction a = Create(Widget, OldPadding, NewPadding, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Widget.SetPadding(IsRedo ? NewPadding : OldPadding);
        return true;
    }
}
