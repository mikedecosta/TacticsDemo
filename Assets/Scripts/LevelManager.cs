using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {
	static LevelManager instance;
	
	public void LoadLevel(string name) {
		Debug.Log ("Request to load level: "+name);
		Application.LoadLevel(name);
	}

	public void QuitRequest() {
		Debug.Log ("Request to quit");
		Application.Quit();
	}
	
	public void LoadNextLevel() {
		Application.LoadLevel(Application.loadedLevel + 1);
	}
}
