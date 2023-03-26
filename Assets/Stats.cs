using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Stats : MonoBehaviour
{
    private static float _sfx;
    public static float Sfx {
        get => _sfx;
        set {
            _sfx = value;
            Save();
        }
    }
    private static float _music;
    public static float Music {
        get => _music;
        set {
            _music = value;
            Save();
        }
    }
    private static float _difficulity;
    public static float Difficulity{
        get => _difficulity;
        set {
            _difficulity = value;
            Save();
        }
    }
    private static float _difficulityChange;
    public static float DifficulityChange{
        get => _difficulityChange;
        set {
            _difficulityChange = value;
            Save();
        }
    }
    
    private static float staSfxInitial;
    private static float staMusicInitial;
    private static float staDifficulityInitial;
    private static float staDifficulityChangeInitial;

    [SerializeField] private float SfxInitial;
    [SerializeField] private float MusicInitial;
    [SerializeField] private float DifficulityInitial;
    [SerializeField] private float DifficulityChangeInitial;

    private static string FilePath => Path.Combine(Application.persistentDataPath, "save.gtx");
    // Start is called before the first frame update
    void Start()
    {
        staSfxInitial = SfxInitial;
        staMusicInitial = MusicInitial;
        staDifficulityChangeInitial= DifficulityChangeInitial;
        staDifficulityInitial = DifficulityInitial;

        if(File.Exists(FilePath)) {
            using var sr = new StreamReader(FilePath);
            try {
                Seri s = (Seri)JsonUtility.FromJson(sr.ReadToEnd(), typeof(Seri));
                Sfx = s.Sfx;
                Music = s.Music;
                Difficulity = s.Difficulity;
                DifficulityChange = s.DifficulityChange;
            } catch(Exception) {
                FromInitial();
            }
        } else {
            FromInitial();
        }
    }

    public static void FromInitial() {
        _sfx = staSfxInitial;
        Music = staMusicInitial;
        Difficulity = staDifficulityInitial;
        DifficulityChange = staDifficulityChangeInitial;
    }

    private static void Save() {
        var str = JsonUtility.ToJson(new Seri() {
            DifficulityChange = DifficulityChange,
            Difficulity = Difficulity,
            Music = Music,
            Sfx = Sfx
        }, true);
        File.WriteAllText(FilePath, str);
    }

    [Serializable]
    private class Seri {
        public float Sfx { get; set; }
        public float Music { get; set; }
        public float Difficulity { get; set; }
        public float DifficulityChange { get; set; }
    }
}
