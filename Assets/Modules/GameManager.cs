using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	#region Auxiliary fields
	const int maxLogLines = 5;
	List<string> logLines = new List<string>();
	#endregion

	#region Inspector fields
	public Text logText;
	#endregion

	#region Public interfaces
	public void PrintLog(string text) {
		logLines.Add(text);
		if(logLines.Count > maxLogLines)
			logLines.RemoveAt(0);
		logText.text = string.Join("\n", logLines);
	}
	#endregion

	#region Life cycle
	void Start() {
	}
	#endregion
}
