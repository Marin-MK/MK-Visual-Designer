using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner.Undo;

public class FlattenUndoAction : BaseUndoAction
{
    Point OldPosition;
    Point NewPosition;
    string OldParentName;
    string NewParentName;
    int OldZIndex;
    int NewZIndex;

    public FlattenUndoAction(DesignWidget Widget, bool RefreshParameters, List<BaseUndoAction>? OtherActions) : base(Widget, RefreshParameters, OtherActions) { }

    public static FlattenUndoAction Create(DesignWidget Widget, Point OldPosition, Point NewPosition, string OldParentName, string NewParentName, int OldZIndex, int NewZIndex, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        FlattenUndoAction a = new FlattenUndoAction(Widget, RefreshParameters, OtherActions);
        a.OldPosition = OldPosition;
        a.NewPosition = NewPosition;
        a.OldParentName = OldParentName;
        a.NewParentName = NewParentName;
        a.OldZIndex = OldZIndex;
        a.NewZIndex = NewZIndex;
        return a;
    }

    public static void Register(DesignWidget Widget, Point OldPosition, Point NewPosition, string OldParentName, string NewParentName, int OldZIndex, int NewZIndex, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        FlattenUndoAction a = Create(Widget, OldPosition, NewPosition, OldParentName, NewParentName, OldZIndex, NewZIndex, RefreshParameters, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Widget.SetParent(IsRedo ? Program.DesignWindow.GetWidgetByName(NewParentName) : Program.DesignWindow.GetWidgetByName(OldParentName));
        Widget.SetPosition(IsRedo ? NewPosition : OldPosition);
        Widget.SetZIndex(IsRedo ? NewZIndex : OldZIndex);
        return true;
    }
}
