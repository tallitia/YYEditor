using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

public class LuaPackager {

    public static void EncodeLuaFiles()
    {
        HandleAllLuaFiles("Assets/Game/Lua/", EditorConst.LUA_ROOT + "/");
        HandleAllLuaFiles("Assets/Game/ToLua/Lua/", EditorConst.LUA_ROOT + "/");

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Encode lua files finish!!");
    }



    #region Database的处理

    public static void EncodeDatabaseFiles()
    {
        HandleAllLuaFiles(EditorConst.LUA_DATABASE, EditorConst.LUA_ROOT + "32/database/", 32);
        HandleAllLuaFiles(EditorConst.LUA_DATABASE, EditorConst.LUA_ROOT + "64/database/", 64);
    }
    #endregion

    public static void EncodeLuaJIT32Files()
    {
        HandleAllLuaFiles("Assets/Game/Lua/", EditorConst.LUA_ROOT + "32/", 32);
        HandleAllLuaFiles("Assets/Game/ToLua/Lua/", EditorConst.LUA_ROOT + "32/", 32);

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Encode LuaJIT32 lua files finish!!");
    }

    public static void EncodeLuaJIT64Files()
    {
        //if (!EditorUtility.DisplayDialog("", "Encode LuaJIT64 Lua Files?", "Yes", "No"))
        //    return;

        HandleAllLuaFiles("Assets/Game/Lua/", EditorConst.LUA_ROOT + "64/", 64);
        HandleAllLuaFiles("Assets/Game/ToLua/Lua/", EditorConst.LUA_ROOT + "64/", 64);

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Encode LuaJIT64 lua files finish!!");
    }

    // 处理所有的lua文件。 mode:0 不用byte  32 luajit32  64 luajit 64
    public static void HandleAllLuaFiles (string path, string destDir, int mode = 0)
    {
        string[] files = Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string src = files[i].Replace("\\", "/");
            string file = src.Replace(path, "");
            if (path.Contains("database")) file = file.ToLower();
            string dest = destDir + file + ".bytes";
            string dir = Path.GetDirectoryName(dest);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            //ios 使用luac
            if (PackageApi.buildTarget == BuildTarget.iOS)
                DoEncodeLuacFile(src, dest, mode);
            else
                DoEncodeJITFile(src, dest, mode);
        }
    }

    //windows 和 Android 的Lua编译方式
    public static void DoEncodeJITFile(string srcFile, string destFile, int mode)
    {
        if (mode == 0)
        {
            File.Copy(srcFile, destFile, true);
            return;
        }

        string currDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory("LuaEncoder/luajit" + mode + "/");
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = "luajit.exe";
        info.Arguments = "-b ../../" + srcFile + " ../../" + destFile;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.ErrorDialog = true;
        info.UseShellExecute = true;

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    //iOS的Lua编译方式
    public static void DoEncodeLuacFile(string srcFile, string destFile, int mode)
    {
        if (mode == 0)
        {
            File.Copy(srcFile, destFile, true);
            return;
        }

        string currDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory("LuaEncoder/luavm/");
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = "./luac";
        info.Arguments = "-o ../../" + srcFile + " ../../" + destFile;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.ErrorDialog = true;
        info.UseShellExecute = false;

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    public static void BuildDatabaseFiles()
    {
        var src = "Assets/Game/Assets/Lua32/database/";
        var files = Directory.GetFiles(src, "*.bytes", SearchOption.AllDirectories);
        var content = string.Empty;
        for (int i = 0; i < files.Length; i++)
        {
            if (content != string.Empty) content += "\n";
            var filename = "lua32_database_" + Path.GetFileName(files[i]).ToLower();
            content += filename + "|" + Util.MD5File(files[i]) + "|" + Util.GetFileSize(files[i]);

            var dest = PackageApi.OUTPUT_ROOT + filename;
            if (File.Exists(dest))
            {
                var old = Util.MD5File(dest);
                var now = Util.MD5File(files[i]);
                if (old == now) continue;
            }
            File.Copy(files[i], dest, true);
        }

        src = "Assets/Game/Assets/Lua64/database/";
        files = Directory.GetFiles(src, "*.bytes", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (content != string.Empty) content += "\n";
            var filename = "lua64_database_" + Path.GetFileName(files[i]).ToLower();
            content += filename + "|" + Util.MD5File(files[i]) + "|" + Util.GetFileSize(files[i]);

            var dest = PackageApi.OUTPUT_ROOT + filename;
            if (File.Exists(dest))
            {
                var old = Util.MD5File(dest);
                var now = Util.MD5File(files[i]);
                if (old == now) continue;
            }
            File.Copy(files[i], dest, true);
        }

        File.WriteAllText(PackageApi.XVERSION_PATH, content);
    }



}
