using UnityEngine;

public class EditorConst
{
    /// 资源根目录
    public const string ASSET_ROOT = "Assets/Game/Assets/";

    /// 脚本根目录
    public const string LUA_ROOT = "Assets/Game/Assets/Lua/";

    ///Database目录
    public const string LUA_DATABASE = LUA_ROOT + "/database/";

    /// 脚本配置文件
    public const string LUA_CONFIG_PATH = "Assets/Game/Editor/Config/LuaConfig.txt";

    /// 资源配置文件 
    public const string ASSET_CONFIG_PATH = "Assets/Game/Editor/Config/AssetConfig.txt";

    /// AssetBundle压缩包路径
    public static string ABZIP_PATH = Application.streamingAssetsPath + "/ab.zip";

    /// AssetBundle路径
    public static string AB_PATH = Application.streamingAssetsPath + "/ab";

    /// AssetBundle输出路径
    public static string AB_OUTPUT_PATH = "AssetBundles/";

}