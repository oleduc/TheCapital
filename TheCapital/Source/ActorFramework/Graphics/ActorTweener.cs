using UnityEngine;
using Verse;

namespace TheCapital
{
  public class ActorTweener
  {
    private Vector3 tweenedPos = new Vector3(0.0f, 0.0f, 0.0f);
    private int lastDrawFrame = -1;
    private Actor actor;
    private Vector3 lastTickSpringPos;
    private const float SpringTightness = 0.09f;

    public ActorTweener(Actor actor)
    {
      this.actor = actor;
    }

    public Vector3 TweenedPos => this.tweenedPos;

    public Vector3 LastTickTweenedVelocity => TweenedPos - lastTickSpringPos;

    public void PreDrawPosCalculation()
    {
      if (lastDrawFrame == RealTime.frameCount)
        return;
      if (lastDrawFrame < RealTime.frameCount - 1)
      {
        ResetTweenedPosToRoot();
      }
      else
      {
        lastTickSpringPos = tweenedPos;
        float tickRateMultiplier = Find.TickManager.TickRateMultiplier;
        if (tickRateMultiplier < 5.0)
        {
          Vector3 vector3 = TweenedPosRoot() - tweenedPos;
          float a = (float) (0.0900000035762787 * (RealTime.deltaTime * 60.0 * tickRateMultiplier));
          if (RealTime.deltaTime > 0.0500000007450581)
            a = Mathf.Min(a, 1f);
          tweenedPos += vector3 * a;
        }
        else
          tweenedPos = TweenedPosRoot();
      }
      lastDrawFrame = RealTime.frameCount;
    }

    public void ResetTweenedPosToRoot()
    {
      tweenedPos = TweenedPosRoot();
      lastTickSpringPos = tweenedPos;
    }

    private Vector3 TweenedPosRoot()
    {
      if (!actor.Spawned)
        return actor.Position.ToVector3Shifted();
      float num = MovedPercent();
      
      return actor.pather.nextCell.ToVector3Shifted() * num + actor.Position.ToVector3Shifted() * (1f - num);
    }

    private float MovedPercent()
    {
      if (!actor.pather.Moving || actor.pather.WillCollideWithPawnOnNextPathCell())
        return 0.0f;
      
      return (float) (1.0 - (double) actor.pather.nextCellCostLeft / (double) actor.pather.nextCellCostTotal);
    }
  }
}