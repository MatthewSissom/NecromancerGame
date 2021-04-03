using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuMusicSliders : State
{
	// Note: Code repurposed from Escape From Demonreach GDD2 project
	// Original creator - Will Bertiz

	public Slider masterSlider;
	public Slider musicSlider;
	public Slider sfxSlider;

	public GameObject canvas;
	private bool exit;

	// Reference: https://docs.unity3d.com/540/Documentation/ScriptReference/UI.Slider-onValueChanged.html
	public void Start()
	{
		//Toggle canvas when switching between states

		MenuManager.Instance.AddEventMethod(typeof(MenuMusicSliders), "begin", () => { canvas.SetActive(true); });

		MenuManager.Instance.AddEventMethod(typeof(MenuInstructions), "begin", () => { canvas.SetActive(false); });

		MenuManager.Instance.AddEventMethod(typeof(MenuMain), "begin", () => { canvas.SetActive(false); });

		MenuManager.Instance.AddEventMethod(typeof(MenuShowAssignments), "begin", () => { canvas.SetActive(false); });

		GameManager.Instance.AddEventMethod(typeof(GameCleanUp), "end", () => { canvas.SetActive(false); });

		//Adds a listener to the main slider and invokes a method when the value changes.
		masterSlider.onValueChanged.AddListener(delegate { MasterSlider(); });
		musicSlider.onValueChanged.AddListener(delegate { MusicSlider(); });
		sfxSlider.onValueChanged.AddListener(delegate { SFXSlider(); });

		FMODUnity.RuntimeManager.GetBus("bus:/").getVolume(out float volumeMaster);
		FMODUnity.RuntimeManager.GetBus("bus:/Music").getVolume(out float volumeMusic);
		FMODUnity.RuntimeManager.GetBus("bus:/SFX").getVolume(out float volumeSFX);

		masterSlider.value = volumeMaster;
		musicSlider.value = volumeMusic;
		sfxSlider.value = volumeSFX;
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
	public void ButtonPressed()
	{
		MenuManager.Instance.GoToMenu("Main");
		exit = true;
	}

	// Invoked when the value of the slider changes.
	public void MasterSlider()
	{
		FMOD.Studio.Bus masterBus;

		masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
		masterBus.setVolume(masterSlider.value);
	}

	public void MusicSlider()
	{
		FMOD.Studio.Bus musicBus;

		musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
		musicBus.setVolume(musicSlider.value);
	}

	public void SFXSlider()
	{
		FMOD.Studio.Bus sfxBus;

		sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
		sfxBus.setVolume(sfxSlider.value);
	}
}