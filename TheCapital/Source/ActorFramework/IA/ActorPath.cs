using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheCapital.IA
{
  public class ActorPath : IDisposable
  {
    private List<IntVec3> nodes = new List<IntVec3>(128);
    private float totalCostInt;
    private int curNodeIndex;
    private bool usedRegionHeuristics;
    public bool inUse;

    public bool Found => totalCostInt >= 0.0;

    public float TotalCost => totalCostInt;

    public int NodesLeftCount => curNodeIndex + 1;

    public int NodesConsumedCount => nodes.Count - NodesLeftCount;

    public bool UsedRegionHeuristics => usedRegionHeuristics;

    public List<IntVec3> NodesReversed => nodes;

    public IntVec3 FirstNode => nodes[nodes.Count - 1];

    public IntVec3 LastNode => nodes[0];

    public static ActorPath NotFound => ActorPathPool.NotFoundPath;

    public void AddNode(IntVec3 nodePosition)
    {
      nodes.Add(nodePosition);
    }

    public void SetupFound(float totalCost, bool usedRegionHeuristics)
    {
      if (this == ActorPath.NotFound)
      {
        Log.Warning("Calling SetupFound with totalCost=" + totalCost + " on PawnPath.NotFound", false);
      }
      else
      {
        totalCostInt = totalCost;
        this.usedRegionHeuristics = usedRegionHeuristics;
        curNodeIndex = nodes.Count - 1;
      }
    }

    public void Dispose()
    {
      ReleaseToPool();
    }

    public void ReleaseToPool()
    {
      if (this == ActorPath.NotFound)
        return;
      totalCostInt = 0.0f;
      usedRegionHeuristics = false;
      nodes.Clear();
      inUse = false;
    }

    public static ActorPath NewNotFound()
    {
      return new ActorPath { totalCostInt = -1f };
    }

    public IntVec3 ConsumeNextNode()
    {
      IntVec3 intVec3 = Peek(1);
      --curNodeIndex;
      return intVec3;
    }

    public IntVec3 Peek(int nodesAhead)
    {
      return nodes[curNodeIndex - nodesAhead];
    }

    public override string ToString()
    {
      if (!Found)
        return "PawnPath(not found)";
      if (!inUse)
        return "PawnPath(not in use)";
      object[] objArray = new object[6]
      {
        "PawnPath(nodeCount= ",
        nodes.Count,
        null,
        null,
        null,
        null
      };
      int index = 2;
      string str;
      if (nodes.Count > 0)
        str = " first=" + FirstNode + " last=" + LastNode;
      else
        str = string.Empty;
      objArray[index] = str;
      objArray[3] = " cost=";
      objArray[4] = totalCostInt;
      objArray[5] = " )";
      return string.Concat(objArray);
    }

    public void DrawPath(Actor pathingActor)
    {
      if (!Found)
        return;
      float num = AltitudeLayer.Item.AltitudeFor();
      if (NodesLeftCount <= 0)
        return;
      for (int nodesAhead = 0; nodesAhead < NodesLeftCount - 1; ++nodesAhead)
      {
        Vector3 vector3Shifted1 = Peek(nodesAhead).ToVector3Shifted();
        vector3Shifted1.y = num;
        Vector3 vector3Shifted2 = Peek(nodesAhead + 1).ToVector3Shifted();
        vector3Shifted2.y = num;
        GenDraw.DrawLineBetween(vector3Shifted1, vector3Shifted2);
      }
      if (pathingActor == null)
        return;
      Vector3 drawPos = pathingActor.DrawPos;
      drawPos.y = num;
      Vector3 vector3Shifted = Peek(0).ToVector3Shifted();
      vector3Shifted.y = num;
      if ((drawPos - vector3Shifted).sqrMagnitude <= 0.00999999977648258)
        return;
      GenDraw.DrawLineBetween(drawPos, vector3Shifted);
    }
  }
}