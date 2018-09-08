using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TheCapital
{
  public class DeadActor : ThingWithComps, IThingHolder, IThoughtGiver, IStrippable, IBillGiver
  {
    public int timeOfDeath = -1;
    private int vanishAfterTimestamp = -1;
    private ThingOwner<Actor> innerContainer;
    private BillStack operationsBillStack;
    public bool everBuriedInSarcophagus;
    private const int VanishAfterTicksSinceDessicated = 6000000;

    public DeadActor()
    {
      operationsBillStack = new BillStack(this);
      innerContainer = new ThingOwner<Actor>(this, true, LookMode.Reference);
    }

    public Actor InnerActor
    {
      get
      {
        if (innerContainer.Count > 0)
          return innerContainer[0];
        return null;
      }
      set
      {
        if (value == null)
        {
          innerContainer.Clear();
        }
        else
        {
          if (innerContainer.Count > 0)
          {
            Log.Error("Setting InnerActor in corpse that already has one.", false);
            innerContainer.Clear();
          }
          innerContainer.TryAdd(value, true);
        }
      }
    }

    public int Age
    {
      get
      {
        return Find.TickManager.TicksGame - timeOfDeath;
      }
      set
      {
        timeOfDeath = Find.TickManager.TicksGame - value;
      }
    }

    public override string Label
    {
      get
      {
        if (Bugged)
        {
          Log.ErrorOnce("Corpse.Label while Bugged", 57361644, false);
          return string.Empty;
        }
        return "DeadLabel".Translate((object) InnerActor.Label);
      }
    }

    public override bool IngestibleNow => false;

    public ConditionDrawMode CurConditionDrawMode
    {
      get
      {
        CompRottable comp = GetComp<CompRottable>();
        if (comp != null)
        {
          if (comp.Stage == RotStage.Rotting)
            return ConditionDrawMode.Weathered;
          if (comp.Stage == RotStage.Dessicated)
            return ConditionDrawMode.Damaged;
        }
        return ConditionDrawMode.Pristine;
      }
    }

    private bool ShouldVanish => false;

    public BillStack BillStack => operationsBillStack;

    public IEnumerable<IntVec3> IngredientStackCells
    {
      get
      {
        var list = new List<IntVec3>();
        return list;
      }
    }

    public bool Bugged => false;

    public bool CurrentlyUsableForBills()
    {
      return InteractionCell.IsValid;
    }

    public bool UsableForBillsAfterFueling()
    {
      return CurrentlyUsableForBills();
    }

    public bool AnythingToStrip()
    {
      return false;
    }

    public ThingOwner GetDirectlyHeldThings()
    {
      return innerContainer;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
      ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public override void PostMake()
    {
      base.PostMake();
      timeOfDeath = Find.TickManager.TicksGame;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
      if (Bugged)
      {
        Log.Error(this + " spawned in bugged state.");
      }
      else
      {
        base.SpawnSetup(map, respawningAfterLoad);
        InnerActor.Rotation = Rot4.South;
        NotifyColonistBar();
      }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
      base.DeSpawn(mode);
      if (Bugged)
        return;
      NotifyColonistBar();
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
      Actor actor = null;
      if (!Bugged)
      {
        actor = InnerActor;
        NotifyColonistBar();
        innerContainer.Clear();
      }
      base.Destroy(mode);
      if (actor == null)
        return;
      PostCorpseDestroy(actor);
    }

    public static void PostCorpseDestroy(Actor actor)
    {
      // Remove related shiet such as inventory, equipment, etc
    }

    public override void TickRare()
    {
      base.TickRare();
      if (Destroyed)
        return;
      if (Bugged)
      {
        Log.Error(this + " has null innerPawn. Destroying.");
        Destroy(DestroyMode.Vanish);
      }
      else
      {
        InnerActor.TickRare();
        if (vanishAfterTimestamp < 0)
          vanishAfterTimestamp = Age + 6000000;
        if (!ShouldVanish)
          return;
        Destroy(DestroyMode.Vanish);
      }
    }

    protected override void IngestedCalculateAmounts(Pawn ingester, float nutritionWanted, out int numTaken, out float nutritionIngested)
    {
      numTaken = 0;
      nutritionIngested = 0;
    }

    public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
    {
      return new List<Thing>();
    }

    public override void ExposeData()
    {
      base.ExposeData();
      Scribe_Values.Look(ref timeOfDeath, "timeOfDeath");
      Scribe_Values.Look(ref vanishAfterTimestamp, "vanishAfterTimestamp");
      Scribe_Values.Look(ref everBuriedInSarcophagus, "everBuriedInSarcophagus");
      Scribe_Deep.Look(ref operationsBillStack, "operationsBillStack", (object) this);
      Scribe_Deep.Look(ref innerContainer, "innerContainer", (object) this);
    }

    public void Strip()
    {
      //this.InnerActor.Strip();
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
      InnerActor.Drawer.renderer.RenderActorAt(drawLoc);
    }

    public Thought_Memory GiveObservedThought()
    {
      // Is this shit disgusting? Might leave a negative impression and this is where you do this shiet
      return null;
    }

    public override string GetInspectString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("DeadTime".Translate((object) Age.ToStringTicksToPeriodVague(true, false)));
      stringBuilder.AppendLine(base.GetInspectString());
      
      return stringBuilder.ToString().TrimEndNewlines();
    }

    [DebuggerHidden]
    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {

      return new List<StatDrawEntry>();
    }

    private void NotifyColonistBar()
    {
      if (InnerActor.Faction != Faction.OfPlayer || Current.ProgramState != ProgramState.Playing)
        return;
      Find.ColonistBar.MarkColonistsDirty();
    }
  }
}