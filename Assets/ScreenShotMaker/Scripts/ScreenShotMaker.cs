#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_OSX //only work in the Editor

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public class ScreenShotMaker : MonoBehaviour{
    #region Variables
    //public
    public bool DontDestroy = true;

    [Tooltip("the key that will trigger a screenshot")]
    public KeyCode ScreenShotKey = KeyCode.F1;

    [Tooltip("the keys that will trigger a screenshot if pressed at the same time")]
    public KeyCode[] ScreenShotKeys = new KeyCode[2];

    [Tooltip("any key will trigger a screenshot")]
    public bool ScreenShotAnyKey = false;

    [Tooltip("a List of ScreenShotsSizes")]
    public List<ScreenShotSize> ScreenShotSizes = new List<ScreenShotSize>();

    //private
	private string _ScreenShotPath;
    private EditorWindow GameView;
    private Rect DefaultRec;
    #endregion

    #region Methods 

	// Use this for initialization
	void Start () 
	{
        if (DontDestroy)
        {
		    DontDestroyOnLoad(gameObject); //no not destory this GameObject
        }

		CreateScreenShotFolder(); //creates a folder to store the screenshots

		System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
		System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetMainGameView.Invoke(null,null);

		GameView = (EditorWindow)Res;
        DefaultRec = GameView.position;
	}

    //creates a folder to store the screenshots
	private void CreateScreenShotFolder() 
	{
        // /Users/JustinGarza/UnityProjects/ScreenShotMaker/Assets/~/ScreenShots
		_ScreenShotPath =  Application.dataPath + "/~/ScreenShots";

        print(_ScreenShotPath);

		if (!Directory.Exists(_ScreenShotPath))
		{
			Directory.CreateDirectory(_ScreenShotPath);
		}
	}


	void Update () 
	{
        
		if (Input.GetKeyDown(ScreenShotKey))
		{
            StartCoroutine("TakeScreenShots");
		}

        if (ScreenShotKeys.Length == 2)
        {
            if (
                ( Input.GetKey(ScreenShotKeys[0]) && Input.GetKeyDown(ScreenShotKeys[1]) )
                || ( Input.GetKeyDown(ScreenShotKeys[0]) && Input.GetKey(ScreenShotKeys[1]) )
                || ( Input.GetKeyDown(ScreenShotKeys[0]) && Input.GetKeyDown(ScreenShotKeys[1]) )
                )
            {
                StartCoroutine("TakeScreenShots");
            }
        }

        if (Input.anyKeyDown && ScreenShotAnyKey)
        {
            StartCoroutine("TakeScreenShots");
        }
			
	}
    
    //take screenshots
    private int i = 0;
    private int stangeInt = 0;

    IEnumerator TakeScreenShots(){
	    string TimeTag = System.DateTime.Now.ToString().Replace("/","").Replace(" ","").Replace(":",""); //get DatetimeTag
	    string NewFileName = _ScreenShotPath + "/" + ScreenShotSizes[i].Name + "_" + TimeTag + ".png";
	    ScreenCapture.CaptureScreenshot(NewFileName); //Save the Image
	    return null;
    }

    #endregion

}


#region ScreenShotSize

[System.Serializable]
public class ScreenShotSize
{
    public bool Enable = true;
	public string Name;
	public Vector2 Size;

}

#endregion

#endif
