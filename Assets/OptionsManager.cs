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
	public void MusicChanged() {
		Stats.Music = _musicSlider.value;
	}
	public void SfxChanged() {
		Stats.Sfx = _sfxSlider.value;
	}
	public void DifficulityChangeChanged() {
		Stats.DifficulityChange = _difChangeSlider.value;
	}
	public void DifficulityChanged() {
		Stats.Difficulity = float.Parse(_difficulityField.text);
	}
	public void RestoreDefaults() {
		Stats.FromInitial();
	}

	private void Start() {
		_musicSlider.value = Stats.Music;
		_sfxSlider.value = Stats.Sfx;
		_difChangeSlider.value = Stats.DifficulityChange;
		_difficulityField.text = Stats.Difficulity.ToString("F2");
	}
}
