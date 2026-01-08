using System.Collections.Generic;
using UnityEngine.Pool;

namespace BandoWare.GameplayTags
{
   public static class GameplayTagContainerExtensionMethods
   {
      public static bool HasTag<T>(this T container, GameplayTag gameplayTag)
         where T : IReadOnlyGameplayTagContainer
      {
         return container.Indices.Implicit != null && BinarySearchUtility.Search(container.Indices.Implicit, gameplayTag.RuntimeIndex) >= 0;
      }

      public static bool HasTagExact<T>(this T container, GameplayTag gameplayTag)
         where T : IReadOnlyGameplayTagContainer
      {
         return container.Indices.Explicit != null && BinarySearchUtility.Search(container.Indices.Explicit, gameplayTag.RuntimeIndex) >= 0;
      }

      public static bool HasAny<T, U>(this T container, in U other)
         where T : IReadOnlyGameplayTagContainer where U : IReadOnlyGameplayTagContainer
      {
         return HasAnyInternal(container.Indices.Implicit, other?.Indices.Explicit);
      }

      public static bool HasAnyExact<T, U>(this T container, in U other)
         where T : IReadOnlyGameplayTagContainer where U : IReadOnlyGameplayTagContainer
      {
         return HasAnyInternal(container.Indices.Explicit, other?.Indices.Explicit);
      }

      private static bool HasAnyInternal(List<int> tagIndices, List<int> otherTagIndices)
      {
         if (otherTagIndices == null || otherTagIndices.Count == 0 || tagIndices == null || tagIndices.Count == 0)
            return false;

         int start = BinarySearchUtility.Search(tagIndices, otherTagIndices[0], 0, tagIndices.Count - 1);
         if (start >= 0)
            return true;

         start = ~start;

         int end = BinarySearchUtility.Search(tagIndices, otherTagIndices[^1], start, tagIndices.Count - 1);
         if (end >= 0)
            return true;

         end = ~end;

         int j = 1;
         int i = start;
         int otherLimit = otherTagIndices.Count - 1;

         while (i < end && j < otherLimit)
         {
            int tagVal = tagIndices[i];
            int otherVal = otherTagIndices[j];

            if (tagVal == otherVal)
               return true;

            if (tagVal < otherVal)
            {
               i++;
            }
            else
            {
               j++;
            }
         }

         return false;
      }

      private static bool HasAllInternal(List<int> tagIndices, List<int> otherTagIndices)
      {
         if (otherTagIndices == null || otherTagIndices.Count == 0)
            return true;

         if (tagIndices == null || tagIndices.Count == 0)
            return false;

         int start = BinarySearchUtility.Search(tagIndices, otherTagIndices[0], 0, tagIndices.Count - 1);
         if (start < 0)
            return false;

         if (otherTagIndices.Count == 1)
            return true;

         int end = BinarySearchUtility.Search(tagIndices, otherTagIndices[^1], 0, tagIndices.Count - 1);
         if (end < 0)
            return false;

         int j = 1;
         end--;
         for (int i = start + 1; i < end; i++)
         {
            if (otherTagIndices[j] == tagIndices[i])
            {
               j++;
               continue;
            }

            if (otherTagIndices[j] > tagIndices[i])
               return false;
         }

         return j == otherTagIndices.Count - 1;
      }

      public static bool HasAll<T, U>(this T container, in U other)
         where T : IReadOnlyGameplayTagContainer where U : IReadOnlyGameplayTagContainer
      {
         return HasAllInternal(container.Indices.Implicit, other?.Indices.Explicit);
      }

      public static bool HasAll<T, U, V>(this T container, in U otherA, in V otherB)
         where T : IReadOnlyGameplayTagContainer where U : IReadOnlyGameplayTagContainer where V : IReadOnlyGameplayTagContainer
      {
         if (otherA.IsEmpty && otherB.IsEmpty)
            return true;

         if (otherA.IsEmpty)
            return HasAll(container, otherB);

         if (otherB.IsEmpty)
            return HasAll(container, otherA);

         using (GenericPool<GameplayTagContainer>.Get(out GameplayTagContainer intersection))
         {
            intersection.AddIntersection(otherA, otherB);
            bool hasAll = HasAll(container, intersection);
            intersection.Clear();

            return hasAll;
         }
      }

      public static bool HasAllExact<T, U>(this T container, in U other)
         where T : IReadOnlyGameplayTagContainer where U : IReadOnlyGameplayTagContainer
      {
         return HasAllInternal(container.Indices.Explicit, other?.Indices.Explicit);
      }
   }
}
