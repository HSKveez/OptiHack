using System.Reflection;
using HarmonyLib;

namespace OptiHack.Patches;

[HarmonyPatch]
internal static class CommandPermissionPatch
{
	[HarmonyTargetMethod]
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("Robust.Client.Console.ClientConsoleHost"), "CanExecute", null, null);
	}
	
	[HarmonyPostfix]
	private static void Postfix(ref bool __result)
	{
		__result = true;
	}
}