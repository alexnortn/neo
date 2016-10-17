using UnityEngine;
using System.Collections;

using UnityStandardAssets.ImageEffects;

public class DynamicDOF : MonoBehaviour {

	public Transform DOF_Controller; 
	public float focusRate;
	public float normalFocusDist;
	public float aperature_min;
	public float aperature_norm;

	private RaycastHit focalHit; //Focal distance
	private Vector3 updatePos;

	// Use this for initialization
	void Start () {
		StartCoroutine("SetFocus");
    }

	void FixedUpdate() {
		Debug.DrawRay(transform.position, transform.forward * focalHit.distance, Color.green); // Mark current DOF
	}

	IEnumerator SetFocus() {
		for(;;) {
			if (Physics.Raycast(transform.position, transform.forward, out focalHit, 250f)) {		
					updatePos = Vector3.Lerp(DOF_Controller.position, focalHit.point, Time.deltaTime * focusRate);
					DOF_Controller.position = updatePos;
					yield return WaitFor.Frames(1);
				}
				else {
					updatePos = Vector3.Lerp(DOF_Controller.position, (transform.forward * normalFocusDist), Time.deltaTime * focusRate); // Shift Focal Distance to something normal
					DOF_Controller.position = updatePos;
					yield return WaitFor.Frames(5);
				}
			}
		}
}