using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Security;

public class PackageApi 
{
    public delegate void BuildAction();
    private delegate AssetBundleBuild[] BundleAction(string src);

    public static string VERSION = "";
    public static string VERSION_PATH = OUTPUT_ROOT + "version.txt";
    public static string XVERSION_PATH = OUTPUT_ROOT + "xversion.txt";

    public static string AB_EXTENSION = ".ab";
    public static string AB_MANIFEST = "index";

    public static List<string> FilterExtensions = new List<string> { ".meta" };

    private static List<BundleAction> allHandler = new List<BundleAction>()
    {
        DoMakeAssetBundleBuild1,
        DoMakeAssetBundleBuild2,
        DoMakeAssetBundleBuild3,
        DoMakeAssetBundleBuild4,
    };

    public static BuildTarget m_BuildTarget;
    public static BuildTarget buildTarget {
        set { m_BuildTarget = value; }
        get
        {
            if (m_BuildTarget == BuildTarget.NoTarget)
                m_BuildTarget = EditorUserBuildSettings.activeBuildTarget;

            return m_BuildTarget;
        }
    }

    public static string OUTPUT_ROOT
    {
        get
        {
            if (string.IsNullOrEmpty(VERSION))
            {
                string content = File.ReadAllText(AppConst.SettingPath);
                SecurityElement setting = SecurityElement.FromString(content);
                VERSION = setting.Attribute("version");
                string p = EditorConst.AB_OUTPUT_PATH;
                EditorConst.AB_OUTPUT_PATH = Directory.GetCurrentDirectory() + "/" + p;
            }
            return EditorConst.AB_OUTPUT_PATH + VERSION + "/" + AppConst.OS + "/";
        }
    }

    [MenuItem("Game/打资源包/Update/iOS")]
    public static void BuildABUpdateiOS()
    {
        BuildAB("iOS", "Update", BuildTargetGroup.iOS, BuildTarget.iOS, BuildNormalAB);
    }

    [MenuItem("Game/打资源包/Update/Android")]
    public static void BuildABUpdateAndroid()
    {
        BuildAB("Android", "Update", BuildTargetGroup.Android, BuildTarget.Android, BuildNormalAB);  
    }

    [MenuItem("Game/打资源包/Update/Win")]
    public static void BuildABUpdatePC()
    {
        BuildAB("Windows", "Update", BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, BuildNormalAB);
    }

    [MenuItem("Game/打资源包/Package/iOS")]
    public static void BuildABZipiOS()
    {
        BuildAB("iOS", "Zip", BuildTargetGroup.iOS, BuildTarget.iOS, BuildZipAB);
    }

    [MenuItem("Game/打资源包/Package/Android")]
    public static void BuildABZipAndroid()
    {
        BuildAB("Android", "Zip", BuildTargetGroup.Android, BuildTarget.Android, BuildZipAB);
    }

    [MenuItem("Game/打资源包/Package/Win")]
    public static void BuildABZipWin()
    {
        BuildAB("Windows", "Zip", BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, BuildZipAB);
    }

    [MenuItem("Game/打资源包/Database/iOS")]
    public static void BuildDatabaseiOS()
    {
        BuildAB("iOS", "Database", BuildTargetGroup.iOS, BuildTarget.iOS, BuildDatabase);
    }

    [MenuItem("Game/打资源包/Database/Android")]
    public static void BuildDatabaseAndroid()
    {
        BuildAB("Android", "Database", BuildTargetGroup.Android, BuildTarget.Android, BuildDatabase);
    }

    [MenuItem("Game/打资源包/Database/Win")]
    public static void BuildDatabaseWin()
    {
        BuildAB("Windows", "Database", BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, BuildDatabase);
    }

    [MenuItem("Game/打资源包/Lua/iOS")]
    public static void BuildLuaiOS()
    {
        BuildAB("iOS", "Lua", BuildTargetGroup.iOS, BuildTarget.iOS, BuildLua);
    }

    [MenuItem("Game/打资源包/Lua/Android")]
    public static void BuildLuaAndroid()
    {
        BuildAB("Android", "Lua", BuildTargetGroup.Android, BuildTarget.Android, BuildLua);
    }

    [MenuItem("Game/打资源包/Lua/Win")]
    public static void BuildLuaWin()
    {
        BuildAB("Windows", "Lua", BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, BuildLua);
    }

    private static void BuildAB(string platform, string typeName, BuildTargetGroup group, BuildTarget target, BuildAction action)
    {
        buildTarget = target;
        string message = string.Format("确认切换到{0}平台打包{1}资源？", platform, typeName);
        bool ret = true;
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            ret = EditorUtility.DisplayDialog("提示", message, "确认", "取消");
        }
        if (ret)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            action();
        }
    }

    private static void BuildNormalAB()
    {
        PackAssetBundles();
    }

    private static void BuildZipAB()
    {
        PackAssetBundlesZip();
    }

    private static void BuildDatabase()
    {
        LuaPackager.EncodeDatabaseFiles();
        UnityEngine.Debug.Log("Build Databases Finish!!");
    }

    private static void BuildLua()
    {
        LuaPackager.EncodeLuaFiles();


        UnityEngine.Debug.Log("Build Lua Finish!!");
    }

    public static List<AssetBundleBuild> PackAssetBundles()
    {
        if (EditorApplication.isCompiling)
        {
            EditorUtility.DisplayDialog("", "Script is compiling, try again later.", "Yes");
            return null;
        }
        //Complie Lua File
        //LuaPackager.EncodeLuaFiles();

        //Read The Build Config File
        Dictionary<string, int> config = new Dictionary<string, int>();
        EditorUtil.ReadAssetConfig(config);

        //Get All AssetBundle File
        List<AssetBundleBuild> buildmap = new List<AssetBundleBuild>();
        foreach (var pair in config)
        {
            BundleAction handler = allHandler[pair.Value-1];
            string path = EditorConst.ASSET_ROOT + pair.Key;
            if (Directory.Exists(path))
            {
                var builds = handler.Invoke(path);
                buildmap.AddRange(builds as IEnumerable<AssetBundleBuild>);
            }
        }

        if (!Directory.Exists(OUTPUT_ROOT)) Directory.CreateDirectory(OUTPUT_ROOT);

        DoMakeAssetBundleAssetConfig(EditorConst.ASSET_CONFIG_PATH, buildmap);
        DoMakeAssetBundleLuaConfig(EditorConst.LUA_CONFIG_PATH, buildmap);
        AssetDatabase.Refresh();

        //Begin Build PipeLine
        BuildPipeline.BuildAssetBundles(OUTPUT_ROOT, buildmap.ToArray(),
            BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget);

        // Handle The Main Manifest
        string manifestPath = OUTPUT_ROOT + Path.GetFileName(Path.GetDirectoryName(OUTPUT_ROOT));
        File.Copy(manifestPath, OUTPUT_ROOT + AB_MANIFEST + AB_EXTENSION, true);
        File.Delete(manifestPath);
        File.Copy(manifestPath + ".manifest", OUTPUT_ROOT + AB_MANIFEST + ".manifest", true);
        File.Delete(manifestPath + ".manifest");
        AssetBundleBuild abb = new AssetBundleBuild();
        abb.assetBundleName = AB_MANIFEST + AB_EXTENSION;
        buildmap.Add(abb);
        BuildVersionFile(VERSION_PATH, OUTPUT_ROOT, buildmap);

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Build assetbundles files finish!!");
        return buildmap;
    }

    // Pack Zip
    public static void PackAssetBundlesZip()
    {
        List<AssetBundleBuild> buildmap = PackAssetBundles();
        if (buildmap == null)
            return;
        string zipPath = OUTPUT_ROOT + "zip/";
        if (Directory.Exists(zipPath)) Directory.Delete(zipPath, true);
        Directory.CreateDirectory(zipPath);

        string abPath = EditorConst.AB_PATH;
        if (Directory.Exists(abPath)) Directory.Delete(abPath, true);

        Zip.ZipDerctory(OUTPUT_ROOT, EditorConst.ABZIP_PATH, ".manifest|.meta");

        //byte[] bytes = File.ReadAllBytes(ABZIP_PATH);
        //byte[] buff = AesTool.Encrypt(bytes, 0, bytes.Length, SDKConst.SECRET);
        //File.WriteAllBytes(ABZIP_PATH, buff);
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Build ab.zip finish!!");
    }

    /// 文件夹打包成一个AB
    public static AssetBundleBuild[] DoMakeAssetBundleBuild1(string src)
    {
        string[] files = Directory.GetFiles(src, "*", SearchOption.AllDirectories);
        string abname = MakeAssetBundleName(src);
        return MakeAssetBundleBuild(abname, files);
    }

    /// 文件打成一个AB
    public static AssetBundleBuild[] DoMakeAssetBundleBuild2(string src)
    {
        string[] files = Directory.GetFiles(src, "*", SearchOption.AllDirectories);
        List<AssetBundleBuild> list = new List<AssetBundleBuild>();
        for (int i = 0; i < files.Length; i++)
        {
            string abname = MakeAssetBundleName(files[i]);
            list.AddRange(MakeAssetBundleBuild(abname, new string[] { files[i] }));
        }
        return list.ToArray();
    }

    /// 文件夹里的文件夹打成一个AB
    public static AssetBundleBuild[] DoMakeAssetBundleBuild3(string src)
    {
        List<AssetBundleBuild> list = new List<AssetBundleBuild>();
        string[] files = Directory.GetFiles(src, "*");
        if (files.Length > 0)
        {
            string abname = MakeAssetBundleName(src);
            list.AddRange(MakeAssetBundleBuild(abname, files));
        }

        string[] dirs = Directory.GetDirectories(src);
        for (int i = 0; i < dirs.Length; i++)
        {
            list.AddRange(DoMakeAssetBundleBuild1(dirs[i]));
        }
        return list.ToArray();
    }

    /// 文件夹里的文件打成一个AB, 不包含文件夹里的文件夹
    public static AssetBundleBuild[] DoMakeAssetBundleBuild4(string src)
    {
        List<AssetBundleBuild> list = new List<AssetBundleBuild>();
        string[] files = Directory.GetFiles(src, "*");
        if (files.Length == 0) return new AssetBundleBuild[0];

        string abname = MakeAssetBundleName(src);
        list.AddRange(MakeAssetBundleBuild(abname, files));
        return list.ToArray();
    }

    // Add AssetConfig To Build Map
    private static void DoMakeAssetBundleAssetConfig(string path, List<AssetBundleBuild> buildmap)
    {
        string abname = Path.GetFileNameWithoutExtension(path).ToLower();
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = abname + AB_EXTENSION;
        build.assetNames = new string[] { path };
        buildmap.Add(build);
    }

    // Add LuaConfig To Build Map
    private static void DoMakeAssetBundleLuaConfig(string path, List<AssetBundleBuild> buildmap)
    {
        string content = string.Empty;
        for (int i = 0; i < buildmap.Count; i++)
        {
            if (!buildmap[i].assetBundleName.StartsWith("lua"))
                continue;

            if (content != string.Empty) content += "\n";
            content += buildmap[i].assetBundleName;
        }
        File.WriteAllText(path, content);

        //add to build
        string abname = Path.GetFileNameWithoutExtension(path).ToLower();
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = abname + AB_EXTENSION;
        build.assetNames = new string[] { path };
        buildmap.Add(build);
    }

    // Get Bundle Name Eg: Assets/Game/Assets/Images/Common --> images_common.ab
    private static string MakeAssetBundleName(string path)
    {
        string abname = path.Replace(EditorConst.ASSET_ROOT, "").Replace("\\", "/").Replace("/", "_");
        return abname.Split('.')[0].ToLower() + AB_EXTENSION;
    }

    // Create Build Map
    private static AssetBundleBuild[] MakeAssetBundleBuild(string abname, string[] files)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            string ext = Path.GetExtension(files[i]);
            if (FilterExtensions.IndexOf(ext) != -1)
                continue;

            list.Add(files[i].Replace("\\", "/"));
        }
        if (list.Count == 0) return new AssetBundleBuild[0];

        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = abname;
        build.assetNames = list.ToArray();
        return new AssetBundleBuild[] { build };
    }

    private static void CompareManifestFile(AssetBundleManifest last, AssetBundleManifest now)
    {
        if (last == null || now == null)
            return;

        List<string> abnames1 = new List<string>(last.GetAllAssetBundles());
        List<string> abnames2 = new List<string>(now.GetAllAssetBundles());
        for (int i = 0; i < abnames1.Count; i++)
        {
            if (abnames2.IndexOf(abnames1[i]) == -1) //delete
                UnityEngine.Debug.Log("Delete AssetBundle: " + abnames1[i]);
        }
        for (int i = 0; i < abnames2.Count; i++)
        {
            if (abnames1.IndexOf(abnames2[i]) == -1) //add
            {
                UnityEngine.Debug.Log("Add AssetBundle: " + abnames2[i]);
                continue;
            }

            //check change
            Hash128 hash1 = last.GetAssetBundleHash(abnames2[i]);
            Hash128 hash2 = now.GetAssetBundleHash(abnames2[i]);
            if (!hash1.Equals(hash2))
                UnityEngine.Debug.Log("Change AssetBundle: " + abnames2[i]);
        }
    }

    // Write Version File
    private static void BuildVersionFile(string path, string output, List<AssetBundleBuild> buildmap)
    {
        string content = string.Empty;
        for (int i = 0; i < buildmap.Count; i++)
        {
            string abname = buildmap[i].assetBundleName;
            if (content != string.Empty) content += "\n";
            string filename = output + abname;
            content += abname + "|" + Util.MD5File(filename) + "|" + Util.GetFileSize(filename);
        }
        File.WriteAllText(path, content);

        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = "version.txt";
        buildmap.Add(build);
    }

}