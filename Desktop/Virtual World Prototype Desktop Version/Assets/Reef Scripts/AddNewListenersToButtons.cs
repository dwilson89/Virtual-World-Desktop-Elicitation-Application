using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*
**Author: Dustin Wilson
**Date: 18/05/2015
**Description: A simple class that inherits from MonoBehaviour, its purpose is to Add onclick
**listeners to the three buttons for the exit menu, save, load and exit.
 */
public class AddNewListenersToButtons : MonoBehaviour {

	//Buttons for the exit menu - [ublic for easy assigning
	public Button save;
	public Button load;
	public Button exit;

	// Use this for initialization
	void Awake () {
		//Assign new onclick listeners to each button from the datacontroller 
		save.GetComponent<Button> ().onClick.AddListener (() => DataContoller.control.Save ());
		load.GetComponent<Button> ().onClick.AddListener (() => DataContoller.control.Load ());
		exit.GetComponent<Button> ().onClick.AddListener (() => DataContoller.control.Quit ());
	}
}
