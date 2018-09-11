using RimWorld;
using UnityEngine;
using Verse;

namespace TheCapital.Trackers
{
  public class ActorDrawTracker
  {
    private Actor actor;
    public ActorTweener tweener;
    private JitterHandler jitterer;
    public ActorLeaner leaner;
    public ActorRenderer renderer;
    public ActorUIOverlay ui;

    private const float MeleeJitterDistance = 0.5f;

    public ActorDrawTracker(Actor actor)
    {
      this.actor = actor;
      tweener = new ActorTweener(actor);
      jitterer = new JitterHandler();
      leaner = new ActorLeaner(actor);
      renderer = new ActorRenderer(actor);
      ui = new ActorUIOverlay(actor);
    }

    public Vector3 DrawPos
    {
      get
      {
        tweener.PreDrawPosCalculation();
        Vector3 vector3 = tweener.TweenedPos + jitterer.CurrentOffset + leaner.LeanOffset;
        vector3.y = actor.def.Altitude;
        return vector3;
      }
    }

    public void DrawTrackerTick()
    {
      if (!actor.Spawned || Current.ProgramState == ProgramState.Playing && !Find.CameraDriver.CurrentViewRect.ExpandedBy(3).Contains(actor.Position))
        return;
      //jitterer.JitterHandlerTick();
      //leaner.LeanerTick();
      renderer.RendererTick();
    }

    public void DrawAt(Vector3 loc)
    {
      renderer.RenderActorAt(loc);
    }

    public void Notify_Spawned()
    {
      tweener.ResetTweenedPosToRoot();
    }

    public void Notify_WarmingCastAlongLine(ShootLine newShootLine, IntVec3 ShootPosition)
    {
      leaner.Notify_WarmingCastAlongLine(newShootLine, ShootPosition);
    }

    public void Notify_DamageApplied(DamageInfo dinfo)
    {
      if (actor.Destroyed)
        return;
      jitterer.Notify_DamageApplied(dinfo);
      renderer.Notify_DamageApplied(dinfo);
    }

    public void Notify_DamageDeflected(DamageInfo dinfo)
    {
      if (actor.Destroyed)
        return;
      jitterer.Notify_DamageDeflected(dinfo);
    }

    public void Notify_MeleeAttackOn(Thing Target)
    {
      if (Target.Position != actor.Position)
      {
        jitterer.AddOffset(0.5f, (Target.Position - actor.Position).AngleFlat);
      }
      else
      {
        if (!(Target.DrawPos != actor.DrawPos))
          return;
        jitterer.AddOffset(0.25f, (Target.DrawPos - actor.DrawPos).AngleFlat());
      }
    }

    public void Notify_DebugAffected()
    {
      for (int index = 0; index < 10; ++index)
        MoteMaker.ThrowAirPuffUp(actor.DrawPos, actor.Map);
      jitterer.AddOffset(0.05f, Rand.Range(0, 360));
    }
  }
}