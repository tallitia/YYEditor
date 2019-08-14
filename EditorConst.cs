using UnityEngine;

public class EditorConst
{
    public const string GAME_ROOT = "Assets/Game/";

    public const string ASSET_ROOT = GAME_ROOT + "Assets/";

    public const string LUA_ROOT = GAME_ROOT + "Lua/";

    public const string LUA_OUT = ASSET_ROOT + "Lua/";

    public const string TOLUA_ROOT = GAME_ROOT + "ToLua/Lua/";

    public const string TOLUA_OUT = ASSET_ROOT + "ToLua/";

    public const string DATABASE_ROOT = LUA_ROOT + "/database/";

    public const string DATABASE_OUT = LUA_OUT + "/database/";

    public const string LUA_CONFIG_PATH = GAME_ROOT + "Editor/Config/LuaConfig.txt";

    public const string ASSET_CONFIG_PATH = GAME_ROOT + "Editor/Config/AssetConfig.txt";

    public static string ABZIP_PATH = Application.streamingAssetsPath + "/ab.zip";

    public static string AB_PATH = Application.streamingAssetsPath + "/ab";

    public static string AB_OUTPUT_PATH = "AssetBundles/";

}