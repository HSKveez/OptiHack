using Content.Shared.Administration;
using Robust.Shared.Console;

namespace OptiHack.Commands
{
    [AnyCommand]
    public class CheatMenuCommand : IConsoleCommand
    {
        public string Command => "optihack.menu";
        
        public string Description => "Toggles the optihack menu";
        
        public string Help => "optihack.menu";
        
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            shell.WriteLine("TODO");
        }
    }
}