using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;

namespace CompBind
{
    /// <summary>
    /// Visualizes a <see cref="BindableComponent"/> instance in the editor.
    /// </summary>
    /// <remarks>
    /// Unless not overwritten, this drawer will also be used for
    /// all user-defined derivations of <see cref="BindableComponent"/>.
    /// </remarks>
    /// 
    [CustomEditor(typeof(BindableComponent), true)]
    class ComponentBindingDrawer : UnityEditor.Editor 
    {
        private ReorderableList list;
        BindableComponent componentBinding;
        string[] callbackNames;

        public void OnEnable()
        {
            componentBinding = target as BindableComponent;

            callbackNames = componentBinding.GetCallbacks().Keys.ToArray();

            list = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("selectedBindings"),
                draggable: true,
                displayAddButton: true,
                displayRemoveButton: true,
                displayHeader: true
            );

            list.drawHeaderCallback = drawHeaderCallback;
            list.elementHeightCallback = elementHeightCallback;
            list.drawElementCallback = drawElementCallback;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        protected void drawHeaderCallback(Rect position)
        {
            EditorGUI.LabelField(position, "Component Bindings");
        }

        protected float elementHeightCallback(int index)
        {
            return 2 * EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
        }

        protected void drawElementCallback(Rect position, int drawerEntryIndex, bool isActive, bool isFocused)
        {
            position.y += EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.BeginChangeCheck();
            var goRect = new Rect(position);
            goRect.width = position.width * 0.5f;
            string bindingPath = EditorGUI.TextField(goRect, serializedObject
                    .FindProperty("selectedBindings")
                    .GetArrayElementAtIndex(drawerEntryIndex)
                    .FindPropertyRelative("BindingPath")
                    .stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                // Binding path was changed
                serializedObject
                    .FindProperty("selectedBindings")
                    .GetArrayElementAtIndex(drawerEntryIndex)
                    .FindPropertyRelative("BindingPath")
                    .stringValue = bindingPath;
            }

            EditorGUI.BeginChangeCheck();
            var popupRect = new Rect(goRect);
            popupRect.x += goRect.width;
            popupRect.x += EditorGUIUtility.standardVerticalSpacing;
            popupRect.width = position.width - goRect.width;
            popupRect.width -= EditorGUIUtility.standardVerticalSpacing;
            int fieldIndex = EditorGUI.Popup(
                popupRect, 
                getIndex(drawerEntryIndex), 
                callbackNames
            );
            if (EditorGUI.EndChangeCheck())
            {
                // Switch selected callback
                serializedObject
                    .FindProperty("selectedBindings")
                    .GetArrayElementAtIndex(drawerEntryIndex)
                    .FindPropertyRelative("CallbackName")
                    .stringValue = callbackNames[fieldIndex];

                serializedObject.ApplyModifiedProperties();
            }
            position.y += EditorGUIUtility.singleLineHeight;
            position.y += EditorGUIUtility.standardVerticalSpacing;
        }
        
        int getIndex(int elementIndex)
        {
            string currentlySelected = serializedObject
                .FindProperty("selectedBindings")
                .GetArrayElementAtIndex(elementIndex)
                .FindPropertyRelative("CallbackName")
                .stringValue;

            return Array.IndexOf(callbackNames, currentlySelected);
        }

    }
}
