using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

public static class Ramen
{
	[InitializeOnLoadMethod]
	private static void Setup()
	{
		EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
	}
	
	private static bool delayCalled = false;
	
	private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
	{
		var current = Event.current;
		if(current.alt && current.type == EventType.MouseDown)
		{
			if(delayCalled) return;
			delayCalled = true;
			
			// 2個噛ませないとうまくいかない。なぞ。
			// Inspectorの再描画タイミングとかにしてみる？
			EditorApplication.delayCall += () => {
				EditorApplication.delayCall += () => {
					delayCalled = false;
					var selectingObjects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
					if(selectingObjects.Length != 1) return;
					
					UnityEngine.Object selecting = selectingObjects[0] as UnityEngine.Object;
					var path = AssetDatabase.GetAssetPath(selecting);
					if(Directory.Exists(path)) return;
					
					Debug.Log(path);
					OpenInspectorWindow(null);
				};
			};
		}
	}
	
	private static void OpenInspectorWindow(GameObject target)
	{
		var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
		var inspectorInstance = ScriptableObject.CreateInstance(inspectorType) as EditorWindow;
		inspectorInstance.Show();
		//var prevSelection = Selection.activeGameObject;
		//Selection.activeGameObject = target;
		var isLocked = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
		isLocked.GetSetMethod().Invoke(inspectorInstance, new object[] { true });
		//Selection.activeGameObject = prevSelection;
	}
}
