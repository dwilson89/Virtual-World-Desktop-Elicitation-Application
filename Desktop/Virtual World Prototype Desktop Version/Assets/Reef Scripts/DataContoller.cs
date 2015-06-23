using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/*
**Author: Dustin Wilson
**Date: 18/05/2015
**Description: A script that follows the singleton design pattern to allow for data 
**persistence across scenes, it contains the functionality for loading, saving and 
**exiting for the application. 
 */
public class DataContoller : MonoBehaviour {

	//Static DataController
	public static DataContoller control;

	//ElicObj - prefab to be used when loading saved data
	public GameObject elicObj;

	//Boolean value to indicate if in exit menu
	public bool inExitMenu = false; //Could be changed to a property

	//Private variables to store loaded data and load state
	private ElicitedData loadedData;
	private bool hasLoaded = false;

	// Set up of the singleton
	void Awake(){
		//if control doesnt exist
		if (control == null) {
			DontDestroyOnLoad (gameObject);//object isnt destroyed on scene load
			control = this; // control is equal to this instance
		
		//if a control does exist and is not this 
		} else if (control != this) {
			Destroy (gameObject);
		}
	}

	// Function called each frame
	void Update(){

		//waits until the level is loaded to populate the scene with data
		if(hasLoaded && !Application.isLoadingLevel){
			PopulateData(loadedData);
			hasLoaded = false;
		}
	}

	/** Function: NewSession
	 ** Purpose: Loads the Virtual_Reef level
	 ** Note: In the future could be changed to take a string parameter indicating the levels name
	 ** This would be if more scenes were created
	 */
	public void NewSession(){
		Application.LoadLevel ("Virtual_Reef");
	}

	/** Function: ExitToMenu
	 ** Purpose: Loads the Menu Scene
	 ** Note: At the moment there is an issue, where a null exception happens
	 ** needs to be investigated. (Not currently called)
	 */
	//public void ExitToMenu (){
	//	Application.LoadLevel ("Menu 3D");
	//}

	/** Function: Quit
	 ** Purpose: Exits the application, or if in the editor ends play mode
	 */
	public void Quit () 
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	/** Function: Save
	 ** Purpose: Saves all the elicited data to a Binary file
	 ** Note:In the future give the user the option to name the file and select where they want to save it
	 ** Add two parameters, file location, and filename, also perhaps change the file type
	 */
	public void Save(){

		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (Application.persistentDataPath +  Path.DirectorySeparatorChar + "elicitedInfo.dat");
		ElicitedData data = new ElicitedData ();
		//find all objects of eliciteddata type
	 	GameObject[] objs = GameObject.FindGameObjectsWithTag("Inspect Element");
		//add them to the list
		foreach(GameObject obj in objs){
			data.AddNewObject(obj.GetComponent<ObjectDataProperties>());
		}
		//save the scene name - (this is added for future improvement when their might be multiple scenes)
		data.SceneName = Application.loadedLevelName;
		//Serialize the data
		bf.Serialize (file, data);

		file.Close();

	}

	/** Function: Load
	 ** Purpose: Loads all the data from a binary file located in the Application persistent data path
	 ** 
	 ** Note: In the future allow the user to choose a file location, add paramter string filepath
	 */
	public void Load(){

		if (File.Exists (Application.persistentDataPath + Path.DirectorySeparatorChar + "elicitedInfo.dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + Path.DirectorySeparatorChar + "elicitedInfo.dat", FileMode.Open);//Application.persistentDataPath + "/elicitedInfo.dat");

			//Deserialize the data
			ElicitedData data = (ElicitedData)bf.Deserialize (file);
			file.Close ();

			//Load the scene 
			Application.LoadLevel (data.SceneName);

			//Indicate that the loadlevel has been called from the load function
			hasLoaded = true;
			//Set the global loadedData to the data loaded from the file
			loadedData = data;
			//Instantiate (elicObj, new Vector3 (1.0f, 1.0f, 1.0f), Quaternion.identity);
		}
	}

	/** Function: PopulateData
	 ** Param: The elicited data that needs to be loaded
	 ** Purpose: Using the data, elicObjs are created in the scene for each object stored in the data file
	 */
	private void PopulateData(ElicitedData data){

		Vector3 pos;
		Quaternion rotatation = new Quaternion (0f, 0f, 0f,0f);
		foreach (ObjectData obj in data.elicitatedObjects) {

			pos = new Vector3(obj.GetPosX(),obj.GetPosY(),obj.GetPosZ());

			//Create a elicited data object
			GameObject newObj = Instantiate(elicObj) as GameObject;
			//Get the child with the data storage script
			GameObject capChild = newObj.transform.FindChild ("Capsule").gameObject;

			//Set its location
			capChild.transform.position = pos;

			//Set the elicited information and area size
			capChild.GetComponent<ObjectDataProperties>().elicitedInformation = obj.GetInfo();
			capChild.GetComponent<ObjectDataProperties>().projectorRadius = obj.GetAreaR();
		}
	}

}

/** Private Class ElicitedData
 ** Purpose: A serialiable object type, to store the elicited objects in the scene. 
 ** Created for the purpose of saving and loading data
 */
[System.Serializable]
class ElicitedData{

	//List of elicitated objects 
	[SerializeField] 
	public List<ObjectData> elicitatedObjects;

	public string SceneName;

	//Constructor
	public ElicitedData(){

		elicitatedObjects = new List <ObjectData> ();
	}

	/** Function: AddNewObject
	 ** Param: Takes an ObjectDataProperties object
	 ** Purpose: Converts the ObjectDataProperties object into an serializable datatype, and then adds it to the
	 ** ObjectData list containing all elicited Objects
	 */
	public void AddNewObject(ObjectDataProperties newObject){

		ObjectData newObj = new ObjectData (newObject.elicitedTransformInfo.transform.position,newObject.elicitedInformation, newObject.projectorRadius);
		Debug.Log (newObj.GetInfo ());
		elicitatedObjects.Add (newObj);
	}
}

/** Private Class ObjectData
 ** Purpose: A serialiable object type, to store the elicited objects information. 
 ** Created for the purpose of saving and loading data.
 ** Note: Created as I was having issues serializing Unity specifics object types
 */
[System.Serializable]
class ObjectData{

	private float positionX;
	private float positionY;
	private float positionZ;
	private string data;
	private float areaR;

	//Constructor
	public ObjectData(Vector3 pos, String dat, float r){
		positionX = pos.x;
		positionY = pos.y;
		positionZ = pos.z;
		data = dat;
		areaR = r;
	}

	//Getters

	public float GetPosX(){
		return positionX;
	}

	public float GetPosY(){
		return positionY;
	}

	public float GetPosZ(){
		return positionZ;
	}

	public string GetInfo(){
		return data;
	}

	public float GetAreaR(){
		return areaR;
	}
}