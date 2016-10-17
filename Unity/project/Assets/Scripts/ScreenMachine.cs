using UnityEngine;
using System.Collections;
using System;

// Capture frames as a screenshot sequence. Images are
// stored as PNG files in a folder - these can be combined into
// a movie using image utility software (eg, QuickTime Pro).

public class ScreenMachine : MonoBehaviour {
	// The folder to contain our screenshots.
	// If the folder exists we will append numbers to create an empty folder.
    public string folder = "ScreenshotFolder";
    public int frameRate = 60;
    public int resolution = 1;
    public int quitAfterSec = 60;
    public bool capture = false;

    private void Start() {
        // Create the folder
        if (capture) {
        	// Set the playback framerate (real time will not relate to game time after this).
	        Time.captureFramerate = frameRate;
	        folder += "_" + System.DateTime.Now.ToString("yyyy.MM.dd.HH.mm");

        	System.IO.Directory.CreateDirectory(folder);
        }
    }
    
    
    private void Update() {        
	    // Capture the screenshot to the specified file.
	    if (capture) {
	    	// Append filename to folder name (format is '0005 shot.png"')
        	string name = string.Format("{0}/{1:D04} shot.png", folder, Time.frameCount);

        	Application.CaptureScreenshot(name, resolution);
        }

        // int framesStop = frameRate * quitAfterSec;

        // if (Time.frameCount == framesStop && UnityEditor.EditorApplication.isPlaying) {
        // 	UnityEditor.EditorApplication.isPlaying = false;
        // }
    }
}