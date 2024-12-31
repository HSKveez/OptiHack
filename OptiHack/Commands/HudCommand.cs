using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using OptiHack.Systems;

namespace OptiHack.Commands;

public class HudCommand : IConsoleCommand
{
    private readonly KiroshiSystem _hudSystem = new();
        
    public string Command => "optihack.hud";
    public string Description => "optihack.hud";
    public string Help => "";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var overlaySystem = EntitySystem.Get<OptiHackOverlaySystem>();
        overlaySystem.Enabled ^= true;
        
        _hudSystem.ToggleIcons();
    }
}