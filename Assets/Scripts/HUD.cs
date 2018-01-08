using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

	//UI elements

	/// DEPRECATED
	/// Text in top-left of screen, can be made visible in editor for debugging
	public Text infoText;

	public Slider throttle;

	public Text autoPilotText;

	public GameObject navballObject;
	public Text ballLeftText;
	public Text ballRightText;
	public Text topText;

	/// The navball script
	NavballScript navball;

	/// Delay for flashing elemenmts (Autopilot indicator)
	int blinkDelay;

	void Start () {
		navball = navballObject.GetComponent<NavballScript> ();
	}
	
	void Update () {
		//Get info from GameController
		infoText.text = GameController.GetData ();
		//Means that lander doesn't exist
		if (GameController.GetData() == "")
			return;
	
		string	[] data = GameController.GetData ().Split ('|');
		//Data contents
		//0:Fuel
		//1:TWR
		//2:Throttle
		//3:Speed
		//4:xSpeed
		//5:ySpeed
		//6:Accel
		//7:Score
		//8:Lives
		//9:Roll
		//10:Prograde angle
		//11:Altitiude
		//12:Autopilot

		//Display data
		topText.text = 
			"Lives: "+data[8]+"\tFuel: "+data[0]+"\n" +
			"Score: "+data[7];

		//Added blank space so that substring is always in bounds
		data[6]+="      ";
		data[11]+="      ";
		data[1]+="      ";
		ballLeftText.text = 
			"Accel: " + data [6].Substring(0,5) + "\n" +
			"Alt  : " + data [11].Substring(0,5) + "\n" +
			"TWR  : " + data [1].Substring(0,5);

		ballRightText.text = 
			"Speed : " + data [3] + "\n" +
			"xSpeed: " + data [4] + "\n" +
			"ySpeed: " + data [5];

		throttle.value = float.Parse (data [2]);
		navball.SetData (float.Parse(data[9]), float.Parse(data[10]));

		//Autopilot indicator in top-right is on when the player has no control over the lander (animation, next level)
		if (bool.Parse (data [12])) {
			autoPilotText.text = "Autopilot";
		} else {
			autoPilotText.text = "";
		}

		blinkDelay--;
		if (blinkDelay < 0) {
			autoPilotText.gameObject.SetActive (!autoPilotText.gameObject.activeInHierarchy);
			blinkDelay = 30;
		}
	}
}
