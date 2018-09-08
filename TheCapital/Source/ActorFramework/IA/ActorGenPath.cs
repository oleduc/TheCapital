using Verse;
using Verse.AI;

namespace TheCapital.IA
{
    public static class ActorGenPath
    {
        public static TargetInfo ResolvePathMode(Actor actor, TargetInfo dest, ref PathEndMode peMode)
        {
            if (dest.HasThing && !dest.Thing.Spawned)
            {
                peMode = PathEndMode.Touch;
                return dest;
            }
            if (peMode == PathEndMode.InteractionCell)
            {
                if (!dest.HasThing)
                    Log.Error("Pathed to cell " + dest + " with PathEndMode.InteractionCell.", false);
                peMode = PathEndMode.OnCell;
                return new TargetInfo(dest.Thing.InteractionCell, dest.Thing.Map, false);
            }
            if (peMode == PathEndMode.ClosestTouch)
                peMode = ResolveClosestTouchPathMode(actor, dest.Map, dest.Cell);
            return dest;
        }

        public static PathEndMode ResolveClosestTouchPathMode(Actor pawn, Map map, IntVec3 target)
        {
            return ShouldNotEnterCell(pawn, map, target) ? PathEndMode.Touch : PathEndMode.OnCell;
        }

        private static bool ShouldNotEnterCell(Actor pawn, Map map, IntVec3 dest)
        {
            if (map.pathGrid.PerceivedPathCostAt(dest) > 30 || !dest.Walkable(map))
                return true;
            
            return false;
        }
    }
}