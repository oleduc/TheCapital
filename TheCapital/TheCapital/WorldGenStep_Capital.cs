using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheCapital
{
    public class WorldGenStep_Capital : WorldGenStep
    {
        public override void GenerateFresh(string seed)
        {
            var faction = FactionUtility.DefaultFactionFrom(DefDatabase<FactionDef>.GetNamed("Capital"));
            var capitalWorldDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalCenter");
            var obj = WorldObjectMaker.MakeWorldObject(capitalWorldDef);
            obj.SetFaction(faction);
            obj.Tile = TileFinder.RandomFactionBaseTileFor(null);
            Log.Message("FOCKEN SHIET:");
            Log.Message(obj.Tile.ToString());
            Log.Message(obj.def.texture);
            Find.WorldObjects.Add(obj);
            WorldGenStep_Features
        }
    }
}