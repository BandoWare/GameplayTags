using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BandoWare.GameplayTags
{
   public struct GameplayTagContainerIndexes
   {
      public bool IsCreated => Explicit != null && Implicit != null;

      internal List<int> Explicit { get; private set; }
      internal List<int> Implicit { get; private set; }

      public static void Create(ref GameplayTagContainerIndexes indexes)
      {
         if (indexes.IsCreated)
         {
            return;
         }

         indexes = new GameplayTagContainerIndexes()
         {
            Explicit = new(),
            Implicit = new()
         };
      }

      public static GameplayTagContainerIndexes Create()
      {
         return new GameplayTagContainerIndexes()
         {
            Explicit = new(),
            Implicit = new()
         };
      }

      internal readonly void Add(in GameplayTagContainerIndexes other)
      {
         Explicit.AddRange(other.Explicit);
         Implicit.AddRange(other.Implicit);
      }

      internal readonly void Clear()
      {
         Explicit.Clear();
         Implicit.Clear();
      }

      internal readonly void CopyFrom(in GameplayTagContainerIndexes other)
      {
         other.Clear();
         other.Add(this);
      }
   }

   public interface IGameplayTagContainer
   {
      public bool IsEmpty { get; }

      public int ExplicitTagCount { get; }

      public int TagCount { get; }

      GameplayTagContainerIndexes Indexes { get; }

      public void AddTag(GameplayTag gameplayTag);

      public void RemoveTag(GameplayTag gameplayTag);

      public GameplayTagEnumerator GetTags();

      public GameplayTagEnumerator GetExplicitTags();

      public void AddTags<T>(in T other) where T : IGameplayTagContainer;

      public void RemoveTags<T>(in T other) where T : IGameplayTagContainer;

      public void Clear();
   }

   [Serializable]
   [DebuggerTypeProxy(typeof(GameplayTagContainerDebugView))]
   [DebuggerDisplay("{DebbugerDisplay,nq}")]
   public class GameplayTagContainer : IGameplayTagContainer, ISerializationCallbackReceiver
   {
      public bool IsEmpty => m_Indices.Explicit.Count == 0;

      public int ExplicitTagCount => m_Indices.Explicit.Count;

      public int TagCount => m_Indices.Implicit.Count;

      public GameplayTagContainerIndexes Indexes => m_Indices;

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It's used for debugging")]
      private string DebuggerDisplay => $"Count (Explicit, Total) = ({ExplicitTagCount}, {TagCount})";

      [SerializeField]
      private List<string> m_SerializedExplicitTags;
      private GameplayTagContainerIndexes m_Indices = new();

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private string DebbugerDisplay => $"Count (Explicit, Total) = ({ExplicitTagCount}, {TagCount})";

      public GameplayTagContainer()
      { }

      public GameplayTagContainer(IGameplayTagContainer other)
      {
         Copy(this, other);
      }

      public GameplayTagContainer Clone()
      {
         GameplayTagContainer clone = new();

         if (IsEmpty)
         {
            return clone;
         }

         clone.m_Indices.CopyFrom(m_Indices);

         return clone;
      }

      public static void Copy<T>(GameplayTagContainer dest, in T src) where T : IGameplayTagContainer
      {
         dest.m_Indices.CopyFrom(src.Indexes);
      }

      public static GameplayTagContainer Intersection<T, U>(in T lhs, in U rhs) where T : IGameplayTagContainer where U : IGameplayTagContainer
      {
         GameplayTagContainer intersection = new();
         intersection.AddIntersection(lhs, rhs);
         return intersection;
      }

      public void AddIntersection<T, U>(in T lhs, in U rhs) where T : IGameplayTagContainer where U : IGameplayTagContainer
      {
         static void OrderedListIntersection(List<int> a, List<int> b, List<int> dst)
         {
            dst.Capacity = Mathf.Max(dst.Capacity, Mathf.Min(a.Count, b.Count));

            int i = 0, j = 0;
            while (i < a.Count && j < b.Count)
            {
               int aElement = a[i], bElement = b[j];
               if (aElement == bElement)
               {
                  dst.Add(aElement);
                  i++;
                  j++;
                  continue;
               }

               if (aElement < bElement)
               {
                  i++;
                  continue;
               }

               j++;
            }
         }

         if (lhs.IsEmpty || rhs.IsEmpty)
         {
            return;
         }

         if (lhs.IsEmpty)
         {
            m_Indices.Add(rhs.Indexes);
         }
         else if (rhs.IsEmpty)
         {
            m_Indices.Add(lhs.Indexes);
         }
         else
         {
            OrderedListIntersection(lhs.Indexes.Explicit, rhs.Indexes.Explicit, m_Indices.Explicit);
            OrderedListIntersection(lhs.Indexes.Implicit, rhs.Indexes.Implicit, m_Indices.Implicit);
         }
      }

      public static GameplayTagContainer Union<T, U>(in T lhs, in U rhs) where T : IGameplayTagContainer where U : IGameplayTagContainer
      {
         static void OrderedListUnion(List<int> a, List<int> b, List<int> dst)
         {
            dst.Capacity = Mathf.Max(dst.Capacity, a.Count + b.Count);

            int i = 0, j = 0;
            while (i < a.Count && j < b.Count)
            {
               int aElement = a[i], bElement = b[j];
               if (aElement == bElement)
               {
                  dst.Add(aElement);
                  i++;
                  j++;
                  continue;
               }

               if (aElement < bElement)
               {
                  dst.Add(aElement);
                  i++;
                  continue;
               }

               dst.Add(bElement);
               j++;
            }

            for (; i < a.Count; i++)
            {
               dst.Add(a[i]);
            }

            for (; j < b.Count; j++)
            {
               dst.Add(b[j]);
            }
         }

         GameplayTagContainer union = new();

         if (lhs.IsEmpty || rhs.IsEmpty)
         {
            return union;
         }

         if (lhs.IsEmpty)
         {
            return new GameplayTagContainer(rhs);
         }

         if (rhs.IsEmpty)
         {
            new GameplayTagContainer(lhs);
         }

         OrderedListUnion(lhs.Indexes.Explicit, rhs.Indexes.Explicit, union.m_Indices.Explicit);
         OrderedListUnion(lhs.Indexes.Implicit, rhs.Indexes.Implicit, union.m_Indices.Implicit);

         return union;
      }

      public GameplayTagEnumerator GetExplicitTags()
      {
         return new GameplayTagEnumerator(m_Indices.Explicit);
      }

      public GameplayTagEnumerator GetTags()
      {
         return new GameplayTagEnumerator(m_Indices.Implicit);
      }

      public void Clear()
      {
         m_Indices.Clear();
         m_SerializedExplicitTags?.Clear();
      }

      public void AddTag(GameplayTag tag)
      {
         GameplayTagContainerIndexes.Create(ref m_Indices);
         int index = BinarySearchUtility.Search(m_Indices.Explicit, tag.RuntimeIndex);
         if (index >= 0)
         {
            return;
         }

         m_Indices.Explicit.Insert(~index, tag.RuntimeIndex);
         AddImplicitTagsFor(tag);
      }

      public void AddTags<T>(in T container) where T : IGameplayTagContainer
      {
         foreach (GameplayTag tag in container.GetExplicitTags())
         {
            AddTag(tag);
         }
      }

      public void RemoveTag(GameplayTag tag)
      {
         if (!m_Indices.IsCreated)
         {
            return;
         }

         int index = BinarySearchUtility.Search(m_Indices.Explicit, tag.RuntimeIndex);
         if (index < 0)
         {
            GameplayTagUtility.WarnNotExplictlyAddedTagRemoval(tag);
            return;
         }

         m_Indices.Explicit.RemoveAt(index);
         FillImplictTags();
      }

      public void RemoveTags<T>(in T other) where T : IGameplayTagContainer
      {
         if (!m_Indices.IsCreated)
         {
            return;
         }

         foreach (GameplayTag tag in other.GetExplicitTags())
         {
            int index = BinarySearchUtility.Search(m_Indices.Explicit, tag.RuntimeIndex);
            if (index < 0)
            {
               GameplayTagUtility.WarnNotExplictlyAddedTagRemoval(tag);
               return;
            }

            m_Indices.Explicit.RemoveAt(index);
         }

         FillImplictTags();
      }

      private void AddImplicitTagsFor(GameplayTag tag)
      {
         ReadOnlySpan<GameplayTag> tags = tag.HierarchyTags;
         for (int i = tags.Length - 1; i >= 0; i--)
         {
            GameplayTag parent = tags[i];
            int index = BinarySearchUtility.Search(m_Indices.Implicit, parent.RuntimeIndex);
            if (index >= 0)
            {
               break;
            }

            m_Indices.Implicit.Insert(~index, parent.RuntimeIndex);
         }
      }

      private void FillImplictTags()
      {
         m_Indices.Implicit.Clear();
         foreach (GameplayTag tag in new GameplayTagEnumerator(m_Indices.Explicit))
         {
            AddImplicitTagsFor(tag);
         }
      }

      void ISerializationCallbackReceiver.OnBeforeSerialize()
      {
         m_SerializedExplicitTags ??= new();

         m_SerializedExplicitTags.Clear();
         if (m_Indices.Explicit == null)
         {
            return;
         }

         foreach (GameplayTag tag in new GameplayTagEnumerator(m_Indices.Explicit))
         {
            if (tag == GameplayTag.None)
            {
               continue;
            }

            m_SerializedExplicitTags.Add(tag.Name);
         }
      }

      void ISerializationCallbackReceiver.OnAfterDeserialize()
      {
         m_Indices = GameplayTagContainerIndexes.Create();
         if (m_SerializedExplicitTags == null || m_SerializedExplicitTags.Count == 0)
         {
            return;
         }

         for (int i = 0; i < m_SerializedExplicitTags.Count;)
         {
            GameplayTag tag = GameplayTagManager.RequestTag(m_SerializedExplicitTags[i]);
            if (tag == GameplayTag.None)
            {
               m_SerializedExplicitTags.RemoveAt(i);
               continue;
            }

            int index = BinarySearchUtility.Search(m_Indices.Explicit, tag.RuntimeIndex);
            if (index < 0)
            {
               m_Indices.Explicit.Insert(~index, tag.RuntimeIndex);
               i++;
               continue;
            }

            m_SerializedExplicitTags.RemoveAt(i);
         }

         FillImplictTags();
      }
   }

   internal class GameplayTagContainerDebugView
   {
      [DebuggerDisplay("{DebuggerDisplay,nq}")]
      public struct Tag
      {
         public string Name { get; set; }
         public bool IsExplicit { get; set; }

         [DebuggerBrowsable(DebuggerBrowsableState.Never)]
         private readonly string DebuggerDisplay
         {
            get
            {
               if (!IsExplicit)
               {
                  return Name;
               }

               return $"{Name} (Explicit)";
            }
         }
      }

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public GameplayTagContainer Container { get; set; }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public Tag[] Tags
      {
         get
         {
            return Container.GetTags()
               .Select(Convert)
               .ToArray();

            Tag Convert(GameplayTag tag)
            {
               return new Tag
               {
                  IsExplicit = Container.HasTagExact(tag),
                  Name = tag.Name
               };
            }
         }
      }

      public GameplayTagContainerDebugView(GameplayTagContainer container)
      {
         Container = container;
      }
   }
}