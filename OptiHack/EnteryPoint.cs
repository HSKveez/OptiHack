using Robust.Client.Input;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;

public sealed class EntryPoint : GameShared
{
    public override void PostInit()
    {
        MarseyLogger.Debug("Building graph...");
        IoCManager.BuildGraph();
        MarseyLogger.Debug("Graph success built!");
        IoCManager.InjectDependencies<EntryPoint>(this);
        KeyBindingRegistration keyBindingRegistration = new KeyBindingRegistration
        {
            Function = "optihack.overlay",
            BaseKey = Keyboard.Key.F2,
            Type = KeyBindingType.Command
        };
        this._inputManager.RegisterBinding(ref keyBindingRegistration, false, false);
        MarseyLogger.Debug("Setup context.");
    }
    [Dependency] private readonly IInputManager _inputManager = null;
}

