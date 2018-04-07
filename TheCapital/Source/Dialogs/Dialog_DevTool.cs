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
                } else if (Find.VisibleMap != null)
                {
                    // Options visible on generated map
                    DoListingItems_Map();
                }
            }
            else
            {
                // Options visible in menus when not in play
            }
        }

        private void DoListingItems_Map()
        {
            Text.Font = GameFont.Tiny;
            DoLabel("Spawning");
            DebugAction("Spawn Transport Helicopter", () =>
            {
                var actorDef = DefDatabase<ThingDef>.GetNamed("TransportHelicopter");
                var thing = ThingMaker.MakeThing(actorDef);
                GenSpawn.Spawn(thing, UI.MouseCell(), Find.VisibleMap);
            });
        }
    }
}