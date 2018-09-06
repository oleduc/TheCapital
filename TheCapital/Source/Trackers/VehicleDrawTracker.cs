using RimWorld;
using UnityEngine;
using Verse;

namespace TheCapital.Trackers
{
  public class VehicleDrawTracker
  {
    private Vehicle vehicle;
    public ActorTweener tweener;
    private JitterHandler jitterer;
    public PawnLeaner leaner;
    public VehicleRenderer renderer;
    public PawnUIOverlay ui;
    private PawnFootprintMaker footprintMaker;
    private PawnBreathMoteMaker breathMoteMaker;
    private const float MeleeJitterDistance = 0.5f;

    public VehicleDrawTracker(Vehicle vehicle)
    {
      this.vehicle = vehicle;
      tweener = new ActorTweener(vehicle);
      jitterer = new JitterHandler();
      leaner = new ActorLeaner(pawn);
      renderer = new ActorRenderer(pawn);
      ui = new ActorUIOverlay(pawn);
      footprintMaker = new ActorFootprintMaker(pawn);
      breathMoteMaker = new ActorBreathMoteMaker(pawn);
    }

    public Vector3 DrawPos
    {
      get
      {
        tweener.PreDrawPosCalculation();
        Vector3 vector3 = tweener.TweenedPos + jitterer.CurrentOffset + leaner.LeanOffset;
        vector3.y = vehicle.def.Altitude;
        return vector3;
      }
    }

    public void DrawTrackerTick()
    {
      if (!this.vehicle.Spawned || Current.ProgramState == ProgramState.Playing && !Find.CameraDriver.CurrentViewRect.ExpandedBy(3).Contains(this.vehicle.Position))
        return;
      this.jitterer.JitterHandlerTick();
      this.footprintMaker.FootprintMakerTick();
      this.breathMoteMaker.BreathMoteMakerTick();
      this.leaner.LeanerTick();
      this.renderer.RendererTick();
    }

    public void DrawAt(Vector3 loc)
    {
      this.renderer.RenderActorAt(loc);
    }

    public void Notify_Spawned()
    {
      this.tweener.ResetTweenedPosToRoot();
    }

    public void Notify_WarmingCastAlongLine(ShootLine newShootLine, IntVec3 ShootPosition)
    {
      this.leaner.Notify_WarmingCastAlongLine(newShootLine, ShootPosition);
    }

    public void Notify_DamageApplied(DamageInfo dinfo)
    {
      if (this.vehicle.Destroyed)
        return;
      this.jitterer.Notify_DamageApplied(dinfo);
      this.renderer.Notify_DamageApplied(dinfo);
    }

    public void Notify_DamageDeflected(DamageInfo dinfo)
    {
      if (this.vehicle.Destroyed)
        return;
      this.jitterer.Notify_DamageDeflected(dinfo);
    }

    public void Notify_MeleeAttackOn(Thing Target)
    {
      if (Target.Position != this.vehicle.Position)
      {
        this.jitterer.AddOffset(0.5f, (Target.Position - this.vehicle.Position).AngleFlat);
      }
      else
      {
        if (!(Target.DrawPos != this.vehicle.DrawPos))
          return;
        this.jitterer.AddOffset(0.25f, (Target.DrawPos - this.vehicle.DrawPos).AngleFlat());
      }
    }

    public void Notify_DebugAffected()
    {
      for (int index = 0; index < 10; ++index)
        MoteMaker.ThrowAirPuffUp(this.vehicle.DrawPos, this.vehicle.Map);
      this.jitterer.AddOffset(0.05f, (float) Rand.Range(0, 360));
    }
  }
}