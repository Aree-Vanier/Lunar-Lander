using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

	/// If true show instructions
	bool showInstructions;

	/// Gameobject holding Menu text
	public GameObject menuHolder;
	/// Gameobject holding Instructions text
	public GameObject instructionsHolder;

	/// Delay between registered keypresses
	int keyDelay = 0;

	void Update () {
		// Display proper view
		if (showInstructions) {
			Instructions ();	
		} else {
			Menu ();
		}
		keyDelay--;
	}

	/// Draw and process menu
	void Menu(){
		menuHolder.SetActive (true);
		instructionsHolder.SetActive (false);
		if (keyDelay < 0) {
			if (Input.GetAxis ("Abort") > 0.5) {
				SceneManager.LoadScene (1);
			}
			if (Input.GetAxis ("Restart") > 0.5) {
				showInstructions = true;
				keyDelay = 10;
			}
		}
	}

	/// Draw and process instructions
	void Instructions(){
		menuHolder.SetActive (false);
		instructionsHolder.SetActive (true);

		if (keyDelay < 0) {
			if (Input.GetAxis ("Abort") > 0.5) {
				showInstructions = false;
				keyDelay = 10;
			}
		}
	}

}
