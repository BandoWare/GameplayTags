using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace BandoWare.GameplayTags
{
   public delegate void OnTagCountChangedDelegate(GameplayTag gameplayTag, int newCount);

   public enum GameplayTagEventType
   {
      NewOrRemoved,
      AnyCountChange
   }

   internal struct DeferredTagChangedDelegate
   {
      public GameplayTag GameplayTag;
      public int NewCount;
      public OnTagCountChangedDelegate Delegate;

      public DeferredTagChangedDelegate(GameplayTag gameplayTag, int newCount, OnTagCountChangedDelegate @delegate)
      {
         GameplayTag = gameplayTag;
         NewCount = newCount;
         Delegate = @delegate;
      }

      public readonly void Execute()
      {
         Delegate(GameplayTag, NewCount);
      }
   }

   internal struct GameplayTagDelegateInfo
   {
      public OnTagCountChangedDelegate OnAnyChange;
      public OnTagCountChangedDelegate OnNewOrRemove;
   }

   public class GameplayTagCountContainer : IGameplayTagContainer
   {
      public bool IsEmpty => m_Indices.Explicit.Count == 0;

      public int ExplicitTagCount => m_Indices.Explicit.Count;

      public int TagCount => m_Indices.Implicit.Count;

      public GameplayTagContainerIndexes Indexes => m_Indices;

      public event OnTagCountChangedDelegate OnAnyTagNewOrRemove;
      public event OnTagCountChangedDelegate OnAnyTagCountChange;

      private Dictionary<GameplayTag, GameplayTagDelegateInfo> m_TagDelegateInfoMap = new();
      private Dictionary<GameplayTag, int> m_TagCountMap = new();
      private Dictionary<GameplayTag, int> m_ExplicitTagCountMap = new();
      private GameplayTagContainerIndexes m_Indices = GameplayTagContainerIndexes.Create();

      public GameplayTagEnumerator GetExplicitTags()
      {
         return new GameplayTagEnumerator(m_Indices.Explicit);
      }

      public GameplayTagEnumerator GetTags()
      {
         return new GameplayTagEnumerator(m_Indices.Implicit);
      }

      public int GetTagCount(GameplayTag tag)
      {
         m_TagCountMap.TryGetValue(tag, out int count);
         return count;
      }

      public int GetExplicitTagCount(GameplayTag tag)
      {
         m_ExplicitTagCountMap.TryGetValue(tag, out int count);
         return count;
      }

      public void RegisterTagEventCallback(GameplayTag tag, GameplayTagEventType eventType, OnTagCountChangedDelegate callback)
      {
         m_TagDelegateInfoMap.TryGetValue(tag, out GameplayTagDelegateInfo delegateInfo);
         GetEventDelegate(ref delegateInfo, eventType) += callback;
         m_TagDelegateInfoMap[tag] = delegateInfo;
      }

      public void RemoveTagEventCallback(GameplayTag tag, GameplayTagEventType eventType, OnTagCountChangedDelegate callback)
      {
         if (m_TagDelegateInfoMap.TryGetValue(tag, out GameplayTagDelegateInfo delegateInfo))
         {
            GetEventDelegate(ref delegateInfo, eventType) -= callback;
            m_TagDelegateInfoMap[tag] = delegateInfo;
         }
      }

      public void RemoveAllTagEventCallbacks()
      {
         m_TagDelegateInfoMap.Clear();
      }

      private static ref OnTagCountChangedDelegate GetEventDelegate(ref GameplayTagDelegateInfo delegateInfo, GameplayTagEventType eventType)
      {
         switch (eventType)
         {
            case GameplayTagEventType.AnyCountChange:
               return ref delegateInfo.OnAnyChange;

            case GameplayTagEventType.NewOrRemoved:
               return ref delegateInfo.OnNewOrRemove;
         }

         throw new ArgumentException(nameof(eventType));
      }

      public void AddTag(GameplayTag tag)
      {
         using (ListPool<DeferredTagChangedDelegate>.Get(out List<DeferredTagChangedDelegate> delegates))
         {
            AddTagInternal(tag, delegates);

            foreach (DeferredTagChangedDelegate del in delegates)
            {
               del.Execute();
            }
         }
      }

      public void AddTags<T>(in T other) where T : IGameplayTagContainer
      {
         using (ListPool<DeferredTagChangedDelegate>.Get(out List<DeferredTagChangedDelegate> delegates))
         {
            foreach (GameplayTag gameplayTag in other.GetExplicitTags())
            {
               AddTagInternal(gameplayTag, delegates);
            }

            foreach (DeferredTagChangedDelegate del in delegates)
            {
               del.Execute();
            }
         }
      }

      private void AddTagInternal(GameplayTag tag, List<DeferredTagChangedDelegate> tagChangeDelegates)
      {
         m_ExplicitTagCountMap.TryGetValue(tag, out int previousExplictTagCount);
         m_ExplicitTagCountMap[tag] = previousExplictTagCount + 1;

         if (previousExplictTagCount == 0)
         {
            int index = ~BinarySearchUtility.Search(m_Indices.Explicit, tag.RuntimeIndex);
            m_Indices.Explicit.Insert(index, tag.RuntimeIndex);
         }

         foreach (GameplayTag tagInHeirarchy in tag.HierarchyTags)
         {
            m_TagDelegateInfoMap.TryGetValue(tagInHeirarchy, out GameplayTagDelegateInfo delegateInfo);
            m_TagCountMap.TryGetValue(tagInHeirarchy, out int previousTagCount);
            m_TagCountMap[tagInHeirarchy] = previousTagCount + 1;

            if (previousTagCount == 0)
            {
               int index = ~BinarySearchUtility.Search(m_Indices.Implicit, tagInHeirarchy.RuntimeIndex);
               m_Indices.Implicit.Insert(index, tagInHeirarchy.RuntimeIndex);

               if (delegateInfo.OnNewOrRemove != null)
               {
                  tagChangeDelegates.Add(new(tagInHeirarchy, 1, delegateInfo.OnNewOrRemove));
               }

               if (OnAnyTagNewOrRemove != null)
               {
                  tagChangeDelegates.Add(new(tagInHeirarchy, 1, OnAnyTagNewOrRemove));
               }
            }

            if (delegateInfo.OnAnyChange != null)
            {
               tagChangeDelegates.Add(new(tagInHeirarchy, previousTagCount + 1, delegateInfo.OnAnyChange));
            }

            if (OnAnyTagCountChange != null)
            {
               tagChangeDelegates.Add(new(tagInHeirarchy, previousTagCount + 1, OnAnyTagCountChange));
            }
         }
      }

      public void RemoveTag(GameplayTag tag)
      {
         using (ListPool<DeferredTagChangedDelegate>.Get(out List<DeferredTagChangedDelegate> tagChangeDelegates))
         {
            RemoveTagInternal(tag, tagChangeDelegates);

            for (int i = 0; i < tagChangeDelegates.Count; i++)
            {
               tagChangeDelegates[i].Execute();
            }
         }
      }

      public void RemoveTags<T>(in T other) where T : IGameplayTagContainer
      {
         using (ListPool<DeferredTagChangedDelegate>.Get(out List<DeferredTagChangedDelegate> tagChangeDelegates))
         {
            foreach (GameplayTag gameplayTag in other.GetExplicitTags())
            {
               RemoveTagInternal(gameplayTag, tagChangeDelegates);
            }

            for (int i = 0; i < tagChangeDelegates.Count; i++)
            {
               tagChangeDelegates[i].Execute();
            }
         }
      }

      private void RemoveTagInternal(GameplayTag tag, List<DeferredTagChangedDelegate> tagChangeDelegates)
      {
         if (!m_ExplicitTagCountMap.TryGetValue(tag, out int explictTagCount))
         {
            GameplayTagUtility.WarnNotExplictlyAddedTagRemoval(tag);
            return;
         }

         if (explictTagCount == 1)
         {
            int index = BinarySearchUtility.Search(m_Indices.Explicit, tag.RuntimeIndex);
            m_Indices.Explicit.RemoveAt(index);
            m_ExplicitTagCountMap.Remove(tag);
         }
         else
         {
            m_ExplicitTagCountMap[tag] = explictTagCount - 1;
         }

         foreach (GameplayTag tagInHierarchy in tag.HierarchyTags)
         {
            m_TagDelegateInfoMap.TryGetValue(tagInHierarchy, out GameplayTagDelegateInfo delegateInfo);
            if (!m_TagCountMap.TryGetValue(tagInHierarchy, out int tagCount))
            {
               break;
            }

            if (tagCount == 1)
            {
               int index = BinarySearchUtility.Search(m_Indices.Implicit, tagInHierarchy.RuntimeIndex);
               m_Indices.Implicit.RemoveAt(index);
               m_TagCountMap.Remove(tagInHierarchy);

               if (delegateInfo.OnNewOrRemove != null)
               {
                  tagChangeDelegates.Add(new DeferredTagChangedDelegate(tagInHierarchy, 0, delegateInfo.OnNewOrRemove));
               }

               if (OnAnyTagNewOrRemove != null)
               {
                  tagChangeDelegates.Add(new DeferredTagChangedDelegate(tagInHierarchy, 0, OnAnyTagNewOrRemove));
               }

               if (delegateInfo.OnAnyChange != null)
               {
                  tagChangeDelegates.Add(new DeferredTagChangedDelegate(tagInHierarchy, 0, delegateInfo.OnAnyChange));
               }

               if (OnAnyTagCountChange != null)
               {
                  tagChangeDelegates.Add(new DeferredTagChangedDelegate(tagInHierarchy, 0, OnAnyTagNewOrRemove));
               }

               continue;
            }

            m_TagCountMap[tagInHierarchy] = tagCount - 1;

            if (delegateInfo.OnAnyChange != null)
            {
               tagChangeDelegates.Add(new DeferredTagChangedDelegate(tagInHierarchy, tagCount - 1, delegateInfo.OnAnyChange));
            }

            if (OnAnyTagCountChange != null)
            {
               tagChangeDelegates.Add(new DeferredTagChangedDelegate(tagInHierarchy, tagCount - 1, OnAnyTagNewOrRemove));
            }
         }
      }

      public void Clear()
      {
         using (ListPool<DeferredTagChangedDelegate>.Get(out List<DeferredTagChangedDelegate> tagChangeDelegates))
         {
            foreach (GameplayTag tag in GetTags())
            {
               m_TagDelegateInfoMap.TryGetValue(tag, out GameplayTagDelegateInfo delegateInfo);

               if (delegateInfo.OnNewOrRemove != null)
               {
                  tagChangeDelegates.Add(new DeferredTagChangedDelegate(tag, 0, delegateInfo.OnNewOrRemove));
               }

               if (OnAnyTagNewOrRemove != null)
               {
                  tagChangeDelegates.Add(new DeferredTagChangedDelegate(tag, 0, OnAnyTagNewOrRemove));
               }
            }

            m_ExplicitTagCountMap.Clear();
            m_TagCountMap.Clear();
            m_Indices.Clear();

            foreach (DeferredTagChangedDelegate del in tagChangeDelegates)
            {
               del.Execute();
            }
         }
      }

      public GameplayTagEnumerator GetEnumerator()
      {
         return new GameplayTagEnumerator(m_Indices.Implicit);
      }

      IEnumerator<GameplayTag> IEnumerable<GameplayTag>.GetEnumerator()
      {
         return GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}