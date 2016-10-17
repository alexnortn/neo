using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour {
	public float arrival_dist = 1;	// Waypoint completion [0-1]
	public bool visited = false;
	public float max_speed = 1;   		// Limit MSTY speed
	public float max_force_mov = 1;			// Movement force
	public float max_force_rot = 1;			// Rotation force
	public float wait = 0;				// Time to wait on arrival [seconds]
	public float gaze = 0;				// Time to gaze at [seconds]
	public string[] arrival_animations;    // MSTY Animation to play on arrival (during wait)
	public string[] transit_animations;	// MSTY Animation to play during transition
	public Transform lookAt;			// Object to lookAt

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
