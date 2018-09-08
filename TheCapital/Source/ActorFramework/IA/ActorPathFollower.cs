using System.Collections.Generic;
using RimWorld;
using TheCapital.Misc;
using TheCapital.Utilities;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheCapital.IA
{
public class ActorPathFollower : IExposable
  {
    public float nextCellCostTotal = 1f;
    private int lastMovedTick = -999999;
    private int foundPathWhichCollidesWithPawns = -999999;
    private int foundPathWithDanger = -999999;
    private int failedToFindCloseUnoccupiedCellTicks = -999999;
    protected Actor actor;
    private bool moving;
    public IntVec3 nextCell;
    private IntVec3 lastCell;
    public float nextCellCostLeft;
    private int cellsUntilClamor;
    private LocalTargetInfo destination;
    private PathEndMode peMode;
    public ActorPath curPath;
    public IntVec3 lastPathedTargetPosition;
    private const int MaxMoveTicks = 450;
    private const int MaxCheckAheadNodes = 20;
    private const float SnowReductionFromWalking = 0.001f;
    private const int ClamorCellsInterval = 12;
    private const int MinCostWalk = 50;
    private const int MinCostAmble = 60;
    private const float StaggerMoveSpeedFactor = 0.17f;
    private const int CheckForMovingCollidingPawnsIfCloserToTargetThanX = 30;
    private const int AttackBlockingHostilePawnAfterTicks = 180;

    public ActorPathFollower(Actor newPawn)
    {
      actor = newPawn;
    }

    public LocalTargetInfo Destination => destination;

    public bool Moving => moving;

    public bool MovingNow
    {
      get
      {
        if (Moving)
          return !WillCollideWithPawnOnNextPathCell();
        return false;
      }
    }

    public IntVec3 LastPassableCellInPath
    {
      get
      {
        if (!Moving || curPath == null)
          return IntVec3.Invalid;
        if (!Destination.Cell.Impassable(actor.Map))
          return Destination.Cell;
        List<IntVec3> nodesReversed = curPath.NodesReversed;
        for (int index = 0; index < nodesReversed.Count; ++index)
        {
          if (!nodesReversed[index].Impassable(actor.Map))
            return nodesReversed[index];
        }
        if (!actor.Position.Impassable(actor.Map))
          return actor.Position;
        return IntVec3.Invalid;
      }
    }

    public void ExposeData()
    {
      Scribe_Values.Look(ref moving, "moving", true, false);
      Scribe_Values.Look(ref nextCell, "nextCell", new IntVec3(), false);
      Scribe_Values.Look(ref nextCellCostLeft, "nextCellCostLeft", 0.0f, false);
      Scribe_Values.Look(ref nextCellCostTotal, "nextCellCostInitial", 0.0f, false);
      Scribe_Values.Look(ref peMode, "peMode", PathEndMode.None, false);
      Scribe_Values.Look(ref cellsUntilClamor, "cellsUntilClamor", 0, false);
      Scribe_Values.Look(ref lastMovedTick, "lastMovedTick", -999999, false);
      if (!moving)
        return;
      Scribe_TargetInfo.Look(ref destination, "destination");
    }

    public void StartPath(LocalTargetInfo dest, PathEndMode peMode)
    {
      dest = (LocalTargetInfo) ActorGenPath.ResolvePathMode(actor, dest.ToTargetInfo(actor.Map), ref peMode);
      if (dest.HasThing && dest.ThingDestroyed)
      {
        Log.Error(actor + " pathing to destroyed thing " + dest.Thing, false);
        PatherFailed();
      }
      else
      {
        if (!PawnCanOccupy(actor.Position) && !TryRecoverFromUnwalkablePosition(true) || moving && curPath != null && (destination == dest && this.peMode == peMode))
          return;
        if (!actor.Map.reachability.CanReach(actor.Position, dest, peMode, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
        {
          PatherFailed();
        }
        else
        {
          this.peMode = peMode;
          destination = dest;
          if (!IsNextCellWalkable() || NextCellDoorToManuallyOpen() != null || nextCellCostLeft == (double) nextCellCostTotal)
            ResetToCurrentPosition();
          if (AtDestinationPosition())
            PatherArrived();
          else
          {
            if (curPath != null)
              curPath.ReleaseToPool();
            curPath = null;
            moving = true;
          }
        }
      }
    }

    public void StopDead()
    {
      if (curPath != null)
        curPath.ReleaseToPool();
      curPath = null;
      moving = false;
      nextCell = actor.Position;
    }

    public void PatherTick()
    {
      lastMovedTick = Find.TickManager.TicksGame;
      
      if (nextCellCostLeft > 0.0)
      {
        nextCellCostLeft -= CostToPayThisTick();
      }
      else
      {
        if (!moving)
          return;
        TryEnterNextPathCell();
      }
    }

    public void TryResumePathingAfterLoading()
    {
      if (!moving)
        return;
      StartPath(destination, peMode);
    }

    public void Notify_Teleported_Int()
    {
      StopDead();
      ResetToCurrentPosition();
    }

    public void ResetToCurrentPosition()
    {
      nextCell = actor.Position;
      nextCellCostLeft = 0.0f;
      nextCellCostTotal = 1f;
    }

    private bool PawnCanOccupy(IntVec3 c)
    {
      if (!c.Walkable(actor.Map))
        return false;

      return true;
    }

    public Building BuildingBlockingNextPathCell()
    {
      return null;
    }

    public bool WillCollideWithPawnOnNextPathCell()
    {
      return WillCollideWithPawnAt(nextCell);
    }

    private bool IsNextCellWalkable()
    {
      return nextCell.Walkable(actor.Map);
    }

    private bool WillCollideWithPawnAt(IntVec3 c)
    {
      return false;
    }

    public Building_Door NextCellDoorToManuallyOpen()
    {
      return null;
    }

    public void PatherDraw()
    {
      if (!DebugViewSettings.drawPaths || curPath == null || !Find.Selector.IsSelected(actor))
        return;
      curPath.DrawPath(actor);
    }

    public bool MovedRecently(int ticks)
    {
      return Find.TickManager.TicksGame - lastMovedTick <= ticks;
    }

    public bool TryRecoverFromUnwalkablePosition(bool error = true)
    {
      bool flag = false;
      for (int index = 0; index < GenRadial.RadialPattern.Length; ++index)
      {
        IntVec3 c = actor.Position + GenRadial.RadialPattern[index];
        if (PawnCanOccupy(c))
        {
          if (c == actor.Position)
            return true;
          if (error)
            Log.Warning(actor + " on unwalkable cell " + actor.Position + ". Teleporting to " + c, false);
          actor.Position = c;
          actor.Notify_Teleported(true, false);
          flag = true;
          break;
        }
      }
      if (!flag)
      {
        actor.Destroy(DestroyMode.Vanish);
        Log.Error(actor.ToStringSafe() + " on unwalkable cell " + actor.Position + ". Could not find walkable position nearby. Destroyed.", false);
      }
      return flag;
    }

    private void PatherArrived()
    {
      StopDead();
    }

    private void PatherFailed()
    {
      StopDead();
    }

    private void TryEnterNextPathCell()
    {
      lastCell = actor.Position;
      actor.Position = nextCell;
      
      if (NeedNewPath() && !TrySetNewPath())
        return;
      
      if (AtDestinationPosition())
        PatherArrived();
      else
        SetupMoveIntoNextCell();
      
    }

    private void SetupMoveIntoNextCell()
    {
      if (curPath.NodesLeftCount <= 1)
      {
        Log.Error(actor + " at " + actor.Position + " ran out of path nodes while pathing to " + destination + ".", false);
        PatherFailed();
      }
      else
      {
        nextCell = curPath.ConsumeNextNode();
        if (!nextCell.Walkable(actor.Map))
          Log.Error(actor + " entering " + nextCell + " which is unwalkable.", false);
        int moveIntoCell = CostToMoveIntoCell(nextCell);
        nextCellCostTotal = moveIntoCell;
        nextCellCostLeft = moveIntoCell;
      }
    }

    private int CostToMoveIntoCell(IntVec3 c)
    {
      return CostToMoveIntoCell(actor, c);
    }

    private static int CostToMoveIntoCell(Actor actor, IntVec3 c)
    {
      int a = (c.x == actor.Position.x || c.z == actor.Position.z ? actor.TicksPerMoveCardinal : actor.TicksPerMoveDiagonal) + actor.Map.pathGrid.CalculatedCostAt(c, false, actor.Position);
      
      if (a > 450)
        a = 450;

      return Mathf.Max(a, 1);
    }

    private float CostToPayThisTick()
    {
      float num = 1f;
      if (num < nextCellCostTotal / 450.0)
        num = nextCellCostTotal / 450f;
      return num;
    }

    private bool TrySetNewPath()
    {
      ActorPath newPath = GenerateNewPath();
      
      if (!newPath.Found)
      {
        PatherFailed();
        return false;
      }
      
      if (curPath != null)
        curPath.ReleaseToPool();
      
      curPath = newPath;

      return true;
    }

    private ActorPath GenerateNewPath()
    {
      lastPathedTargetPosition = destination.Cell;
      var pawnPath = actor.Map.pathFinder.FindPath(actor.Position, destination, Converter.ActorToPawn(actor), peMode);
      
      return Converter.PawnPathToActorPath(pawnPath);
    }

    private bool AtDestinationPosition()
    {
      
      return actor.CanReachImmediate(destination, peMode);
    }

    private bool NeedNewPath()
    {
      if (!destination.IsValid ||
          curPath == null ||
          (!curPath.Found || curPath.NodesLeftCount == 0) ||
          destination.HasThing && destination.Thing.Map != actor.Map ||
          (actor.Position.InHorDistOf(curPath.LastNode, 15f) ||
           actor.Position.InHorDistOf(destination.Cell, 15f)) &&
          !ActorReachabilityImmediate.CanReachImmediate(curPath.LastNode, destination, actor.Map, peMode, actor) ||
          curPath.UsedRegionHeuristics && curPath.NodesConsumedCount >= 75)
        return true;
      if (lastPathedTargetPosition != destination.Cell)
      {
        float horizontalSquared = (actor.Position - destination.Cell).LengthHorizontalSquared;
        float num = (double) horizontalSquared <= 900.0 ? ((double) horizontalSquared <= 289.0 ? ((double) horizontalSquared <= 100.0 ? ((double) horizontalSquared <= 49.0 ? 0.5f : 2f) : 3f) : 5f) : 10f;
        if ((lastPathedTargetPosition - destination.Cell).LengthHorizontalSquared > num * (double) num)
          return true;
      }

      for (int nodesAhead = 0; nodesAhead < 20 && nodesAhead < curPath.NodesLeftCount; ++nodesAhead)
      {
        IntVec3 c = curPath.Peek(nodesAhead);
        if (!c.Walkable(actor.Map))
          return true;
      }
      
      return false;
    }
  }
}