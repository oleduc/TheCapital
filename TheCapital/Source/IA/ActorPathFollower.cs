using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheCapital.IA
{
public class Pawn_PathFollower : IExposable
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
    public PawnPath curPath;
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

    public Pawn_PathFollower(Actor newPawn)
    {
      actor = newPawn;
    }

    public LocalTargetInfo Destination
    {
      get
      {
        return destination;
      }
    }

    public bool Moving
    {
      get
      {
        return moving;
      }
    }

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
      dest = (LocalTargetInfo) GenPath.ResolvePathMode(actor, dest.ToTargetInfo(actor.Map), ref peMode);
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
          if (!destination.HasThing && actor.Map.pawnDestinationReservationManager.MostRecentReservationFor(actor) != destination.Cell)
            actor.Map.pawnDestinationReservationManager.ObsoleteAllClaimedBy(actor);
          if (AtDestinationPosition())
            PatherArrived();
          else if (actor.Downed)
          {
            Log.Error(actor.LabelCap + " tried to path while downed. This should never happen. curJob=" + actor.CurJob.ToStringSafe<Job>(), false);
            PatherFailed();
          }
          else
          {
            if (curPath != null)
              curPath.ReleaseToPool();
            curPath = null;
            moving = true;
            actor.jobs.posture = PawnPosture.Standing;
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
      if (WillCollideWithPawnAt(this.actor.Position))
      {
        if (FailedToFindCloseUnoccupiedCellRecently())
          return;
        IntVec3 cell;
        if (CellFinder.TryFindBestPawnStandCell(actor, out cell, true) && cell != actor.Position)
        {
          actor.Position = cell;
          ResetToCurrentPosition();
          if (!moving || !TrySetNewPath())
            return;
          TryEnterNextPathCell();
        }
        else
          failedToFindCloseUnoccupiedCellTicks = Find.TickManager.TicksGame;
      }
      else
      {
        if (this.actor.stances.FullBodyBusy)
          return;
        if (moving && WillCollideWithPawnOnNextPathCell())
        {
          nextCellCostLeft = nextCellCostTotal;
          if ((curPath != null && curPath.NodesLeftCount < 30 || PawnUtility.AnyPawnBlockingPathAt(nextCell, this.actor, false, true, false)) && (!BestPathHadPawnsInTheWayRecently() && TrySetNewPath()))
          {
            ResetToCurrentPosition();
            TryEnterNextPathCell();
          }
          else
          {
            if (Find.TickManager.TicksGame - lastMovedTick < 180)
              return;
            Actor actor = PawnUtility.PawnBlockingPathAt(nextCell, this.actor, false, false, false);
            if (actor == null || !this.actor.HostileTo(actor) || this.actor.TryGetAttackVerb((Thing) actor, false) == null)
              return;
            this.actor.jobs.StartJob(new Job(JobDefOf.AttackMelee, (LocalTargetInfo) actor)
            {
              maxNumMeleeAttacks = 1,
              expiryInterval = 300
            }, JobCondition.Incompletable, (ThinkNode) null, false, true, (ThinkTreeDef) null, new JobTag?(), false);
          }
        }
        else
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
      Building edifice = c.GetEdifice(actor.Map);
      if (edifice != null)
      {
        Building_Door buildingDoor = edifice as Building_Door;
        if (buildingDoor != null && !buildingDoor.PawnCanOpen(actor) && !buildingDoor.Open)
          return false;
      }
      return true;
    }

    public Building BuildingBlockingNextPathCell()
    {
      Building edifice = nextCell.GetEdifice(actor.Map);
      if (edifice != null && edifice.BlocksPawn(actor))
        return edifice;
      return null;
    }

    public bool WillCollideWithPawnOnNextPathCell()
    {
      return WillCollideWithPawnAt(nextCell);
    }

    private bool IsNextCellWalkable()
    {
      return nextCell.Walkable(actor.Map) && !WillCollideWithPawnAt(nextCell);
    }

    private bool WillCollideWithPawnAt(IntVec3 c)
    {
      if (!PawnUtility.ShouldCollideWithPawns(actor))
        return false;
      return PawnUtility.AnyPawnBlockingPathAt(c, actor, false, false, false);
    }

    public Building_Door NextCellDoorToManuallyOpen()
    {
      Building_Door buildingDoor = actor.Map.thingGrid.ThingAt<Building_Door>(nextCell);
      if (buildingDoor != null && buildingDoor.SlowsPawns && (!buildingDoor.Open && buildingDoor.PawnCanOpen(actor)))
        return buildingDoor;
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
      if (actor.jobs.curJob == null)
        return;
      actor.jobs.curDriver.Notify_PatherArrived();
    }

    private void PatherFailed()
    {
      StopDead();
      actor.jobs.curDriver.Notify_PatherFailed();
    }

    private void TryEnterNextPathCell()
    {
      Building building = BuildingBlockingNextPathCell();
      if (building != null)
      {
        Building_Door buildingDoor = building as Building_Door;
        if (buildingDoor == null || !buildingDoor.FreePassage)
        {
          if (actor.CurJob != null && actor.CurJob.canBash || actor.HostileTo(building))
          {
            actor.jobs.StartJob(new Job(JobDefOf.AttackMelee, (LocalTargetInfo) building)
            {
              expiryInterval = 300
            }, JobCondition.Incompletable, (ThinkNode) null, false, true, (ThinkTreeDef) null, new JobTag?(), false);
            return;
          }
          PatherFailed();
          return;
        }
      }
      Building_Door manuallyOpen = NextCellDoorToManuallyOpen();
      if (manuallyOpen != null)
      {
        Stance_Cooldown stanceCooldown = new Stance_Cooldown(manuallyOpen.TicksToOpenNow, (LocalTargetInfo) manuallyOpen, null);
        stanceCooldown.neverAimWeapon = true;
        actor.stances.SetStance((Stance) stanceCooldown);
        manuallyOpen.StartManualOpenBy(actor);
        manuallyOpen.CheckFriendlyTouched(actor);
      }
      else
      {
        lastCell = actor.Position;
        actor.Position = nextCell;
        if (actor.RaceProps.Humanlike)
        {
          --cellsUntilClamor;
          if (cellsUntilClamor <= 0)
          {
            GenClamor.DoClamor(actor, 7f, ClamorDefOf.Movement);
            cellsUntilClamor = 12;
          }
        }
        actor.filth.Notify_EnteredNewCell();
        if ((double) actor.BodySize > 0.899999976158142)
          actor.Map.snowGrid.AddDepth(actor.Position, -1f / 1000f);
        Building_Door buildingDoor = actor.Map.thingGrid.ThingAt<Building_Door>(lastCell);
        if (buildingDoor != null && !actor.HostileTo(buildingDoor))
        {
          buildingDoor.CheckFriendlyTouched(actor);
          if (!buildingDoor.BlockedOpenMomentary && !buildingDoor.HoldOpen && (buildingDoor.SlowsPawns && buildingDoor.PawnCanOpen(actor)))
          {
            buildingDoor.StartManualCloseBy(actor);
            return;
          }
        }
        if (NeedNewPath() && !TrySetNewPath())
          return;
        if (AtDestinationPosition())
          PatherArrived();
        else
          SetupMoveIntoNextCell();
      }
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
      Building edifice = c.GetEdifice(actor.Map);
      if (edifice != null)
        a += edifice.PathWalkCostFor(actor);
      if (a > 450)
        a = 450;
      if (actor.CurJob != null)
      {
        Actor locomotionUrgencySameAs = actor.jobs.curDriver.locomotionUrgencySameAs;
        if (locomotionUrgencySameAs != null && locomotionUrgencySameAs != actor && locomotionUrgencySameAs.Spawned)
        {
          int moveIntoCell = CostToMoveIntoCell(locomotionUrgencySameAs, c);
          if (a < moveIntoCell)
            a = moveIntoCell;
        }
        else
        {
          switch (actor.jobs.curJob.locomotionUrgency)
          {
            case LocomotionUrgency.Amble:
              a *= 3;
              if (a < 60)
              {
                a = 60;
              }
              break;
            case LocomotionUrgency.Walk:
              a *= 2;
              if (a < 50)
              {
                a = 50;
              }
              break;
            case LocomotionUrgency.Jog:
              a = a;
              break;
            case LocomotionUrgency.Sprint:
              a = Mathf.RoundToInt(a * 0.75f);
              break;
          }
        }
      }
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
      PawnPath newPath = GenerateNewPath();
      if (!newPath.Found)
      {
        PatherFailed();
        return false;
      }
      if (curPath != null)
        curPath.ReleaseToPool();
      curPath = newPath;
      for (int nodesAhead = 0; nodesAhead < 20 && nodesAhead < curPath.NodesLeftCount; ++nodesAhead)
      {
        IntVec3 c = curPath.Peek(nodesAhead);
        if (PawnUtility.ShouldCollideWithPawns(actor) && PawnUtility.AnyPawnBlockingPathAt(c, actor, false, false, false))
          foundPathWhichCollidesWithPawns = Find.TickManager.TicksGame;
        if (PawnUtility.KnownDangerAt(c, actor.Map, actor))
          foundPathWithDanger = Find.TickManager.TicksGame;
        if (foundPathWhichCollidesWithPawns == Find.TickManager.TicksGame && foundPathWithDanger == Find.TickManager.TicksGame)
          break;
      }
      return true;
    }

    private PawnPath GenerateNewPath()
    {
      lastPathedTargetPosition = destination.Cell;
      return actor.Map.pathFinder.FindPath(actor.Position, destination, actor, peMode);
    }

    private bool AtDestinationPosition()
    {
      return actor.CanReachImmediate(destination, peMode);
    }

    private bool NeedNewPath()
    {
      if (!destination.IsValid || curPath == null || (!curPath.Found || curPath.NodesLeftCount == 0) || destination.HasThing && destination.Thing.Map != actor.Map || (actor.Position.InHorDistOf(curPath.LastNode, 15f) || actor.Position.InHorDistOf(destination.Cell, 15f)) && !ReachabilityImmediate.CanReachImmediate(curPath.LastNode, destination, actor.Map, peMode, actor) || curPath.UsedRegionHeuristics && curPath.NodesConsumedCount >= 75)
        return true;
      if (lastPathedTargetPosition != destination.Cell)
      {
        float horizontalSquared = (actor.Position - destination.Cell).LengthHorizontalSquared;
        float num = (double) horizontalSquared <= 900.0 ? ((double) horizontalSquared <= 289.0 ? ((double) horizontalSquared <= 100.0 ? ((double) horizontalSquared <= 49.0 ? 0.5f : 2f) : 3f) : 5f) : 10f;
        if ((lastPathedTargetPosition - destination.Cell).LengthHorizontalSquared > num * (double) num)
          return true;
      }
      bool flag1 = PawnUtility.ShouldCollideWithPawns(actor);
      bool flag2 = curPath.NodesLeftCount < 30;
      IntVec3 other = IntVec3.Invalid;
      for (int nodesAhead = 0; nodesAhead < 20 && nodesAhead < curPath.NodesLeftCount; ++nodesAhead)
      {
        IntVec3 c = curPath.Peek(nodesAhead);
        if (!c.Walkable(actor.Map) || flag1 && !BestPathHadPawnsInTheWayRecently() && (PawnUtility.AnyPawnBlockingPathAt(c, actor, false, true, false) || flag2 && PawnUtility.AnyPawnBlockingPathAt(c, actor, false, false, false)) || !BestPathHadDangerRecently() && PawnUtility.KnownDangerAt(c, actor.Map, actor))
          return true;
        Building_Door edifice = c.GetEdifice(actor.Map) as Building_Door;
        if (edifice != null && (!edifice.CanPhysicallyPass(actor) && !actor.HostileTo(edifice) || edifice.IsForbiddenToPass(actor)) || nodesAhead != 0 && c.AdjacentToDiagonal(other) && (PathFinder.BlocksDiagonalMovement(c.x, other.z, actor.Map) || PathFinder.BlocksDiagonalMovement(other.x, c.z, actor.Map)))
          return true;
        other = c;
      }
      return false;
    }

    private bool BestPathHadPawnsInTheWayRecently()
    {
      return foundPathWhichCollidesWithPawns + 240 > Find.TickManager.TicksGame;
    }

    private bool BestPathHadDangerRecently()
    {
      return foundPathWithDanger + 240 > Find.TickManager.TicksGame;
    }

    private bool FailedToFindCloseUnoccupiedCellRecently()
    {
      return failedToFindCloseUnoccupiedCellTicks + 100 > Find.TickManager.TicksGame;
    }
  }
}