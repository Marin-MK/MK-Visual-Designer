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
    public bool IsSavedState = false;

    protected string WidgetName;
    protected bool RefreshParameters;
    protected DesignWidget Widget => Program.DesignWindow.GetWidgetByName(WidgetName);

    public BaseUndoAction(DesignWidget Widget, bool RefreshParameters, List<BaseUndoAction>? OtherActions)
    {
        this.WidgetName = Widget.Name;
        this.RefreshParameters = RefreshParameters;
        this.OtherActions = OtherActions ?? new List<BaseUndoAction>();
    }

    public void Register()
    {
        Program.UndoList.Add(this);
        Program.RedoList.Clear();
        Program.UnsavedChanges = true;
        if (!Program.MainWindow.Text.EndsWith("*")) Program.MainWindow.SetText(Program.MainWindow.Text + "*");
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
        bool OldMayRefresh = Widget?.MayRefresh ?? true;
        if (Widget != null) Widget.MayRefresh = false;
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
        if (RefreshParameters) Program.ParameterPanel.Refresh();
        if (Widget != null) Widget.MayRefresh = OldMayRefresh;
    }
}
