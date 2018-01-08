using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LanderControl : MonoBehaviour {

	Rigidbody2D body;


	//Flags

	///Controllability flag
	///Trigger: engine or cabin is destroyed
	///Effect: disables controls
	public bool control = true;

	///Active lander flag
	///Trigger: Disabled when a new lander spawns
	///Effects: Tell code that this is active lander, all code is disabled if false
	public bool active = true;

	///Abort success Flag
	///Trigger: Abort successfully completed
	///Effect: Code knows that abort was successfully completed
	public bool abortSuccess;

	///Abort flag
	///Trigger: Abort sequence initiated
	///Effect: Runs abort code
	public bool abort;

	///Destroyed flag
	///Trigger: Engine Destroyed
	///Effect: Prevents abort
	public bool destroyed;

	///Dead flag
	///Triggered: cabin is killed
	///Effect: disables everything and permits respawn
	public bool dead = false;


	///Speed multiplier
	public int speed = 300;
	///Speed limit
	public int speedLimit = 50;
	///Speed from last frame, used to calculate acceleration
	float oldSpeed;


	///Speed that lander spins at
	public int rollSpeed;

	///All partScripts
	public PartScript[] parts;	
	/// The local variable for the cabin's partScript, used to set camera target and abort flag
	PartScript cabin;

	///Engine Particle Emission for changing emission amount
	ParticleSystem.EmissionModule rate;

	///Local Variable for last collected control input
	Vector2 controls;

	///Local variable for main camera, used to change target and follow settings
	[HideInInspector]
	public Camera cam;


	//Audio sources
	[HideInInspector]
	public AudioSource goNogo;
	AudioSource seconds60;
	AudioSource seconds30;
	AudioSource abortSound;
	AudioSource engineSound;
	[HideInInspector]
	public AudioSource explosionSound;

	/// Lander fuel
	[HideInInspector]
	public float fuel;
	///Fuel at start of round used to set fuel and calculate TWR
	public int startFuel = 7500;




	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody2D> ();


		foreach (PartScript part in parts) {
			//Sets the local rate variable if part is the engine
			if(part.type == "Engine"){
				rate = part.gameObject.GetComponent<ParticleSystem>().emission;
			}
			//Sets the local cabin variable if part is the cabin
			if (part.type == "Cabin") {
				cabin = part;
			}
		}
			
		fuel = startFuel;

		//Gets audioSources
		goNogo = GetComponents<AudioSource> () [0];
		seconds30 = GetComponents<AudioSource> () [1];
		seconds60 = GetComponents<AudioSource> () [2];
		abortSound = GetComponents<AudioSource> () [3];
		engineSound = GetComponents<AudioSource> () [4];
		explosionSound = GetComponents<AudioSource> () [5];

		//plays start animation
		if (GameController.playStartAnimation) {
			transform.position = new Vector2 (-301, 126);
			cam.transform.position = transform.position;
			body.AddRelativeForce (Vector2.down * 2000);
			goNogo.Play ();
		} else {
			transform.position = new Vector2 (51.3f, 126);
			body.velocity = new Vector2(30, 0);
			InputHandler.setThrottle();
		}
		//Disables animation in future lives
		GameController.playStartAnimation = false;
	}
	
	void FixedUpdate () {

		//if go/Nogo is playing, animation isn't done
		if (goNogo.isPlaying) {
			Animate ();
			return;
		}

		//if the level is done or the lander is inactive, don't run the code
		if (GameController.levelCompleteFlag || !active)
			return;

		//Abort if possible
		if (InputHandler.getAbort () && !destroyed)
				Abort ();

		//Gets new control data is lander is controlable
		if (control) {
			controls.x = InputHandler.GetInput ().x;
			if (fuel > 0) {
				controls.y = InputHandler.GetInput ().y;
			}
		}


		//Add speed, set fire and engine sound based on last input controls
		if(controls.y < 0) controls.y = 0;
		if (fuel > 0) {
			body.AddRelativeForce (new Vector2 (0, controls.y * speed * Time.deltaTime));
			rate.rateOverTime = controls.y * 100;
			engineSound.volume = controls.y / 4;
		} else {
			engineSound.Stop ();
		}

		//Rotate craft
		transform.Rotate (new Vector3 (0, 0, -controls.x * rollSpeed*Time.deltaTime));


		//Prevent overspeed
		if (body.velocity.x > speedLimit)
			body.velocity = new Vector2 (speedLimit, body.velocity.y);		
		if (body.velocity.y > speedLimit)
			body.velocity = new Vector2 (body.velocity.x, speedLimit);
	
		//Drain Fuel
		if (fuel <= 0) {
			rate.rateOverTime = 0;
			fuel = 0;
		} else {
			fuel -= controls.y;
		}

		//Set mass based on fuel
		body.mass = ((fuel-startFuel)/startFuel+2);


		//Respawn
		if (Input.GetKey (KeyCode.R) && dead && active) {
			if (GameController.lives > 0)
				GameController.Respawn ();
			else
				GameController.GameOver ();
		}

		//Set oldSpeed
		oldSpeed = GetSpeed ();

		//Stop camera follow if lander is dead
		if (dead && active) {
			cam.GetComponent<CameraLock> ().stop();
		}

		//Play 60 and 30 second fuel warnings
		if (fuel > 7198 && fuel < 7202) {
			seconds60.Play ();
		}
		if (fuel > 3598 && fuel < 3602) {
			seconds30.Play ();
		}

	}

	 ///Calculates linear speed using a2+b2=c2
	/// 
	/// Returns: Linear speed
	public float GetSpeed(){

		return Mathf.Sqrt(Mathf.Pow(body.velocity.x, 2)+ Mathf.Pow(body.velocity.y, 2));
	}


	///Calls explode on all parts
	public void Explode(){
		control = false;
		foreach (PartScript part in parts) {
			part.Explode ();
		}
		fuel = 0;
		destroyed = true;
		if (active) {
			abortSound.Stop ();
			cam.gameObject.GetComponent<CameraLock> ().changeTarget (cabin.gameObject, false);
		}
	}


	/// Adds Fuel
	public void Refuel(int fuel){
		this.fuel += fuel;
	}



	///Calculates thrust-wieght ratio
	///
	/// Returns: thrust-wieght ratio
	public float GetTWR(){
		return(0.015f*speed-body.mass);
	}

	///Calculates acceleration
	///
	/// Returns: acceleration ratio
	public float GetAccel(){
		return(oldSpeed - GetSpeed());
	}

	///Gets a more accurate speed calculation based on last frame's speed
	/// 
	/// Returns: more accurate speed
	public float GetAccurateSpeed(){
		return((oldSpeed+GetSpeed())/3);
	}

	///Gets speed split into X and Y
	/// 
	/// Returns: Vector2 with X and Y speed
	public Vector2 GetSplitSpeed(){
		return body.velocity;
	}
		

	///Initiates abort sequence
	void Abort(){
		abortSound.Play ();
		cam.GetComponent<CameraLock>().changeTarget(cabin.gameObject, true);
		cabin.abort = true;
		control = false;
		abort = true;
		foreach(PartScript part in parts){
			//Parts can't count for points
			if (!part.type.Equals("Cabin"))
				part.attached = false;
		}
	}

	///Sets dead and removes life if not yet dead
	public void Kill(){
		if (!dead) {
			dead = true;
			GameController.lives--;
		}
	}


	///Uses cosine law to calculate prograde vector
	/// 
	/// Returns angle of prograde vector
	public float GetPrograde(){
		//If the animation is playing, the prograde is 270 degrees
		if (goNogo.isPlaying) {
			return 270;
		}
		float a = body.velocity.y;
		float b = body.velocity.x;
		float c = GetSpeed ()+0.001f;

		float p = 0;

		float s1 = Mathf.Pow (a, 2) + Mathf.Pow (c, 2) - Mathf.Pow (b, 2);
		float s2 = 2 * a * c;
		float s3 = s1 / s2;

		p = Mathf.Rad2Deg*Mathf.Acos (s3);

		if (body.velocity.x > 0) {
			p = -p;
		}

		return p;

	}

	///Fires raycast to get altitude
	/// 
	/// Returns altitude
	public float GetAlt(){
		float alt = 0;

		LayerMask mask = 1 << 8;

		RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, 70, mask);

		//1.5 accounts for lander height

		alt = Vector2.Distance (transform.position, hit.point)-1.5f;

		return alt;
	}

	/// Code that runs the animation at the start
	void Animate(){

		body.AddForce (-Physics2D.gravity);
		InputHandler.setThrottle ();
		rate.rateOverTime = 200;
		engineSound.volume = 50;


		body.AddRelativeForce (Vector2.up);
		print (body.velocity.x);
	}
}
