using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class EditorUtil {

    private static string s_TempPath = null;
    private static string s_LuaTempPath = null;

    public static string LuaOutPath
    {
        get
        {
            if (s_TempPath == null)
            {
                //s_TempPath = Directory.GetParent(Application.dataPath) + "/Temp/Lua/";
                s_TempPath = Application.dataPath + "/Game/Assets/";
                if (!Directory.Exists(s_TempPath))
                    Directory.CreateDirectory(s_TempPath);
            }
            return s_TempPath;
        }
    }

    //创建一份temp作为对比， 用于NewCopy对比。
    public static string GetLuaTempPath(string pSrcFile)
    {
        if (s_LuaTempPath == null)
        {
            s_LuaTempPath = Directory.GetParent(Application.dataPath) + "/Temp/LuaTemp/";
            if (!Directory.Exists(s_LuaTempPath))
                Directory.CreateDirectory(s_LuaTempPath);
        }
        string pSrc = FormatPath(pSrcFile);
        string rootLua = FormatPath(Path.GetFullPath(EditorConst.LUA_ROOT));
        string rootToLua = FormatPath(Path.GetFullPath(EditorConst.TOLUA_ROOT));
        string relativepath;
        if (pSrc.StartsWith(rootToLua))
            relativepath = pSrc.Replace(rootToLua, "tolua/");
        else
            relativepath = pSrc.Replace(rootLua, "");

        string destPath = s_LuaTempPath + relativepath;
        return destPath;
    }

    //如果src 比较新才拷贝
    public static bool NewCopy(string pSrcFile, string pDestFile)
    {
        if (!File.Exists(pDestFile))
        {
            string dir = Path.GetDirectoryName(pDestFile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.Copy(pSrcFile, pDestFile);
            return true;
        }
        FileInfo srcFile = new FileInfo(pSrcFile);
        FileInfo destFile = new FileInfo(pDestFile);
        DateTime srcTime = srcFile.LastWriteTime;
        DateTime destTime = destFile.LastWriteTime;

        if (srcTime > destTime)
        {
            File.Copy(pSrcFile, pDestFile);
            return true;
        }
        return false;
    }

    public static string FormatPath(string pPath)
    {
        return pPath.Replace("\\", "/");
    }

    //路径 | 打包方式(1:文件夹打包成一个AB, 2:文件打成一个AB, 3:文件夹里的文件夹打成一个AB, 4:文件夹里的文件打成一个AB)
    public static void ReadAssetConfig(Dictionary<string, int> pConfig)
    {
        string[] lines = File.ReadAllLines(EditorConst.ASSET_CONFIG_PATH);
        for (int i = 0; i < lines.Length; i++)
        {
            string[] line = lines[i].Split('|');
            pConfig.Add(line[0], int.Parse(line[1]));
        }
    }





}
