using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuOptions : State
{
	// Note: Code repurposed from Escape From Demonreach GDD2 project
	// Original creator - Will Bertiz

	[SerializeField]
	private Slider masterSlider;
	[SerializeField]
	private Slider musicSlider;
	[SerializeField]
	private Slider sfxSlider;
	[SerializeField]
	private Slider handedness;

	private FMOD.Studio.EventInstance testSound;

	public GameObject canvas;
	private bool exit;

	bool sfxDisabled;

	// Reference: https://docs.unity3d.com/540/Documentation/ScriptReference/UI.Slider-onValueChanged.html
	public void Start()
	{
		//Toggle canvas when switching between states
		canvas.SetActive(false);
		MenuManager.Instance.AddEventMethod(typeof(MenuOptions), "begin", () => { canvas.SetActive(true); });
		MenuManager.Instance.AddEventMethod(typeof(MenuInstructions), "begin", () => { canvas.SetActive(false); });
		MenuManager.Instance.AddEventMethod(typeof(MenuMain), "begin", () => { canvas.SetActive(false); });
		GameManager.Instance.AddEventMethod(typeof(GameCleanUp), "end", () => { canvas.SetActive(false); });

		InitVolumeOptions();

		// Init handedness slider, default to right hand
		handedness.value = PlayerPrefs.GetInt("handedness", 1);
		handedness.onValueChanged.AddListener((float val) => PlayerPrefs.SetInt("handedness", (int)val) );
	}

	public override IEnumerator Routine()
	{
		Begin();

		exit = false;
		while (!exit)
		{
			yield return null;
		}

		End();
		yield break;
	}

	// Method for button presses
	public void ReturnToMainMenu()
	{
		PlayerPrefs.Save();
		MenuManager.Instance.GoToMenu("Main");
		exit = true;
	}

	// Invoked when the value of the slider changes.
	private void MasterSliderChanged(float newValue)
	{
		FMOD.Studio.Bus masterBus;
		masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
		masterBus.setVolume(newValue);

		PlayerPrefs.SetFloat("volumeMaster", newValue);
	}

	private void MusicSliderChanged(float newValue)
	{
		FMOD.Studio.Bus musicBus;
		musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
		musicBus.setVolume(newValue);

		PlayerPrefs.SetFloat("volumeMusic", newValue);
	}

	private void SFXSliderChanged(float newValue)
	{
		FMOD.Studio.Bus sfxBus;
		sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
		sfxBus.setVolume(newValue);

		PlayerPrefs.SetFloat("volumeSFX", newValue);

		PlayTestSFXSound();
	}

	private void PlayTestSFXSound()
	{
		if (sfxDisabled)
			return;

		IEnumerator Timer()
        {
			sfxDisabled = true;
			yield return new WaitForSeconds(0.5f);
			sfxDisabled = false;
		}
		StartCoroutine(Timer());

		FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Cats/Vocals/General/Meows");
	}

	private void InitVolumeOptions()
	{
		// Get default volume values from FMOD
		FMODUnity.RuntimeManager.GetBus("bus:/").getVolume(out float volumeMaster);
		FMODUnity.RuntimeManager.GetBus("bus:/Music").getVolume(out float volumeMusic);
		FMODUnity.RuntimeManager.GetBus("bus:/SFX").getVolume(out float volumeSFX);

		// Load saved volume vals if possible
		volumeMaster = PlayerPrefs.GetFloat("volumeMaster", volumeMaster);
		volumeMusic = PlayerPrefs.GetFloat("volumeMusic", volumeMusic);
		volumeSFX = PlayerPrefs.GetFloat("volumeSFX", volumeSFX);

		// Set sliders to actual values
		masterSlider.value = volumeMaster;
		musicSlider.value = volumeMusic;
		sfxSlider.value = volumeSFX;

		//Adds a listener to the main slider and invokes a method when the value changes.
		masterSlider.onValueChanged.AddListener(MasterSliderChanged);
		musicSlider.onValueChanged.AddListener(MusicSliderChanged);
		sfxSlider.onValueChanged.AddListener(SFXSliderChanged);
	}
}