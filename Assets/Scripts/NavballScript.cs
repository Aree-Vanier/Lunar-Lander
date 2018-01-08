using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavballScript : MonoBehaviour {

	/// The roll indicator (v thing)
	public GameObject rollIndicator;
	/// The prograde indicator (Arrow)
	public GameObject progradeIndicator;

	/// Angle of roll indicator
	public float roll;
	/// Angle of pitch indicator
	public float prograde;

	void Update () {
		rollIndicator.transform.eulerAngles = new Vector3 (0, 0, roll);
		if(!float.IsNaN(prograde))
			progradeIndicator.transform.eulerAngles = new Vector3 (0, 0, prograde);
	}

	///Set roll and pitch indicator positions
	public void SetData(float roll, float prograde){
		this.roll = roll;
		this.prograde = prograde;
	}
}
