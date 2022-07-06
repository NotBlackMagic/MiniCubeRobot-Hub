using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	//Camera Settings
	float maxZoom = 5.0f;
	float minZoom = 0.25f;
	float zoomStep = 0.25f;

	Camera mainCamera;

    //Start is called before the first frame update
    void Start() {
		//Limit frame rate
		Application.targetFrameRate = 60;

		//Get main camera
		mainCamera = Camera.main;
	}

    //Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.KeypadMinus)) {
			//Zoom out
			if(mainCamera.orthographicSize <= (maxZoom - zoomStep)) {
				mainCamera.orthographicSize += zoomStep;
			}
		}
		if(Input.GetKeyDown(KeyCode.KeypadPlus)) {
			//Zoom in
			if(mainCamera.orthographicSize >= (minZoom + zoomStep)) {
				mainCamera.orthographicSize -= zoomStep;
			}
		}
    }
}
