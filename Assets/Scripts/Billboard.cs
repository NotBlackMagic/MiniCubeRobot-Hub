using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {

	public bool lockXZ = false;
	public Camera mainCamera;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
		if (lockXZ == true) {
			//Locked X and Z Axis, only rotate around Z-Axis
			Vector3 targetPosition = new Vector3(-mainCamera.transform.position.x, this.transform.position.y, -mainCamera.transform.position.z);
			transform.LookAt(targetPosition);
		}
		else {
			//Rotate around all azis
			transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
		}
	}
}
