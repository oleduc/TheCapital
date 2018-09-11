using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheCapital.Dialogs
{
    public class Dialog_DebugTool : Dialog_DebugOptionLister
    {
        protected override void DoListingItems()
        {
            DoGap();
            
            DoLabel("The Capital - Development tool");
            
            if (Current.ProgramState == ProgramState.Playing)
            {
                if (WorldRendererUtility.WorldRenderedNow)
                {
                    // Options visible on world map
                } else if (Find.CurrentMap != null)
                {
                    // Options visible on generated map
                    DoListingItems_Map();
                }
            }
        }

        private void DoListingItems_Map()
        {
            Text.Font = GameFont.Tiny;
            DoLabel("Spawning");
            DebugAction("Vehicles", () =>
            {
                List<DebugMenuOption> debugMenuOptionList = new List<DebugMenuOption>();
                debugMenuOptionList.Add(new DebugMenuOption("Parked Transport Helicopter", DebugMenuOptionMode.Tool, () =>
                {
                    var actorDef = DefDatabase<ThingDef>.GetNamed("ParkedTransportHelicopter");
                    var thing = ThingMaker.MakeThing(actorDef);
                    GenSpawn.Spawn(thing, UI.MouseCell(), Find.CurrentMap);
                }));
                debugMenuOptionList.Add(new DebugMenuOption("Transport Helicopter", DebugMenuOptionMode.Tool, () =>
                {
                    var actorDef = DefDatabase<ThingDef>.GetNamed("TransportHelicopter");
                    var thing = ThingMaker.MakeThing(actorDef);
                    GenSpawn.Spawn(thing, UI.MouseCell(), Find.CurrentMap);
                }));
                Find.WindowStack.Add(new Dialog_DebugOptionListLister(debugMenuOptionList));
            });
        }
    }
}