using RimWorld;
using TheCapital.IA;
using TheCapital.Trackers;
using UnityEngine;
using Verse;

namespace TheCapital
{
    public class Actor : ThingWithComps
    {
        public ActorStoryTracker story;
        public ActorPathFollower pather;
        private ActorDrawTracker drawer;
        public bool Disabled => false;
        
        public int TicksPerMoveCardinal => TicksPerMove(false);

        public int TicksPerMoveDiagonal => TicksPerMove(true);

        public override string LabelShort => "Actor Label";

        public override void ExposeData()
        {
          base.ExposeData();
            Log.Message("ExposeData of Actor!");
          Scribe_Deep.Look(ref pather, "pather", (object) this);
          Scribe_Deep.Look(ref story, "story", (object) this);
          if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;
        }
        
        
        
        public override void Tick()
        {
            base.Tick();
            bool suspended = Suspended;
            if (!suspended)
            {
                if (Spawned)
                    //pather.PatherTick();
                if (Spawned)
                {
                    Drawer.DrawTrackerTick();
                    //this.rotationTracker.RotationTrackerTick();
                }
            }
        }
        
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
        
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Drawer.DrawAt(drawLoc);
        }
        
        public void Notify_Teleported(bool endCurrentJob = true, bool resetTweenedPos = true)
        {
            if (resetTweenedPos)
                Drawer.tweener.ResetTweenedPosToRoot();
            
            pather.Notify_Teleported_Int();
        }
    }
}