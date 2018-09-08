using System;
using TheCapital.IA;
using Verse;
using Verse.AI;

namespace TheCapital.Utilities
{
    public static class Converter
     {
         public static Pawn ActorToPawn(Actor actor)
         {
             Pawn pawn = (Pawn) (ThingWithComps) actor;
             // Add missing stuff

             
             return (Pawn) (ThingWithComps) actor;
         }
         
         public static ActorPath PawnPathToActorPath(PawnPath pawnPath)
         {
             throw new NotImplementedException();
             return null;
         }
     }
 }