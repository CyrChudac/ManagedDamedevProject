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
    private static bool _randomizedLightColors;
    public static bool RandomizedLightColors {
        get => _randomizedLightColors;
        set {
            _randomizedLightColors = value;
            Save();
        }
    }
    private static Vector2Int _mapMaxSize;
    public static Vector2Int MapMaxSize {
        get => _mapMaxSize;
        set {
            _mapMaxSize = value;
            Save();
        }
    }
    private static int _fireQuality;
    public static int FireQuality {
        get => _fireQuality;
        set {
            _fireQuality = value;
            Save();
        }
    }
    
    private static float staSfxInitial;
    private static float staMusicInitial;
    private static float staDifficulityInitial;
    private static float staDifficulityChangeInitial;
    private static bool randomizedLightColorsInitial;
    private static Vector2Int mapMaxSizeInitial;
    private static int fireQualityInitial;

    [SerializeField] private float SfxInitial;
    [SerializeField] private float MusicInitial;
    [SerializeField] private float DifficulityInitial;
    [SerializeField] private float DifficulityChangeInitial;
    [SerializeField] private bool RandomizedLightColorsInitial;
    [SerializeField] private Vector2Int MapMaxSizeInitial;
    [SerializeField] private int FireQualityInitial;

    private static string FilePath => Path.Combine(Application.persistentDataPath, "save.gtx");
    // Start is called before the first frame update
    void Start()
    {
        staSfxInitial = SfxInitial;
        staMusicInitial = MusicInitial;
        staDifficulityChangeInitial= DifficulityChangeInitial;
        staDifficulityInitial = DifficulityInitial;
        randomizedLightColorsInitial = RandomizedLightColorsInitial;
        mapMaxSizeInitial = MapMaxSizeInitial;
        fireQualityInitial = FireQualityInitial;

        if(File.Exists(FilePath)) {
            try {
                using var sr = new StreamReader(FilePath);
                Seri s = (Seri)JsonUtility.FromJson(sr.ReadToEnd(), typeof(Seri));
                _sfx = s.Sfx;
                _music = s.Music;
                _difficulity = s.Difficulity;
                _difficulityChange = s.DifficulityChange;
                _randomizedLightColors = s.RandomizedColors;
            } catch(Exception) {
                FromInitial();
            }
        } else {
            FromInitial();
        }
    }

    public static void FromInitial() {
        _sfx = staSfxInitial;
        _music = staMusicInitial;
        _difficulity = staDifficulityInitial;
        _difficulityChange = staDifficulityChangeInitial;
        _randomizedLightColors = randomizedLightColorsInitial;
        _mapMaxSize = mapMaxSizeInitial;
        FireQuality = fireQualityInitial;//this saves
    }

    private static void Save() {
        var str = JsonUtility.ToJson(new Seri() {
            DifficulityChange = DifficulityChange,
            Difficulity = Difficulity,
            Music = Music,
            Sfx = Sfx,
            RandomizedColors = RandomizedLightColors,
            MapMaxSize = MapMaxSize,
            FireQuality = FireQuality
        }, true);
        lock(_lock) {
            File.WriteAllText(FilePath, str);
        }
    }

    private static object _lock = new object();

    [Serializable]
    private class Seri {
        public float Sfx;
        public float Music;
        public float Difficulity;
        public float DifficulityChange;
        public bool RandomizedColors;
        public Vector2Int MapMaxSize;
        public int FireQuality;
    }
}
