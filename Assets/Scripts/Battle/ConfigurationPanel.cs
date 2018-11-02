using System;
using UnityEngine;
using UnityEngine.UI;
using UtilityMethods;

public class ConfigurationPanel : MonoBehaviour {
	public Toggle bloodShowToggle;
	public GameObject ignoreWarningToggles;
	private Toggle[] toggles;
    public Slider textObjectDurationSlider;
    public Slider soundEffectVolumeSlider;
    public Slider BGMVolumeSlider;
    public Slider NPCBehaviourDurationSlider;
	
	
    public void onTextObjectDisplaySliderMoved() {
        Configuration.textObjectDuration = textObjectDurationSlider.value;
    }
    public void onSoundEffectVolumeSliderMoved() {
        Configuration.SEVolume = soundEffectVolumeSlider.value;
    }
    public void onBGMVolumeSliderMoved() {
        Configuration.BGMVolume = BGMVolumeSlider.value;
        SoundManager.Instance.ChangeBGMVolume(Configuration.BGMVolume);
    }
    public void onNPCBehaviourDurationSliderMoved() {
        Configuration.NPCBehaviourDuration = NPCBehaviourDurationSlider.value / 2;
	    Setting.FastAITurn = NPCBehaviourDurationSlider.value < 0.05f;    
    }

	public void OnEnable() {
		textObjectDurationSlider.value = Configuration.textObjectDuration;
		soundEffectVolumeSlider.value = Configuration.SEVolume;
		BGMVolumeSlider.value = Configuration.BGMVolume;
		NPCBehaviourDurationSlider.value = Configuration.NPCBehaviourDuration;
		
		bloodShowToggle.isOn = Configuration.showRealBlood;
		toggles = ignoreWarningToggles.GetComponentsInChildren<Toggle>(ignoreWarningToggles);
		for (int i = 0; i < toggles.Length; i++){
			toggles[i].isOn = Configuration.ignoreWarning[(Warning.WarningType)i];
			toggles[i].GetComponentInChildren<Text>().text = Language.Find("Config_IgnoreWarn_" + (Warning.WarningType)i);
		}
	}
    public void OnDisable(){
	    Configuration.showRealBlood = bloodShowToggle.isOn;
	    for (int i = 0; i < toggles.Length; i++)
		    Configuration.ignoreWarning[(Warning.WarningType)i] = toggles[i].isOn;
	    TileManager.Instance.SetBloodShow();
    }
}
