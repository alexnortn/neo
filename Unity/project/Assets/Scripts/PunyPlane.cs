/* *
 * 
 *  This class creates a small, 4 vertex plane.  
 *  It's provided purely for instructional purposes,
 *  as an example of how one can instantiate custom 
 *  GameObjects with graphical representations.
 * 
 *  It is not necessary to understand how this code
 *  works, but feel free to study it if you wish
 *  to get a small taste of how polygons and planes
 *  are created procedurally in Unity.
 * 
 * */
 
 
 
using UnityEngine;
using System.Collections;
 
public class PunyPlane : MonoBehaviour {
 
	public static Material sharedMaterial;
 
	/* *
	 * Call Create without parameters to return a PunyPlane of 1 x 1 world units.
	 * */
	public static PunyPlane Create() {
		return Create(1.0f,1.0f);
	}
 
 
	/* *
	 * Call Create with a height and width expressed in world units.
	 * */
	public static PunyPlane Create(float width, float height) {
 
		/* *
		 * We start by creating a GameObject to represent our plane,
		 * giving it the requisite components in order to accomplish
		 * this goal, a <MeshFilter> and a <MeshRenderer>.
		 * */
		GameObject go = new GameObject();
				   go.name = "PunyPlane";

		MeshFilter mf = go.AddComponent<MeshFilter>();
		MeshRenderer mr = go.AddComponent<MeshRenderer>();
 
		/* *
		 * Now it's time to create our PunyPlane component.  
		 * */
		PunyPlane pp;		
		pp = go.AddComponent<PunyPlane>() as PunyPlane;
 
		if (mf.sharedMesh == null) {
			mf.sharedMesh = new Mesh();
		}
 
		Mesh mesh = mf.sharedMesh;
 
 
		/*	
			Here is a diagram of our plane,
			with the verts labeled. 
		 0    3
		  ----
		  | /|
		  |/ |
		  ---- 
		 1    2		 
 
		 */
 
		Vector3 p0 = new Vector3(-width * 0.5f, -height * 0.5f, 0);
		Vector3 p1 = new Vector3(-width * 0.5f, height * 0.5f, 0);
		Vector3 p2 = new Vector3(width * 0.5f, height * 0.5f, 0);
		Vector3 p3 = new Vector3(width * 0.5f, -height * 0.5f, 0);
 
		/* *
		 * Make sure the Mesh is cleared of all old data, then 
		 * apply the new verts and triangles in order to form
		 * a square plane.
		 * */
		mesh.Clear();
		mesh.vertices = new Vector3[]{p0,p1,p2,p3};
		mesh.triangles = new int[]{
			0,1,3,
			3,1,2			
		};
 
		/* *
		 * And we'll want to set up the uv coordinates to match
		 * the verts listed above.  This is so the plane can wear
		 * a texture without it appearing mangled, flipped, or 
		 * inverted.
		 * */
		mesh.uv = new Vector2[]{
			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(1,0)
		};

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();
 
 
		/* *
		 * Using "Unlit/Texture" because of the assumption that 
		 * this will be a flat, 2D sprite and will not need to 
		 * be affected by things like lighting.  This will make
		 * the plane render more efficiently.
		 * */
		if(sharedMaterial == null)sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
		mr.sharedMaterial = sharedMaterial;
		mr.sharedMaterial.shader = Shader.Find("Unlit/Texture");
 
		return pp;
	}
 }
 