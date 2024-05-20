using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BandoWare.GameplayTags
{
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