using UnityEngine;
using System.Collections;

public class PlayerControllerScript : MonoBehaviour {

	public static  PlayerControllerScript control;

	//it will contain all the relavant player data and controls needed from scene to scene


	void Awake(){
		if (control == null) {
			DontDestroyOnLoad (gameObject);
			control = this;
		} else if (control != this) {
			Destroy (gameObject);
		}
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
