using Verse;

namespace TheCapital.Utilities
{
    public static class ActorConverter
     {
         public static Pawn ActorToPawn(Actor actor)
         {
             Pawn pawn = (Pawn) (ThingWithComps) actor;
             // Add missing stuff

             
             return (Pawn) (ThingWithComps) actor;
         }
     }
 }