using System.Collections.Generic;
using Harmony;
using UnityEngine;
using Verse;

namespace TheCapital.Comps
{
    [StaticConstructorOnStartup]
    public class CompMotorized : ThingComp
    {
        private static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true);

        public void handleStartCommand()
        {
            parent.Destroy();
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var test = new Command_Action();
            test.defaultLabel = "Start";
            test.icon = LaunchCommandTex;
            test.action = () => handleStartCommand();
            return base.CompGetGizmosExtra().Add(test);
        }
    }
}