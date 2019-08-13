using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace GameEditor
{
    public class EditorUtils
    {
        [MenuItem("Game/Tools/删掉Missing脚本")]
        public static void CleanupMissingScript()
        {
            GameObject[] pAllObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
            int r;
            int j;
            for (int i = 0; i < pAllObjects.Length; i++)
            {
                if (pAllObjects[i].hideFlags == HideFlags.None)//HideFlags.None 获取Hierarchy面板所有Object            
                {
                    var components = pAllObjects[i].GetComponents<Component>();
                    var serializedObject = new SerializedObject(pAllObjects[i]);
                    var prop = serializedObject.FindProperty("m_Component");
                    r = 0;
                    for (j = 0; j < components.Length; j++)
                    {
                        if (components[j] == null)
                        {
                            prop.DeleteArrayElementAtIndex(j - r);
                            r++;
                        }
                    }
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        [MenuItem("Game/Tools/清除Editor缓存", false)]
        static void ClearPlayerData()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Game/Tools/清空AB文件夹", false)]
        static void ClearCacheAB()
        {
            if (Directory.Exists(AppConst.CachePath))
                Directory.Delete(AppConst.CachePath, true);
        }



        [MenuItem("Assets/Game/Split Altas", false, 4)]
        static void SplitAtlas()
        {
            Object[] selects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            foreach (Object o in selects)
            {
                string path = AssetDatabase.GetAssetPath(o);
                string dir = Path.GetDirectoryName(path);
                string name = Path.GetFileNameWithoutExtension(path);

                TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
                imp.isReadable = true;
                AssetDatabase.ImportAsset(path);

                var hCount = 4;
                var vCount = 4;
                var atlas = o as Texture2D;
                var w = atlas.width / hCount;
                var h = atlas.height / vCount;

                for (int i = 0; i < vCount; i++)
                {
                    for (int j = 0; j < hCount; j++)
                    {
                        var tex = new Texture2D(w, h);
                        var x = j * w;
                        var y = (vCount - i - 1) * h;
                        for (int n = 0; n < w; n++)
                        {
                            for (int m = 0; m < h; m++)
                            {
                                var color = atlas.GetPixel(n + x, m + y);
                                tex.SetPixel(n, m, color);
                            }
                        }

                        var bytes = tex.EncodeToPNG();
                        var save = dir + "/" + name + "/";
                        if (!Directory.Exists(save)) Directory.CreateDirectory(save);
                        var filename = name + "_" + (i * 4 + j).ToString("00") + ".png";
                        File.WriteAllBytes(save + filename, bytes);
                    }
                }

                File.Delete(path);
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Game/Set Sprite Tag", false)]
        static void SetSpriteTag()
        {
            Object[] selects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            foreach (Object o in selects)
            {
                string path = AssetDatabase.GetAssetPath(o);
                string dir = Path.GetDirectoryName(path);
                if (!dict.ContainsKey(dir))
                    dict.Add(dir, new List<string>());

                dict[dir].Add(path);
            }

            foreach (var pair in dict)
            {
                if (pair.Value.Count == 0)
                    continue;

                var list = pair.Value;
                string[] dirs = Path.GetDirectoryName(list[0]).Split('/');
                string name = dirs[dirs.Length - 2] + "_" + dirs[dirs.Length - 1];
                for (int i = 0; i < list.Count; i++)
                {
                    TextureImporter asset = TextureImporter.GetAtPath(list[i]) as TextureImporter;
                    string tag = asset.spritePackingTag;
                    if (tag != name.ToLower())
                    {
                        asset.spritePackingTag = name.ToLower();
                        asset.SaveAndReimport();
                    }
                }
            }
            Debug.Log("Set Tag Success..");
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Game/Set Sprite Sheet", false)]
        static void SetSpriteSheet()
        {
            Object[] selects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            foreach (Object o in selects)
            {
                string path = AssetDatabase.GetAssetPath(o);
                string dir = Path.GetDirectoryName(path);
                if (!dict.ContainsKey(dir))
                    dict.Add(dir, new List<string>());

                dict[dir].Add(path);
            }

            foreach (var pair in dict)
            {
                if (pair.Value.Count == 0)
                    continue;

                var hcount = 2;
                var list = pair.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    var name = Path.GetFileNameWithoutExtension(list[i]);
                    var tex = AssetDatabase.LoadAssetAtPath<Texture>(list[i]);
                    var asset = TextureImporter.GetAtPath(list[i]) as TextureImporter;
                    asset.spriteImportMode = SpriteImportMode.Multiple;

                    var meta = new SpriteMetaData[2];
                    meta[0] = new SpriteMetaData();
                    meta[0].rect = new Rect(0f, 0f, tex.width / hcount, tex.height);
                    meta[0].name = name + "_0";
                    meta[1] = new SpriteMetaData();
                    meta[1].rect = new Rect(tex.width / hcount, 0f, tex.width / hcount, tex.height);
                    meta[1].name = name + "_1";
                    asset.spritesheet = meta;
                    asset.SaveAndReimport();
                }
            }
            Debug.Log("Set Sheet Success..");
            AssetDatabase.Refresh();
        }


        [MenuItem("Assets/Game/Set Texture Format(32bit)", false)]
        static void SetTextureFormat32()
        {
            Object[] selects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            foreach (Object o in selects)
            {
                string path = AssetDatabase.GetAssetPath(o);
                TextureImporter asset = TextureImporter.GetAtPath(path) as TextureImporter;
                TextureImporterPlatformSettings texSettings = new TextureImporterPlatformSettings();
                texSettings.overridden = true;
                texSettings.format = TextureImporterFormat.RGBA32;
                texSettings.name = "iPhone";
                asset.SetPlatformTextureSettings(texSettings);
                asset.SaveAndReimport();
                AssetDatabase.ImportAsset(path);
            }
            AssetDatabase.Refresh();
            Debug.Log("Set Texture Format Success..");
        }

        [MenuItem("Assets/Game/Set Texture Format(16bit)", false)]
        static void SetTextureFormat16()
        {
            Object[] selects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            foreach (Object o in selects)
            {
                string path = AssetDatabase.GetAssetPath(o);
                TextureImporter asset = TextureImporter.GetAtPath(path) as TextureImporter;
                TextureImporterPlatformSettings texSettings = new TextureImporterPlatformSettings();
                texSettings.overridden = true;
                texSettings.format = TextureImporterFormat.RGBA16;
                texSettings.name = "iPhone";
                asset.SetPlatformTextureSettings(texSettings);
                asset.SaveAndReimport();
                AssetDatabase.ImportAsset(path);
            }
            AssetDatabase.Refresh();
            Debug.Log("Set Texture Format Success..");
        }

        [MenuItem("Assets/Game/Create Character Data", false)]
        static void CreateCharacterData()
        {
            Object[] selects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            foreach (Object o in selects)
            {
                string path = AssetDatabase.GetAssetPath(o);
                string dir = Path.GetDirectoryName(path);
                if (!dict.ContainsKey(dir))
                    dict.Add(dir, new List<string>());

                dict[dir].Add(path);
            }

            foreach (var pair in dict)
            {
                if (pair.Value.Count == 0)
                    continue;

                var list = pair.Value;
                list.Sort();

                string path = Path.GetDirectoryName(list[0]);
                string name = Path.GetFileName(path);
                Sprite[] sprites = new Sprite[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(list[i]);
                }

                var go = new GameObject(name);
                var com = go.AddComponent<CharacterData>();
                com.sprites = sprites;

                var dir = Path.GetDirectoryName(path);
                var filename = dir + "/" + name + ".prefab";
                PrefabUtility.CreatePrefab(filename, go);

                GameObject.DestroyImmediate(go);
            }

            AssetDatabase.Refresh();

            Debug.Log("Create Sprite Animation Finish!!");
        }

        [MenuItem("Assets/Game/Create Sprite Animation", false)]
        static void CreateSpriteAnimation()
        {
            Object[] selects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            foreach (Object o in selects)
            {
                string path = AssetDatabase.GetAssetPath(o);
                string dir = Path.GetDirectoryName(path);
                if (!dict.ContainsKey(dir))
                    dict.Add(dir, new List<string>());

                dict[dir].Add(path);
            }

            foreach (var pair in dict)
            {
                if (pair.Value.Count == 0)
                    continue;

                var list = pair.Value;
                list.Sort();

                string path = Path.GetDirectoryName(list[0]);
                string name = Path.GetFileName(path);
                Sprite[] sprites = new Sprite[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(list[i]);
                }

                var go = new GameObject(name);
                var img = go.AddComponent<Image>();
                img.rectTransform.anchorMin = new Vector2(0.5f, 0f);
                img.rectTransform.anchorMax = new Vector2(0.5f, 0f);
                img.rectTransform.pivot = new Vector2(0.5f, 0f);

                var anim = go.AddComponent<UISpriteAnimation>();
                anim.sprites = sprites;
                anim.fps = 10;
                anim.isLoop = true;
                anim.isSnap = false;


                var folder = Path.GetFileName(Path.GetDirectoryName(path));
                var savePath = "Assets/Game/Assets/Animations/" + folder;
                var filename = savePath + "/" + name + ".prefab";
                PrefabUtility.CreatePrefab(filename, go);

                GameObject.DestroyImmediate(go);
            }

            AssetDatabase.Refresh();

            Debug.Log("Create Sprite Animation Finish!!");
        }


        [MenuItem("Assets/Game/Create Sprite Render Animation", false)]
        static void CreateSpriteRenderAnimation()
        {
            Object[] selects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            foreach (Object o in selects)
            {
                string path = AssetDatabase.GetAssetPath(o);
                string dir = Path.GetDirectoryName(path);
                if (!dict.ContainsKey(dir))
                    dict.Add(dir, new List<string>());

                dict[dir].Add(path);
            }

            foreach (var pair in dict)
            {
                if (pair.Value.Count == 0)
                    continue;

                var list = pair.Value;
                list.Sort();

                string path = Path.GetDirectoryName(list[0]);
                string name = Path.GetFileName(path);
                Sprite[] sprites = new Sprite[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(list[i]);
                }

                var go = new GameObject(name);
                go.AddComponent<SpriteRenderer>();

                var anim = go.AddComponent<UISpriteRenderAnimation>();
                anim.sprites = sprites;
                anim.fps = 10;
                anim.isLoop = true;
                anim.isSnap = false;

                var folder = Path.GetFileName(Path.GetDirectoryName(path));
                var savePath = "Assets/Game/Assets/Animations/" + folder;
                if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
                var filename = savePath + "/" + name + ".prefab";
                PrefabUtility.CreatePrefab(filename, go);
                GameObject.DestroyImmediate(go);
            }

            AssetDatabase.Refresh();
            Debug.Log("Create Sprite Render Animation Finish!!");
        }

        [MenuItem("Assets/Game/Snap Sprite Animation")]
        static void SanpSpriteAnimation()
        {
            Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            foreach (Object o in selects)
            {
                var go = o as GameObject;
                if (go == null) continue;

                var anim = go.GetComponent<UISpriteAnimation>();
                if (anim == null) continue;

                var img = anim.GetComponent<Image>();
                if (img != null)
                {
                    var tex = img.sprite.texture;
                    var size = img.rectTransform.sizeDelta;
                    if (size.x != tex.width || size.y != tex.height)
                    {
                        Debug.Log("Sanp Sprite:" + img.name);
                        img.SetNativeSize();
                        EditorUtility.SetDirty(o);
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Snap  Sprite Animation Finish!!");
        }

        [MenuItem("Assets/Game/Format Battle Effect", false)]
        static void FormatBattleEffect()
        {
            Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            foreach (Object o in selects)
            {
                var path = AssetDatabase.GetAssetPath(o);
                if (!path.EndsWith(".prefab")) continue;

                var go = GameObject.Instantiate(o) as GameObject;
                var coms = go.GetComponents<Component>();
                var spanim = go.GetComponent<UISpriteAnimation>();
                if (spanim != null)
                {
                    Object.DestroyImmediate(go);
                    continue;
                }
                var anim = go.GetComponent<UIParticleAnimation>();
                if (anim != null)
                {
                    Object.DestroyImmediate(go);
                    continue;
                }

                var all = go.GetComponentsInChildren<ParticleSystem>();
                var time = 0f;
                var loop = false;
                for (int j = 0; j < all.Length; j++)
                {
                    var par = all[j];
                    var duration = 0f;
                    var delay = par.main.startDelay.constant;
                    var life = par.main.startLifetime.constant;
                    if (par.emission.rateOverTime.constant <= 0f)
                        duration = delay + life;
                    else
                        duration = delay + Mathf.Max(par.main.duration, life);
                    time = Mathf.Max(time, duration);
                    loop = loop || par.main.loop;

                    var render = par.GetComponent<Renderer>();
                    if (render != null && render.sortingOrder < 580)
                    {
                        var order = render.sortingOrder % 100;
                        render.sortingOrder = 580 + order;
                    }
                }

                anim = go.AddComponent<UIParticleAnimation>();
                anim.duration = time;
                anim.isLoop = loop;

                go.transform.localPosition = Vector3.zero;
                PrefabUtility.ReplacePrefab(go, o);
                Object.DestroyImmediate(go);
            }
            AssetDatabase.Refresh();
            Debug.Log("Format Battle Effect Finish...");
        }

        [MenuItem("Assets/Game/Format Battle Effect Order", false)]
        static void FormatBattleEffectOrder()
        {
            Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            foreach (Object o in selects)
            {
                var path = AssetDatabase.GetAssetPath(o);
                if (!path.EndsWith(".prefab")) continue;

                var go = o as GameObject;
                var all = go.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < all.Length; i++)
                {
                    var render = all[i].GetComponent<Renderer>();
                    if (render != null && render.sortingOrder < 580)
                    {
                        var order = render.sortingOrder % 100;
                        render.sortingOrder = 580 + order;
                    }
                }
            }
            AssetDatabase.Refresh();
            Debug.Log("Format Battle Effect Finish...");
        }

        [MenuItem("Assets/Game/Find All Chinese", false)]
        static void FindAllChinese()
        {
            Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            Regex regex = new Regex("[\u4E00-\u9FFF]+"); //中文正则表达式
            foreach (Object o in selects)
            {
                var prefab = o as GameObject;
                if (prefab == null) continue;
                var list = prefab.GetComponentsInChildren<Text>(true);
                var txt = "";
                if (list.Length > 0)
                {
                    var path = AssetDatabase.GetAssetPath(o);
                    txt += "PATH:" + path;
                }

                var find = false;
                for (int i = 0; i < list.Length; i++)
                {
                    var mc = regex.Matches(list[i].text);
                    if (mc.Count == 0) continue;
                    var lang = list[i].GetComponent<UILanguageSelector>();
                    if (lang == null || string.IsNullOrEmpty(lang.module) || lang.index == 0)
                    {
                        find = true;
                        var tmp = list[i].transform.name;
                        if (list[i].transform.parent)
                            tmp = list[i].transform.parent.name + "/" + tmp;
                        txt += "\nNAME:" + tmp + " -> " + list[i].text;
                    }
                }
                if (find)
                {
                    Debug.Log(txt);
                }
            }
        }


        [MenuItem("Assets/Game/Replace All Font", false)]
        static void FindAllFont()
        {
            Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Assets/Fonts/Text/sh.ttf");
            foreach (Object o in selects)
            {
                var prefab = o as GameObject;
                if (prefab == null) continue;
                var list = prefab.GetComponentsInChildren<Text>(true);
                var ischange = false;
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i].font.name == "sh")
                    {
                        list[i].font = font;
                        ischange = true;
                    }
                }
                if (ischange)
                    EditorUtility.SetDirty(o);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("字体替换完成!!");
        }


        [MenuItem("Assets/Game/Find All Sprite", false)]
        static void FindAllSprite()
        {
            Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            foreach (Object o in selects)
            {
                var prefab = o as GameObject;
                if (prefab == null) continue;

                var filter = new string[] { "controls010" };
                var list = prefab.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < list.Length; i++)
                {
                    var sprite = list[i].sprite;
                    if (sprite == null) continue;

                    for (int j = 0; j < filter.Length; j++)
                    {
                        if (sprite.name.IndexOf(filter[j]) != -1)
                        {
                            Debug.Log("#########:" + prefab.name + " -> " + list[i].gameObject.name + " -> " + sprite.name);
                            break;
                        }
                    }
                }
            }
        }

    }



}
