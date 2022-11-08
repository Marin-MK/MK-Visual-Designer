using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VisualDesigner.Undo;

public class GenericUndoAction<T> : BaseUndoAction
{
    string SetMethodName;
    T OldValue;
    T NewValue;

    public GenericUndoAction(DesignWidget Widget, List<BaseUndoAction>? OtherActions) : base(Widget, OtherActions) { }

    public static GenericUndoAction<T> Create(DesignWidget Widget, string SetMethodName, T OldValue, T NewValue, List<BaseUndoAction>? OtherActions = null)
    {
        GenericUndoAction<T> a = new GenericUndoAction<T>(Widget, OtherActions);
        a.SetMethodName = SetMethodName;
        a.OldValue = OldValue;
        a.NewValue = NewValue;
        return a;
    }

    public static void Register(DesignWidget Widget, string SetMethodName, T OldValue, T NewValue, List<BaseUndoAction>? OtherActions = null)
    {
        GenericUndoAction<T> a = Create(Widget, SetMethodName, OldValue, NewValue, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Type type = Widget.GetType();
        MethodInfo? method = type.GetMethod(SetMethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, new Type[] { typeof(T) });
        if (method == null) throw new Exception($"No SetText method found on type {type.Name}");
        method.Invoke(Widget, new object[] { IsRedo ? NewValue : OldValue });
        return true;
    }
}
