using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner.Undo;

public class BaseUndoAction
{
    public virtual string Title => GetType().Name;
    public List<BaseUndoAction> OtherActions;

    protected DesignWidget Widget;

    public BaseUndoAction(DesignWidget Widget, List<BaseUndoAction>? OtherActions)
    {
        this.Widget = Widget;
        this.OtherActions = OtherActions ?? new List<BaseUndoAction>();
    }

    public void Register()
    {
        Program.UndoList.Add(this);
        Program.RedoList.Clear();
    }

    public virtual bool Trigger(bool IsRedo)
    {
        return true;
    }

    public void RevertTo(bool IsRedo)
    {
        List<BaseUndoAction> ListA = IsRedo ? Program.RedoList : Program.UndoList;
        List<BaseUndoAction> ListB = IsRedo ? Program.UndoList : Program.RedoList;
        int Index = ListA.IndexOf(this);
        for (int i = ListA.Count - 1; i >= Index; i--)
        {
            BaseUndoAction action = ListA[i];
            bool success = action.Trigger(IsRedo);
            action.OtherActions.ForEach(a => success &= a.Trigger(IsRedo));
            if (success)
            {
                ListB.Add(action);
                ListA.RemoveAt(i);
            }
        }
    }
}
