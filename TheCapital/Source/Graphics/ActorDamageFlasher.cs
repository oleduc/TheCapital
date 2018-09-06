using UnityEngine;
using Verse;

namespace TheCapital
{
    public class ActorDamageFlasher
    {
        private int lastDamageTick = -9999;
        private const int DamagedMatTicksTotal = 16;

        public ActorDamageFlasher(Actor actor)
        {
        }

        private int DamageFlashTicksLeft => lastDamageTick + 16 - Find.TickManager.TicksGame;

        public bool FlashingNowOrRecently => DamageFlashTicksLeft >= -1;

        public Material GetDamagedMat(Material baseMat)
        {
            return DamagedMatPool.GetDamageFlashMat(baseMat, DamageFlashTicksLeft / 16f);
        }

        public void Notify_DamageApplied(DamageInfo dinfo)
        {
            if (!dinfo.Def.harmsHealth)
                return;
            lastDamageTick = Find.TickManager.TicksGame;
        }
    }
}