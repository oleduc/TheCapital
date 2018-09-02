using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheCapital
{
    public class WorldGenStep_Capital : WorldGenStep
    {
        private List<WorldObject> _capitalWorldObjs = new List<WorldObject>();
        
        // Zone tiles
        private List<int> _neighboors = new List<int>();
        private List<int> _core = new List<int>();
        private List<int> _wall = new List<int>();
        private List<int> _outerRim = new List<int>();
        private List<int> _dominion = new List<int>();

        private WorldObjectDef _centerDef;
        private WorldObjectDef _downtownDef;
        private WorldObjectDef _housingDef;
        private WorldObjectDef _farmDef;
        private WorldObjectDef _factoryDef;
        private WorldObjectDef _powerPlantDef;
        private WorldObjectDef _windFarmDef;
        private WorldObjectDef _defenseBaseDef;
        
        public override void GenerateFresh(string seed)
        {
            var faction = FactionUtility.DefaultFactionFrom(DefDatabase<FactionDef>.GetNamed("Capital"));

            Initialize();
            // Setup capital center
            var capitalCenter = WorldObjectMaker.MakeWorldObject(_centerDef);
            capitalCenter.Tile = TileFinder.RandomSettlementTileFor(null);
            _capitalWorldObjs.Add(capitalCenter);

            MapSurroundings(capitalCenter.Tile);

            GenerateDowntown();
            GenerateCapitalCore();
            GenerateCapitalOuterRim();

            // Place into the world
            foreach (var worldObj in _capitalWorldObjs)
            {
                worldObj.SetFaction(faction);
                Find.World.worldObjects.Add(worldObj);
            }
        }

        public override int SeedPart { get; }

        private void Initialize()
        {
            _capitalWorldObjs = new List<WorldObject>();
            _neighboors = new List<int>();
            _core = new List<int>();
            _wall = new List<int>();
            _outerRim = new List<int>();
            _dominion = new List<int>();
            
            _centerDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalCenter");
            _downtownDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalDowntown");
            _housingDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalHousing");
            _farmDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalFarm");
            _factoryDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalFactory");
            _powerPlantDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalPowerPlant");
            _windFarmDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalWindFarm");
            _defenseBaseDef = DefDatabase<WorldObjectDef>.GetNamed("CapitalDefenseBase");
        }

        private void MapSurroundings(int capitalCenterTileId)
        {
            Find.WorldFloodFiller.FloodFill(capitalCenterTileId, x => true, (tile, dist) =>
            {
                var tileInfo = Find.WorldGrid[tile];
                if (tileInfo.WaterCovered) return false;
                
                if (dist == 1)
                    _neighboors.Add(tile);
                
                if (dist > 1 && dist <= 3)
                    _core.Add(tile);
                 
                if (dist == 4)
                    _wall.Add(tile);
                
                if (dist > 5 && dist <= 9)
                    _outerRim.Add(tile);

                if (dist > 9 && dist < 14)
                    _dominion.Add(tile);
                
                return dist > 14;
            });
        }

        private void GenerateDowntown()
        {
            foreach (var neighboor in _neighboors)
            {
                var downtown = WorldObjectMaker.MakeWorldObject(_downtownDef);
                downtown.Tile = neighboor;
                _capitalWorldObjs.Add(downtown);
            }
        }
        
        private void GenerateCapitalCore()
        {
            var coreChances = new List<WorldObjectDef>
            {
                _housingDef,
                _housingDef,
                _housingDef,
                _downtownDef,
                _factoryDef,
                _factoryDef,
                null,
                null
            };
            
            var powerPlant = WorldObjectMaker.MakeWorldObject(_powerPlantDef);
            powerPlant.Tile = _core.RandomElement();
            _core.Remove(powerPlant.Tile);
            _capitalWorldObjs.Add(powerPlant);

            foreach (var coreTileId in _core)
            {
                var defType = coreChances.RandomElement();
                
                if (defType != null)
                {
                    var obj = WorldObjectMaker.MakeWorldObject(defType);
                    obj.Tile = coreTileId;
                    _capitalWorldObjs.Add(obj);
                }
            }
        }

        private void GenerateCapitalOuterRim()
        {
            for (var i = 0; i < 14; i++)
            {
                var farm = WorldObjectMaker.MakeWorldObject(_farmDef);
                farm.Tile = _outerRim.RandomElement();
                _outerRim.Remove(farm.Tile);
                _capitalWorldObjs.Add(farm);
            }
            
            for (var i = 0; i < 4; i++)
            {
                var windFarm = WorldObjectMaker.MakeWorldObject(_windFarmDef);
                windFarm.Tile = _outerRim.RandomElement();
                _outerRim.Remove(windFarm.Tile);
                _capitalWorldObjs.Add(windFarm);
            }
            
            for (var i = 0; i < 4; i++)
            {
                var militaryBase = WorldObjectMaker.MakeWorldObject(_defenseBaseDef);
                militaryBase.Tile = _outerRim.RandomElement();
                _outerRim.Remove(militaryBase.Tile);
                _capitalWorldObjs.Add(militaryBase);
            }
        }
    }
}