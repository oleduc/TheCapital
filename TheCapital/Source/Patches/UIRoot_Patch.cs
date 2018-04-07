using System;
using Harmony;
using Verse;

namespace TheCapital.Patches
{
    [HarmonyPatch(typeof(UIRoot))]
    [HarmonyPatch("UIRootOnGUI")]
    [HarmonyPatch(new Type[0])]
    public class UIRoot_Patch
    {
        [HarmonyPostfix]
        private static void OnGUIHook() {
            Hooks.Instance.OnGUIHook();
        }
    }
}