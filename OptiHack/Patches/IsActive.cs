using System.Reflection;
using Content.Client.Administration.Managers;
using HarmonyLib;

namespace OptiHack.Patches;

[HarmonyPatch]
internal class IsActive
{
	[HarmonyTargetMethod]
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(AccessTools.TypeByName("Content.Client.Administration.Managers.ClientAdminManager"), "IsActive", null, null);
	}
	
	[HarmonyPrefix]
	private static bool PrefSkip(ref bool __result, ClientAdminManager __instance)
	{
		__result = true;
		return false;
	}
}