using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandingPad : MonoBehaviour{

	///Score value of the pad
	int value = 0;
	///Fuel given by landing on the pad
	int fuel = 500;
	/// Active Pad
	/// Trigger: False once pad is collected
	/// Effect: When false, pad gives nothing when landed on
	bool active = true;
	/// If true, this is the last pad on the map
	public bool lastPad;

	void Start(){
		//Rename object
		gameObject.name = "Landing Pad";
		gameObject.tag = "Landing Pad";
		//Add Canvas
		gameObject.AddComponent<Canvas> ();
		gameObject.AddComponent<CanvasScaler> ();
		gameObject.GetComponent<CanvasScaler> ().dynamicPixelsPerUnit = 100 + GetComponent<BoxCollider2D>().size.x*2;
		//Add text to display score
		gameObject.AddComponent<Text> ();
		gameObject.GetComponent<Text> ().text = "\n+"+value;
		gameObject.GetComponent<Text> ().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		gameObject.GetComponent<Text> ().alignment = TextAnchor.MiddleCenter;
	}

	/// Called when player lands on the pad
	public void Land(GameObject player){
		if (active) {
			print ("The eagle has landed");
			//Deactivate
			active = false;
			//Add score and fuel
			GameController.AddScore (value);
			player.GetComponent<LanderControl>().Refuel (fuel);
			gameObject.GetComponent<Text> ().text = "";
			if (lastPad) {
				//Complete level
				GameController.LevelComplete ();
			}
		}
	}

	/// Calculates the score value of the pad
	public void CalculateScore(float highestPoint){
		//NOTE: highestPoint isn't actively used, but is there for possible additions

		float tempValue;

		//Calcualate score based on distance from start, distance from y=0 and width of pad
		tempValue = (int) (transform.position.x / 10 + Mathf.Abs(transform.position.y)/10 + -gameObject.GetComponent<BoxCollider2D>().size.x/4);

		//Add 100 points to last pad
		if (lastPad) {
			tempValue += 100;
		}

		//Adjust tempValue based on level and store
		value = (int) tempValue * GameController.level / 2;
		//Make sure that value is >0
		if (value < 1)
			value = 1;

		//Set text if it exists
		if(GetComponent<Text>() == null) return;
		gameObject.GetComponent<Text> ().text = "\n+"+value;
	}

}
