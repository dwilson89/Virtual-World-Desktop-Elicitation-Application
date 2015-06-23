using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UpdateHeight : MonoBehaviour {
	public Vector2 info;
	public Vector2 textPos;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

		GameObject modeText = GameObject.FindWithTag("inspect content");
		info = modeText.GetComponent<RectTransform>().sizeDelta;
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, info.y + 40f);

		// need to offset the y position for the child
		textPos = modeText.GetComponent<RectTransform> ().anchoredPosition;
		modeText.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (textPos.x, -info.y / 2f);

	}
}
