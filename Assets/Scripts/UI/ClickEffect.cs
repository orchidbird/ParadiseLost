using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ClickEffect : MonoBehaviour {
    public ParticleSystem effect;
    bool clicking = false;
    float clickTime = 0;
    const float maxTime = 1;
    void Start() {
        ParticleSystem.MainModule main = effect.main;
        main.startLifetime = Setting.clickEffectDuration;
        main.duration = Setting.clickEffectDuration;
    }
    void Update() {
        if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
            clickTime = 0;
            clicking = true;
            Reposition(Input.mousePosition);
            effect.Clear();
            effect.Play();
        }
        if (clicking) {
            float t1 = Setting.clickEffectDuration;
            float t2 = Setting.longClickDurationThreshold;
            clickTime += Time.deltaTime;
            if (effect.time > t1 * 0.9f) {
                effect.time = t1 * 0.9f;
                effect.Pause();
            }
            Reposition(Input.mousePosition);
            if(clickTime > t1 * 0.9f) {
                float GB = (t2 - clickTime) / (2 * (t2 - t1 * 0.9f));
                Utility.ChangeParticleColor(effect, new Color(1, GB, GB, 1));
            }
        }
        if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) {
            clicking = false;
            effect.Play();
        }
    }
    void Reposition(Vector3 mousePosition) {
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 position = transform.position;
        position.z = Camera.main.transform.position.z + 0.5f;
        transform.position = position;
    }
}
