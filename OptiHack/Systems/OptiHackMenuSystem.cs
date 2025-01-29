using OptiHack.UI;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace OptiHack.Systems;

public class OptiHackMenuSystem : EntitySystem
{
    private readonly IUserInterfaceManager _ui = IoCManager.Resolve<IUserInterfaceManager>();
    private OptiHackMenu _menu= null!;
    
    public void ToggleMenu()
    {
        if (!_menu.IsOpen)
            _menu.OpenCentered();
        else
            _menu.Close();
    }
    
    public override void Initialize()
    {
        _menu = _ui.CreateWindow<OptiHackMenu>();
        MarseyLogger.Info("OptiHackMenu init done.");
    }
    
    public override void Shutdown()
    {
        _menu.Close();
        _menu.Dispose();
    }

}