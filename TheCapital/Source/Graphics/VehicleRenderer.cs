using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TheCapital
{
  public class VehicleRenderer
  {
    private Vehicle vehicle;
    public VehicleGraphicSet graphics;
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

    public VehicleRenderer(Vehicle vehicle)
    {
      this.vehicle = vehicle;
      graphics = new VehicleGraphicSet(vehicle);
    }

    private ConditionDrawMode CurConditionDrawMode
    {
      get
      {
        if (this.vehicle.isDestroyed && this.pawn.Corpse != null)
          return this.pawn.Corpse.CurRotDrawMode;
        return RotDrawMode.Fresh;
      }
    }

    public void RenderPawnAt(Vector3 drawLoc)
    {
      this.RenderPawnAt(drawLoc, this.CurRotDrawMode, !this.pawn.health.hediffSet.HasHead);
    }

    public void RenderPawnAt(Vector3 drawLoc, RotDrawMode bodyDrawType, bool headStump)
    {
      if (!this.graphics.AllResolved)
        this.graphics.ResolveAllGraphics();
      if (this.pawn.GetPosture() == PawnPosture.Standing)
      {
        this.RenderPawnInternal(drawLoc, 0.0f, true, bodyDrawType, headStump);
        if (this.pawn.carryTracker != null)
        {
          Thing carriedThing = this.pawn.carryTracker.CarriedThing;
          if (carriedThing != null)
          {
            Vector3 drawPos = drawLoc;
            bool behind = false;
            bool flip = false;
            if (this.pawn.CurJob == null || !this.pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref drawPos, ref behind, ref flip))
            {
              if (carriedThing is Pawn || carriedThing is Corpse)
                drawPos += new Vector3(0.44f, 0.0f, 0.0f);
              else
                drawPos += new Vector3(0.18f, 0.0f, 0.05f);
            }
            if (behind)
              drawPos.y -= 5f / 128f;
            else
              drawPos.y += 5f / 128f;
            carriedThing.DrawAt(drawPos, flip);
          }
        }
        if (this.pawn.def.race.specialShadowData != null)
        {
          if (this.shadowGraphic == null)
            this.shadowGraphic = new Graphic_Shadow(this.pawn.def.race.specialShadowData);
          this.shadowGraphic.Draw(drawLoc, Rot4.North, (Thing) this.pawn, 0.0f);
        }
        if (this.graphics.nakedGraphic != null && this.graphics.nakedGraphic.ShadowGraphic != null)
          this.graphics.nakedGraphic.ShadowGraphic.Draw(drawLoc, Rot4.North, (Thing) this.pawn, 0.0f);
      }
      else
      {
        Rot4 rot4_1 = this.LayingFacing();
        Building_Bed buildingBed = this.pawn.CurrentBed();
        bool renderBody;
        float angle;
        Vector3 rootLoc;
        if (buildingBed != null && this.pawn.RaceProps.Humanlike)
        {
          renderBody = buildingBed.def.building.bed_showSleeperBody;
          Rot4 rotation = buildingBed.Rotation;
          rotation.AsInt += 2;
          angle = rotation.AsAngle;
          Vector3 shiftedWithAltitude = this.pawn.Position.ToVector3ShiftedWithAltitude((AltitudeLayer) Mathf.Max((int) buildingBed.def.altitudeLayer, 15));
          shiftedWithAltitude.y += 7f / 256f;
          float num = -this.BaseHeadOffsetAt(Rot4.South).z;
          Vector3 vector3 = rotation.FacingCell.ToVector3();
          rootLoc = shiftedWithAltitude + vector3 * num;
          rootLoc.y += 1f / 128f;
        }
        else
        {
          renderBody = true;
          rootLoc = drawLoc;
          if (!this.pawn.Dead && this.pawn.CarriedBy == null)
            rootLoc.y = AltitudeLayer.LayingPawn.AltitudeFor() + 1f / 128f;
          if (this.pawn.Downed || this.pawn.Dead)
            angle = this.wiggler.downedAngle;
          else if (this.pawn.RaceProps.Humanlike)
          {
            angle = rot4_1.AsAngle;
          }
          else
          {
            Rot4 rot4_2 = Rot4.West;
            switch (this.pawn.thingIDNumber % 2)
            {
              case 0:
                rot4_2 = Rot4.West;
                break;
              case 1:
                rot4_2 = Rot4.East;
                break;
            }
            angle = rot4_2.AsAngle;
          }
        }
        this.RenderPawnInternal(rootLoc, angle, renderBody, rot4_1, rot4_1, bodyDrawType, false, headStump);
      }
      if (this.pawn.Spawned && !this.pawn.Dead)
      {
        this.pawn.stances.StanceTrackerDraw();
        this.pawn.pather.PatherDraw();
      }
      this.DrawDebug();
    }

    public void RenderPortrait()
    {
      Vector3 zero = Vector3.zero;
      float angle;
      if (this.pawn.Dead || this.pawn.Downed)
      {
        angle = 85f;
        zero.x -= 0.18f;
        zero.z -= 0.18f;
      }
      else
        angle = 0.0f;
      this.RenderPawnInternal(zero, angle, true, Rot4.South, Rot4.South, this.CurRotDrawMode, true, !this.pawn.health.hediffSet.HasHead);
    }

    private void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, RotDrawMode draw, bool headStump)
    {
      this.RenderPawnInternal(rootLoc, angle, renderBody, this.pawn.Rotation, this.pawn.Rotation, draw, false, headStump);
    }

    private void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
    {
      if (!this.graphics.AllResolved)
        this.graphics.ResolveAllGraphics();
      Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
      Mesh mesh1 = (Mesh) null;
      if (renderBody)
      {
        Vector3 loc = rootLoc;
        loc.y += 1f / 128f;
        if (bodyDrawType == RotDrawMode.Dessicated && !this.pawn.RaceProps.Humanlike && (this.graphics.dessicatedGraphic != null && !portrait))
        {
          this.graphics.dessicatedGraphic.Draw(loc, bodyFacing, (Thing) this.pawn, angle);
        }
        else
        {
          mesh1 = !this.pawn.RaceProps.Humanlike ? this.graphics.nakedGraphic.MeshAt(bodyFacing) : MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
          List<Material> materialList = this.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
          for (int index = 0; index < materialList.Count; ++index)
          {
            Material damagedMat = this.graphics.flasher.GetDamagedMat(materialList[index]);
            GenDraw.DrawMeshNowOrLater(mesh1, loc, quaternion, damagedMat, portrait);
            loc.y += 1f / 256f;
          }
          if (bodyDrawType == RotDrawMode.Fresh)
          {
            Vector3 drawLoc = rootLoc;
            drawLoc.y += 5f / 256f;
            this.woundOverlays.RenderOverBody(drawLoc, mesh1, quaternion, portrait);
          }
        }
      }
      Vector3 vector3_1 = rootLoc;
      Vector3 vector3_2 = rootLoc;
      if (bodyFacing != Rot4.North)
      {
        vector3_2.y += 7f / 256f;
        vector3_1.y += 3f / 128f;
      }
      else
      {
        vector3_2.y += 3f / 128f;
        vector3_1.y += 7f / 256f;
      }
      if (this.graphics.headGraphic != null)
      {
        Vector3 vector3_3 = quaternion * this.BaseHeadOffsetAt(headFacing);
        Material mat1 = this.graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
        if ((Object) mat1 != (Object) null)
          GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(headFacing), vector3_2 + vector3_3, quaternion, mat1, portrait);
        Vector3 loc1 = rootLoc + vector3_3;
        loc1.y += 1f / 32f;
        bool flag = false;
        if (!portrait || !Prefs.HatsOnlyOnMap)
        {
          Mesh mesh2 = this.graphics.HairMeshSet.MeshAt(headFacing);
          List<ApparelGraphicRecord> apparelGraphics = this.graphics.apparelGraphics;
          for (int index = 0; index < apparelGraphics.Count; ++index)
          {
            if (apparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
            {
              if (!apparelGraphics[index].sourceApparel.def.apparel.hatRenderedFrontOfFace)
              {
                flag = true;
                Material damagedMat = this.graphics.flasher.GetDamagedMat(apparelGraphics[index].graphic.MatAt(bodyFacing, (Thing) null));
                GenDraw.DrawMeshNowOrLater(mesh2, loc1, quaternion, damagedMat, portrait);
              }
              else
              {
                Material damagedMat = this.graphics.flasher.GetDamagedMat(apparelGraphics[index].graphic.MatAt(bodyFacing, (Thing) null));
                Vector3 loc2 = rootLoc + vector3_3;
                loc2.y += !(bodyFacing == Rot4.North) ? 9f / 256f : 1f / 256f;
                GenDraw.DrawMeshNowOrLater(mesh2, loc2, quaternion, damagedMat, portrait);
              }
            }
          }
        }
        if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
        {
          Mesh mesh2 = this.graphics.HairMeshSet.MeshAt(headFacing);
          Material mat2 = this.graphics.HairMatAt(headFacing);
          GenDraw.DrawMeshNowOrLater(mesh2, loc1, quaternion, mat2, portrait);
        }
      }
      if (renderBody)
      {
        for (int index = 0; index < this.graphics.apparelGraphics.Count; ++index)
        {
          ApparelGraphicRecord apparelGraphic = this.graphics.apparelGraphics[index];
          if (apparelGraphic.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell)
          {
            Material damagedMat = this.graphics.flasher.GetDamagedMat(apparelGraphic.graphic.MatAt(bodyFacing, (Thing) null));
            GenDraw.DrawMeshNowOrLater(mesh1, vector3_1, quaternion, damagedMat, portrait);
          }
        }
      }
      if (!portrait && this.pawn.RaceProps.Animal && (this.pawn.inventory != null && this.pawn.inventory.innerContainer.Count > 0) && this.graphics.packGraphic != null)
        Graphics.DrawMesh(mesh1, vector3_1, quaternion, this.graphics.packGraphic.MatAt(bodyFacing, (Thing) null), 0);
      if (portrait)
        return;
      this.DrawEquipment(rootLoc);
      if (this.pawn.apparel != null)
      {
        List<Apparel> wornApparel = this.pawn.apparel.WornApparel;
        for (int index = 0; index < wornApparel.Count; ++index)
          wornApparel[index].DrawWornExtras();
      }
      Vector3 bodyLoc = rootLoc;
      bodyLoc.y += 11f / 256f;
      this.statusOverlays.RenderStatusOverlays(bodyLoc, quaternion, MeshPool.humanlikeHeadSet.MeshAt(headFacing));
    }

    private void DrawEquipment(Vector3 rootLoc)
    {
      if (this.pawn.Dead || !this.pawn.Spawned || (this.pawn.equipment == null || this.pawn.equipment.Primary == null) || this.pawn.CurJob != null && this.pawn.CurJob.def.neverShowWeapon)
        return;
      Stance_Busy curStance = this.pawn.stances.curStance as Stance_Busy;
      if (curStance != null && !curStance.neverAimWeapon && curStance.focusTarg.IsValid)
      {
        Vector3 vector3 = !curStance.focusTarg.HasThing ? curStance.focusTarg.Cell.ToVector3Shifted() : curStance.focusTarg.Thing.DrawPos;
        float num = 0.0f;
        if ((double) (vector3 - this.pawn.DrawPos).MagnitudeHorizontalSquared() > 1.0 / 1000.0)
          num = (vector3 - this.pawn.DrawPos).AngleFlat();
        Vector3 drawLoc = rootLoc + new Vector3(0.0f, 0.0f, 0.4f).RotatedBy(num);
        drawLoc.y += 5f / 128f;
        this.DrawEquipmentAiming((Thing) this.pawn.equipment.Primary, drawLoc, num);
      }
      else
      {
        if (!this.CarryWeaponOpenly())
          return;
        if (this.pawn.Rotation == Rot4.South)
        {
          Vector3 drawLoc = rootLoc + new Vector3(0.0f, 0.0f, -0.22f);
          drawLoc.y += 5f / 128f;
          this.DrawEquipmentAiming((Thing) this.pawn.equipment.Primary, drawLoc, 143f);
        }
        else if (this.pawn.Rotation == Rot4.North)
        {
          Vector3 drawLoc = rootLoc + new Vector3(0.0f, 0.0f, -0.11f);
          // ISSUE: explicit reference operation
          // ISSUE: variable of a reference type
          Vector3& local = @drawLoc;
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          (^local).y = (^local).y;
          this.DrawEquipmentAiming((Thing) this.pawn.equipment.Primary, drawLoc, 143f);
        }
        else if (this.pawn.Rotation == Rot4.East)
        {
          Vector3 drawLoc = rootLoc + new Vector3(0.2f, 0.0f, -0.22f);
          drawLoc.y += 5f / 128f;
          this.DrawEquipmentAiming((Thing) this.pawn.equipment.Primary, drawLoc, 143f);
        }
        else
        {
          if (!(this.pawn.Rotation == Rot4.West))
            return;
          Vector3 drawLoc = rootLoc + new Vector3(-0.2f, 0.0f, -0.22f);
          drawLoc.y += 5f / 128f;
          this.DrawEquipmentAiming((Thing) this.pawn.equipment.Primary, drawLoc, 217f);
        }
      }
    }

    public void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
    {
      float num1 = aimAngle - 90f;
      Mesh mesh;
      float num2;
      if ((double) aimAngle > 20.0 && (double) aimAngle < 160.0)
      {
        mesh = MeshPool.plane10;
        num2 = num1 + eq.def.equippedAngleOffset;
      }
      else if ((double) aimAngle > 200.0 && (double) aimAngle < 340.0)
      {
        mesh = MeshPool.plane10Flip;
        num2 = num1 - 180f - eq.def.equippedAngleOffset;
      }
      else
      {
        mesh = MeshPool.plane10;
        num2 = num1 + eq.def.equippedAngleOffset;
      }
      float angle = num2 % 360f;
      Graphic_StackCount graphic = eq.Graphic as Graphic_StackCount;
      Material material = graphic == null ? eq.Graphic.MatSingle : graphic.SubGraphicForStackCount(1, eq.def).MatSingle;
      Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(angle, Vector3.up), material, 0);
    }

    private bool CarryWeaponOpenly()
    {
      return (this.pawn.carryTracker == null || this.pawn.carryTracker.CarriedThing == null) && (this.pawn.Drafted || this.pawn.CurJob != null && this.pawn.CurJob.def.alwaysShowWeapon || this.pawn.mindState.duty != null && this.pawn.mindState.duty.def.alwaysShowWeapon);
    }

    private Rot4 LayingFacing()
    {
      if (this.pawn.GetPosture() == PawnPosture.LayingOnGroundFaceUp)
        return Rot4.South;
      if (this.pawn.RaceProps.Humanlike)
      {
        switch (this.pawn.thingIDNumber % 4)
        {
          case 0:
            return Rot4.South;
          case 1:
            return Rot4.South;
          case 2:
            return Rot4.East;
          case 3:
            return Rot4.West;
        }
      }
      else
      {
        switch (this.pawn.thingIDNumber % 4)
        {
          case 0:
            return Rot4.South;
          case 1:
            return Rot4.East;
          case 2:
            return Rot4.West;
          case 3:
            return Rot4.West;
        }
      }
      return Rot4.Random;
    }

    public Vector3 BaseHeadOffsetAt(Rot4 rotation)
    {
      Vector2 headOffset = this.pawn.story.bodyType.headOffset;
      switch (rotation.AsInt)
      {
        case 0:
          return new Vector3(0.0f, 0.0f, headOffset.y);
        case 1:
          return new Vector3(headOffset.x, 0.0f, headOffset.y);
        case 2:
          return new Vector3(0.0f, 0.0f, headOffset.y);
        case 3:
          return new Vector3(-headOffset.x, 0.0f, headOffset.y);
        default:
          Log.Error("BaseHeadOffsetAt error in " + (object) this.pawn, false);
          return Vector3.zero;
      }
    }

    public void Notify_DamageApplied(DamageInfo dam)
    {
      this.graphics.flasher.Notify_DamageApplied(dam);
      this.wiggler.Notify_DamageApplied(dam);
    }

    public void RendererTick()
    {
      this.wiggler.WigglerTick();
      this.effecters.EffectersTick();
    }

    private void DrawDebug()
    {
      if (!DebugViewSettings.drawDuties || !Find.Selector.IsSelected((object) this.pawn) || (this.pawn.mindState == null || this.pawn.mindState.duty == null))
        return;
      this.pawn.mindState.duty.DrawDebug(this.pawn);
    }
  }
}