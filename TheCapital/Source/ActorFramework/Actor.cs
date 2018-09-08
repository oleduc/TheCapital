using RimWorld;
using TheCapital.IA;
using TheCapital.Trackers;
using UnityEngine;
using Verse;

namespace TheCapital
{
    public class Actor : ThingWithComps
    {
        public ActorPathFollower pather;
        
        private ActorDrawTracker drawer;
        public bool Disabled => false;
        
        public int TicksPerMoveCardinal => TicksPerMove(false);

        public int TicksPerMoveDiagonal => TicksPerMove(true);

        public override string LabelShort => "Actor Label";

        private int TicksPerMove(bool diagonal)
        {
            float statValue = this.GetStatValue(StatDefOf.MoveSpeed);

            float num = statValue / 60f;
            float f;
            if (num == 0.0)
            {
                f = 450f;
            }
            else
            {
                f = 1f / num;
                if (Spawned && !Map.roofGrid.Roofed(Position))
                    f /= Map.weatherManager.CurMoveSpeedMultiplier;
                if (diagonal)
                    f *= 1.41421f;
            }
            return Mathf.Clamp(Mathf.RoundToInt(f), 1, 450);
        }
        
        public ActorDrawTracker Drawer
        {
            get
            {
                if (drawer == null)
                    drawer = new ActorDrawTracker(this);
                return drawer;
            }
        }
        
        public void Notify_Teleported(bool endCurrentJob = true, bool resetTweenedPos = true)
        {
            if (resetTweenedPos)
                Drawer.tweener.ResetTweenedPosToRoot();
            
            pather.Notify_Teleported_Int();
        }
    }
}