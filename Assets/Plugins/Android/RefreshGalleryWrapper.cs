using UnityEngine;
using System.Collections;

public class RefreshGalleryWrapper : MonoBehaviour {

	#if UNITY_ANDROID

	void Start()
	{
		SetGalleryPath ();
	}

	void SetGalleryPath()
	{
		string captureAndSaveGameObjectName = "CaptureAndSave";
		CaptureAndSave captureAndSave = GameObject.FindObjectOfType<CaptureAndSave>();
		if (captureAndSave != null) {
			captureAndSaveGameObjectName =	captureAndSave.gameObject.name;
		}
		
		AndroidJavaClass javaClass = new AndroidJavaClass("com.astricstore.androidutil.AndroidGallery");
		javaClass.CallStatic("SetGalleryPath",captureAndSaveGameObjectName);
	}

	void RefreshGallery(string path)
	{
		AndroidJavaClass javaClass = new AndroidJavaClass("com.astricstore.androidutil.AndroidGallery");
		javaClass.CallStatic("RefreshGallery",path);
	}
	#endif
}
