using Content.Client.Administration.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace OptiHack.Commands
{
    [AnyCommand]
    public class OptiHackPlayerList : IConsoleCommand
    {
        // Token: 0x1700000C RID: 12
        // (get) Token: 0x0600007F RID: 127 RVA: 0x000039DE File Offset: 0x00001BDE
        public string Command => "optihack.players";

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x06000080 RID: 128 RVA: 0x000039E5 File Offset: 0x00001BE5
        public string Description => "Shows a player list";

        // Token: 0x1700000E RID: 14
        // (get) Token: 0x06000081 RID: 129 RVA: 0x000039DE File Offset: 0x00001BDE
        public string Help => "optihack.players";

        // Token: 0x06000082 RID: 130 RVA: 0x000039EC File Offset: 0x00001BEC
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            AdminSystem adminSystem = IoCManager.Resolve<IEntityManager>().System<AdminSystem>();
            foreach (PlayerInfo playerInfo in adminSystem.PlayerList)
            {
                shell.WriteLine(playerInfo.Username);
            }
        }
    }
}