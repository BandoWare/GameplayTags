using BandoWare.Editor;
using BandoWare.GameplayTags;
using BandoWare.GameplayTags.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BandoWare.GameplayAbilities.Editor
{
   [CustomPropertyDrawer(typeof(GameplayTagContainer))]
   public class GameplayTagContainerPropertyDrawer : PropertyDrawer
   {
      private const float k_Gap = 2.0f;
      private static GUIContent s_TempContent = new();
      private static readonly GUIContent s_RemoveTagContent = new("-", "Remove tag");

      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
         SerializedProperty tagGuidsProperty = property.FindPropertyRelative("m_SerializedExplicitTags");
         if (tagGuidsProperty.arraySize > 0)
         {
            return Mathf.Max(2, tagGuidsProperty.arraySize) * EditorGUIUtility.singleLineHeight;
         }

         return EditorGUIUtility.singleLineHeight;
      }

      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
         label = EditorGUI.BeginProperty(position, label, property);

         position = EditorGUI.PrefixLabel(position, label);

         int oldIndentLevel = EditorGUI.indentLevel;
         EditorGUI.indentLevel = 0;

         SerializedProperty explicitTagsProperty = property.FindPropertyRelative("m_SerializedExplicitTags");

         Rect editButtonRect = position;
         editButtonRect.width = 70;
         editButtonRect.height = EditorGUIUtility.singleLineHeight;
         if (GUI.Button(editButtonRect, "Edit...", EditorStyles.popup))
         {
            GameplayTagContainerTreeView tagTreeView = new(new TreeViewState(), explicitTagsProperty);
            Rect activatorRect = editButtonRect;
            activatorRect.width = position.width;
            tagTreeView.ShowPopupWindow(activatorRect, 280f);
         }

         if (explicitTagsProperty.arraySize > 0)
         {
            Rect clearButtonRect = editButtonRect;
            clearButtonRect.y = editButtonRect.yMax + 1;
            if (GUI.Button(clearButtonRect, "Clear All"))
            {
               explicitTagsProperty.arraySize = 0;
            }

            Rect tagsRect = position;
            tagsRect.xMin = editButtonRect.xMax + k_Gap;
            tagsRect.width = 0;

            Rect tagRect = tagsRect;
            tagRect.height = EditorGUIUtility.singleLineHeight;
            for (int i = 0; i < explicitTagsProperty.arraySize; i++)
            {
               SerializedProperty element = explicitTagsProperty.GetArrayElementAtIndex(i);
               GameplayTag tag = GameplayTagManager.RequestTag(element.stringValue);

               s_TempContent.text = element.stringValue;
               s_TempContent.tooltip = tag.Description;
               tagRect.width = EditorStyles.label.CalcSize(s_TempContent).x + 22;
               tagsRect.width = Mathf.Max(tagsRect.width, tagRect.width);
               tagsRect.yMax = Mathf.Max(tagRect.yMax, tagsRect.yMax);

               Rect removeButtonRect = tagRect;
               removeButtonRect.width = 14;
               removeButtonRect.yMax -= 2;
               removeButtonRect.yMin += 2;
               removeButtonRect.x += 2;
               if (GUI.Button(removeButtonRect, s_RemoveTagContent))
               {
                  explicitTagsProperty.DeleteArrayElementAtIndex(i);
                  Event.current.Use();
               }
               else
               {
                  Rect labelRect = tagRect;
                  labelRect.xMin = removeButtonRect.xMax;
                  GUI.Label(labelRect, s_TempContent);
               }

               tagRect.y = tagRect.yMax;
            }

            CoreEditorUtils.DrawSolidBorder(tagsRect, new Color(1, 1, 1, 0.15f));
         }

         EditorGUI.indentLevel = oldIndentLevel;
         EditorGUI.EndProperty();
      }
   }
}
