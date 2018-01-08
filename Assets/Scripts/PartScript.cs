using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartScript : MonoBehaviour {

	///Lander that part belings to
	public GameObject lander;
	///LanderScript that part belings to
	LanderControl landerScript;

	///Abort flag
	///Trigger: Abort started by player
	///Effect: Cabin runs abort code
	public bool abort;

	///Part type used to identify part.
	/// Ex. "Leg", "Engine", "Cabin"
	public string type = "part";
	/// Force applied to part on exposion
	public Vector2 explodeForce;
	/// Speed at which part breaks
	public float breakSpeed;

	/// A GameObject where parts are stored once they break off the lander
	GameObject partHolder;

	/// Prefab used by cabin and engine to create explosion particles
	public Object particles;

	///Is the part attached to the lander
	public bool attached = true;

	void Start () {
		//Get lander script
		landerScript = lander.GetComponent<LanderControl> ();
		//Get part holder
		partHolder = GameController.GetTerrain ().partHolder;
	}

	
	void Update () {
		//Only run by cabin
		if (abort) {
			if (attached){
				//Detaches the cabin
				Explode ();
				gameObject.tag = "Untagged";
				//Kill velocity kept from lander
				GetComponent<Rigidbody2D> ().angularVelocity = 0;
				//Play abort sound
				GetComponent<AudioSource>().Play ();
			}
			//If lander becomes incative, stop sound
			if (landerScript.active == false) {
				GetComponent<AudioSource> ().Stop ();
			}
			//Speed up
			GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.up*Time.deltaTime*1000);
			//Kill rotation issues from collisions with lander base
			GetComponent<Rigidbody2D> ().freezeRotation = true;
			//Rotate upward
			if (transform.eulerAngles.z > 180) {
				transform.Rotate (0, 0, 0.5f);
				print ("Left");
			}else if (transform.eulerAngles.z < 180) {
				transform.Rotate (0, 0, -0.5f);
				print ("Right");
			}
			//Run engine particles
			ParticleSystem.EmissionModule e = GetComponent<ParticleSystem> ().emission;
			e.rateOverTime = 500;
			//if high enough, abort is successful
			if (transform.position.y > GameController.highestPoint+50) {
				landerScript.abortSuccess=true;
				landerScript.Kill ();
			}
		}
		
	}

	void OnCollisionEnter2D (Collision2D other){
		print (type + "\t" + landerScript.GetAccurateSpeed());
		if ((other.gameObject.tag == "Ground" || other.gameObject.tag == "Landing Pad")&& landerScript.GetAccurateSpeed() > breakSpeed) {
			if (type == "Engine") {
				//If the engine hits the ground, explode
				landerScript.Explode ();
				landerScript.explosionSound.Play ();
			} else if (type == "Cabin") {
				// If the cabin hits the ground, kill the player and fail any active abort 
				Instantiate (particles, transform.position, transform.rotation, partHolder.transform);
				landerScript.control = false;
				gameObject.SetActive (false);
				GameController.CabinKilled ();
				if (abort) {
					landerScript.abortSuccess = false;
				}
				landerScript.Kill ();
				landerScript.explosionSound.Play ();
			}else if (attached){
				//If its anything else, play snap sound
				GetComponent<AudioSource>().Play ();
			}
			Detach ();

		}else if(other.gameObject.tag == "Landing Pad" && attached){
			//If the landing is smooth and on a landing pad
			other.gameObject.GetComponent<LandingPad> ().Land (lander);
		}
	}



	///Detach part and apply force
	public void Explode(){
		Detach ();
		//Change to ground
		gameObject.tag = "Ground";
		//Add explosion force
		GetComponent<Rigidbody2D> ().AddRelativeForce (explodeForce);
		attached = false;
		if (type == "Engine" && gameObject.activeInHierarchy) {
			Instantiate (particles, transform.position, transform.rotation, partHolder.transform);
			gameObject.SetActive (false);
		}
	}

	/// Detach the part from the lander
	void Detach(){
		//Move to partHolder
		transform.parent = partHolder.transform;
		//Add a rigidbody
		if(GetComponent<Rigidbody2D>() == null) gameObject.AddComponent<Rigidbody2D> ();
		///Detach
		attached = false;
	}
}
