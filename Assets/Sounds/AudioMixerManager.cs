using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class AudioMixerManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource normalSource;
    [SerializeField]
    private AudioSource enemySpottedSource;
    [SerializeField]
    private float fadeTime = 0.5f;
    [SerializeField]
    private float fadeOutValue = 0.0001f;

    private bool spotting;
    private bool normalSet = true, enemySet = true;


    private float ChangeSpeed => Stats.Music / fadeTime;
    
    public void Seen() {
        normalSet = enemySet = false;
        spotting = true;
    }

    public void Unseen() {
        normalSet = enemySet = false;
        spotting = false;
    }

	private void Start() {
        normalSource.volume = Stats.Music;
        normalSource.Play();
        enemySpottedSource.volume = fadeOutValue;
	}

	private void Update() {
        if(normalSet && enemySet) {
            return;
        }
        if(spotting) {
            FadeInOut(enemySpottedSource, normalSource, ref enemySet, ref normalSet);
        } else {
            FadeInOut(normalSource, enemySpottedSource, ref normalSet, ref enemySet);
        }
	}

    private void FadeInOut(AudioSource fadeIn, AudioSource fadeOut, ref bool inSet, ref bool outSet) {
        fadeOut.volume -= ChangeSpeed * Time.deltaTime;
        if(fadeOut.volume < fadeOutValue){
            outSet = true;
            fadeOut.volume = fadeOutValue;
        }
        fadeIn.volume += ChangeSpeed * Time.deltaTime;
        if(fadeIn.volume > Stats.Music){
            inSet = true;
            fadeIn.volume = Stats.Music;
        }
    }
}
