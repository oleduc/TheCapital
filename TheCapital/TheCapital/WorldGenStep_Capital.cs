using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheCapital
{
    public class WorldGenStep_Capital : WorldGenStep
    {
        private List<WorldObject> capitalWorldObjs = new List<WorldObject>();
        
        // Zone tiles
        private List<int> neighboors = new List<int>();
        private List<int> core = new List<int>();
        private List<int> wall = new List<int>();
        private List<int> outerRim = new List<int>();
        private List<int> dominion = new List<int>();

        private WorldObjectDef centerDef;
        private WorldObjectDef downtownDef;
        private WorldObjectDef housingDef;
        private WorldObjectDef farmDef;
        private WorldObjectDef factoryDef;
        private WorldObjectDef powerPlantDef;
        private WorldObjectDef windFarmDef;
        private WorldObjectDef defenseBaseDef;
        
        public override void GenerateFresh(string seed)
        {
            var faction = FactionUtility.DefaultFactionFrom(DefDatabase<FactionDef>.GetNamed("Capital"));

            Initialize();
            
            // Setup capital center
            var capitalCenter = WorldObjectMaker.MakeWorldObject(centerDef);
            capitalCenter.Tile = TileFinder.RandomFactionBaseTileFor(null);
            capitalWorldObjs.Add(capitalCenter);

            MapSurroundings(capitalCenter.Tile);

            GenerateDowntown();
            GenerateCapitalCore();
            GenerateCapitalOuterRim();

            // Place into the world
            foreach (var worldObj in capitalWorldObjs)
            {
                worldObj.SetFaction(faction);
                Find.World.worldObjects.Add(worldObj);
            }
        }

        private void Initialize()
        {
            capitalWorldObjs = new List<WorldObject>();
            neighboors = new List<int>();
            core = new List<int>();
            wall = new List<int>();
            outerRim = new List<int>();
            dominion = new List<int>();
            
            centerDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalCenter");
            downtownDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalDowntown");
            housingDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalHousing");
            farmDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalFarm");
            factoryDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalFactory");
            powerPlantDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalPowerPlant");
            windFarmDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalWindFarm");
            defenseBaseDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalDefenseBase");
        }

        private void MapSurroundings(int capitalCenterTileId)
        {
            Find.WorldFloodFiller.FloodFill(capitalCenterTileId, x => true, (tile, dist) =>
            {
                var tileInfo = Find.WorldGrid[tile];
                if (tileInfo.WaterCovered) return false;
                
                if (dist == 1)
                    neighboors.Add(tile);
                
                if (dist > 1 && dist <= 3)
                    core.Add(tile);
                 
                if (dist == 4)
                    wall.Add(tile);
                
                if (dist > 5 && dist <= 9)
                    outerRim.Add(tile);

                if (dist > 9 && dist < 14)
                    dominion.Add(tile);
                
                return dist > 14;
            });
        }

        private void GenerateDowntown()
        {
            foreach (var neighboor in neighboors)
            {
                var downtown = WorldObjectMaker.MakeWorldObject(downtownDef);
                downtown.Tile = neighboor;
                capitalWorldObjs.Add(downtown);
            }
        }
        
        private void GenerateCapitalCore()
        {
            var coreChances = new List<WorldObjectDef>
            {
                housingDef,
                housingDef,
                housingDef,
                downtownDef,
                factoryDef,
                factoryDef,
                null,
                null
            };
            
            var powerPlant = WorldObjectMaker.MakeWorldObject(powerPlantDef);
            powerPlant.Tile = core.RandomElement();
            core.Remove(powerPlant.Tile);
            capitalWorldObjs.Add(powerPlant);

            foreach (var coreTileId in core)
            {
                var defType = coreChances.RandomElement();
                
                if (defType != null)
                {
                    var obj = WorldObjectMaker.MakeWorldObject(defType);
                    obj.Tile = coreTileId;
                    capitalWorldObjs.Add(obj);
                }
            }
        }

        private void GenerateCapitalOuterRim()
        {
            for (var i = 0; i < 14; i++)
            {
                var farm = WorldObjectMaker.MakeWorldObject(farmDef);
                farm.Tile = outerRim.RandomElement();
                outerRim.Remove(farm.Tile);
                capitalWorldObjs.Add(farm);
            }
            
            for (var i = 0; i < 4; i++)
            {
                var windFarm = WorldObjectMaker.MakeWorldObject(windFarmDef);
                windFarm.Tile = outerRim.RandomElement();
                outerRim.Remove(windFarm.Tile);
                capitalWorldObjs.Add(windFarm);
            }
            
            for (var i = 0; i < 4; i++)
            {
                var militaryBase = WorldObjectMaker.MakeWorldObject(defenseBaseDef);
                militaryBase.Tile = outerRim.RandomElement();
                outerRim.Remove(militaryBase.Tile);
                capitalWorldObjs.Add(militaryBase);
            }
        }
    }
}