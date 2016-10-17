using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	private float itr;
	private float rotator;

	// Use this for initialization
	void Start () {
		itr = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		itr += 0.01f;
		rotator = Mathf.Sin(itr) * 10f;
		this.gameObject.transform.localEulerAngles = new Vector3(rotator, rotator, 0f);
	}
}
