using System.Collections;
using System.Collections.Generic;

namespace BandoWare.GameplayTags
{
   public struct GameplayTagEnumerator : IEnumerator<GameplayTag>, IEnumerable<GameplayTag>
   {
      public readonly GameplayTag Current
      {
         get
         {
            GameplayTagDefinition definition = GameplayTagManager.GetDefinitionFromRuntimeIndex(m_Indices[m_CurrentIndex]);
            return definition.Tag;
         }
      }

      readonly object IEnumerator.Current => Current;

      private readonly List<int> m_Indices;
      private int m_CurrentIndex;


      internal GameplayTagEnumerator(List<int> indices)
      {
         m_Indices = indices;
         m_CurrentIndex = -1;
      }

      public readonly void Dispose()
      {
      }

      public bool MoveNext()
      {
         m_CurrentIndex++;
         return m_Indices != null && m_CurrentIndex < m_Indices.Count;
      }

      public void Reset()
      {
         m_CurrentIndex = -1;
      }

      public readonly GameplayTagEnumerator GetEnumerator()
      {
         return this;
      }

      readonly IEnumerator<GameplayTag> IEnumerable<GameplayTag>.GetEnumerator()
      {
         return this;
      }

      readonly IEnumerator IEnumerable.GetEnumerator()
      {
         return this;
      }
   }
}