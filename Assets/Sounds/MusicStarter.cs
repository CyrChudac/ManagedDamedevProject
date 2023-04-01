using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[ExecuteAfter(typeof(Stats))]
public class MusicStarter : MonoBehaviour
{
    [SerializeField]
    private bool randomizeTime = true;
    // Start is called before the first frame update
    void Start()
    {
        var audio = GetComponent<AudioSource>();
        audio.volume = Stats.Music;
        if(randomizeTime) {
            audio.time = audio.clip.length * Random.value;
        }
        audio.Play();
    }
}
