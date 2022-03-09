using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RobotLaserScan : MonoBehaviour {
	//Configurations
	public bool showLaserScanLines = true;
	public bool showLaserScanPointCloud = true;
	public Vector2 laserScanStart = new Vector2(-177.1875f, -19.6875f);
	public Vector2 laserScanStep = new Vector2(5.625f, 5.625f);
	public Vector2 laserScanMatrix = new Vector2(64, 8);

	public Color laserLineColor = new Color(0.0f,0.0f,1.0f);
	public float laserLineWidth = 0.005f;
	public Material laserLineMaterial;
	public float laserScanMinRange = 0.01f;
	public float laserScanMaxRange = 4.0f;

	public Color pointCloudColor = new Color(1.0f, 0.0f, 0.0f);
	public float pointCloudScale = 0.01f;

	//Local Laser Scan Variables
	bool laserScanUpdate = false;
	float[][] laserScanRanges;
	GameObject[][] laserScanLines;

	//Vector3[] pointCloud;
	ParticleSystem particleSystem;
	ParticleSystem.Particle[] pointCloud;

	// Start is called before the first frame update
	void Start() {
		//Init ranges variable
		laserScanRanges = new float[(int)laserScanMatrix.y][];
		for (int v = 0; v < laserScanMatrix.y; v++) {
			laserScanRanges[v] = new float[(int)laserScanMatrix.x];
		}

		//Create Laser Scan lines
		laserScanLines = new GameObject[(int)laserScanMatrix.y][];
		for (int v = 0; v < laserScanMatrix.y; v++) {
			laserScanLines[v] = new GameObject[(int)laserScanMatrix.x];
		}

		for(int v = 0; v < 8; v++) {
			for (int h = 0; h < laserScanMatrix.x; h++) {
				if (laserScanLines[v][h] == null) {
					laserScanLines[v][h] = new GameObject();
				}

				laserScanLines[v][h].transform.position = this.transform.position;
				laserScanLines[v][h].transform.parent = this.transform;
				float angleH = (laserScanStart.x + laserScanStep.x * h);        //Horizontal Angle (around Vector point up), in degrees
				float angleV = -(laserScanStart.y + laserScanStep.y * v);        //Vertical Angle, in degrees
				laserScanLines[v][h].transform.rotation = Quaternion.Euler(angleV, angleH, 0);

				LineRenderer lineRenderer = laserScanLines[v][h].AddComponent<LineRenderer>();

				lineRenderer.material = laserLineMaterial;
				lineRenderer.material.color = laserLineColor;

				lineRenderer.useWorldSpace = false;
				lineRenderer.startWidth = laserLineWidth;
				lineRenderer.endWidth = laserLineWidth;
			}
		}

		//Get Partiucle System
		particleSystem = GetComponent<ParticleSystem>();
	}

	// Update is called once per frame
	void Update() {
		if(laserScanUpdate == true) {
			//Update Laser Scan Lines
			for (int v = 0; v < 8; v++) {
				for (int h = 0; h < laserScanMatrix.x; h++) {
					if (laserScanRanges[v] != null && h < laserScanRanges[v].Length) {
						LineRenderer lineRendererUpdate = laserScanLines[v][h].GetComponent<LineRenderer>();

						if (showLaserScanLines == true) {
							//Set start and end position
							lineRendererUpdate.SetPosition(0, Vector3.zero);
							if (laserScanRanges[v][h] < laserScanMaxRange && laserScanRanges[v][h] > laserScanMinRange) {
								Vector3 lineEnd = Vector3.zero + Vector3.forward * laserScanRanges[v][h];
								lineRendererUpdate.SetPosition(1, lineEnd);
								lineRendererUpdate.enabled = true;
							}
							else {
								lineRendererUpdate.SetPosition(1, Vector3.zero);
								lineRendererUpdate.enabled = false;
							}
						}
						else {
							lineRendererUpdate.SetPosition(1, Vector3.zero);
							lineRendererUpdate.enabled = false;
						}

						//Debug.Log("Laser Scan: Index = " + h + " : " + laserScanLines[v][h].transform.rotation.eulerAngles.y + " deg : " + laserScanRanges[v][h] + "m");

						//Set start and end width, shape as triangle
						//lineRendererUpdate.startWidth = 0.0f;
						//lineRendererUpdate.endWidth = robotLaserScan.ranges[4][i] * Mathf.Tan(collisionSensorAngle * Mathf.Deg2Rad) * 2.0f;
					}
				}
			}

			if (showLaserScanPointCloud == true) {
				particleSystem.SetParticles(pointCloud, pointCloud.Length);

				//showLaserScanPointCloud = false;
			}
			else {
				particleSystem.Clear();
			}

			laserScanUpdate = false;
		}
	}
	Vector3 SphericalToCartesian(float radius, float polar, float elevation) {
		//https://blog.nobel-joergensen.com/2010/10/22/spherical-coordinates-in-unity/
		Vector3 cartesian = new Vector3();

		float a = radius * Mathf.Cos(elevation);
		cartesian.x = a * Mathf.Cos(polar);
		cartesian.y = radius * Mathf.Sin(elevation);
		cartesian.z = a * Mathf.Sin(polar);
		//cartesian.x = radius * Mathf.Sin(elevation) * Mathf.Cos(polar);
		//cartesian.z = radius * Mathf.Sin(elevation) * Mathf.Sin(polar);
		//cartesian.y = radius * Mathf.Cos(elevation);

		return cartesian;
	}

	Vector3[] LaserScanToPointCloud(float[][] ranges) {
		Vector3[] points = new Vector3[ranges.Length * ranges[0].Length];

		for (int v = 0; v < ranges.Length; v++) {
			for (int h = 0; h < ranges[0].Length; h++) {
				//Convert from spherical to cartesian
				Vector2 pointCloudStart = new Vector2(-177.1875f, -19.6875f);
				float angleH = (90 - (pointCloudStart.x + laserScanStep.x * h)) * Mathf.Deg2Rad;	//Horizontal Angle (around Vector point up), in radians
				float angleV = (pointCloudStart.y + laserScanStep.y * v) * Mathf.Deg2Rad;	//Vertical Angle, in radians
				points[v * ranges[0].Length + h] = SphericalToCartesian(ranges[v][h], angleH, angleV);
			}
		}

		return points;
	}

	public void UpdateLaserScan(float[][] ranges) {
		laserScanRanges = ranges;

		Vector3[] positions = LaserScanToPointCloud(ranges);
		pointCloud = new ParticleSystem.Particle[positions.Length];

		for(int i = 0; i < positions.Length; i++) {
			pointCloud[i].position = positions[i];
			pointCloud[i].startColor = pointCloudColor;
			pointCloud[i].startSize = pointCloudScale;
		}

		laserScanUpdate = true;
	}
}
