using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BandoWare.GameplayTags.Editor
{
   public class GameplayTagTreeView : GameplayTagTreeViewBase
   {
      private static GUIContent s_TempContent = new();
      private Action m_OnSelectionChange;
      private SerializedProperty m_TagNameProperty;

      public GameplayTagTreeView(TreeViewState treeViewState, SerializedProperty tagProperty, Action onSelectionChange)
      : base(treeViewState)
      {
         m_OnSelectionChange = onSelectionChange;
         m_TagNameProperty = tagProperty.FindPropertyRelative("m_Name");
         m_TagNameProperty.serializedObject.Update();

         GameplayTag tag = GameplayTagManager.RequestTag(m_TagNameProperty.stringValue);
         if (!tag.IsNone && !tag.IsValid)
         {
            GameplayTagTreeViewItem item = FindItem(tag.RuntimeIndex);
            if (item != null)
               SetSelection(new int[] { item.id });

            while (item != null)
            {
               SetExpanded(item.id, true);
               item = item.parent as GameplayTagTreeViewItem;
            }
         }
      }

      protected override void OnToolbarGUI()
      {
         if (ToolbarButton("Reset"))
         {
            m_TagNameProperty.stringValue = null;
            m_TagNameProperty.serializedObject.ApplyModifiedProperties();
         }
      }

      protected override bool CanMultiSelect(TreeViewItem item)
      {
         return false;
      }

      protected override void RowGUI(RowGUIArgs args)
      {
         bool isNone = args.item is not GameplayTagTreeViewItem;
         float indent = GetContentIndent(args.item);
         Rect rect = args.rowRect;
         rect.xMin += indent;

         if (isNone)
         {
            if (GUI.Button(rect, args.label, EditorStyles.label))
            {
               m_TagNameProperty.stringValue = null;
               m_TagNameProperty.serializedObject.ApplyModifiedProperties();

               m_OnSelectionChange?.Invoke();
            }

            return;
         }

         GameplayTagTreeViewItem item = args.item as GameplayTagTreeViewItem;

         
         if (IsSelected(item.id) && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp)
         {
            m_TagNameProperty.stringValue = item.Tag.Name;
            m_TagNameProperty.serializedObject.ApplyModifiedProperties();
         
            m_OnSelectionChange?.Invoke();
         }
         
         if (GUI.Button(rect, s_TempContent, EditorStyles.label))
         {
            m_TagNameProperty.stringValue = item.Tag.Name;
            m_TagNameProperty.serializedObject.ApplyModifiedProperties();

            m_OnSelectionChange?.Invoke();
         }

         DoTagRowGUI(rect, item);
      }
   }
}