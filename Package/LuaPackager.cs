using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

public class LuaPackager {

    public static void EncodeDatabaseFiles()
    {
        HandleAllLuaFiles(EditorConst.DATABASE_ROOT, EditorUtil.LuaOutPath + "/Lua32/database/", 32);
        HandleAllLuaFiles(EditorConst.DATABASE_ROOT, EditorUtil.LuaOutPath + "/Lua64/database/", 64);
    }

    public static void EncodeLuaFiles()
    {
#if UNITY_EDITOR_OSX
        HandleAllLuaFiles(EditorConst.LUA_ROOT, EditorUtil.LuaOutPath + "/Lua32/", 64);
        HandleAllLuaFiles(EditorConst.LUA_ROOT, EditorUtil.LuaOutPath + "/Lua64/", 64);
#else
        HandleAllLuaFiles(EditorConst.LUA_ROOT, EditorUtil.LuaOutPath + "/Lua32/", 32);
        HandleAllLuaFiles(EditorConst.LUA_ROOT, EditorUtil.LuaOutPath + "/Lua64/", 64);

        HandleAllLuaFiles(EditorConst.TOLUA_ROOT, EditorUtil.LuaOutPath + "/Lua32/", 32);
        HandleAllLuaFiles(EditorConst.TOLUA_ROOT, EditorUtil.LuaOutPath + "/Lua64/", 64);
#endif
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Encode LuaJIT32 lua files finish!!");
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
            src = Path.GetFullPath(src);
            //都是用jit
            //if (IOUtil.NewCopy(src, luaTempFile))
#if UNITY_EDITOR_OSX
            DoEncodeLuacFile(src, dest, mode);
#else
            DoEncodeJITFile(src, dest, mode);
#endif
        }
    }

    //JIT 的Lua编译方式
    private static void DoEncodeJITFile(string srcFile, string destFile, int mode)
    {
        if (mode == 0)
        {
            File.Copy(srcFile, destFile, true);
            return;
        }

        string currDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory("LuaEncoder/Luajit" + mode + "/");
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = "luajit.exe";
        info.Arguments = "-b " + srcFile + " " + destFile;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.ErrorDialog = true;
        info.UseShellExecute = true;

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    //LUAC 的编译方式
    private static void DoEncodeLuacFile(string srcFile, string destFile, int mode)
    {
        if (mode == 0)
        {
            File.Copy(srcFile, destFile, true);
            return;
        }
        string currDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(currDir + "/LuaEncoder/LuaC/");
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "./luac";
            info.Arguments = "-s -o " + destFile + " " + srcFile;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.ErrorDialog = true;
            info.UseShellExecute = false;

            Process pro = Process.Start(info);
            pro.WaitForExit();
        }
        catch(System.Exception e)
        {
            //LogUtil.LogExInfo("编译Lua异常 " , e);
        }
        finally
        {
            Directory.SetCurrentDirectory(currDir);
        }


    }

    [MenuItem("Game/打资源包/Copy")]
    public static void CreateDatabaseFile()
    {
        string lua32 = EditorUtil.LuaOutPath + "/Lua32/database";
        if (Directory.Exists(lua32))
        {
            string[] files = Directory.GetFiles(lua32, "*.bytes");
            foreach (var item in files)
            {
                File.Copy(item, Application.streamingAssetsPath + "/ab/" + "lua32_database_" + Path.GetFileNameWithoutExtension(item).ToLower()+".bytes", true);
            }
        }

        string lua64 = EditorUtil.LuaOutPath + "/Lua64/database";
        if (Directory.Exists(lua32))
        {
            string[] files = Directory.GetFiles(lua64, "*.bytes");
            foreach (var item in files)
            {
                File.Copy(item, Application.streamingAssetsPath + "/ab/" + "lua64_database_" + Path.GetFileNameWithoutExtension(item).ToLower() + ".bytes", true);
            }
        }
        AssetDatabase.Refresh();

    }

}
