using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TheCapital
{
    public class CapitalWorldObject : MapParent
    {
        public override void Print(LayerSubMesh subMesh)
        {
            PrintQuadTangentialToPlanet(DrawPos, DrawPos, 0.7f * Find.WorldGrid.averageTileSize, 0.015f, subMesh);
        }
       
        protected override bool UseGenericEnterMapFloatMenuOption
        {
            get { return true; }
        }

        public bool Visitable
        {
            get
            {
                return true;
            }
        }

        public bool Attackable
        {
            get
            {
                Log.Message("FUCK FASCISTS SCUM");
                return true;
            }
        }
        
        public static void PrintQuadTangentialToPlanet(Vector3 pos, Vector3 posForTangents, float size, float altOffset, LayerSubMesh subMesh, bool counterClockwise = false, bool randomizeRotation = false, bool printUVs = true)
        {
            Vector3 first;
            Vector3 second;
            GetTangentsToPlanet(posForTangents, out first, out second, randomizeRotation);
            Vector3 normalized = posForTangents.normalized;
            float num = size * 0.5f;
            Vector3 vector3_1 = pos - first * num - second * num + normalized * altOffset;
            Vector3 vector3_2 = pos - first * num + second * num + normalized * altOffset;
            Vector3 vector3_3 = pos + first * num + second * num + normalized * altOffset;
            Vector3 vector3_4 = pos + first * num - second * num + normalized * altOffset;
            int count = subMesh.verts.Count;
            subMesh.verts.Add(vector3_1);
            subMesh.verts.Add(vector3_2);
            subMesh.verts.Add(vector3_3);
            subMesh.verts.Add(vector3_4);
            if (printUVs)
            {
                subMesh.uvs.Add((Vector3) new Vector2(0.0f, 0.0f));
                subMesh.uvs.Add((Vector3) new Vector2(0.0f, 1f));
                subMesh.uvs.Add((Vector3) new Vector2(1f, 1f));
                subMesh.uvs.Add((Vector3) new Vector2(1f, 0.0f));
            }
            if (counterClockwise)
            {
                subMesh.tris.Add(count + 2);
                subMesh.tris.Add(count + 1);
                subMesh.tris.Add(count);
                subMesh.tris.Add(count + 3);
                subMesh.tris.Add(count + 2);
                subMesh.tris.Add(count);
            }
            else
            {
                subMesh.tris.Add(count);
                subMesh.tris.Add(count + 1);
                subMesh.tris.Add(count + 2);
                subMesh.tris.Add(count);
                subMesh.tris.Add(count + 2);
                subMesh.tris.Add(count + 3);
            }
        }
        
        public static void GetTangentsToPlanet(Vector3 pos, out Vector3 first, out Vector3 second, bool randomizeRotation = false)
        {
            Vector3 upwards = !randomizeRotation ? Vector3.right : Rand.UnitVector3;
            Quaternion quaternion = Quaternion.LookRotation(pos.normalized, upwards);
            first = quaternion * Vector3.up;
            second = quaternion * Vector3.right;
        }
    }
}