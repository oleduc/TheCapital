using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TheCapital
{
  public class ActorRenderer
  {
    private Actor actor;
    public ActorGraphicSet graphics;
    private Graphic_Shadow shadowGraphic;
    private const float CarriedThingDrawAngle = 16f;
    private const float SubInterval = 0.00390625f;
    private const float YOffset_PrimaryEquipmentUnder = 0.0f;
    private const float YOffset_Behind = 0.00390625f;
    private const float YOffset_Body = 0.0078125f;
    private const float YOffsetInterval_Clothes = 0.00390625f;
    private const float YOffset_Wounds = 0.01953125f;
    private const float YOffset_Shell = 0.0234375f;
    private const float YOffset_Head = 0.02734375f;
    private const float YOffset_OnHead = 0.03125f;
    private const float YOffset_PostHead = 0.03515625f;
    private const float YOffset_CarriedThing = 0.0390625f;
    private const float YOffset_PrimaryEquipmentOver = 0.0390625f;
    private const float YOffset_Status = 0.04296875f;

    public ActorRenderer(Actor actor)
    {
      this.actor = actor;
      graphics = new ActorGraphicSet(actor);
    }

    private ConditionDrawMode CurConditionDrawMode => ConditionDrawMode.Pristine;

    public void RenderActorAt(Vector3 drawLoc)
    {
      RenderActorAt(drawLoc, CurConditionDrawMode);
    }

    public void RenderActorAt(Vector3 drawLoc, ConditionDrawMode bodyDrawType)
    {
      if (!graphics.AllResolved)
        graphics.ResolveAllGraphics();
    }

    public void RenderPortrait()
    {

    }
    
    public void RendererTick()
    {

    }
    
    public void Notify_DamageApplied(DamageInfo dam)
    {
      graphics.flasher.Notify_DamageApplied(dam);
    }
  }
}