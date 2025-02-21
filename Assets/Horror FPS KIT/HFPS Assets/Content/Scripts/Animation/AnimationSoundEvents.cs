using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class events {
	public string eventName;
	public AudioClip eventSound;
    public SoundMode playMode = SoundMode.Default;
    [HideInInspector]
    public bool isPlayed = false;
}

public enum SoundMode { Default, Once }

public class AnimationSoundEvents : MonoBehaviour {

	public float soundVolume = 0.75f;
	public List<events> SoundEvents = new List<events> ();

	public void EventPlaySound (string SoundEvent) {
		for (int i = 0; i < SoundEvents.Count; i++) {
			if (SoundEvents [i].eventName == SoundEvent) {
				if(SoundEvents[i].eventSound)
                {
                    if (SoundEvents[i].playMode == SoundMode.Once && !SoundEvents[i].isPlayed)
                    {
                        AudioSource.PlayClipAtPoint(SoundEvents[i].eventSound, transform.position, soundVolume);
                        SoundEvents[i].isPlayed = true;
                    }

                    if(SoundEvents[i].playMode == SoundMode.Default)
                    {
                        AudioSource.PlayClipAtPoint(SoundEvents[i].eventSound, transform.position, soundVolume);
                    }
				}
			}
		}
	}
}
