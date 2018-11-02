using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DarkTonic.MasterAudio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour {
	private static SoundManager instance = null;
	public static SoundManager Instance { get { return instance; } }

	AudioSource audioSource;
	public PlaylistController Controller;

	static Dictionary<string, AudioClip> BGMs;
	static Dictionary<string, AudioClip> SEs;

	public void PlayBGM(string name){
		if (Controller.activeAudio.clip != null && Controller.activeAudio.clip.name == name) return;
		var musicSettings = Controller.CurrentPlaylist.MusicSettings;
		var MS = new MusicSetting();
		MS.clip = BGMs[name];
		musicSettings.Add(MS);
		Controller.PlayNextSong();
		musicSettings.RemoveAt(0);
		Controller.activeAudio.loop = true;
		//Controller.activeAudio.volume = Configuration.BGMVolume;
		//Debug.Log("음량: " + Controller.activeAudio.volume);
	}

	public void PlaySE(string name){
		//Debug.Log("효과음 재생: " + name);
		Controller.transitioningAudio.volume = 1;
		var SE = SEs.ContainsKey(name) ? SEs[name] : Resources.Load<AudioClip>("SEs/" + name);
		Controller.transitioningAudio.PlayOneShot(SE, Configuration.SEVolume);
	}

    public void ChangeBGMVolume(float BGMVolume){
	    Controller.activeAudio.volume = BGMVolume;
	    Controller.transitioningAudio.volume = BGMVolume;
    }

	void Awake () {
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}
		instance = this;
		LoadBGMsAndSEs ();
		audioSource = gameObject.GetComponent<AudioSource>();
	}

	static void LoadBGMsAndSEs(){
		AudioClip[] BGMLists = Resources.LoadAll<AudioClip> ("BGMs");
		BGMs = new Dictionary<string, AudioClip> ();
		foreach (AudioClip BGM in BGMLists) {
			BGMs [BGM.name] = BGM;
		}
		AudioClip[] SELists = Resources.LoadAll<AudioClip> ("SEs");
		SEs = new Dictionary<string, AudioClip> ();
		foreach (AudioClip SE in SELists) {
			SEs [SE.name] = SE;
		}
	}
}
