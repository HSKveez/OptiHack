using Content.Client.ContextMenu.UI;
using HarmonyLib;
using Robust.Shared.GameObjects;

namespace OptiHack.Patches
{
    [HarmonyPatch(typeof(EntityMenuElement))]
    [HarmonyPatch("GetEntityDescription")]
    internal class EntityMenuPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(EntityUid entity, EntityMenuElement __instance, ref string __result)
        {
            string text = (AccessTools.Method(typeof(EntityMenuElement), "GetEntityDescriptionAdmin", null, null).Invoke(__instance, new object[]
            {
                entity
            }) as string)!;
            if (text == null)
            {
                throw new InvalidOperationException();
            }
            __result = text;
            return false;
        }
    }
}