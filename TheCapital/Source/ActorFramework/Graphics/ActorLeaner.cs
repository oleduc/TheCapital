using UnityEngine;
using Verse;

namespace TheCapital
{
    public class ActorLeaner
    {
        private IntVec3 shootSourceOffset = new IntVec3(0, 0, 0);
        private Actor actor;
        private float leanOffsetCurPct;
        private const float LeanOffsetPctChangeRate = 0.075f;
        private const float LeanOffsetDistanceMultiplier = 0.5f;

        public ActorLeaner(Actor actor)
        {
            this.actor = actor;
        }

        public Vector3 LeanOffset => shootSourceOffset.ToVector3() * 0.5f * leanOffsetCurPct;

        public void LeanerTick()
        {
            if (ShouldLean())
            {
                leanOffsetCurPct += 0.075f;
                if (leanOffsetCurPct <= 1.0)
                    return;
                leanOffsetCurPct = 1f;
            }
            else
            {
                leanOffsetCurPct -= 0.075f;
                if (leanOffsetCurPct >= 0.0)
                    return;
                leanOffsetCurPct = 0.0f;
            }
        }

        public bool ShouldLean()
        {
            return false;
        }

        public void Notify_WarmingCastAlongLine(ShootLine newShootLine, IntVec3 ShootPosition)
        {
            shootSourceOffset = newShootLine.Source - actor.Position;
        }
    }
}