using UnityEngine;
using Verse;

namespace TheCapital.Defs
{
    public class BodyTypeDef : Def
    {
        [NoTranslate]
        public string bodyGraphicPath;
        [NoTranslate]
        public string bodyWeatheredPath;
        [NoTranslate]
        public string bodyDamagedPath;
        [NoTranslate]
        public string bodyDestroyedPath;

    }
}