using System.Reflection;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace TheCapital
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = HarmonyInstance.Create("com.github.harmony.rimworld.mod.TheCapital");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    
    public class TheCapitalMod : Mod
    {
        public TheCapitalMod(ModContentPack content) : base(content)
        {
      
        }
    }
}