using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BandoWare.GameplayTags
{
   public enum GameplayTagFlags
   {
      None = 0,
      HideInEditor = 1 << 0,
   }

   internal class GamplayTagRegistrationContext
   {
      private List<GameplayTagDefinition> m_Definition = new();
      private Dictionary<string, GameplayTagDefinition> m_TagsByName = new();

      public void RegisterTag(string name, string description = null, GameplayTagFlags flags = GameplayTagFlags.None)
      {
         GameplayTagUtility.ValidateName(name);

         if (m_TagsByName.ContainsKey(name))
         {
            return;
         }

         GameplayTagDefinition definition = new(name, description, flags);

         m_TagsByName.Add(name, definition);
         m_Definition.Add(definition);
      }

      public GameplayTagDefinition[] GenerateDefinitions()
      {
         RegisterMissingParents();
         SetTagRuntimeIndices();
         FillParentsAndChildren();

         return m_Definition.ToArray();
      }

      private void RegisterMissingParents()
      {
         List<GameplayTagDefinition> definitions = new(m_Definition);
         foreach (GameplayTagDefinition definition in definitions)
         {
            string[] parentTagNames = GameplayTagUtility.GetHeirarchyNames(definition.TagName);

            GameplayTagFlags flags = definition.Flags;
            foreach (string parentTagName in Enumerable.Reverse(parentTagNames))
            {
               if (m_TagsByName.TryGetValue(parentTagName, out GameplayTagDefinition parentTag))
               {
                  flags |= parentTag.Flags;
                  continue;
               }

               RegisterTag(parentTagName, string.Empty, flags);
            }
         }

         m_Definition.Sort((a, b) => string.Compare(a.TagName, b.TagName, StringComparison.OrdinalIgnoreCase));
      }

      private void FillParentsAndChildren()
      {
         Dictionary<GameplayTagDefinition, List<GameplayTagDefinition>> childrenLists = new();

         foreach (GameplayTagDefinition definition in m_Definition)
         {
            string[] parentTagNames = GameplayTagUtility.GetHeirarchyNames(definition.TagName);
            for (int i = 0; i < parentTagNames.Length - 1; i++)
            {
               string parentTagName = parentTagNames[i];
               GameplayTagDefinition parentDefinition = m_TagsByName[parentTagName];
               if (!childrenLists.TryGetValue(parentDefinition, out List<GameplayTagDefinition> children))
               {
                  children = new();
                  childrenLists.Add(parentDefinition, children);
               }

               children.Add(definition);
            }
         }

         foreach ((GameplayTagDefinition definition, List<GameplayTagDefinition> children) in childrenLists)
         {
            definition.SetChildren(children);
            foreach (GameplayTagDefinition child in children)
            {
               child.SetParent(definition);
            }
         }
      }

      private void SetTagRuntimeIndices()
      {
         for (int i = 0; i < m_Definition.Count; i++)
         {
            m_Definition[i].SetRuntimeIndex(i);
         }
      }
   }

   [DebuggerDisplay("{TagName,nq}")]
   internal class GameplayTagDefinition
   {
      public GameplayTag Tag => new(TagName, RuntimeIndex);

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public ReadOnlySpan<GameplayTagDefinition> Children => new(m_Children);

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public ReadOnlySpan<GameplayTag> ParentTags => new(m_ParentTags);

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public ReadOnlySpan<GameplayTag> ChildTags => new(m_ChildTags);

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public ReadOnlySpan<GameplayTag> HierarchyTags => new(m_HierarchyTags);

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public string TagName { get; }

      public string Description { get; }
      public GameplayTagFlags Flags { get; }

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public string Label { get; }
      public int HierarchyLevel { get; }
      public int RuntimeIndex { get; internal set; }
      public GameplayTagDefinition ParentTagDefinition { get; private set; }

      private GameplayTag[] m_ChildTags;

      private GameplayTag[] m_HierarchyTags;

      private GameplayTag[] m_ParentTags;

      private GameplayTagDefinition[] m_Children;

      private GameplayTagDefinition()
      { }

      public GameplayTagDefinition(string name, string description, GameplayTagFlags flags = GameplayTagFlags.None)
      {
         TagName = name;
         Description = description;
         Flags = flags;

         Label = GameplayTagUtility.GetLabel(name);
         HierarchyLevel = GameplayTagUtility.GetHeirarchyLevelFromName(name);
      }

      public void SetParent(GameplayTagDefinition parent)
      {
         ParentTagDefinition = parent;
         List<GameplayTag> tags = new();

         GameplayTagDefinition current = parent;
         while (current != null)
         {
            tags.Add(current.Tag);
            current = current.ParentTagDefinition;
         }

         tags.Reverse();
         m_ParentTags = tags.ToArray();

         tags.Add(Tag);
         m_HierarchyTags = tags.ToArray();
      }

      public void SetChildren(List<GameplayTagDefinition> children)
      {
         m_Children = children.ToArray();
         m_ChildTags = children.Select(c => c.Tag).ToArray();
      }

      public void SetRuntimeIndex(int index)
      {
         RuntimeIndex = index;
      }
   }
}