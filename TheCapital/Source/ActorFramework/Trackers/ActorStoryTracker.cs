using RimWorld;
using UnityEngine;
using Verse;
using BodyTypeDef = TheCapital.Defs.BodyTypeDef;

namespace TheCapital.Trackers
{
    public class ActorStoryTracker : IExposable
    {
        public Color bodyColor;
        public BodyTypeDef bodyType;
        public void ExposeData()
        {
            Scribe_Defs.Look(ref bodyType, "bodyType");
        }
    }
}