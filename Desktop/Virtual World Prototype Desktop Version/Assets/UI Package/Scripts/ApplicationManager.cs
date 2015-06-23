using UnityEngine;
using System.Collections;

public class ApplicationManager : MonoBehaviour {


	public void NewSession(){
		Application.LoadLevel ("Virtual_Reef");
	}

	public void Quit () 
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}
}
