using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotPath : MonoBehaviour {
	//Line Renderer Holder
	LineRenderer lineRenderer;

	//Update Call
	public float refreshRate = 0.1f;
	public float segmentLength = 0.01f;
	private float timestamp = 0;

	// Start is called before the first frame update
	void Start() {
		lineRenderer = gameObject.GetComponent<LineRenderer>();

		//Init with 2 points, first is ankor/fixed and second is current point
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, transform.position);
		lineRenderer.SetPosition(1, transform.position);
	}

	// Update is called once per frame
	void Update() {
		if ((timestamp + refreshRate) < Time.time) {
			//Check distance between last and current point
			float distance = Vector3.Distance(lineRenderer.GetPosition((lineRenderer.positionCount - 2)), lineRenderer.GetPosition((lineRenderer.positionCount - 1)));
			if(distance >= segmentLength) {
				//Add new point
				Vector3 newPoint = transform.position;
				lineRenderer.positionCount += 1;
				lineRenderer.SetPosition((lineRenderer.positionCount - 1), newPoint);
			}
			else {
				//Just update position of current point
				lineRenderer.SetPosition((lineRenderer.positionCount - 1), transform.position);
			}

			timestamp = Time.time;
		}
	}
}
