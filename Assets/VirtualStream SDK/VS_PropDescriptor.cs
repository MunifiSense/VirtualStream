using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VS_PropDescriptor : MonoBehaviour
{
	public enum ObjectType
	{
		Head,
		Neck,
		LeftHand,
		RightHand,
		LeftShoulder,
		RightShoulder,
		Hip,		
		Other
	}
	[Tooltip("Where does this object go?")]
	public ObjectType type;
	//[Tooltip("Scale of the prop.")]
	//public Vector3 scale = Vector3.one;

#if UNITY_EDITOR
	[ContextMenu("Create VirtualStream Bundle")]
	public void BuildBundle()
	{
		string path = "Assets/VirtualStream/Props";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, path + "/" + gameObject.name + ".prefab");
		string assetPath = AssetDatabase.GetAssetPath(prefab);
		AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
		assetImporter.SetAssetBundleNameAndVariant(gameObject.name, "prop.vstream");
		/*AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
		buildMap[0].assetBundleName = gameObject.name + ".vstream";
		buildMap[0].assetNames = new string[] { path };*/

		AssetBundleManifest bundle = BuildPipeline.BuildAssetBundles(path,
									/*buildMap,*/
									BuildAssetBundleOptions.None,
									BuildTarget.StandaloneWindows64);
		if (bundle != null)
		{
			// Success
			Debug.Log(gameObject.name + ".prop.vstream succesfully created!");
			if (File.Exists(assetPath))
			{
				FileUtil.DeleteFileOrDirectory(assetPath);
				FileUtil.DeleteFileOrDirectory(path + "/Props");
				FileUtil.DeleteFileOrDirectory(path + "/" + gameObject.name + ".prop.vstream.manifest");
				FileUtil.DeleteFileOrDirectory(path + "/" + "Props.manifest");
			}
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Success!", gameObject.name + ".prop.vstream succesfully created!", "Okay");
		}
		else
		{
			// Failure
			Debug.Log("Vstream prop creation failed.");
			if (File.Exists(assetPath))
			{
				FileUtil.DeleteFileOrDirectory(assetPath);
				FileUtil.DeleteFileOrDirectory(path + "/Props");
				FileUtil.DeleteFileOrDirectory(path + "/" + gameObject.name + ".prop.vstream.manifest");
				FileUtil.DeleteFileOrDirectory(path + "/" + "Props.manifest");
			}
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Error!", "VStream prop creation failed.", "Okay");
		}
	}
#endif
}