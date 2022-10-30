using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {
	public static GameManager instance;
	public GameManager() => instance = this;

	#region Core fields
	public int gameOverThreshold;
	public string startUpMessage, overMessage;
	public Button gameOverButton;

	[SerializeField] Button pilgrimage;

	[Serializable]
	public struct Grade {
		public float upgradingPrice;
		public float adherentAttractiveness;
		public float fundGainPerPilgrimage;
		public float pilgrimageFrequency;
	}
	[SerializeField] List<Temple> temples;
	public Button upgradeTempleButton;
	public Text templeInfoText;
	public List<Grade> templeGrades;
	public float upgradingRatioChangePerClick;

	float adherents;
	int pilgrimageCount;
	float fund;
	float upgradingRatio = 1;
	#endregion

	#region Auxiliary fields
	#region Log
	const int maxLogLines = 6;
	List<string> logLines = new List<string>();
	public Text logText;
	#endregion Log
	#region Status
	[Serializable]
	public struct Status {
		[SerializeField] RectTransform entries;
		[SerializeField] GameObject entryPrefab;

		public Text Find(string name) {
			return entries.GetComponentsInChildren<Text>()
				.Where(text => text.gameObject.name == "Name" && text.text == name)
				.FirstOrDefault()?.transform.parent.Find("Value").GetComponent<Text>();
		}
		public void Set(string name, object value) {
			var found = Find(name);
			if(!found) {
				var entry = Instantiate(entryPrefab, entries);
				entry.transform.Find("Name").GetComponent<Text>().text = name;
				found = entry.transform.Find("Value").GetComponent<Text>();
			}
			found.text = value.ToString();
		}
		public string Get(string name) => Find(name)?.text;
		public T GetAs<T>(string name) where T : class => Get(name) as T;
	}
	public Status status;
	#endregion
	Temple selectedTemple;
	bool gameOver = false;
	#endregion

	#region Properties
	public float Adherents {
		get => adherents;
		set => status.Set("朝圣者", $"{adherents = value} 人");
	}
	public int PilgrimageCount {
		get => pilgrimageCount;
		set => status.Set("总朝圣次数", pilgrimageCount = value);
	}
	public float Fund {
		get => fund;
		set {
			status.Set("资金", fund = value);
		}
	}
	public Temple SelectedTemple {
		get => selectedTemple;
		set {
			if(selectedTemple)
				selectedTemple.Selected = false;
			if(selectedTemple = value)
				selectedTemple.Selected = true;
			UpdateTempleInfo();
		}
	}
	#endregion

	#region Private methods
	float UpgradeCostOf(Temple temple) => templeGrades[temple.Level].upgradingPrice * upgradingRatio;

	void UpdateStatus() {
		float adherentAttractiveness = temples.Select(temple => temple.adherentAttractiveness).Aggregate(0f, (a, b) => a + b);
		status.Set("朝圣指数", $"{adherentAttractiveness} 人/秒");
		float fundGainPerPilgrimage = temples.Select(temple => temple.fundGainPerPilgrimage).Aggregate(0f, (a, b) => a + b);
		status.Set("教税", $"{fundGainPerPilgrimage} /次朝圣");
	}

	void UpdateTempleInfo() {
		if(selectedTemple) {
			bool upgradable = selectedTemple.Level < templeGrades.Count;
			var lines = new List<string> {
				$"当前等级：{selectedTemple.Level}",
			};
			if(upgradable)
				lines.Add($"升级花费：{UpgradeCostOf(selectedTemple)}");
			lines.AddRange(new string[] {
				$"朝圣指数：+{selectedTemple.adherentAttractiveness} 人/秒",
				$"教税：{selectedTemple.fundGainPerPilgrimage} /次朝圣",
			});
			templeInfoText.text = string.Join("\n", lines);
			upgradeTempleButton.interactable = upgradable;
		}
		else {
			templeInfoText.text = "";
			upgradeTempleButton.interactable = false;
		}
	}
	#endregion

	#region Public interfaces
	public void PrintLog(string text) {
		logLines.Add(text);
		if(logLines.Count > maxLogLines)
			logLines.RemoveAt(0);
		logText.text = string.Join("\n", logLines);
	}
	public void PrintImportantLog(string text) => PrintLog($"<color=red>{text}</color>");

	public void Pilgrimage(float fund) {
		++PilgrimageCount;
		Fund += fund;
	}

	public void ManualPilgrimage() => Pilgrimage(1);

	public void UpdateTempleCount() {
		int count = temples.Count(temple => temple.Level > 0);
		if(count > 0)
			pilgrimage.gameObject.SetActive(true);
		status.Set("神庙数", count);
	}

	public void UpgradeTemple() {
		if(!selectedTemple)
			return;
		if(selectedTemple.Level >= templeGrades.Count)
			return;
		float price = UpgradeCostOf(selectedTemple);
		if(price > Fund) {
			PrintLog($"升级寺庙需要花费 {price}，当前资金不足。");
			return;
		}

		Grade grade = templeGrades[selectedTemple.Level];
		selectedTemple.pilgrimageFrequency = grade.pilgrimageFrequency;
		selectedTemple.adherentAttractiveness = grade.adherentAttractiveness;
		selectedTemple.fundGainPerPilgrimage = grade.fundGainPerPilgrimage;

		++selectedTemple.Level;
		Fund -= price;
		PrintLog($"花费 {price} 将寺庙升级到了 {selectedTemple.Level} 级。");
		upgradingRatio *= upgradingRatioChangePerClick;
		selectedTemple.Selected = true;

		UpdateTempleCount();
		UpdateTempleInfo();
		UpdateStatus();
	}

	public void GameOver() {
		PrintImportantLog(overMessage);
		foreach(var temple in temples)
			temple.Stop();
		gameOverButton.gameObject.SetActive(true);
	}

	public void Quit() {
		Application.Quit();
	}
	#endregion

	#region Life cycle
	void Start() {
		Adherents = 0;
		PilgrimageCount = 0;
		Fund = 10;
		SelectedTemple = null;
		pilgrimage.gameObject.SetActive(false);
		UpdateTempleCount();
		PrintImportantLog(startUpMessage);
		gameOverButton.gameObject.SetActive(false);
	}

	void FixedUpdate() {
		if(!gameOver && Adherents > gameOverThreshold) {
			GameOver();
			gameOver = true;
		}
	}
	#endregion
}
