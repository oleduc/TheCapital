using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheCapital
{
    public class Actor : ThingWithComps
    {
        public bool Disabled => false;
        
        public Pawn_DrawTracker Drawer
        {
            get
            {
                if (this.drawer == null)
                    this.drawer = new Pawn_DrawTracker(this);
                return this.drawer;
            }
        }
        
        public void Notify_Teleported(bool endCurrentJob = true, bool resetTweenedPos = true)
        {
            if (resetTweenedPos)
                this.Drawer.tweener.ResetTweenedPosToRoot();
            this.pather.Notify_Teleported_Int();
            if (!endCurrentJob || this.jobs == null || this.jobs.curJob == null)
                return;
            this.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
        }
    }
}