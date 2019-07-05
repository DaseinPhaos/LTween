#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Luxko.EditorTools
{
    public class PrettyListViewer : ReorderableList
    {
        public PrettyListViewer(
            SerializedObject serializedObject,
            SerializedProperty elements,
            HeaderCallbackDelegate onDrawHeader,
            ElementCallbackDelegate onDrawElement,
            ElementHeightCallbackDelegate elementHeightGetter,
            ReorderCallbackDelegate onReorder = null,
            AddCallbackDelegate onAdd = null,
            RemoveCallbackDelegate onRemove = null
        ) : base(serializedObject, elements)
        {
            drawHeaderCallback = onDrawHeader;
            drawElementCallback = onDrawElement;
            elementHeightCallback = elementHeightGetter;

            if (onReorder != null)
            {
                draggable = true;
                onReorderCallback = onReorder;
            }
            else
            {
                draggable = false;
            }

            if (onAdd != null)
            {
                displayAdd = true;
                onAddCallback = onAdd;
            }
            else
            {
                displayAdd = false;
            }

            if (onRemove != null)
            {
                displayRemove = true;
                onRemoveCallback = onRemove;
            }
            else
            {
                displayRemove = false;
            }
        }

        public PrettyListViewer(SerializedProperty sp, string label)
        : this(
            sp.serializedObject,
            sp,
            onDrawHeader: (rect) =>
            {
                rect.x += 20;
                rect.width -= 20;
                var oldExpanded = sp.isExpanded;
                sp.isExpanded = EditorGUI.Foldout(rect, oldExpanded, label);
                if (sp.isExpanded != oldExpanded)
                {
                    for (int i = 0; i < sp.arraySize; ++i)
                    {
                        sp.GetArrayElementAtIndex(i).isExpanded = sp.isExpanded;
                    }
                }
            },
            onDrawElement: (rect, index, isActive, isFocused) =>
            {
                rect.x += 20;
                rect.width -= 20;
                var child = sp.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, child, child.isExpanded);
            },
            elementHeightGetter: (index) =>
            {
                var child = sp.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(child, child.isExpanded) + 10;
            },
            onReorder: (list) =>
            {
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
            },
            onAdd: (list) =>
            {
                list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
            },
            onRemove: (list) =>
            {
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                list.serializedProperty.serializedObject.ApplyModifiedProperties();
            }
        )
        {

        }
    }
}

#endif