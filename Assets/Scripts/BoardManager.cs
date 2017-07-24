using UnityEngine;
using System.Collections;

public class BoardManager : MonoBehaviour {

	private const float TILE_SIZE = 1.0f;
	private const float TILE_OFFSET = 0.5f;
	
	private int hoverX = -1;
	private int hoverY = -1;
	private float hoverZ = -1f;
	private GameObject selectionHues;
	
	public GameObject hoverTile;
	
	
	// Use this for initialization
	void Start () {
		selectionHues = GameObject.Find("SelectionHues");
		
		if (!selectionHues) {
			selectionHues = new GameObject("SelectionHues");
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdateHover();
		DrawChessboard();
	}
	
	private void UpdateHover() {
		if (!Camera.main) {
			return;
		}
		
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 45f, LayerMask.GetMask("BoardSelectionLayer"))) {
			hoverX = (int)hit.point.x;
			hoverY = (int)hit.point.z;
			hoverZ = hit.point.y;
		} else {
			hoverX = -1;
			hoverY = -1;
			hoverZ = -1;
		}
	}
	
	public Vector3 mouseRayToCoordinate() {
		RaycastHit hit;
		Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 45f, LayerMask.GetMask("BoardSelectionLayer"));
		return new Vector3 (
			(int) hit.point.x,
			(int) hit.point.y,
			(int) hit.point.z
		);
	}
	
	private void DrawChessboard() {
		string hoverName = "";
		if (hoverX >= 0 && hoverY >= 0) {
			hoverName = "(" + hoverX + "," + hoverY + ") " + hoverTile.name;
			if (!GameObject.Find(hoverName)) {
				GameObject newHoverSquare = Instantiate(hoverTile, new Vector3(hoverX + TILE_OFFSET, hoverZ + .01f, hoverY + TILE_OFFSET), Quaternion.identity) as GameObject;
				newHoverSquare.transform.parent = selectionHues.transform;
				newHoverSquare.name = hoverName;
			}
		} else {
			hoverName = "destoryMe";
		}
	
		foreach(Transform child in selectionHues.transform) {
			if (hoverName != "" && child.gameObject.name != hoverName) {
				Destroy(child.gameObject);
			}
		}
	}
	
}
