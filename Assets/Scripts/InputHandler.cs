using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour {


	/**
	 * NOTE: This class is used in order to create a custom throttle with better control over how it responds than unity default
	 * The math may seem random, but it has been calculated to work the way I want
	 */


	/// X and Y output data
	static Vector2 data;
	/// Edited Y axis input
	static float currentThrottle;
	/// X axis input
	float x;

	/// Input is divided by this to increase distance between 0 and max while reducing max value before output
	public int divider;

	// Delay unytil next keypress is registered
	int keyDelay;

	/// Max. Delay between keypresses in frames
	public int maxKeyDelay;

	/// The maximum throttle value.
	public int maxThrottle;

	static InputHandler instance;

	void Start () {
		if (instance == null) {
			instance = this;
		} else {
			Destroy (gameObject);
		}
	}
	
	void Update () {
		if (keyDelay < 0) {
			if (Input.GetAxis("Throttle")>0.5){
				//Increase throttle
				currentThrottle += 2;
				keyDelay = maxKeyDelay;
			}
			//Reduce throttle if key isn't pressed
			if(currentThrottle > 0) currentThrottle--;
		}
		keyDelay--;
		//Get roll input
		x = Input.GetAxis("Horizontal");
		//Cap the throttle
		if (currentThrottle > maxThrottle) {
			currentThrottle = maxThrottle;
		}
		//Save the inputs to data
		data = new Vector2 (x, currentThrottle/divider);
	}

	/// Gets custom throttle and roll control
	/// 
	/// Returns: X and Y input
	public static Vector2 GetInput(){
		return(data);
	}

	/// Gets if the abort key was pressed
	public static bool getAbort(){
		return(Input.GetAxis("Abort") > 0.5);
	}

	public static bool GetRestart(){
		return(Input.GetAxis("Restart") > 0.5);
	}

	/// Gets the throttle percentage.
	public static float getThrottlePercent(){
		return(currentThrottle / instance.maxThrottle *100); 
	}

	/// Sets throttle to 50% for start of level and animation
	public static void setThrottle(){
		currentThrottle = instance.maxThrottle/2;
	}

	/// Sets the throttle to 0% for last pad landing
	public static void killThrottle(){
		currentThrottle = 0;
	}
}
