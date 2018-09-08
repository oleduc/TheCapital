using TheCapital.IA;
using Verse;
using Verse.AI;

namespace TheCapital.Misc
{
    public static class ActorReachabilityImmediate
    {
        public static bool CanReachImmediate(IntVec3 start, LocalTargetInfo target, Map map, PathEndMode peMode, Actor actor)
        {
            if (!target.IsValid)
                return false;
            target = (LocalTargetInfo) ActorGenPath.ResolvePathMode(actor, target.ToTargetInfo(map), ref peMode);
            if (target.HasThing)
            {
                Thing thing = target.Thing;
                if (thing.Spawned)
                {
                    if (thing.Map != map)
                        return false;
                }
                else
                    return actor != null;
            }
            if (!target.HasThing || target.Thing.def.size.x == 1 && target.Thing.def.size.z == 1)
            {
                if (start == target.Cell)
                    return true;
            }
            else if (start.IsInside(target.Thing))
                return true;
            return peMode == PathEndMode.Touch && TouchPathEndModeUtility.IsAdjacentOrInsideAndAllowedToTouch(start, target, map);
        }

        public static bool CanReachImmediate(this Actor actor, LocalTargetInfo target, PathEndMode peMode)
        {
            if (!actor.Spawned)
                return false;
            return CanReachImmediate(actor.Position, target, actor.Map, peMode, actor);
        }

        public static bool CanReachImmediateNonLocal(this Actor actor, TargetInfo target, PathEndMode peMode)
        {
            if (!actor.Spawned || target.Map != null && target.Map != actor.Map)
                return false;
            return actor.CanReachImmediate((LocalTargetInfo) target, peMode);
        }

        public static bool CanReachImmediate(IntVec3 start, CellRect rect, Map map, PathEndMode peMode, Actor actor)
        {
            IntVec3 intVec3 = rect.ClosestCellTo(start);
            return CanReachImmediate(start, intVec3, map, peMode, actor);
        }
    }
}