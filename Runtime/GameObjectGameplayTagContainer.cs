﻿using UnityEngine;

namespace BandoWare.GameplayTags
{
   public class GameObjectGameplayTagContainer : MonoBehaviour
   {
      public GameplayTagCountContainer GameplayTagContainer => m_GameplayTagContainer;

      [SerializeField]
      private GameplayTag m_Tag;

      [SerializeField]
      private GameplayTagContainer m_PersistentTags;

      private GameplayTagCountContainer m_GameplayTagContainer;

      private void Awake()
      {
         m_GameplayTagContainer = new GameplayTagCountContainer();
         m_GameplayTagContainer.AddTags(m_PersistentTags);
      }

      public static implicit operator GameplayTagCountContainer(GameObjectGameplayTagContainer container)
      {
         return container.GameplayTagContainer;
      }
   }
}