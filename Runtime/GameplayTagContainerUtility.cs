using UnityEngine.Pool;

namespace BandoWare.GameplayTags
{
   public static class GameplayTagContainerUtility
   {
      public static bool HasAll<T, U, V>(this T containerA, in U containerB, in V other) where T : IGameplayTagContainer where U : IGameplayTagContainer where V : IGameplayTagContainer
      {
         if (containerA.IsEmpty && containerB.IsEmpty)
         {
            return true;
         }

         if (containerA.IsEmpty)
         {
            return containerB.HasAll(other);
         }

         if (containerB.IsEmpty)
         {
            return containerA.HasAll(other);
         }

         using (GenericPool<GameplayTagContainer>.Get(out GameplayTagContainer intersection))
         {
            intersection.AddIntersection(containerA, containerB);
            bool hasAll = intersection.HasAll(other);
            intersection.Clear();

            return hasAll;
         }
      }

      public static bool HasAllExact<T, U, V>(this T containerA, in U containerB, in V other) where T : IGameplayTagContainer where U : IGameplayTagContainer where V : IGameplayTagContainer
      {
         if (containerA.IsEmpty && containerB.IsEmpty)
         {
            return true;
         }

         if (containerA.IsEmpty)
         {
            return containerB.HasAllExact(other);
         }

         if (containerB.IsEmpty)
         {
            return containerA.HasAllExact(other);
         }

         using (GenericPool<GameplayTagContainer>.Get(out GameplayTagContainer intersection))
         {
            intersection.AddIntersection(containerA, containerB);
            bool hasAllExact = intersection.HasAllExact(other);
            intersection.Clear();

            return hasAllExact;
         }
      }
   }
}
