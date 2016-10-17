using UnityEngine;
using System.Collections;

public class MSTY : MonoBehaviour {
	public Transform waypointObj;
	public Transform cameraTransform;
	public float rayLength;
	public float rayLength2;
	
	private Transform[] waypoints;
	private float itr;
	private float rotator;

	// Wander Variables
	public float delta;
	public float radius;
	public float projection;
	public float wander_lust;
	public float damping;
	
	private float elevation;
	private float polar;
	private int waypoint_index;

	// Locomotion Globals
	private float arrival_dist;
	private float max_speed;
	private float max_force_mov;
	private float max_force_rot;
	private float wait;
	private string[] arrival_animations;
	private string[] transit_animations;
	private Transform lookAt;
	private Vector3 acceleration;
	private Vector3 velocity;

	// Navigator Globals
	public int navigator_offset; // 10-30 degrees feels right

	// Animation Class
	public class AnimState {
		public string name;
		public bool hasPlayed;
		
		public AnimState(string _name) {
			name = _name;
			hasPlayed = false;
		}
	}

	// Animation
	Animator animator;
	AnimState[] animArriveStates;
	AnimState[] animTransitStates;
	string[] animator_bools;

	// Coroutines
	// keep a copy of the executing script
    private IEnumerator Co_Transmission;

	// Use this for initialization
	void Start () {
		itr = 0f;

		elevation = 0f;
		polar = 0f;

		waypoint_index = 1; // Start on item 2

		delta = 0.3f;
		radius = 35f;
		projection = 75f;
		max_speed = 1.5f;
		max_force_rot = 1f;
		max_force_mov = 0.05f;
		wander_lust = 0.75f;
		damping = 0.98f;

		acceleration = new Vector3();
		velocity = new Vector3();

		// Setup Animator
		animator = transform.GetChild(0).GetComponent<Animator>();
		animator_bools = new string[9];
		setConditions();
		
		int i0 = 0;
		waypoints = new Transform[waypointObj.transform.childCount];
		foreach (Transform t in waypointObj.transform) {
            waypoints[i0++] = t;
        }

        parseWaypoint(waypoints[0]);

        // Set MSTY position to first waypoint
        Vector3 startingPos = waypoints[0].transform.position;
        transform.position = startingPos;

        parseWaypoint(waypoints[1]);

        StartCoroutine("MPS"); // Start the Object Avoidance System
        StartCoroutine("Navigator"); // Start the Navigation System

        Co_Transmission = Transmission(animator, animator_bools, animTransitStates);
        StartCoroutine(Co_Transmission); // Start off In-Transit

        // GetAnimationClip();
	}
	
	// Update is called once per frame
	void Update () {

	}

	void FixedUpdate() {
		// navigate();
		animate();
		// Update physics + object position
		updatePhys(max_speed, max_force_rot);

		// JFF Face User if MSTY is Idle
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // Get info about current animation
		if (stateInfo.IsName("Idle")){
			// lookToward(cameraTransform.position - transform.position); // Rotate towards velocity vector;
		}
	}

	// Setup Animator transition condition bools
	void setConditions() {
		// Limit to [ 9 ]
		animator_bools[0] = "Awesome";
		animator_bools[1] = "Curious";
		animator_bools[2] = "Surprise";
		animator_bools[3] = "Awed";
		animator_bools[4] = "Laser";
		animator_bools[5] = "Wave_Frantic";
		animator_bools[6] = "Wave_Friendly";
		animator_bools[7] = "Wave_Way";
		animator_bools[8] = "Fly";
	}

	// Raycasting Pathfinding System
	void seekAndDestroy() {
		RaycastHit hit;
		Vector3 reflectVec = new Vector3();

		// Frontal Array
		for (int i = -1; i < 2; i++) {
			for (int j = -1; j < 2; j++) {
				int x = navigator_offset * i;
				int y = navigator_offset * j;
				Vector3 vec = transform.forward;
						vec = Quaternion.AngleAxis(x, Vector3.right) * vec;
						vec = Quaternion.AngleAxis(y, Vector3.up) * vec;
						vec.Normalize();
				Ray eyeRay = new Ray(transform.position, vec);
				Debug.DrawRay(transform.position, vec * rayLength2, Color.yellow);

				if (Physics.Raycast(eyeRay, out hit, rayLength2)) {
					reflectVec = Vector3.Reflect(transform.forward, hit.normal);
					Debug.DrawRay(hit.point, reflectVec * rayLength2, Color.green);

					applyForce(seek(reflectVec, max_speed, max_force_mov));
				}
			}
		}

		Vector3[] cardinal_vectors;
			  	 cardinal_vectors = new Vector3[6];
			  	 cardinal_vectors[0] = transform.forward; 	// Forward
			  	 cardinal_vectors[1] = -transform.up; 		// Down
			  	 cardinal_vectors[2] = transform.up; 		// Up
			  	 cardinal_vectors[3] = -transform.right; 	// Left
			  	 cardinal_vectors[4] = transform.right; 	// Right
			  	 cardinal_vectors[5] = -transform.forward; 	// Back

		float cardinalLength;			  	 
		//  Forward cardinal Array
		for (int i = 0; i < cardinal_vectors.Length; i++) {
			cardinalLength = i == 0
				? rayLength
				: rayLength2 / 2;

			Vector3 vec = cardinal_vectors[i];
			Ray lookRay = new Ray(transform.position, vec);
			Debug.DrawRay(transform.position, vec * cardinalLength, Color.blue);

			if (Physics.Raycast(lookRay, out hit, cardinalLength)) {
				reflectVec = Vector3.Reflect(vec, hit.normal);
				Debug.DrawRay(hit.point, reflectVec * cardinalLength, Color.green);

				applyForce(seek(reflectVec, max_speed, max_force_mov));
			}
		}			  	
	}

	void animate() {
		float speed = velocity.magnitude;
		animator.SetFloat("MST", speed);
	}

	// MSTY Animations Switching System
	IEnumerator Transmission(Animator _animator, string[] anim_bools, AnimState[] _animStates, bool arrived=false) {
		foreach(string other in anim_bools) {
			_animator.SetBool(other, false); // Disable all other animations
		}

		if (_animStates.Length == 0) {
			yield return null;
		}

		foreach(AnimState anim in _animStates) {
			if (anim.hasPlayed) {
				yield return null;
			}

			_animator.SetBool(anim.name, true); // Activate new animation state
			anim.hasPlayed = true;
			
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // Get info about current animation
			int waitFrames = (int) (stateInfo.length * 60); // 60FPS

			Debug.Log("Animation name " + anim.name + " Length:" + stateInfo.length);

			if (arrived) {
				Debug.Log("Updating waitTime:" + waitFrames);
				wait += stateInfo.length;
			}
			
			yield return WaitFor.Frames(waitFrames);
		}
	}

	// MSTY Positioning System
	IEnumerator MPS() {
		for(;;) {
			seekAndDestroy();
			yield return WaitFor.Frames(1);
		}
	}

	void navigate(out bool arrived) {

		arrived = false;
		arrive(waypoints[waypoint_index].position, max_speed, out arrived, max_force_mov, arrival_dist);

		if (arrived) {
        		
        		StopCoroutine(Co_Transmission);  // Stop the Transit-Coroutine
        	Co_Transmission = Transmission(animator, animator_bools, animArriveStates, arrived);
        		StartCoroutine(Co_Transmission); // Start the Arrive-Coroutine

		}
		
		// Extra forces
		Vector3 wanderVec = wander() * wander_lust;
		applyForce(wanderVec);
	}

	// MSTY Navigation System
	IEnumerator Navigator() {
		bool arrived = false;

		for(;;) {

			if (arrived) {

				if (wait > 0) {
					Debug.Log("Dallas Green");
					int secToFrame = (int) (wait * 60f);
					yield return WaitFor.Frames(secToFrame);
				} 

				Waypoint waypoint_prop = waypoints[waypoint_index].gameObject.GetComponent<Waypoint>();
						 waypoint_prop.visited = true;

				Debug.Log("Arrived");

	        	if (waypoint_index < waypoints.Length - 1) {
					waypoint_index++;
					parseWaypoint(waypoints[waypoint_index]);
				} 
				else {
			        waypoint_index = 0; // Loop
			        Debug.Log("Loop");

			        foreach(Transform waypoint in waypoints) {
			        	Waypoint waypoint_prop_reset = waypoint.gameObject.GetComponent<Waypoint>();
			        			 waypoint_prop_reset.visited = false;
			        }
				}

					StopCoroutine(Co_Transmission);  // Stop the Arrive-Coroutine
					Debug.Log("AnimTransit:" + animTransitStates);
	        	Co_Transmission = Transmission(animator, animator_bools, animTransitStates);
	        		StartCoroutine(Co_Transmission); // Start the Transit-Coroutine

	        	// waypoint_index = (int) Random.Range(0, (waypoints.Length - 1));
	        	// parseWaypoint(waypoints[waypoint_index]); // Random Index

			}

			navigate(out arrived);
			yield return WaitFor.Frames(1);
				
		}
	}

	void parseWaypoint(Transform waypoint) {
		Waypoint waypoint_prop = waypoint.gameObject.GetComponent<Waypoint>();
				 arrival_dist = waypoint_prop.arrival_dist;
				 max_speed = waypoint_prop.max_speed;
				 max_force_mov = waypoint_prop.max_force_mov;
				 max_force_rot = waypoint_prop.max_force_rot;
				 wait += waypoint_prop.wait; // You were killing the wait Variable
				 arrival_animations = waypoint_prop.arrival_animations;
				 transit_animations = waypoint_prop.transit_animations;
				 lookAt = waypoint_prop.lookAt;

		animArriveStates = new AnimState[arrival_animations.Length];
		animTransitStates = new AnimState[transit_animations.Length];

		parseAnimations(arrival_animations, animArriveStates);
		parseAnimations(transit_animations, animTransitStates);
	}

	// Create animation structs
	void parseAnimations(string[] animations, AnimState[] _animStates) {
		for(int i = 0; i < animations.Length; i++) {
			_animStates[i] = new AnimState(animations[i]);
		}
	}

	void GetAnimationClip() {
		if (!animator) return; // no animator

			foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips) {
				Debug.Log(clip);
				// if (clip.name == name) {
				// 	return clip;
				// }
			}
		return; // no clip by that name
	}

	void lookAround() {
		itr += 0.025f;
		rotator = Mathf.Sin(itr) * 15f;
		transform.localEulerAngles = new Vector3(rotator, rotator, 0f);
	}

	bool ProximityCheck() {
		// for (int i = 0; i < enemies.Length; i++) {
		// 	if (Vector3.Distance(transform.position, enemies[i].transform.position) < dangerDistance) {
		// 		return true;
		// 	}
		// }

		return false;
	}

	
	//----------- Locomotion

	Vector3 wander() {
		elevation += Random.Range(-delta, delta); 	// Increment elevation offset
		polar += Random.Range(-delta, delta);		// Increment polar offset

		Vector3 sphericalProjection = new Vector3();		
		
		SphericalCoordinates.SphericalToCartesian(radius, polar, elevation, out sphericalProjection); // Calculate spherical coordinates

		Vector3 target = transform.position + transform.forward * projection + sphericalProjection; // Project target in front of object

		// debugWander(target, radius);

		return seek(target, max_speed, max_force_mov);
	}

	void debugWander(Vector3 target, float radius) {
		// Wander Sphere
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position, radius);

		// Wander Target
		Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(target, 3);

        // Wander Target Line
        Debug.DrawLine(transform.position, target, Color.green);

        // Wander Line
        Debug.DrawLine(transform.position, transform.position + transform.forward * radius, Color.green);
	}

	void applyForce(Vector3 force) {
		acceleration = acceleration + force;
	}

	void lookToward(Vector3 heading) {
		if (heading != Vector3.zero) {
			transform.rotation = Quaternion.Slerp(
			transform.rotation,
			Quaternion.LookRotation(heading.normalized),
			Time.deltaTime * max_force_rot
			);
		}
	}

	void updatePhys(float max_speed, float max_force_rot) {
		velocity = velocity + acceleration;

		lookToward(velocity); // Rotate towards velocity vector;

        velocity = Vector3.ClampMagnitude(velocity, max_speed);
        velocity = velocity * damping;

		Vector3 pos = transform.position + velocity;
		transform.position = pos; // Update object position

		// transform.Translate(transform.Forward + forward);

		acceleration = acceleration * 0; // Reset acceleration
	}

	void arrive(Vector3 target, float max_speed, out bool arrived, float max_force_mov, float distanceFrom) {
		Vector3 force = target - transform.position; // Vector pointing from object to target
		float distance = force.magnitude;
				force.Normalize();

		if (distance < distanceFrom) {
			if (distance < 35f) {
				arrived = true;
				return;
			}

			force = force * map(distance, 0f, distanceFrom, 0f, max_speed);
			
		}
		else {
			force = force * max_speed;
		}

		force = force - velocity;
		force = Vector3.ClampMagnitude(force, max_force_mov);

		applyForce(force);

		// Debug Target
		Debug.DrawLine(transform.position, target, Color.cyan);


		arrived = false;
	}

	Vector3 seek(Vector3 target, float max_speed, float max_force_mov) {
		Vector3 force = target - transform.position; // Vector pointing from object to target
				force.Normalize();
				force = force * max_speed; // Limit force
				force = force - velocity; // Steering = Desired - Velocity
				force = Vector3.ClampMagnitude(force, max_force_mov);

		return  force;
	}

	//----------- Utilities

	Vector3 limit(Vector3 vec, float limit_val) {
		float magSq = vec.sqrMagnitude;
		if (magSq > limit_val * limit_val) {
			vec.Normalize();
			vec = vec * limit_val;
		}
        return vec;
    }

    float map(float n, float start1, float stop1, float start2, float stop2) {
    	return ((n-start1)/(stop1-start1))*(stop2-start2)+start2;
    }


	//----------- Waypoint Iterator

	// MPS


	//----------- Animation
	




}
