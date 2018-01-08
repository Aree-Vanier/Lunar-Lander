using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLock : MonoBehaviour {

	///Farthest right the camera will go
	public float maxX;
	///Farthest left the camera will go
	public float minX;
	///Highest the camrea will go
	public float maxY;

	/// Target gameobject to follow
	public GameObject target;
	/// Offset from the target
	public Vector3 offset;
	/// Camera Object
	Camera cam;

	///Follow Target Flag
	///Trigger: Set to true by SetTarget(), set to faslse by Stop()
	///Effect: Camera follows target if true, stops following target if false
	public bool follow = true;


	void Start () {
		offset = new Vector3 (0, 0, -10);
		cam = GetComponent<Camera> ();
	}


	
	void Update () {
		//Continually set Min and Max to adapt for changes in terrain
		setMinMax ();
		//Follow the target
		if (follow && target!=null) {
			transform.position = target.transform.position + offset;
		}


		//Zoom camera if close to ground

		//Mask only hits terrain
		LayerMask mask = 1 << 8;

		//Cast raycast
		RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, 70, mask);
		if (hit.collider != null) {
			//Get Distance
			float distance = Vector2.Distance(hit.point, transform.position);
			if (distance < 30)
				//Zoom in
				cam.orthographicSize = 30;
		}
		else {
			//Zoom out
			cam.orthographicSize = 80;
		}


		//Prevents camera from exceeding min/max area
		if (transform.position.x > maxX)
			transform.position = new Vector3(maxX, transform.position.y, -10);
		if (transform.position.x < minX)
			transform.position = new Vector3(minX, transform.position.y, -10);
		if (transform.position.y > maxY)
			transform.position = new Vector3(transform.position.x, maxY, -10);
	}

	/// Changes the target to newTarget, gets a new offset if changeoffset is true.
	public void changeTarget(GameObject newTarget, bool changeOffset){
		target = newTarget;
		if (changeOffset) {
			offset = transform.position - target.transform.position;
			offset = new Vector3 (offset.x, offset.y, -10);
		}
		follow = true;
	}

	/// Stops following target
	public void stop(){
			follow = false;
	}

	/// Sets the min and max values for camera area, based on terrain dimensions
	void setMinMax(){
		//Get terrain from GameControler
		TerrainGeneration terrain = GameController.GetTerrain ();

		minX = terrain.minX + cam.orthographicSize*1.4f;
		maxX = terrain.maxX - cam.orthographicSize*1.4f;
		maxY = terrain.highestY + cam.orthographicSize+70; 
	}
}
