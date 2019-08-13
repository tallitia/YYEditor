using UnityEngine;
using UnityEditor;
using System.IO;


public class LuaCreateWindow : EditorWindow
{
    [MenuItem("Game/Lua/创建Lua模块", false, 1)]
    static void CreateModule()
    {
        GetWindow<LuaCreateWindow>(false, "创建Lua模块", true).Show();
    }

    private new string name;
    private const string TemplatePath = "Assets/Game/Editor/Config/template#.txt";
    private const string ModulePath = "Assets/Game/Lua/module/";

    private void OnGUI()
    {
        EditorGUIUtility.labelWidth = 30;
        GUILayout.Space(10);
        GUILayout.Label("Name:");

        EditorGUIUtility.labelWidth = 100;
        string newname = EditorGUILayout.TextField("", name);

        if (newname != name)
            name = newname;

        GUILayout.Space(10);
        if (GUILayout.Button("Create", GUILayout.Width(100f)))
            DoCreateModule(name);
    }



    private void DoCreateLua(string name, string type)
    {
        string filepath = ModulePath + name + "/" + name + "_" + type + ".lua";
        string template = LoadTemplate(type);
        File.WriteAllText(filepath, template.Replace("#", name));
    }

    private string LoadTemplate(string type)
    {
        string path = TemplatePath.Replace("#", type);
        string text = "";
        if (File.Exists(path))
            text = File.ReadAllText(path);
        return text;
    }

    private void DoCreateModule(string name)
    {
        string path = ModulePath + name;
        if (Directory.Exists(path) && EditorUtility.DisplayDialog("Lua Creator", "Replace '" + name + "' module?", "Yes", "No"))
            Directory.Delete(path, true);

        Directory.CreateDirectory(path);

        DoCreateLua(name, "model");
        DoCreateLua(name, "view");
        DoCreateLua(name, "ctrl");
        DoCreateLua(name, "module");

        Debug.Log("Create '" + name + "' lua module finish.");
        AssetDatabase.Refresh();
    }

}