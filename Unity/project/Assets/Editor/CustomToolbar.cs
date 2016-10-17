/* *
 * CustomToolBar is used to add some new functionality to the Unity3D Editor.  
 * In this case we're going to add some menu items to the top level toolbar.
 * */
 
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Linq;
 
public class CustomToolbar : MonoBehaviour {
 
 
 
 
	/* *
	 * 
	 * The following two methods are added to the Unity default toolbar,
	 * as sub-menus under the GameObject dropdown.  It should appear at 
	 * the very bottom of the GameObject dropdown list.
	 * 
	 * GameObject ->
	 * 		Primitives (We made this one) ->
	 * 			Create XXX
	 * 
	 * */
	[MenuItem ("GameObject/Primitives/Create PunyPlane in Heirarchy")]
	public static void MakePunyPlane(){
 
		// This function simply creates a PunyPlane in the top level of the
		// Heirarchy.
		PunyPlane.Create();
 
	}
 
	[MenuItem ("GameObject/Primitives/Create PunyPlane As Child of Selection")]
	public static void MakePunyPlaneAsChildOfSelection(){
 
		// We start by creating a PunyPlane in the top level of the 
		// Heirarchy.
		PunyPlane pp = PunyPlane.Create();
 
		// And if we've got a GameObject currently selected in the 
		// heirarchy, we'll reparent the PunyPlane to our selected
		// object.
		if(Selection.activeGameObject){
			pp.gameObject.transform.parent = Selection.activeGameObject.transform;
		}
	}

	//  Searching.. Seek and Destroy!
	// Recursively delete all files + subdirectories within target directory
	public static void DeleteDirectory(string target_dir) {
	    string[] files = Directory.GetFiles(target_dir);
	    string[] dirs = Directory.GetDirectories(target_dir);

	    foreach (string file in files)
	    {
	        File.SetAttributes(file, FileAttributes.Normal);
	        File.Delete(file);
	    }

	    foreach (string dir in dirs)
	    {
	        DeleteDirectory(dir);
	    }

	    Directory.Delete(target_dir, false);
	}

	/* *
	 * Set material for model + submeshes 
	 * */
	[MenuItem ("GameObject/Materials/ModelSet")]
		public static void ModelSet(){

			string base_path = "Assets/Resources/Meshes/";
			string end_path = "Assets/Resources/Prefabs/pinky2/";
			string[] subdirectoryEntries = Directory.GetDirectories(base_path);

			if (Directory.Exists(end_path)) {
				DeleteDirectory(end_path);
				Debug.Log("Destroy");
			}

			Directory.CreateDirectory(end_path);	

			// Walk through mesh directory structure
			for(int i = 0; i < subdirectoryEntries.Length; i++) {
				string newDir = end_path + new DirectoryInfo(subdirectoryEntries[i]).Name;
				Directory.CreateDirectory(newDir); // Create our new folder structure;

				// Iterate through all meshes in directory
				DirectoryInfo dir = new DirectoryInfo(subdirectoryEntries[i]);
				FileInfo[] info = dir.GetFiles("*.obj");
				
				foreach (FileInfo file in info) {
					string path = subdirectoryEntries[i] + "/" + Path.GetFileName(file.ToString());
					// Get + Instantiate mesh
					Object dendrite = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
					GameObject clone = Instantiate(dendrite) as GameObject;

					// Trim off (clone) from end of go
					clone.transform.name = clone.transform.name.Remove(clone.transform.name.Length-7);
					clone.tag = "Dendrite";

					// Get + Instantiate mesh-collider
					GameObject[] colliders = GameObject.FindGameObjectsWithTag("Dendrite_Collider");

					// Find matching name
					foreach(GameObject collider in colliders) {
						if (collider.transform.name == clone.transform.name) {
							clone.AddComponent<MeshCollider>();
							clone.GetComponent<MeshCollider>().sharedMesh = collider.GetComponentInChildren<MeshFilter>().sharedMesh;
							break;
						}
					}

					// Define Material(s) for dendrites
					// Material newMat = Resources.Load("Materials/Dendrite", typeof(Material)) as Material;					
					Material newMat = new Material (Shader.Find("Standard"));
					Directory.CreateDirectory(newDir + "/Materials/");
	     			AssetDatabase.CreateAsset(newMat, newDir + "/Materials/" + clone.transform.name + "_dendrite.mat");

					Color meshColor = Color.white;
						  meshColor = Color.HSVToRGB(0.0f,0.0f,Random.Range(0.35f, 1f));

         			newMat.SetColor("_Color", meshColor);

	     			// Material newMat = (Material)Resources.Load("MaterialName", typeof(Material));

					Renderer rend;

					// Debug.Log(clone.transform.GetChild(0).transform.childCount);
					clone.transform.localScale = new Vector3(0.01f,0.01f,0.01f); 

					if (clone.transform.GetChild(0).transform.GetComponent<Renderer>() != null) {
						rend = clone.transform.GetChild(0).transform.GetComponent<Renderer>();
						if (rend != null) {
						     rend.material = newMat;
						}
					}
					else {
						foreach (Transform child in clone.transform.GetChild(0).transform) {
							rend = child.GetComponent<Renderer>();

							if (rend != null) {
							     rend.material = newMat;
							}
						}
					}

					Object prefab = PrefabUtility.CreateEmptyPrefab(newDir + "/" + clone.transform.name + ".prefab");
		    		PrefabUtility.ReplacePrefab(clone, prefab, ReplacePrefabOptions.ConnectToPrefab);

					// Remove gameObject from scene
					Object.DestroyImmediate(clone);
				}
			}
		}

	[MenuItem ("GameObject/Materials/MaterialSet")]
		public static void MaterialSet(){
			Color meshColor,
				  meshColor2;
			float h,s,b,bb;
			foreach (Material mat in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[]) {
				if (!mat.name.Contains("_dendrite")) {
					Debug.Log("NOOOO " + mat.name);
				}
				else {
					Debug.Log(mat.name);

					h = Random.Range(0.6f, 0.7f);
					s = Random.Range(0.5f, 0.85f);
					b = Random.Range(0.7f, 0.95f);
					bb = Random.Range(0.0f, 2.0f);

					meshColor = Color.HSVToRGB(h,s,b);
					meshColor2 = Color.HSVToRGB(h,s,bb);

					mat.SetFloat("_Shininess", 0.5f);
					mat.SetFloat("_Metallic", 0.01f);
	         		mat.SetColor("_Color", meshColor);
	         		mat.SetColor("_EmissionColor", meshColor2);
	         	}
			}

		}

	[MenuItem ("GameObject/Positioning/RandomSet")]
		public static void RandomSet(){
			GameObject[] dendrites_high;
			GameObject[] dendrites_low;

			dendrites_high = GameObject.FindGameObjectsWithTag("Dendrite_High");
			dendrites_low = GameObject.FindGameObjectsWithTag("Dendrite_Low");

			Vector3 pos = new Vector3();
			float offset_high = 100f;
			float offset_low = 250f;

			for (int i = 0; i < dendrites_high.Length; i++) {
				GameObject dendrite = dendrites_high[i];

				offset_high = Random.value > 0.5 ? offset_high * -1 : offset_high;

				pos.x = Random.Range(-75, 75) + offset_high;
				pos.y = Random.Range(-50, 50);
				pos.z = 25f * i + Random.Range(-25, 25);

				dendrite.transform.localPosition = pos;
			}

			for (int i = 0; i < dendrites_low.Length; i++) {
				GameObject dendrite = dendrites_low[i];

				offset_low = Random.value > 0.5 ? offset_low * -1 : offset_low;

				pos.x = Random.Range(-250, 250) + offset_low;
				pos.y = Random.Range(-50, 50);
				pos.z = 25f * i + Random.Range(-50, 50);

				dendrite.transform.localPosition = pos;
			}
		}
 
 
	/* *
	 * And finally, we create a brand new menu item in the Unity editor.
	 * This will actually appear as a new top-level item called "Scriptocalypse"
	 * with a command "Say Hello".  The "Scriptocalypse" item should appear 
	 * between "Terrain" and "Window".
	 * 
	 *   Scriptocalypse ->
	 * 		Say Hello
	 * 
	 * */
	[MenuItem ("Scriptocalypse/Say Hello")]
	public static void SayHello(){
		Debug.Log("Hello from the Unity3D toolbar.");	
	}
 
 
 
 
}