using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour  {

	/// The active lander
	GameObject lander;
	/// The active Lander's LanderScript
	LanderControl landerScript;
	/// The prefab for the lander
	public Object landerPrefab;

	///Game Over flag
	///Trigger: Lives < 0
	///Effect: Game will go to gameOver instead of respawning
	bool gameOverFlag = false;

	///Player's lives
	public static int lives = 3;
	///Player's score
	public static int score;
	///Main camera, used to target new player and passed to player
	public Camera cam;
	///Highest point in terrain
	public static float highestPoint = 0;
	//Local reference to terrain
	public GameObject terrain;
	/// Text that appears after death or abort
	public Text deathText;
	///Current level Level
	public static int level = 1;
	//Instance for non-static referance
	static GameController instance;

	///Level completion Flag
	///Trigger: Landing on last pad
	///Effect: Starts next level
	public static bool levelCompleteFlag;


	///Play start Flag
	///Trigger: Start of a level
	///Effect: Plays the start animation on the lander
	public static bool playStartAnimation = true;

	void Start () {
		Restart ();
	}

	///Resets static variables at the start of game scene
	void Restart(){
		lives = 3;
		score = 0;
		level = 1;
		playStartAnimation = true;
		Respawn ();
	}

	void Awake(){
		if (instance == null) {
			instance = this;
		}
		else
			Destroy (gameObject);
	}

	
	void Update () {
		if (!instance.gameOverFlag) {
			highestPoint = terrain.GetComponent<TerrainGeneration> ().highestY;
			//Check if abort occured and is complete
			if (landerScript.dead && landerScript.abort) {
				if (landerScript.abortSuccess) {
					deathText.text = "Abort Success\nPress R to continue";
				} else {
					deathText.text = "Abort Failed\nPress R to continue";
				}
			}
			//Check if abort didn't happen and lander is dead
			if (landerScript.dead && !landerScript.abort) {	
				deathText.text = "You Died\nPress R to continue";
			}
		}

		//If the lander died after final landing, stop playing "Eagle has landed" sound effect
		if (landerScript.destroyed || landerScript.abort) {
			instance.gameObject.GetComponent<AudioSource> ().Stop ();
		}

		//If the level is complete await keypress to load next level
		if (levelCompleteFlag) {
			deathText.text = "Level Complete!\nPress R to continue";
			InputHandler.killThrottle ();
			if(InputHandler.GetRestart()){
				NextLevel ();
			}
		}

	}

	///Add points to player's score
	public static void AddScore(int points){
		score += points;
	}

	///Remove half of the player's points if the cabin was killed
	public static void CabinKilled(){
		score /= 2;
	}

	///Start next level
	static void NextLevel(){
		level++;
		//Reset player
		Respawn ();
		//Generate new terrain
		instance.terrain.GetComponent<TerrainGeneration> ().Regen ();
		//Add bonus life
		lives++;
		levelCompleteFlag = false;
		instance.deathText.text = "";
		//Stop playing "Eagle has landed" sound effect
		instance.gameObject.GetComponent<AudioSource> ().Stop ();
	}

	///Called when player lands on last pad
	public static void LevelComplete(){
		instance.gameObject.GetComponent<AudioSource> ().Play ();
		levelCompleteFlag = true;
		playStartAnimation = true;
	}

	/// Respawns the player
	public static void Respawn(){
		instance.deathText.text = "";
		//If there is already a lander
		if (instance.lander != null) {
			//Send gameobject to partHolder
			instance.lander.transform.parent = instance.terrain.GetComponent<TerrainGeneration> ().partHolder.transform;
			//Deactiveate lander
			instance.landerScript.active = false;
		}
		//Create new lander
		instance.lander = (GameObject) Instantiate (instance.landerPrefab);
		instance.landerScript = instance.lander.GetComponent<LanderControl> ();
		//Camera targets new lander
		instance.cam.GetComponent<CameraLock> ().changeTarget (instance.lander, false);
		//Pass camera to new lander
		instance.landerScript.cam = instance.cam;
	}

	///Change to game over scene
	public static void GameOver(){
		//Save the score for gameOver scene
		PlayerPrefs.SetInt ("score", score);
		instance.gameOverFlag = true;
		SceneManager.LoadScene ("GameOver", LoadSceneMode.Single);
	}

	/// Used to get the terrain
	/// 
	/// Returns: Terrain
	public static TerrainGeneration GetTerrain(){
		return(instance.terrain.GetComponent<TerrainGeneration>());
	}

	/// Gets HUD data. 
	/// 
	/// Returns: Hud data as string: fuel|twr|throttle|speed|xSpeed|ySpeed|accel|score|lives|roll|prograde angle|autopilot
	public static string GetData(){
		string info = "";
		//lander doesnt exist
		if (instance.lander == null) {
			return "";
		}
		info += (int)instance.landerScript.fuel;
		info += "|" + instance.landerScript.GetTWR ().ToString ("0.00");
		if (instance.landerScript.fuel > 0)
			info += "|" + InputHandler.getThrottlePercent ().ToString ("0.000");
		else
			info += "|0";
		info += "|" + instance.landerScript.GetSpeed ().ToString ("0.0");
		info += "|" + instance.landerScript.GetSplitSpeed ().x.ToString ("0.0");
		info += "|" + instance.landerScript.GetSplitSpeed ().y.ToString ("0.0");
		info += "|" + instance.landerScript.GetAccel ().ToString ("0.00");
		info += "|" + score.ToString ();
		info += "|" + lives.ToString ();
		info += "|" + instance.landerScript.transform.eulerAngles.z.ToString("0.0");
		info += "|" + instance.landerScript.GetPrograde ().ToString ("0.0");
		info += "|" + Mathf.Abs(instance.landerScript.GetAlt ()).ToString ("0.0");
		info += "|" + (instance.landerScript.goNogo.isPlaying||levelCompleteFlag);

		return info;
	}

//	OLD info text generation
//	string info = "";
//	info += "Fuel: " + (int)instance.landerScript.fuel + "\n";
//	info += "TWR: " + instance.landerScript.GetTWR ().ToString ("0.00") + "\n";
//	if (instance.landerScript.fuel > 0)
//		info += "Throttle: " + InputHandler.getThrottlePercent ().ToString ("0.0") + "%\n";
//	else
//		info += "Throttle: 0%\n";
//	info += "Speed: " + instance.landerScript.GetSpeed ().ToString ("0.0") + "\n";
//	info += "Accel: " + instance.landerScript.GetAccel ().ToString ("0.00") + "\n";
//	info += "Score: " + score.ToString () + "\n";
//	info += "Lives: " + lives.ToString () + "\n";
}
