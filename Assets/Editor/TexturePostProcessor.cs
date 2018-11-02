using UnityEngine;
using UnityEditor;
 
public class TexturePostProcessor : AssetPostprocessor
{
	void OnPostprocessTexture(Texture2D texture)
	{
		TextureImporter importer = assetImporter as TextureImporter;
		importer.anisoLevel = 1;
		importer.filterMode = FilterMode.Bilinear;
		importer.textureFormat = TextureImporterFormat.DXT5;
 
		Object asset = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Texture2D));
		if (asset)
		{
			EditorUtility.SetDirty(asset);
		}
		else
		{
			importer.anisoLevel = 1;
			importer.filterMode = FilterMode.Bilinear;
			importer.textureFormat = TextureImporterFormat.DXT5;
		} 
	}
}
