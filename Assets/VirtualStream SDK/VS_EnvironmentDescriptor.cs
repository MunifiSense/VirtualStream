using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VS_EnvironmentDescriptor : MonoBehaviour
{
	[Tooltip("Where the avatar should be in the environment. make sure the Z-axis (blue arrow) is facing the camera")]
	public GameObject avatarLocation;
	[Tooltip("A camera pointing at where the avatar should be.")]
	public Camera camera;

#if UNITY_EDITOR
	[ContextMenu("Create VirtualStream Bundle")]
	public void BuildBundle()
	{
		if (!ValidateEnvironment())
		{
			return;
		}

		string path = "Assets/VirtualStream/Environments";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, path + "/" + gameObject.name + ".prefab");
		string assetPath = AssetDatabase.GetAssetPath(prefab);
		AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
		assetImporter.SetAssetBundleNameAndVariant(gameObject.name, "environment.vstream");
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
			Debug.Log(gameObject.name + ".environment.vstream succesfully created!");
			if (File.Exists(assetPath))
			{
				FileUtil.DeleteFileOrDirectory(assetPath);
				FileUtil.DeleteFileOrDirectory(path + "/Environments");
				FileUtil.DeleteFileOrDirectory(path + "/" + gameObject.name + ".environment.vstream.manifest");
				FileUtil.DeleteFileOrDirectory(path + "/" + "Environments.manifest");
			}
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Success!", gameObject.name + ".environment.vstream succesfully created!", "Okay");
		}
		else
		{
			// Failure
			Debug.Log("Vstream environment creation failed.");
			if (File.Exists(assetPath))
			{
				FileUtil.DeleteFileOrDirectory(assetPath);
				FileUtil.DeleteFileOrDirectory(path + "/Environments");
				FileUtil.DeleteFileOrDirectory(path + "/" + gameObject.name + ".environment.vstream.manifest");
				FileUtil.DeleteFileOrDirectory(path + "/" + "Environments.manifest");
			}
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Error!", "VStream environment creation failed.", "Okay");
		}
	}
	public bool ValidateEnvironment()
	{
		if(avatarLocation == null)
		{
			EditorUtility.DisplayDialog("Error!", "There is no avatar location specified!", "Okay");
			Debug.Log("There is no avatar location!");
			return false;
		}
		if (camera == null)
		{
			EditorUtility.DisplayDialog("Error!", "There is no camera object specified!" +
				" Add a game object with a Camera component, position it to face where the avatar would be, and make sure it is a child" +
				" of the object with the descriptor", "Okay");
			Debug.Log("There is no camera specified!");
			return false;
		}
		else if (!camera.transform.IsChildOf(gameObject.transform))
		{
			Debug.Log("Camera is not child of the object with the descriptor!");
			EditorUtility.DisplayDialog("Error!", "Camera object is not a child of the game object with the descriptor! " +
				"Make sure the camera object is under " + gameObject.name + " in the hierachy.", "Okay");
			return false;
		}

		return true;
	}

	public void Reset()
	{
		if(avatarLocation == null)
		{
			avatarLocation = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			avatarLocation.name = "Avatar";
			avatarLocation.transform.parent = gameObject.transform;
		}
	}
#endif
}