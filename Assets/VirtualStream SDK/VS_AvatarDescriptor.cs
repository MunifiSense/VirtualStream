using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Animator))]
public class VS_AvatarDescriptor : MonoBehaviour
{
	[Serializable]
	public class Emote
	{
		public AnimationClip AnimationClip;
		public bool PreventBlinking;
	}

	//public Vector3 ViewPosition;

	[Header("Visemes")]
	public SkinnedMeshRenderer FaceMesh;
	public string sil = "vrc.v_sil";
	public string PP = "vrc.v_pp";
	public string FF = "vrc.v_ff";
	public string TH = "vrc.v_th";
	public string DD = "vrc.v_dd";
	public string kk = "vrc.v_kk";
	public string CH = "vrc.v_ch";
	public string SS = "vrc.v_ss";
	public string nn = "vrc.v_nn";
	public string RR = "vrc.v_rr";
	public string aa = "vrc.v_aa";
	public string E = "vrc.v_e";
	public string ih = "vrc.v_ih";
	public string oh = "vrc.v_oh";
	public string ou = "vrc.v_ou";

	[Header("Eyes")]
	public Transform LeftEye;
	public Transform RightEye;
	public string Blink = "Blink";

	[Header("Emotes")]
	public Emote[] Emotes;

#if UNITY_EDITOR
	[ContextMenu("Create VirtualStream Bundle")]
	public void BuildBundle()
	{
		if (!VerifyAvatar())
		{
			EditorUtility.DisplayDialog("Error", "A problem occured! Check the Unity log!", "Okay");
		}

		string path = "Assets/VirtualStream/Avatars";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, path + "/" + gameObject.name + ".prefab");
		string assetPath = AssetDatabase.GetAssetPath(prefab);
		AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
		assetImporter.SetAssetBundleNameAndVariant(gameObject.name, "avatar.vstream");
		/*AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
		buildMap[0].assetBundleName = gameObject.name + ".vstream";
		buildMap[0].assetNames = new string[] {path};*/
		AssetBundleManifest bundle = BuildPipeline.BuildAssetBundles(path,
									BuildAssetBundleOptions.None,
									BuildTarget.StandaloneWindows64);
		if (bundle != null)
		{
			// Success
			if (System.IO.File.Exists(assetPath))
			{
				FileUtil.DeleteFileOrDirectory(assetPath);
				FileUtil.DeleteFileOrDirectory(path + "/Avatars");
				FileUtil.DeleteFileOrDirectory(path + "/" + gameObject.name + ".avatar.vstream.manifest");
				FileUtil.DeleteFileOrDirectory(path + "/" + "Avatars.manifest");
			}
			EditorUtility.DisplayDialog("Success!", gameObject.name + ".avatar.vstream succesfully created!", "Okay");
			AssetDatabase.Refresh();
		}
		else
		{
			// Failure
			if (System.IO.File.Exists(assetPath))
			{
				FileUtil.DeleteFileOrDirectory(assetPath);
				FileUtil.DeleteFileOrDirectory(path + "/Avatars");
				FileUtil.DeleteFileOrDirectory(path + "/" + gameObject.name + ".avatar.vstream.manifest");
				FileUtil.DeleteFileOrDirectory(path + "/" + "Avatars.manifest");
			}
			EditorUtility.DisplayDialog("Error!", "VStream avatar creation failed.", "Okay");
			AssetDatabase.Refresh();
		}
	}
#endif
	public bool VerifyAvatar()
	{
		/*if(ViewPosition.x == 0 && ViewPosition.y == 0 && ViewPosition.z == 0)
		{
			Debug.Log("You probably want to set the view point. Unless you really want it at (0,0,0).");
		}*/
		// Check that face mesh is not null
		if(FaceMesh == null)
		{
			Debug.Log("Face Mesh is not specified.");
			return false;
		}

		// Check that shape keys are valid
		List<string> shapeKeys = new List<string>(FaceMesh.sharedMesh.blendShapeCount);
		for(int i = 0; i < FaceMesh.sharedMesh.blendShapeCount; i++)
		{
			shapeKeys.Add(FaceMesh.sharedMesh.GetBlendShapeName(i));
		}

		if (!shapeKeys.Contains(sil)    ||
			!shapeKeys.Contains(PP)     ||
			!shapeKeys.Contains(FF)     ||
			!shapeKeys.Contains(TH)     ||
			!shapeKeys.Contains(DD)     ||
			!shapeKeys.Contains(kk)     ||
			!shapeKeys.Contains(CH)     ||
			!shapeKeys.Contains(SS)     ||
			!shapeKeys.Contains(nn)     ||
			!shapeKeys.Contains(RR)     ||
			!shapeKeys.Contains(aa)     ||
			!shapeKeys.Contains(E)      ||
			!shapeKeys.Contains(ih)     ||
			!shapeKeys.Contains(oh)     ||
			!shapeKeys.Contains(ou))
		{
			Debug.Log("Specified shape keys for visemes don't exist on the specified mesh!");
			return false;
		}

		if(LeftEye == null)
		{
			Debug.Log("Left eye isn't specified! Eye tracking won't work properly in VStream!");
		}
		else if(RightEye == null)
		{
			Debug.Log("Right eye isn't specified! Eye tracking won't work properly in VStream!");
		}

		if (!shapeKeys.Contains(Blink))
		{
			Debug.Log("Blinking shape key does not exist on mesh! Avatar blinking won't work properly in VStream!");
		}

		return true;
	}

	/*void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawSphere(ViewPosition, 0.01f);
	}*/
}