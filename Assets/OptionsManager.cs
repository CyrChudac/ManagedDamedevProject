using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
	[SerializeField]
	private Slider _musicSlider;
	[SerializeField]
	private Slider _sfxSlider;
	[SerializeField]
	private Slider _difChangeSlider;
	[SerializeField]
	private TMPro.TMP_InputField _difficulityField;
	[SerializeField]
	private Slider _fireQuality;
	[SerializeField]
	private Toggle _randomLights;
	[SerializeField]
	private TMPro.TMP_InputField _maxMapX;
	[SerializeField]
	private TMPro.TMP_InputField _maxMapY;
	[SerializeField]
	private AudioSource audioSource;
	public void MusicChanged() {
		Stats.Music = _musicSlider.value;
		audioSource.volume = Stats.Music;

	}
	public void SfxChanged() {
		Stats.Sfx = _sfxSlider.value;
	}
	public void DifficulityChangeChanged() {
		Stats.DifficulityChange = _difChangeSlider.value;
	}
	public void DifficulityChanged() {
		if(float.TryParse(_difficulityField.text, out var val))
			Stats.Difficulity = Mathf.Max(0, val);
	}
	public void FireQualityChanged() {
		Stats.FireQuality = (int) _fireQuality.value;
	}
	public void RandomLightsChanged() {
		Stats.RandomizedLightColors = _randomLights.isOn;
	}
	public void MaxMapXChanged() {
		if(int.TryParse(_maxMapX.text, out var val))
			Stats.MapMaxSize = new Vector2Int(val, Stats.MapMaxSize.y);
		_maxMapX.text = Stats.MapMaxSize.x.ToString();
	}
	public void MaxMapYChanged() {
		if(int.TryParse(_maxMapY.text, out var val))
			Stats.MapMaxSize = new Vector2Int(Stats.MapMaxSize.x, val);
		_maxMapY.text = Stats.MapMaxSize.y.ToString();
	}

	public void RestoreDefaults() {
		Stats.FromInitial();
		Start();
	}

	private void Start() {
		_musicSlider.value = Stats.Music;
		_sfxSlider.value = Stats.Sfx;
		_difChangeSlider.value = Stats.DifficulityChange;
		_difficulityField.text = Stats.Difficulity.ToString("F2");
		_fireQuality.value = Stats.FireQuality;
		_randomLights.isOn = Stats.RandomizedLightColors;
		_maxMapX.text = Stats.MapMaxSize.x.ToString();
		_maxMapY.text = Stats.MapMaxSize.y.ToString();
	}
}
