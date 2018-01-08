using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class TerrainGeneration : MonoBehaviour {


	//Screen size in pixels: 100x138
	public int height = 100;
	public int width = 138;

	/// X position of last point
	float lastX = 0;
	/// Y position of last point
	float lastY = 0;
	/// Difference between last 2 y positions
	float deltaY = 0;
	/// Highest Y position
	public float highestY = 0;
	///Lowest X
	public float minX;
	//Highest X
	public float maxX;
	/// Counter that prevents landing pads from being too close together
	int landCounter;
	/// The GameObject that stores detached parts
	public GameObject partHolder;
	/// List of landing pads
	List<LandingPad> landingPads = new List<LandingPad>();

	/// Points that form the terrain
	List<Vector2> terrainPoints = new List<Vector2>();
	/// Minimap camera
	public Camera minimap;

	void Start () {
		Regen ();
	}


	public void Regen(){
		Delete ();
		GenerateRelative ();
		Draw ();
		//Scale minimap to fit width
		minimap.orthographicSize = 5;
		minimap.transform.position = new Vector3(lastX/2+transform.position.x, 25, -20);
		minimap.orthographicSize = 100;
		print (lastX - transform.position.x);
	}
		
	/// Generates the terrain using positions relative to previous points
	void GenerateRelative(){
		lastX = 0;
		//Add some buffer segments so that camera limits cover entire terrain
		addSegment (0, 2);
		addSegment (10, 0);
		int count = 1;
		//add 109 segments
		while (count <= 109) {
			float y = 0;
			// adjust angle based on previous angle
			if (deltaY > 2 || lastY < 0) {
				y = Random.Range (0, 10) / (deltaY + 2);
			
			} else if (deltaY < -2 || lastY > 200) {
				y = Random.Range (0, 10) / (deltaY - 2);
					
			}else {
				y = Random.Range (-15, 15)*deltaY*2;
			}
			//if the new Y is infinity or NaN (caused by issues with division by 0) then generate a usable Y
			if (y == Mathf.Infinity || float.IsNaN(y)) {
				y = lastY + 1;
			}
			//Create X
			float x = Random.Range (5, 20);
			//If segment count is divisivble by 10, make slope 0 degrees
			if (count % 10 == 0 && landCounter < 0)
				y = 0;


			//If slope is 0 and there hasn't been a landing pad recently set landCounter to 3 otherwise add a hill
			if (y == lastY && landCounter < 0) {
				landCounter = 3;
			}
			else if (y == lastY && landCounter >= 0) {
				y = Random.Range (50/x, 100/x);

			}

			landCounter--;


			addSegment (x, y+lastY);

			count++;
		}
		//Set min and max X
		maxX = lastX+transform.position.x;
		minX = terrainPoints [0].x+transform.position.x;
		//Add a buffer segment
		addSegment (10, 2+lastY);
	}

	/// Adds a segment to the points
	void addSegment(float x, float y){
		//Move X to absolute position
		x += lastX;
		//Add point
		terrainPoints.Add (new Vector2 (x, y));

		//Update position variables
		deltaY = y-lastY;
		if (deltaY == 0)
			deltaY = 0.1f;
		lastX = x;
		lastY = y;
		if (y > highestY)
			highestY = y;
	}
		
	/// Convert Vector2 list to Vector3 array
	/// 
	/// Returns: Vector3 array
	Vector3[] toVector3(List<Vector2> v2){
		Vector3[] points = new Vector3[v2.Count];
		for (int i = 0; i < v2.Count; i++) {
			Vector2 oldPoint = v2 [i];
			points [i] = new Vector3 (oldPoint.x, oldPoint.y, 0);
		}
		return points;
	}

	/// Draw the line and place the colliders.
	void Draw(){
		try{
			LineRenderer line = GetComponent<LineRenderer> ();

			//Add extra hitbox before and after map
			Vector2 point = new Vector2(terrainPoints[0].x-100, terrainPoints[0].y+1);
			this.terrainPoints.Insert(0, point);			
			point = new Vector2(this.terrainPoints[0].x, this.terrainPoints[0].y+150);
			this.terrainPoints.Insert(0, point);
			point = new Vector2(this.terrainPoints[this.terrainPoints.Count-1].x+100, this.terrainPoints[this.terrainPoints.Count-1].y+1);
			this.terrainPoints.Add(point);			
			point = new Vector2(this.terrainPoints[this.terrainPoints.Count-1].x, terrainPoints[terrainPoints.Count-1].y+150);
			terrainPoints.Add(point);

			Vector3[] points = toVector3 (this.terrainPoints);

			//Set the amount of points in the line
			line.positionCount = points.Length;

			//Absolute offset of 0,0
			Vector3 offset = new Vector3 (-68.8f,-50,0);


			for (int i = 0; i < points.Length; i++) {
				//Set point on line
				line.SetPosition (i, points [i]);

				//Cant put collider before first line
				if (i > 0) {
					//Get start and end points
					Vector3 startPos = points [i - 1];
					Vector3 endPos = points [i];
					//Create the gameobject to hold the collider
					BoxCollider2D col = new GameObject ("Collider").AddComponent<BoxCollider2D> ();
					col.gameObject.tag = gameObject.tag;
					col.gameObject.layer = gameObject.layer;
					//Colldier is child of this
					col.transform.parent = this.transform;
					//Get legnth of collider
					float lineLength = Vector3.Distance (startPos, endPos);

					col.size = new Vector3 (lineLength, 1f, 1f);
					//Calculate position to place collider
					Vector3 midPoint = (startPos + endPos) / 2;
					//Place collider adjusted for offset
					col.transform.position = midPoint+offset; 

					//Calculate angle of line
					float angle = (Mathf.Abs (startPos.y - endPos.y) / Mathf.Abs (startPos.x - endPos.x));
					if ((startPos.y < endPos.y && startPos.x > endPos.x) || (endPos.y < startPos.y && endPos.x > startPos.x)) {
						angle *= -1;
					}
					angle = Mathf.Rad2Deg * Mathf.Atan (angle);
					//Rotate collider
					col.transform.Rotate (0, 0, angle);

					//Place landing pad if line is flat
					if (angle == 0) {
						col.gameObject.AddComponent<LandingPad> ();
						col.gameObject.GetComponent<LandingPad> ().CalculateScore (highestY);
						landingPads.Add(col.gameObject.GetComponent<LandingPad>());
					}
				}
			}
			//Setup the last pad
			landingPads[landingPads.Count-1].lastPad = true;
			landingPads[landingPads.Count-1].CalculateScore(highestY);
		} catch {
			print ("Regening");
			Regen ();
		}


	}

	void Delete(){

		LineRenderer line = GetComponent<LineRenderer> ();
		line.positionCount = 0;
		terrainPoints.Clear ();


		foreach (Transform child in transform) {
			Destroy (child.gameObject);
		}

		foreach (Transform child in partHolder.transform) {
			Destroy (child.gameObject);
		}
	}

}
	