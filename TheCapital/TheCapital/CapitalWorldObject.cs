using RimWorld.Planet;
using Verse;

namespace TheCapital
{
    public class CapitalWorldObject : WorldObject
    {
        public override void Print(LayerSubMesh subMesh)
        {
            WorldRendererUtility.PrintQuadTangentialToPlanet(this.DrawPos, 0.7f * Find.WorldGrid.averageTileSize, 0.015f, subMesh);
        }
    }
}