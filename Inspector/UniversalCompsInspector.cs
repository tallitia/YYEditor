using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;


[CustomEditor (typeof(UUniversalComps))]
public class UIResourceInspector : UnityEditor.Editor
{
	private UUniversalComps _view;

	private const string DefaultItemName = "NewItem";

	private static readonly Type[] allowedTypes = (new Type[] {
		typeof(GameObject),
		typeof(Transform),
		typeof(RectTransform),
		typeof(Image),
        typeof(RawImage),
        typeof(Button),
        typeof(Text),
        typeof(Slider),
		typeof(Scrollbar),
		typeof(Toggle),
		typeof(ToggleGroup),
		typeof(InputField),
		typeof(ScrollRect),
		typeof(GridLayoutGroup),
		typeof(VerticalLayoutGroup),
        typeof(UIScrollPage),
        typeof(Dropdown),
        typeof(UILayoutHorizontal),
        typeof(UILayoutHorizontalTiled),
        typeof(UILayoutVertical),
        typeof(UILayoutVerticalTiled),
    }).OrderBy (t => t.Name).ToArray ();

	void Awake ()
	{
		_view = target as UUniversalComps;
	}

	public override void OnInspectorGUI ()
	{
		DrawResources (_view.resources);
	}

	protected static void DrawResources (UIResource resources)
	{
		int delete = -1;
		if (resources.componentItems == null)
			resources.componentItems = new UIResourceComponentItem[] { };
		UIResourceComponentItem[] items = resources.componentItems;

		int maxUnnamedIndex = 1;
		int moveFrom = 0, moveTo = 0;
		for (int i = 0; i < items.Length; i++) {
			if (items [i] == null)
				items [i] = new UIResourceComponentItem ();
			if (!string.IsNullOrEmpty (items [i].key) && items [i].key.StartsWith (DefaultItemName)) {
				int unnamedIndex = int.Parse (items [i].key.Substring (DefaultItemName.Length));
				if (unnamedIndex > 0) {
					maxUnnamedIndex = Mathf.Max (unnamedIndex + 1, maxUnnamedIndex);
				}
			}
			int index = i;
            if (DrawResource(items[i], ref index))
            {
                delete = i;
            }
            if (index != i) {
				moveFrom = i;
				moveTo = index;
			}
		}

		if (moveFrom != moveTo && moveFrom >= 0 && moveFrom < items.Length) {
			moveTo = Math.Max (0, Math.Min (items.Length, moveTo));
			List<UIResourceComponentItem> list = new List<UIResourceComponentItem> (items);
			var src = list [moveFrom];
			list.RemoveAt (moveFrom);
			if (moveTo < moveFrom)
				list.Insert (moveTo, src);
			else
				list.Insert (moveTo - 1, src);
			items = resources.componentItems = list.ToArray ();
		}
		if (delete >= 0) {
			UIResourceComponentItem[] newItems = new UIResourceComponentItem[items.Length - 1];
			if (delete > 0)
				Array.ConstrainedCopy (items, 0, newItems, 0, delete);
			if (items.Length - delete - 1 > 0)
				Array.ConstrainedCopy (items, delete + 1, newItems, delete, items.Length - delete - 1);
			resources.componentItems = newItems;
		}

		GUILayout.Space (10);
		if (GUILayout.Button ("Add")) {
			GenericMenu menu = new GenericMenu ();
			for (int i = 0; i < allowedTypes.Length; i++) {
				Type t = allowedTypes [i];
				menu.AddItem (new GUIContent (t.Name), false, () => AddItem (resources, t, maxUnnamedIndex));
			}
			menu.ShowAsContext ();
		}

		if (!CheckKeys (items)) {
			EditorGUILayout.HelpBox ("关键字不能重复或留空", MessageType.Warning);
		}
	}

	private static void AddItem (UIResource resources, Type t, int maxUnnamedIndex)
	{
		UIResourceComponentItem[] res = new UIResourceComponentItem[resources.componentItems.Length + 1];
		Array.ConstrainedCopy (resources.componentItems, 0, res, 0, resources.componentItems.Length);
		res [res.Length - 1] = new UIResourceComponentItem ();
		res [res.Length - 1].typeName = t.FullName;
		res [res.Length - 1].key = DefaultItemName + maxUnnamedIndex;
		resources.componentItems = res;
	}

	private static bool CheckKeys (UIResourceComponentItem[] items)
	{
		HashSet<string> keys = new HashSet<string> ();
		for (int i = 0; i < items.Length; i++) {
			if (string.IsNullOrEmpty (items [i].key))
				return false;
			if (keys.Contains (items [i].key))
				return false;
			keys.Add (items [i].key);
		}
		return true;
	}

    private static bool DrawResource(UIResourceComponentItem r, ref int index)
    {
        bool delete = false;
        EditorGUILayout.BeginHorizontal();

        Color forecolor = GUI.color;
        GUI.color = Color.gray;
        index = EditorGUILayout.DelayedIntField(index, GUILayout.Width(20));
        EditorGUILayout.LabelField(r.typeName, GUILayout.Width(140));
        GUI.color = forecolor;

        r.key = EditorGUILayout.TextField(r.key);

        Type t = string.IsNullOrEmpty(r.typeName) ? null : allowedTypes.FirstOrDefault(type => type.FullName == r.typeName);
        if (t == null || !t.IsSubclassOf(typeof(UnityEngine.Object)))
        {
            t = typeof(UnityEngine.Object);
        }
        UnityEngine.Object go = EditorGUILayout.ObjectField(r.value, t, true);
        //if (r.value == null && go != null && (string.IsNullOrEmpty(r.key) || r.key.StartsWith(DefaultItemName)))
        if (go != null && (string.IsNullOrEmpty(r.key) || Regex.IsMatch(r.key, "^[A-Z].*")))
        { // 为了保持一致
            string name = go.name;
            name = name.Substring(0, 1).ToLower() + name.Substring(1);
            r.key = name;
        }
        r.value = go;

        Color color = GUI.color;
        GUI.color = Color.red;
        if (GUILayout.Button("X", GUILayout.Height(14), GUILayout.Width(50)))
        {
            delete = true;
        }
        GUI.color = color;
        EditorGUILayout.EndHorizontal();
        return delete;
    }

}
