using System.Collections.Generic;
using Verse;

namespace TheCapital.IA
{
    public class ActorPathPool
    {
        private static readonly ActorPath NotFoundPathInt = ActorPath.NewNotFound();
        private List<ActorPath> paths = new List<ActorPath>(64);
        private Map map;

        public ActorPathPool(Map map)
        {
            this.map = map;
        }

        public static ActorPath NotFoundPath => NotFoundPathInt;

        public ActorPath GetEmptyPawnPath()
        {
            for (int index = 0; index < paths.Count; ++index)
            {
                if (!paths[index].inUse)
                {
                    paths[index].inUse = true;
                    return paths[index];
                }
            }
            if (paths.Count > map.mapPawns.AllPawnsSpawnedCount + 2)
            {
                Log.ErrorOnce("PawnPathPool leak: more paths than spawned pawns. Force-recovering.", 664788);
                paths.Clear();
            }
            ActorPath actorPath = new ActorPath();
            paths.Add(actorPath);
            actorPath.inUse = true;
            return actorPath;
        }
    }
}