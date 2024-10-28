using Content.Shared.Administration;
using Robust.Shared.Console;
using OptiHack.Systems;

namespace OptiHack.Commands;

[AnyCommand]
public class EntityScannerCommand : IConsoleCommand
{
    private readonly EntityScannerSystem _entityScanner = new();

    public string Command => "optihack.scan";
    public string Description => "optihack.scan {uid} {slotId}";
    public string Help => "";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1)
        {
            shell.WriteLine("invalid args");
            return;
        }

        var uid = _entityScanner.TargetUidParser(shell, args[0]);
        if (args.Length == 1)
        {
            _entityScanner.ScanAllSlots(shell,uid);
        }
        else
        {
            _entityScanner.ScanSlot(shell, uid, args[1]);
        }
    }
}