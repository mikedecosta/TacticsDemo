using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class GameBoard : MonoBehaviour {
	private GameObject[][] squares;

	void Start() {
		GameObject[] flatSquares = GameObject.FindGameObjectsWithTag("Square");
		GameObject parent;
		foreach(GameObject square in flatSquares) {
			parent = square.transform.parent.gameObject;
			Match matches = Regex.Match(parent.name, "Row [(]([0-9]+)[)]");
			string row = matches.Groups[1].Value;
			matches = Regex.Match(square.name, "Square [(]([0-9]+)[)]");
			string column = matches.Groups[1].Value;
			Debug.Log (row+","+column+": => ("+square.transform.position.x+","+square.transform.position.z+")");
		}
	}
			
}
