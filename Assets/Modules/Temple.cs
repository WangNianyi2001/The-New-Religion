using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

public class Temple : MonoBehaviour {
	int level = 0;
	public Color defaultColor = Color.white, builtColor, selectedColor;
	public Image buttonImage;
	public Text gradeText;

	[NonSerialized] public float adherentAttractiveness = 0;
	[NonSerialized] public float fundGainPerPilgrimage = 0;
	[NonSerialized] public float pilgrimageFrequency = 0;

	public int Level {
		get => level;
		set {
			level = value;
			gradeText.text = level.ToString();
		}
	}
	public Color Color {
		set => buttonImage.color = value;
	}
	public bool Built => level > 0;
	public bool Selected {
		set => Color = value ? selectedColor : Built ? builtColor :	defaultColor;
	}

	public void OnClick() {
		GameManager.instance.SelectedTemple = this;
	}

	IEnumerator AutoPilgrimage() {
		var game = GameManager.instance;
		for(; ; ) {
			while(pilgrimageFrequency == 0)
				yield return new WaitForFixedUpdate();
			game.Pilgrimage(fundGainPerPilgrimage);
			game.Adherents += adherentAttractiveness;
			yield return new WaitForSeconds(1 / pilgrimageFrequency);
		}
	}

	Coroutine auto;
	public void Stop() {
		StopCoroutine(auto);
	}

	void Start() {
		Selected = false;
		auto = StartCoroutine(AutoPilgrimage());
	}
}
