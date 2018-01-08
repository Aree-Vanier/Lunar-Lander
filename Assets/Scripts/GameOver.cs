using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour {

	///Array of top 5 highscores
	int[] scores = new int[5];
	///Array of top 5 names
	string[] names = new string[5];

	//Vaious text fields
	public Text scoreText;
	public Text highscoreText;
	public Text nameText;
	public Text restartText;

	/// Gameobject holding name setting text
	public GameObject nameHolder;
	/// Gameobject holding highscore display text
	public GameObject highscoreHolder;

	//Name setting variables

	///Name Ediding done flag
	///Trigger: When player is done setting name of if score isn't a highscore
	///Effect: Switches do display scores instead of setting name
	bool nameDone = false;
	///Player's name
	string userName;
	///Char array used for setting username
	char[] userNameRaw = {'A','A', 'A'};
	///Current char being edited in username
	int currentChar = 0;
	///All allowed characters for username, array is used for easy setting of values
	char[] lettersArray = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8','9'};
	///All allowed characters for username, lists is used for easy reading of values
	List<char> letters = new List<char>();

	/// Local variable for player's score
	int score;

	/// Delay before accepting next keypress
	int keyDelay = 0;

	/// Position in highscore list. -1 if not on list
	int scoreValue;

	/// Editor-only value that can be used to clear score list
	public bool clearScores;


	void Start () {
		//Set local score
		score = PlayerPrefs.GetInt("score");
	
		//Copy letters from array to list
		letters.AddRange (lettersArray);
		
		scoreText.text = "Score: " + score;
		Load ();

		//Add delay to prevent accidentally exiting immediately
		if(scoreValue == -1)
			keyDelay = 10;

	}
	
	void Update () {
		//Clear the scores if checked in editor
		if (clearScores) {
			PlayerPrefs.DeleteAll ();
		}
		//Check that score is a highscore
		if (scoreValue != -1) {
			if(!nameDone){
				//If name isn't done, display nameHolder
				nameHolder.SetActive (true);
				EnterName ();

				//If name just got finised, add score to list and save
				if (nameDone) {
					AddScore ();
					Save ();
					scoreValue = -1;
				}
			}
		} else {
			//If score ins't a highscore, then name is done
			nameDone = true;
		}

		if (nameDone) {
			//Draw highscores, not name editor
			nameHolder.SetActive (false);
			highscoreHolder.SetActive (true);
			DrawScores ();
			//Return to menu
			if (keyDelay < 0) {
				restartText.text = "Press SPACE to return to menu";
				if (Input.GetAxis("Abort")>0.5) {
					print ("SCENE");
					SceneManager.LoadScene (0);
				}
			}
		}

		keyDelay--;
	}

	/// Load highscores
	void Load(){
		//Reset scoreValue
		scoreValue = -1;
		//Iterate over available highscore slots
		for (int i = 0; i < scores.Length; i++) {
			//Get score at position i, if it doesn't exist, replace with 0
			if (PlayerPrefs.HasKey ("score" + i)) {
				scores [i] = PlayerPrefs.GetInt ("score" + i);
			} else {
				scores [i] = 0;
			}
			//If the player's score is greater than the loaded score, and is smaller that previously loaded scores, then this is where it goes
			if (scores [i] < score && scoreValue == -1) {
				scoreValue = i;
			}
			//Get name at position i, if it doesn't exist, replace with ---
			if (PlayerPrefs.HasKey ("name" + i)) {
				names [i] = PlayerPrefs.GetString ("name" + i);
			} else {
				names [i] = "---";
			}
		}
	}

	/// Draws the scores.
	void DrawScores(){
		string s = "";
		//Format data for each score
		for (int i = 0; i < scores.Length; i++) {
			s += i + 1 + ": ";
			s += names [i] + "    ";
			s += scores [i] + "\n";
		}

		highscoreText.text = s;

	}

	/// Save new higscore list
	void Save(){
		for (int i = 0; i < scores.Length; i++) {
			PlayerPrefs.SetInt ("score" + i, scores[i]);
			PlayerPrefs.SetString ("name" + i, names[i]);
		}
	}

	/// Add player's score to highscores list
	void AddScore(){
		for (int i = scores.Length-2; i >= 0; i--) {
			//If the score is less than the player's score, bump it down
			if (i >= scoreValue) {
				scores [i + 1] = scores [i];
				names [i + 1] = names [i];
			}
		}
		//Insert the player's score
		scores [scoreValue] = score;
		names [scoreValue] = userName;
	}
		
	/// Handles name entering process
	void EnterName(){
		if (keyDelay < 0) {
			///Index of the new letter in letters list
			int index = 0;
			//If up key
			if (Input.GetAxis ("Vertical") > 0.5) {
				//Get index of next letter in letters
				index = letters.IndexOf (userNameRaw [currentChar]) + 1;
				//If it's out of bounds, wrap around
				if (index > letters.Count - 1)
					index = 0;
				//Set currentChar to letter
				userNameRaw [currentChar] = letters [index];
				keyDelay = 10;
			}
			//If down key
			if (Input.GetAxis ("Vertical") < -0.5) {
				//Get index of next previous in letters
				index = letters.IndexOf (userNameRaw [currentChar]) - 1;
				print (index);
				//If it's out of bounds, wrap around
				if (index < 0)
					index = letters.Count - 1;
				//Set currentChar to letter
				userNameRaw [currentChar] = letters [index];
				keyDelay = 10;
			}
			//If left key
			if (Input.GetAxis ("Horizontal") < -0.5) {
				//Focus character to left
				currentChar--;
				//If it's out of bounds, wrap around
				if (currentChar < 0) {
					currentChar = userNameRaw.Length - 1;
				}
				keyDelay = 10;
			}
			//If right key
			if (Input.GetAxis ("Horizontal") > 0.5) {
				//Focus character on right
				currentChar++;
				//If it's out of bounds, wrap around
				if (currentChar > userNameRaw.Length - 1) {
					currentChar = 0;
				}
				keyDelay = 10;
			}
		}

		//Finish editing name
		if (Input.GetAxis("Abort")>0.5) {
			userName = CharToString (userNameRaw);
			nameDone = true;
			keyDelay = 20;
		}
			

		//Get string of name
		string s = CharToString (userNameRaw)+"\n";

		//Draw pointer under letter
		for (int i = 0; i < userNameRaw.Length; i++) {
			if(i==currentChar)
				s += "^";
			else
				s += " ";
		}

		//Draw name and pointer
		nameText.text = s;
	}

	/// Converts passed char array to string
	/// 
	/// Returns: String
	string CharToString(char[] array){
		string s = "";

		foreach (char c in array) {
			s += c.ToString();
		}

		return s;
	}

}
