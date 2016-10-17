/* *
 * 
 * The GameManager kicks off all of our game logic.  In this case, there isn't much logic.
 * We simply instantiate a few PunyPlane objects and place them in the heirarchy.
 * Assuming that this code has been added as a component to one of the GameObjects in the
 * Heirarchy, this should take effect when you press "Play" or when you view the compiled
 * application in the Unity player.
 * 
 * */
 
using UnityEngine;
using System.Collections;
 
public class GameManager : MonoBehaviour {
	public float fogDens;
 
 
 
	void Start () {

        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0.05f;

		StartCoroutine("FadeFog");
	}

	void Update() {
		if (Input.GetKeyDown("space")) {
      		Application.LoadLevel("neo-4");
		}
	}

	IEnumerator FadeFog() {
		for (float f = 0.1f; f >= 0.0075; f -= 0.00015f) {
			fogDens = f;
			RenderSettings.fogDensity = f;
			yield return null;
		}
	}

 
 
}