using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BandoWare.GameplayTags
{
   public class GameplayTagManager
   {
      private static Dictionary<string, GameplayTagDefinition> s_TagDefinitionsByName = new();
      private static GameplayTagDefinition[] s_TagsDefinitions;
      private static GameplayTag[] s_Tags;
      private static bool s_IsInitialized;

      public static ReadOnlySpan<GameplayTag> GetAllTags()
      {
         InitializeIfNeeded();
         return new ReadOnlySpan<GameplayTag>(s_Tags);
      }

      internal static GameplayTagDefinition GetDefinitionFromRuntimeIndex(int runtimeIndex)
      {
         InitializeIfNeeded();
         return s_TagsDefinitions[runtimeIndex];
      }

      public static GameplayTag RequestTag(string name)
      {
         if (!TryGetDefinition(name, out GameplayTagDefinition definition))
         {
            return GameplayTag.None;
         }

         return definition.Tag;
      }

      public static bool RequestTag(string name, out GameplayTag tag)
      {
         if (TryGetDefinition(name, out GameplayTagDefinition definition))
         {
            tag = definition.Tag;
            return true;
         }

         tag = GameplayTag.None;
         return false;
      }

      private static bool TryGetDefinition(string name, out GameplayTagDefinition definition)
      {
         InitializeIfNeeded();
         return s_TagDefinitionsByName.TryGetValue(name, out definition);
      }

      public static void InitializeIfNeeded()
      {
         if (s_IsInitialized)
         {
            return;
         }

         GamplayTagRegistrationContext context = new();

         foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
         {
            foreach (GameplayTagAttribute attribute in assembly.GetCustomAttributes<GameplayTagAttribute>())
            {
               try
               {
                  context.RegisterTag(attribute.TagName, attribute.Description, attribute.Flags);
               }
               catch (Exception exception)
               {
                  Debug.LogError($"Failed to register tag {attribute.TagName} from assembly {assembly.FullName} with error: {exception.Message}");
               }
            }
         }

         s_TagsDefinitions = context.GenerateDefinitions();

         // Skip the first tag definition which is the "None" tag.
         IEnumerable<GameplayTag> tags = s_TagsDefinitions
            .Select(definition => definition.Tag)
            .Skip(1);

         s_Tags = Enumerable.ToArray(tags);
         foreach (GameplayTagDefinition definition in s_TagsDefinitions)
         {
            s_TagDefinitionsByName[definition.TagName] = definition;
         }

         s_IsInitialized = true;
      }
   }
}