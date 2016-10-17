using UnityEngine;
using System.Collections;

public class DebugVertNorm : MonoBehaviour {
	private Mesh mesh;
 	public float normalLength = 1;

	// Use this for initialization
	void Start () {
		mesh = GetComponent<MeshFilter>().mesh;
	}
	
	// Update is called once per frame
	void Update () {
		for (int i=0; i<mesh.vertices.Length; i++){
	         Vector3 norm = transform.TransformDirection(mesh.normals[i]);
	         Vector3 vert = transform.TransformPoint(mesh.vertices[i]);
	         Debug.DrawRay(vert, norm * normalLength, Color.red);
	     }
	}
}
