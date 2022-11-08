using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner.Undo;

public class NameUndoAction : BaseUndoAction
{
    string OldName;
    string NewName;

    public NameUndoAction(DesignWidget Widget, List<BaseUndoAction>? OtherActions) : base(Widget, OtherActions) { }

    public static NameUndoAction Create(DesignWidget Widget, string OldName, string NewName, List<BaseUndoAction>? OtherActions = null)
    {
        NameUndoAction a = new NameUndoAction(Widget, OtherActions);
        a.OldName = OldName;
        a.NewName = NewName;
        return a;
    }

    public static void Register(DesignWidget Widget, string OldName, string NewName, List<BaseUndoAction>? OtherActions = null)
    {
        NameUndoAction a = Create(Widget, OldName, NewName, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Widget.Name = IsRedo ? NewName : OldName;
        if (Program.ParameterPanel.Widget == Widget) Program.ParameterPanel.Refresh();
        return true;
    }
}
