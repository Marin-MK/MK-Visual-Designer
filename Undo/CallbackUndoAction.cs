namespace VisualDesigner.Undo;

public class CallbackUndoAction : BaseUndoAction
{
    Action<bool, DesignWidget> Callback;

    public CallbackUndoAction(DesignWidget Widget, bool RefreshParameters, List<BaseUndoAction>? OtherActions) : base(Widget, RefreshParameters, OtherActions) { }

    public static CallbackUndoAction Create(DesignWidget Widget, Action<bool, DesignWidget> Callback, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        CallbackUndoAction a = new CallbackUndoAction(Widget, RefreshParameters, OtherActions);
        a.Callback = Callback;
        return a;
    }

    public static void Register(DesignWidget Widget, Action<bool, DesignWidget> Callback, bool RefreshParameters, List<BaseUndoAction>? OtherActions = null)
    {
        CallbackUndoAction a = Create(Widget, Callback, RefreshParameters, OtherActions);
        a.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        Callback(IsRedo, Widget);
        return true;
    }
}
