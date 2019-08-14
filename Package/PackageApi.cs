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

    public static string VERSION = "";
    public static string VERSION_PATH = OUTPUT_ROOT + "version.txt";
    public static string XVERSION_PATH = OUTPUT_ROOT + "xversion.txt";

    public static string AB_EXTENSION = ".ab";
    public static string AB_MANIFEST = "index";

    public static List<string> FilterExtensions = new List<string> { ".meta" };

    public static BuildTarget buildTarget;

    public static string OUTPUT_ROOT
    {
        get
        {
            if (string.IsNullOrEmpty(VERSION))
            {
                string content = File.ReadAllText(AppConst.SettingPath);
                SecurityElement setting = SecurityElement.FromString(content);
                VERSION = setting.Attribute("version");

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
        bool ret = EditorUtility.DisplayDialog("提示", message, "确认", "取消");
        if (ret)
        {
            UnityEngine.Debug.Log("1111");
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            action();
        }
    }

    private static void BuildNormalAB()
    {
        PackAssetBundles(false);
    }

    private static void BuildZipAB()
    {
        PackAssetBundlesZip(false);
    }

    private static void BuildDatabase()
    {
        LuaPackager.EncodeDatabaseFiles();
        LuaPackager.BuildDatabaseFiles();
        UnityEngine.Debug.Log("Build Databases Finish!!");
    }

    private static void BuildLua()
    {
        LuaPackager.EncodeLuaJIT32Files();
        LuaPackager.EncodeLuaJIT64Files();
        UnityEngine.Debug.Log("Build Databases Finish!!");
    }

    /// pack assetbundles, multi: multi-language
    public static List<AssetBundleBuild> PackAssetBundles(bool multi)
    {
        if (EditorApplication.isCompiling)
        {
            EditorUtility.DisplayDialog("", "Script is compiling, try again later.", "Yes");
            return null;
        }

        LuaPackager.EncodeLuaJIT32Files();
        LuaPackager.EncodeLuaJIT64Files();

        //read config
        Dictionary<string, int> config = new Dictionary<string, int>();
        string[] lines = File.ReadAllLines(EditorConst.ASSET_CONFIG_PATH);
        for (int i = 0; i < lines.Length; i++)
        {
            //路径 | 打包方式(1:文件夹打包成一个AB, 2:文件打成一个AB, 3:文件夹里的文件夹打成一个AB, 4:文件夹里的文件打成一个AB)
            string[] line = lines[i].Split('|');
            config.Add(line[0], int.Parse(line[1]));
        }

        //make build map
        List<AssetBundleBuild> buildmap = new List<AssetBundleBuild>();
        foreach (var pair in config)
        {
            string mname = "DoMakeAssetBundleBuild" + pair.Value;
            MethodInfo method = typeof(PackageApi).GetMethod(mname, BindingFlags.Static | BindingFlags.Public);
            if (method == null) continue;
            string src = EditorConst.ASSET_ROOT + pair.Key;
            object builds = method.Invoke(null, new object[] { src });
            buildmap.AddRange(builds as IEnumerable<AssetBundleBuild>);
        }

        if (!Directory.Exists(OUTPUT_ROOT)) Directory.CreateDirectory(OUTPUT_ROOT);
        PackageApi.BuildAssetConfig(EditorConst.ASSET_CONFIG_PATH, buildmap);
        PackageApi.BuildLuaConfig(EditorConst.LUA_CONFIG_PATH, buildmap);
        AssetDatabase.Refresh();

        //build assetbundles
        BuildPipeline.BuildAssetBundles(OUTPUT_ROOT, buildmap.ToArray(),
            BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget);

        //rename manifest file
        string manifestPath = OUTPUT_ROOT + Path.GetFileName(Path.GetDirectoryName(OUTPUT_ROOT));
        File.Copy(manifestPath, OUTPUT_ROOT + AB_MANIFEST + AB_EXTENSION, true);
        File.Delete(manifestPath);
        File.Copy(manifestPath + ".manifest", OUTPUT_ROOT + AB_MANIFEST + ".manifest", true);
        File.Delete(manifestPath + ".manifest");

        //add manifest to version control
        AssetBundleBuild abb = new AssetBundleBuild();
        abb.assetBundleName = AB_MANIFEST + AB_EXTENSION;
        buildmap.Add(abb);

        if (multi)
        {
            //build fonts
            PackageApi.BuildFontAssetBundle("zh", buildmap);
            PackageApi.BuildFontAssetBundle("hk", buildmap);
            PackageApi.BuildFontAssetBundle("en", buildmap);
        }

        //build version file
        PackageApi.BuildVersionFile(VERSION_PATH, OUTPUT_ROOT, buildmap);

        //build database file
        LuaPackager.BuildDatabaseFiles();

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Build assetbundles files finish!!");
        return buildmap;
    }

    /// ex:Assets/Game/Assets/Images/Common --> images_common.ab
    private static string MakeAssetBundleName(string path)
    {
        string abname = path.Replace(EditorConst.ASSET_ROOT, "").Replace("\\", "/").Replace("/", "_");
        return abname.Split('.')[0].ToLower() + AB_EXTENSION;
    }

    /// create the assetbundle build 
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

    // build asset config
    private static void BuildAssetConfig(string path, List<AssetBundleBuild> buildmap)
    {
        string abname = Path.GetFileNameWithoutExtension(path).ToLower();
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = abname + AB_EXTENSION;
        build.assetNames = new string[] { path };
        buildmap.Add(build);
    }

    // build lua config
    private static void BuildLuaConfig(string path, List<AssetBundleBuild> buildmap)
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

    // build version file
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
    }

    /// build font assetbundle
    private static void BuildFontAssetBundle(string lang, List<AssetBundleBuild> buildmap)
    {
        var map = new List<AssetBundleBuild>();
        var backup = "Assets/Game/Assets/Fonts/Backup";
        var raw = "Assets/Game/Assets/Fonts/Text";
        var abname = "fonts_text.ab";

        File.Copy(raw + "/sh.ttf", raw + "temp.ttf", true);
        File.Copy(backup + "/sh-" + lang + ".ttf", raw + "/sh.ttf", true);
        AssetDatabase.Refresh();

        var files = Directory.GetFiles(raw, "*", SearchOption.AllDirectories);
        List<string> list = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            string ext = Path.GetExtension(files[i]);
            if (ext == ".meta")
                continue;

            list.Add(files[i].Replace("\\", "/"));
        }

        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = abname;
        build.assetNames = list.ToArray();
        map.Add(build);

        var temp = OUTPUT_ROOT + "temp/";
        if (Directory.Exists(temp)) Directory.Delete(temp, true);
        Directory.CreateDirectory(temp);

        BuildPipeline.BuildAssetBundles(temp, map.ToArray(),
            BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget);

        // 重命名ab
        var filename = "fonts_text_" + lang + ".ab";
        var now = Util.MD5File(temp + abname);
        var old = "";
        if (File.Exists(OUTPUT_ROOT + filename))
            old = Util.MD5File(OUTPUT_ROOT + filename);
        if (now != old)
            File.Copy(temp + abname, OUTPUT_ROOT + filename, true);
        Directory.Delete(temp, true);

        // 添加到版本管理
        build.assetBundleName = filename;
        buildmap.Add(build);

        File.Copy(raw + "temp.ttf", raw + "/sh.ttf", true);
        File.Delete(raw + "temp.ttf");
        AssetDatabase.Refresh();
    }

    /// pack assetbundles zip ab.zip, multi: multi-language
    public static void PackAssetBundlesZip(bool multi)
    {
        List<AssetBundleBuild> buildmap = PackAssetBundles(multi);
        if (buildmap == null)
            return;

        //create temp folder
        string zipPath = OUTPUT_ROOT + "ZIP/";
        if (Directory.Exists(zipPath)) Directory.Delete(zipPath, true);
        Directory.CreateDirectory(zipPath);

        string abPath = EditorConst.AB_PATH;
        if (Directory.Exists(abPath)) Directory.Delete(abPath, true);
        Directory.CreateDirectory(abPath);

        //add version file
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = "version.txt";
        buildmap.Add(build);

        //copy assetbundle file
        for (int i = 0; i < buildmap.Count; i++)
        {
            string abname = buildmap[i].assetBundleName;
            if (abname.EndsWith(".txt") || abname.EndsWith(".bytes"))
            {
                FileInfo file = new FileInfo(OUTPUT_ROOT + abname);
                file.CopyTo(Path.Combine(zipPath, abname), true);
            }
            if (abname.EndsWith(".txt") || abname.EndsWith(".ab"))
            {
                FileInfo file = new FileInfo(OUTPUT_ROOT + abname);
                file.CopyTo(Path.Combine(abPath, abname), true);
            }
        }

        //copy database version
        var lines = File.ReadAllLines(XVERSION_PATH);
        for (int i=0; i<lines.Length; i++)
        {
            var line = lines[i].Split('|');
            File.Copy(OUTPUT_ROOT + line[0], Path.Combine(zipPath, line[0]), true);
        }
        var xname = Path.GetFileName(XVERSION_PATH);
        File.Copy(XVERSION_PATH, Path.Combine(zipPath, xname), true);

        //create zip files
        Zip.ZipDerctory(zipPath, EditorConst.ABZIP_PATH, ".manifest|.meta");

        //byte[] bytes = File.ReadAllBytes(ABZIP_PATH);
        //byte[] buff = AesTool.Encrypt(bytes, 0, bytes.Length, SDKConst.SECRET);
        //File.WriteAllBytes(ABZIP_PATH, buff);

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Build ab.zip finish!!");
    }


}