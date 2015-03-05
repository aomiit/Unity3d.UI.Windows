﻿using UnityEngine;
using System.Collections;
using ME;

using System.Reflection;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using ADB = UnityEditor.AssetDatabase;
#endif

namespace UnityEngine.UI.Windows.Plugins.Flow {

	public class FlowDatabase {

		public static void RemoveLayoutComponent(Transform element) {

			var root = element.root;

			GameObject.DestroyImmediate(element.gameObject);

			FlowDatabase.SaveLayout(root.GetComponent<WindowLayout>());

		}

		public static WindowLayoutElement AddLayoutElementComponent(string name, Transform root, int siblingIndex) {
			
			var go = new GameObject(name);
			go.transform.SetParent(root);
			go.transform.localScale = Vector3.one;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			
			go.AddComponent<RectTransform>();
			
			go.transform.SetSiblingIndex(siblingIndex);
			
			var layoutElement = go.AddComponent<WindowLayoutElement>();
			layoutElement.comment = "NEW LAYOUT ELEMENT";
			
			var canvas = go.AddComponent<CanvasGroup>();
			canvas.alpha = 1f;
			canvas.blocksRaycasts = true;
			canvas.interactable = true;
			canvas.ignoreParentGroups = false;
			layoutElement.canvas = canvas;
			
			//FlowSceneView.GetItem().SetLayoutDirty();
			FlowDatabase.SaveLayout(layoutElement.transform.root.GetComponent<WindowLayout>());

			return layoutElement;

		}
		
		public static TWith ReplaceComponents<TReplace, TWith>(TReplace source, System.Type withType) where TReplace : Component where TWith : Component {

			var cachedFields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

			// Add a new component
			var newInstance = source.gameObject.AddComponent(withType);
			
			var newFields = newInstance.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

			foreach (var field in newFields) {

				var cache = cachedFields.FirstOrDefault((f) => f.Name == field.Name);
				if (cache != null) {

					field.SetValue(newInstance, cache.GetValue(source));

				}

			}

			// Destroy the old one
			Object.DestroyImmediate(source);

			return newInstance as TWith;

		}
		
		#if UNITY_EDITOR
		private static T LoadPrefabTemplate<T>(string directory, string templateName) where T : Component {
			
			var go = Resources.Load("UI.Windows/Templates/" + directory + "/" + templateName) as GameObject;
			if (go == null) return null;

			return go.GetComponent<T>();
			
		}
		
		public static WindowLayout LoadLayout(WindowLayout prefab) {
			
			return UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as WindowLayout;
			
		}
		
		public static void SaveLayout(WindowLayout instance) {
			
			UnityEditor.PrefabUtility.ReplacePrefab(instance.gameObject, UnityEditor.PrefabUtility.GetPrefabParent(instance.gameObject), UnityEditor.ReplacePrefabOptions.ConnectToPrefab);
			ADB.Refresh();

		}
		
		public static WindowLayout GenerateLayout(FlowWindow window, FlowWindowLayoutTemplate layout) {
			
			WindowLayout instance = null;
			
			if (window.compiled == false) return instance;
			
			var tplName = layout.name;//"3Buttons";
			var tplData = layout;//FlowSystem.LoadPrefabTemplate<WindowLayout>(FlowSystem.LAYOUT_FOLDER, tplName);
			if (tplData != null) {
				
				var sourcepath = ADB.GetAssetPath(tplData);
				var filepath = window.compiledDirectory + "/" + FlowSystem.LAYOUT_FOLDER + "/" + tplName + "Layout.prefab";
				filepath = filepath.Replace("//", "/");
				
				System.IO.File.Copy(sourcepath, filepath, true);
				ADB.Refresh();
				
				var source = ADB.LoadAssetAtPath(filepath, typeof(GameObject)) as GameObject;
				var prefab = source.GetComponent<WindowLayout>();
				instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as WindowLayout;
				
				var name = window.compiledNamespace + "." + window.compiledClassName;
				instance = FlowDatabase.ReplaceComponents<FlowWindowLayoutTemplate, WindowLayout>(instance as FlowWindowLayoutTemplate, System.Type.GetType(name));
				
				FlowDatabase.SaveLayout(instance);

				GameObject.DestroyImmediate(instance.gameObject);
				instance = (ADB.LoadAssetAtPath(filepath, typeof(GameObject)) as GameObject).GetComponent<WindowLayout>();
				
			} else {
				
				Debug.LogError("Template Loading Error: " + tplName);
				
			}
			
			return instance;
			
		}
		
		public static WindowBase LoadScreen(WindowBase prefab) {
			
			return UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as WindowBase;
			
		}
		
		public static void SaveScreen(WindowBase instance) {
			
			UnityEditor.PrefabUtility.ReplacePrefab(instance.gameObject, UnityEditor.PrefabUtility.GetPrefabParent(instance.gameObject), UnityEditor.ReplacePrefabOptions.ReplaceNameBased);
			ADB.Refresh();

		}
		
		public static WindowBase GenerateScreen(FlowWindow window, FlowLayoutWindowTypeTemplate template) {
			
			WindowBase instance = null;
			
			if (window.compiled == false) return instance;
			
			var tplName = template.name;//"Layout";
			var tplData = template;//FlowSystem.LoadPrefabTemplate<WindowBase>(FlowSystem.SCREENS_FOLDER, tplName);
			if (tplData != null) {
				
				var sourcepath = ADB.GetAssetPath(tplData);
				var filepath = window.compiledDirectory + "/" + FlowSystem.SCREENS_FOLDER + "/" + tplName + "Screen.prefab";
				filepath = filepath.Replace("//", "/");
				
				System.IO.File.Copy(sourcepath, filepath, true);
				ADB.Refresh();
				
				var source = ADB.LoadAssetAtPath(filepath, typeof(GameObject)) as GameObject;
				var prefab = source.GetComponent<WindowBase>();
				instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as WindowBase;

				var name = window.compiledNamespace + "." + window.compiledClassName;
				instance = FlowDatabase.ReplaceComponents<FlowLayoutWindowTypeTemplate, WindowBase>(instance as FlowLayoutWindowTypeTemplate, System.Type.GetType(name));

				FlowDatabase.SaveScreen(instance);

				GameObject.DestroyImmediate(instance.gameObject);
				instance = (ADB.LoadAssetAtPath(filepath, typeof(GameObject)) as GameObject).GetComponent<WindowBase>();
				
			} else {
				
				Debug.LogError("Template Loading Error: " + tplName);
				
			}
			
			return instance;
			
		}
		#endif

	}

}