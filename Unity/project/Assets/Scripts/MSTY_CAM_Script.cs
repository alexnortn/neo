using UnityEngine;
using UnityEngine.VR;
using System.Collections;

public class MSTY_CAM_Script : MonoBehaviour {
	public GameObject target;
	public float offset;
	public float followDist;
	public float follow_speed; // 100-200
	public float translate_speed; // 1-5
	
	// Optional Parameters
	public bool lookAt;
	public bool wander;
	public bool perlin;
	public bool gaze;
	public bool seenIt;
	public bool human;
	public bool follow;
	public bool allowHMD;
	public bool vr;
	public float vrDepthZ;
	public float wanderLust;
	public float max_force_rot;

	// Waypoints
	public Transform waypointObj;
	private Transform[] waypoints;
	
	// Co-control
	private bool MiltonRunning;
	private bool GazerRunning;

	private float speed;
	private float itr;
	private float rotator;

	// Animation
	public Animation anim;

	// Animation Controller
	Animator animator;

	// Use this for initialization
	void Start() {
		itr = 0f;
		GazerRunning = false;
		MiltonRunning = false;
		seenIt = false;

		// if (allowHMD == false) {
		// 	VRSettings.enabled = false; // Toggle VR support for DEV
		// }

	}
	
	// Update is called once per frame
	void Update() {
		// Look around naturally
		if (wander)	{
			lookAround(wanderLust);
		}

		// Look around more naturally
		if (perlin) {
			lookPerlin();
		}

		// Look around most naturally
		if (human) {
			humanLook();
		}

		// Look to Waypoints
		if (gaze && !GazerRunning) {
			lookAt = false;
			perlin = false;
			wander = false;
			human = false;

			StartCoroutine("Gazer");
			GazerRunning = true;
		} 

		// Stop Looking to Waypoints
		if (!gaze && GazerRunning || lookAt || perlin || wander || human) {
			StopCoroutine("Gazer");
			GazerRunning = false;
		}

		// Rotate the camera every frame so it keeps looking at the target 
		if (lookAt) {
			Vector3 heading = target.transform.position - transform.position;
			lookToward(heading);
		}

		if (vr) {
			transform.Translate(Vector3.forward * Time.deltaTime * translate_speed, Space.World);
			if (transform.position.z > vrDepthZ) {
				Application.LoadLevel("neo-4");

				// Vector3 reset = transform.position;
				// 		reset.z = -100;

				// transform.position = reset;
			}
		}

	}

	void FixedUpdate() {
		if (follow) {
			float distanceSq = (target.transform.position - transform.position).sqrMagnitude;
			float minDistSq = followDist*followDist;
			float maxDistSq = minDistSq * 2;

			speed = map(distanceSq, minDistSq, maxDistSq, 0, follow_speed);
			speed = Mathf.Clamp(speed, 0, follow_speed);
			float step = speed * Time.deltaTime;

			if (distanceSq > followDist * followDist) {
				Vector3 move = target.transform.position - transform.position;
						move.Normalize();
						// move *= -1;
				transform.Translate(move * step, Space.World);
			}
		}
	}

	void lookAround(float theta) {
		itr += 0.01f;
		rotator = Mathf.Sin(itr) * theta;
		this.gameObject.transform.localEulerAngles = new Vector3(rotator, rotator, 0f);
	}

	Quaternion lookAround2(float theta) {
		itr += 0.01f;
		rotator = Mathf.Sin(itr) * theta;

		Quaternion rot = Quaternion.Euler(rotator, rotator, 0f);
		
		return rot;
	}

	void lookPerlin(float step=0.01f, float theta=15f) {
		float offset_x = theta * Mathf.PerlinNoise(Time.time * step, 0.0f);
		float offset_y = theta * Mathf.PerlinNoise(0.0f, Time.time * step);

		float x = Mathf.Sin(offset_x) * theta;
		float y = Mathf.Cos(offset_y) * theta;

		Vector3 perlinVect = new Vector3(x, y, 0f);
		
		lookToward(perlinVect);	
	}

	Quaternion lookPerlin2(float step=0.01f, float theta=15f) {
		float offset_x = theta * Mathf.PerlinNoise(Time.time * step, 0.0f);
		float offset_y = theta * Mathf.PerlinNoise(0.0f, Time.time * step);

		float x = Mathf.Cos(offset_x) * theta;
		float y = Mathf.Sin(offset_y) * theta;

		Quaternion rot = Quaternion.Euler(x, y, 0f);

		return rot;
	}

	// Combine Perlin + Look Rotations
	void humanLook() {
		Quaternion lookRot = lookAround2(15f);
		Quaternion perlinRot = lookPerlin2();

		Quaternion lurk = Quaternion.Slerp(lookRot, perlinRot, 0.8f);

		lookToward2(lurk);	
	}

	// Rotate in direction of heading at constant rate
	void lookToward(Vector3 heading) {
		if (heading != Vector3.zero) {
			transform.rotation = Quaternion.Slerp(
			transform.rotation,
			Quaternion.LookRotation(heading.normalized),
			Time.deltaTime * max_force_rot
			);
		}
	}

		// Rotate in direction of heading at constant rate
	void lookToward2(Quaternion heading) {

		float step = 5 * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, heading, step);

	}

	// MSTY Navigation System
	IEnumerator Gazer() {
		int i = 0;
		int i0 = 0;

		waypoints = new Transform[waypointObj.transform.childCount];
		Waypoint waypoint;
		
		foreach (Transform t in waypointObj.transform) {
            waypoints[i0++] = t;
        }

		for(;;) {

			waypoint = waypoints[i].gameObject.GetComponent<Waypoint>();

			if (MiltonRunning) {
				Debug.Log("Shut er down");
				StopCoroutine("MiltonGazer");
				MiltonRunning = false;
			}
			
			if (waypoint.gaze > 0f && !seenIt)   {
					Debug.Log("Start er Up");
					StartCoroutine("MiltonGazer", waypoint.transform.position);
					MiltonRunning = true;

					seenIt = true;
				
				int secToFrame = (int) (waypoint.gaze * 60f);
				yield return WaitFor.Frames(secToFrame);
			}

			if (waypoint.visited) {
				if (i < waypoints.Length - 1) {
					i++;
				}
				else {
					i = 0;
				}

				seenIt = false;

			}
	
			yield return WaitFor.Frames(1);

		}
	}

	IEnumerator MiltonGazer(Vector3 target) {
		for(;;) {
			Vector3 heading = target - transform.position;
			Quaternion rot = Quaternion.Euler(heading.x, heading.y, 0f);
			lookToward2(rot);
			Debug.Log("MiltonGazer");	
			yield return WaitFor.Frames(1);
		}
	}

	// Distance Check
	IEnumerator DC() {
		for(;;) {
			// move();
			yield return WaitFor.Frames(5);
		}
	}

	// void move() {
		// Vector3 dist = target.transform.position - transform.position;
		// distance = dist.magnitude;

		// follow = distance > followDist
		// 	? true
		// 	: false;

	// }

	//----------- Utilities
	float map(float n, float start1, float stop1, float start2, float stop2) {
    	return ((n-start1)/(stop1-start1))*(stop2-start2)+start2;
    }

}
