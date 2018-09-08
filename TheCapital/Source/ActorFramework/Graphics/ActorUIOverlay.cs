using RimWorld;
using TheCapital.Utilities;
using Verse;

namespace TheCapital
{
    public class ActorUIOverlay
    {
        private Actor actor;
        private const float PawnLabelOffsetY = -0.6f;
        private const int PawnStatBarWidth = 32;
        private const float ActivityIconSize = 13f;
        private const float ActivityIconOffsetY = 12f;

        public ActorUIOverlay(Actor actor)
        {
            this.actor = actor;
        }

        public void DrawPawnGUIOverlay()
        {
            if (!actor.Spawned || actor.Map.fogGrid.IsFogged(actor.Position))
                return;
            
            var pawn = Converter.ActorToPawn(actor);
            GenMapUI.DrawPawnLabel(pawn, GenMapUI.LabelDrawPosFor(pawn, -0.6f), 1f, 9999f, null, GameFont.Tiny, true, true);

            actor.Map.overlayDrawer.DrawOverlay(actor, OverlayTypes.QuestionMark);
        }
    }
}