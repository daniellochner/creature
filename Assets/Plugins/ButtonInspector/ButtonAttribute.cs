using UnityEngine;
using Object = UnityEngine.Object;
using System;

#if (UNITY_EDITOR)
using UnityEditor;
using System.Reflection;
#endif

namespace BasicTools.ButtonInspector {
	[AttributeUsage(AttributeTargets.Field)]
	public class ButtonAttribute : PropertyAttribute {

		public string text;
		public string method;
		public bool registerUndo;

		public ButtonAttribute(string text, string method = null, bool registerUndo = false) {
			this.text = text;
			this.method = method;
			this.registerUndo = registerUndo;
		}
	}

#if (UNITY_EDITOR)
	[CustomPropertyDrawer(typeof(ButtonAttribute))]
	public class ButtonDrawer : PropertyDrawer {

		private MethodInfo method;

		public void CheckPoint(Object target, string name) {
			if (Application.isPlaying || target == null) {
				return;
			}

			if (target) {
				Undo.RecordObject(target, name);
			}
			else {
				Debug.LogError("Undo.RecordObject Error");
			}
		}

		public static void CheckChanges(Object target) {
			if (Application.isPlaying || target == null) {
				return;
			}

			EditorUtility.SetDirty(target);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return 27.0f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			ButtonAttribute button = attribute as ButtonAttribute;

			if (property.propertyType == SerializedPropertyType.Boolean) {
				float width = position.width;
				position.height = 25.0f;
				position.width = 250.0f;
				position.x = (width - 250.0f) / 2;
				if (button.registerUndo) {
					CheckPoint(property.serializedObject.targetObject, string.Format("Button {0} changes", button.text));
				}
				if (GUI.Button(position, button.text)) {
					if (string.IsNullOrEmpty(button.method) == false) {
						if (method == null) {
							method = property.serializedObject.targetObject.GetType().GetMethod(button.method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
						}
						method.Invoke(property.serializedObject.targetObject, null);
					}
				}
				if (GUI.changed) {
					if (button.registerUndo) {
						CheckChanges(property.serializedObject.targetObject);
					}
				}
			}
			else {
				EditorGUI.LabelField(position, label.text, "Use Button with bool property.");
			}
		}
	}
#endif
}
