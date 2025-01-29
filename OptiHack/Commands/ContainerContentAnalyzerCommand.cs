using Content.Shared.Administration;
using Robust.Shared.Console;
using OptiHack.Systems;

namespace OptiHack.Commands;

[AnyCommand]
public class ContainerContentAnalyzerCommand : IConsoleCommand
{
    private readonly ContainerContentAnalyzerSystem _containerContentAnalyzer = new();

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

        var uid = _containerContentAnalyzer.TargetUidParser(args[0]);
        if (args.Length == 1)
        {
            var output = _containerContentAnalyzer.ScanAllSlots(uid);
            WriteOutputToShell(shell, output);
        }
        else
        {
            var output = _containerContentAnalyzer.ScanSlot(uid, args[1]);
            WriteOutputToShell(shell, output);
        }
    }

    private void WriteOutputToShell(IConsoleShell shell, List<string> output)
    {
        foreach (var line in output)
        {
            shell.WriteLine(line);
        }
    }
}