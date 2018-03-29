using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheCapital
{
    public class WorldGenStep_Capital : WorldGenStep
    {
        public override void GenerateFresh(string seed)
        {
            var neighboors = new List<int>();
            var faction = FactionUtility.DefaultFactionFrom(DefDatabase<FactionDef>.GetNamed("Capital"));
            
            // Get city object definitions
            var capitalWorldDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalCenter");
            var capitalFarmDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalFarm");
            var capitalFactoryDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalFactory");
            var capitalPowerPlantDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalPowerPlant");
            var capitalDefenseBaseDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalDefenseBase");

            var capitalCenter = WorldObjectMaker.MakeWorldObject(capitalWorldDef);
            capitalCenter.SetFaction(faction);
            capitalCenter.Tile = TileFinder.RandomFactionBaseTileFor(null);
            Find.WorldGrid.GetTileNeighbors(capitalCenter.Tile, neighboors);
            
            var districts = new List<WorldObject>
            {
                WorldObjectMaker.MakeWorldObject(capitalFactoryDef),
                WorldObjectMaker.MakeWorldObject(capitalPowerPlantDef),
                WorldObjectMaker.MakeWorldObject(capitalDefenseBaseDef),
                WorldObjectMaker.MakeWorldObject(capitalFarmDef),
                WorldObjectMaker.MakeWorldObject(capitalFarmDef),
                WorldObjectMaker.MakeWorldObject(capitalFarmDef)
            };


            foreach (var district in districts)
            {
                district.SetFaction(faction);
            }
    
            foreach (var tileId in neighboors)
            {
                var dist = districts.RandomElement();
                districts.Remove(dist);
                dist.Tile = tileId;
                
                Find.WorldObjects.Add(dist);
            }
            
            Find.WorldObjects.Add(capitalCenter);
        }
    }
}