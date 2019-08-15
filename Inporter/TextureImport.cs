using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

public class TextureImport : AssetPostprocessor
{
	private static List<string> ignoreFileList = null;

	public static void readIgnoreFiles ()
	{
		ignoreFileList = new List<string> ();
		//try {
		//	StreamReader sr = new StreamReader (new FileStream (Application.dataPath + "/UnResizeUimageList.txt", FileMode.Open));
		//	string line = null;
		//	while ((line = sr.ReadLine ()) != null) {
		//		ignoreFileList.Add ("Assets/" + line);
		//	}
		//	sr.Close ();
		//} catch (Exception e) {
            
		//}
	}

	void OnPostprocessTexture (Texture2D texture)
	{
		string atlasName = new DirectoryInfo (Path.GetDirectoryName (assetPath)).Name;
		TextureImporter importer = assetImporter as TextureImporter;

		if (assetPath.IndexOf ("/fx/") >= 0 || assetPath.ToLower ().IndexOf ("emoji") > 0) {
			return;
		}

		if (assetPath.IndexOf ("/TextureAlphaAtlas/") >= 0) {
//			if (!textureDicCount.ContainsKey (assetPath)) {
//				textureDicCount.Add (assetPath, 0);
//			}
//			if (textureDicCount [assetPath] >= 2) {
//				return;
//			}
//			int count = textureDicCount [assetPath];
//			textureDicCount [assetPath] = count++;

			importer.textureType = TextureImporterType.Default;
			importer.mipmapEnabled = false;
			importer.mipMapBias = 0;
			importer.borderMipmap = false;
			importer.isReadable = false;
			importer.npotScale = TextureImporterNPOTScale.None;
			importer.textureType = TextureImporterType.Default;
			importer.filterMode = FilterMode.Bilinear;
			importer.SetPlatformTextureSettings ("Standalone", 1024, TextureImporterFormat.AutomaticCompressed);
			importer.SetPlatformTextureSettings ("Android", 1024, TextureImporterFormat.AutomaticCompressed);
			importer.SetPlatformTextureSettings ("iPhone",1024, TextureImporterFormat.RGB16);//PVRTC_RGB4
			TextureImporterSettings tt = new TextureImporterSettings ();
			importer.ReadTextureSettings (tt);
			importer.SetTextureSettings (tt);
			return;
		}

		if (assetPath.IndexOf ("/AddOns/") >= 0 || assetPath.IndexOf ("/CustomImages/") >= 0
		    || assetPath.IndexOf ("/Resources/") >= 0 || assetPath.IndexOf ("/Design/") >= 0
		    || assetPath.IndexOf ("/NewWorldMapWindow/") >= 0) {
			return;
		}
		if (ignoreFileList == null) {
			readIgnoreFiles ();
		}
		
		if (assetPath.Contains ("Game/Assets/Textures")) {
			importer.textureType = TextureImporterType.Default;
			importer.mipmapEnabled = false;
			importer.mipMapBias = 0;
			importer.borderMipmap = false;
			importer.mipmapFadeDistanceStart = 0;
			importer.mipmapFadeDistanceEnd = 0;
			importer.mipMapsPreserveCoverage = false;
			importer.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality16Bit;
			int maxWidth = 2048;

			TextureImporterSettings tt2 = new TextureImporterSettings ();
			importer.ReadTextureSettings (tt2);

			if (importer.maxTextureSize == 1024) {
				maxWidth = 1024;
				importer.npotScale = TextureImporterNPOTScale.ToLarger;
				importer.filterMode = FilterMode.Point;
			} else {
				maxWidth = 2048;
				importer.npotScale = TextureImporterNPOTScale.ToLarger;
				importer.filterMode = FilterMode.Bilinear;
			}

			if (importer.alphaIsTransparency) {
				importer.SetPlatformTextureSettings ("Standalone", maxWidth, TextureImporterFormat.RGBA32);
				importer.SetPlatformTextureSettings ("Android", maxWidth, TextureImporterFormat.ETC2_RGBA8, false);
			} else {
				importer.SetPlatformTextureSettings ("Standalone", maxWidth, TextureImporterFormat.RGB24);
				importer.SetPlatformTextureSettings ("Android", maxWidth, TextureImporterFormat.ETC_RGB4, false);
			}

			TextureImporterPlatformSettings iosSettings = importer.GetPlatformTextureSettings ("iPhone");
			if (iosSettings.format != TextureImporterFormat.RGBA16 && iosSettings.format != TextureImporterFormat.RGB16) {
				if (importer.alphaIsTransparency) {
					importer.SetPlatformTextureSettings ("iPhone", maxWidth, TextureImporterFormat.PVRTC_RGBA4);
				} else {
					importer.SetPlatformTextureSettings ("iPhone", maxWidth, TextureImporterFormat.PVRTC_RGB4);
				}
			} else {
				//importer.SetPlatformTextureSettings ("iPhone", maxWidth, TextureImporterFormat.RGBA16);
			}
			TextureImporterSettings tt = new TextureImporterSettings ();
			importer.ReadTextureSettings (tt);
			importer.SetTextureSettings (tt);
			return;
		} else {
			importer.maxTextureSize = 1024;
		}

		importer.spritePackingTag = atlasName;
		//先不要
		if (assetPath.IndexOf ("img_b_Noneg") < 0) {
			importer.spriteImportMode = SpriteImportMode.Single;
			importer.filterMode = FilterMode.Bilinear;
			importer.borderMipmap = false;
			importer.crunchedCompression = false;
			importer.isReadable = false;
			importer.mipmapEnabled = false;
			importer.npotScale = TextureImporterNPOTScale.None;
			importer.sRGBTexture = true;
			importer.textureCompression = TextureImporterCompression.Compressed;
			importer.textureShape = TextureImporterShape.Texture2D;
			importer.textureType = TextureImporterType.Sprite;

       
            if (importer.alphaIsTransparency) {
				importer.SetPlatformTextureSettings ("Standalone", importer.maxTextureSize, TextureImporterFormat.RGBA32);
			} else {
				importer.SetPlatformTextureSettings ("Standalone", importer.maxTextureSize, TextureImporterFormat.RGBA32);
			}
//			importer.SetPlatformTextureSettings ("Standalone", importer.maxTextureSize, TextureImporterFormat.DXT5Crunched);
			importer.SetPlatformTextureSettings ("Android", importer.maxTextureSize, TextureImporterFormat.ETC2_RGBA8, true);
			if ((assetPath.IndexOf ("PublicResources") >= 0) && EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS) {
				importer.SetPlatformTextureSettings ("iPhone", importer.maxTextureSize, TextureImporterFormat.PVRTC_RGBA4);//PVRTC_RGBA4
			} else {
                TextureImporterPlatformSettings iosSettings = importer.GetPlatformTextureSettings("iPhone");
                if (iosSettings.format != TextureImporterFormat.RGBA32) {
					importer.SetPlatformTextureSettings ("iPhone", importer.maxTextureSize, TextureImporterFormat.PVRTC_RGBA4);//PVRTC_RGBA4
				} else {
					importer.SetPlatformTextureSettings ("iPhone", importer.maxTextureSize, TextureImporterFormat.PVRTC_RGBA4);//PVRTC_RGBA4
				}
			}
			TextureImporterSettings tt = new TextureImporterSettings ();
			importer.ReadTextureSettings (tt);
			tt.spriteMeshType = SpriteMeshType.FullRect;
			tt.mipmapEnabled = false;
			importer.SetTextureSettings (tt);
		} else {
			importer.textureType = TextureImporterType.Default;
		}
		//if (assetPath.IndexOf ("PublicResources") >= 0) {
		//	string fileName = assetPath.Substring (assetPath.LastIndexOf ("/") + 1);
		//	if (fileName.IndexOf ("U_") < 0) {
		//		fileName = fileName.Substring (0, fileName.IndexOf ("."));
		//		AssetDatabase.RenameAsset (assetPath, "U_" + fileName);
		//		AssetDatabase.ImportAsset (assetPath);
		//	}
		//}
	}

}
