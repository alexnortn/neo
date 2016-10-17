using UnityEngine;
using System.Collections;

public class ExampleClass : MonoBehaviour {
	public GameObject go;

    void Start() {
        Mesh mesh = go.GetComponent<MeshFilter>().mesh;
        Debug.Log("Submeshes: " + mesh.subMeshCount);
    }
}