using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mapping;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(MeshFilter))]
public class RobotLaserScan : MonoBehaviour {
	//Configurations
	public bool showLaserScanLines = true;
	public bool showLaserScanPointCloud = true;
	public bool showLaserScanMesh = true;
	public Vector2 laserScanStart = new Vector2(-177.1875f, -19.6875f);
	public Vector2 laserScanStep = new Vector2(5.625f, 5.625f);
	public Vector2 laserScanMatrix = new Vector2(64, 8);

	public Color laserLineColor = new Color(0.0f,0.0f,1.0f);
	public float laserLineWidth = 0.005f;
	public Material laserLineMaterial;
	public float laserScanMinRange = 0.01f;
	public float laserScanMaxRange = 4.0f;

	public Color refrencePointCloudColor = new Color(0.0f, 0.0f, 1.0f);
	public Color newPointCloudColor = new Color(1.0f, 0.0f, 0.0f);
	public Color matchedPointCloudColor = new Color(0.0f, 1.0f, 0.0f);
	public float pointCloudScale = 0.01f;

	//Local Laser Scan Variables
	bool laserScanUpdate = false;
	float[][] laserScanRanges;
	GameObject[][] laserScanLines;
	Mesh laserScanMesh;
	Vector2[] laserScanMeshUV;

	Vector3[] refrencePosition;
	Vector3[] matchingPosition;
	public ParticleSystem refrencePointCloudParticleSystem;
	public ParticleSystem newPointCloudParticleSystem;
	public ParticleSystem matchedPointCloudParticleSystem;
	ParticleSystem.Particle[] refrencePointCloud;
	ParticleSystem.Particle[] newPointCloud;
	ParticleSystem.Particle[] matchedPointCloud;

	//China LiDAR
	LiDARConnection liDARConnection = new LiDARConnection();

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

		for (int v = 0; v < laserScanMatrix.y; v++) {
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

				lineRenderer.enabled = false;
			}
		}

		//Create Laser Scan Mesh
		laserScanMesh = new Mesh();
		GetComponent<MeshFilter>().mesh = laserScanMesh;
		laserScanMesh.vertices = new Vector3[(int)(laserScanMatrix.x * laserScanMatrix.y)]; // LaserScanToPointCloud(laserScanRanges, 0.01f, 2.0f);
		laserScanMesh.uv = laserScanMeshUV;

		//Get triangles
		int index = 0;
		int trianglesCount = (int)(2 * (laserScanMatrix.x - 1) * (laserScanMatrix.y - 1));
		int[] laserScanTriangles = new int[3 * trianglesCount];
		for (int x = 0; x < (laserScanMatrix.x - 1); x++) {
			for (int y = 0; y < (laserScanMatrix.y - 1); y++) {
				laserScanTriangles[6 * index + 0] = x + y * laserScanRanges[0].Length;
				laserScanTriangles[6 * index + 1] = x + (y + 1) * laserScanRanges[0].Length;
				laserScanTriangles[6 * index + 2] = x + 1 + y * laserScanRanges[0].Length;

				laserScanTriangles[6 * index + 3] = x + 1 + y * laserScanRanges[0].Length;
				laserScanTriangles[6 * index + 4] = x + (y + 1) * laserScanRanges[0].Length;
				laserScanTriangles[6 * index + 5] = x + 1 + (y + 1) * laserScanRanges[0].Length;
				index = index + 1;
			}
		}
		laserScanMesh.triangles = laserScanTriangles;

		if (liDARConnection.Connect("COM3", 115200, OnNewLaserScan, OnNewRPM) == true) {
			Debug.Log("LiDAR Connected");
		}
	}

	// Update is called once per frame
	void Update() {
		if(Input.GetKeyDown(KeyCode.N)) {
			//Get new Scan
			//if(refrencePosition == null) {
			//	refrencePosition = LaserScanToPointCloud(laserScanRanges, 0.01f, 2.0f);
			//}
			//else {
			//	refrencePosition = matchingPosition;
			//}

			refrencePosition = LaserScanGraphics.LaserScanToPointCloud(laserScanRanges, laserScanStart.x, laserScanStep.x, laserScanStart.y, laserScanStep.y); //LaserScanToPointCloud(laserScanRanges, 0.01f, 2.0f);
			refrencePointCloud = new ParticleSystem.Particle[refrencePosition.Length];
			for (int i = 0; i < refrencePosition.Length; i++) {
				refrencePointCloud[i].position = refrencePosition[i];
				refrencePointCloud[i].startColor = refrencePointCloudColor;
				refrencePointCloud[i].startSize = pointCloudScale;
			}
			refrencePointCloudParticleSystem.SetParticles(refrencePointCloud, refrencePointCloud.Length);

			//Translate and rotate point cloud some amount
			matchingPosition = Mapping.ICP.TranslatePointCloud(refrencePosition, new Vector3(-0.1f, 0.0f, 0.1f));

			float rotY = -10.0f;
			float[,] rot = new float[3, 3];
			rot[0, 0] = Mathf.Cos(rotY * Mathf.Deg2Rad);
			rot[0, 1] = 0;
			rot[0, 2] = Mathf.Sin(rotY * Mathf.Deg2Rad);
			rot[1, 0] = 0;
			rot[1, 1] = 1;
			rot[1, 2] = 0;
			rot[2, 0] = -Mathf.Sin(rotY * Mathf.Deg2Rad);
			rot[2, 1] = 0;
			rot[2, 2] = Mathf.Cos(rotY * Mathf.Deg2Rad);

			matchingPosition = Mapping.ICP.RotatePointCloud(matchingPosition, rot);

			//matchingPosition = LaserScanToPointCloud(laserScanRanges, 0.01f, 2.0f);

			newPointCloud = new ParticleSystem.Particle[matchingPosition.Length];
			for (int i = 0; i < matchingPosition.Length; i++) {
				newPointCloud[i].position = matchingPosition[i];
				newPointCloud[i].startColor = newPointCloudColor;
				newPointCloud[i].startSize = pointCloudScale;
			}
			newPointCloudParticleSystem.SetParticles(newPointCloud, newPointCloud.Length);

			matchedPointCloud = new ParticleSystem.Particle[matchingPosition.Length];
			for (int i = 0; i < matchingPosition.Length; i++) {
				matchedPointCloud[i].position = matchingPosition[i];
				matchedPointCloud[i].startColor = matchedPointCloudColor;
				matchedPointCloud[i].startSize = pointCloudScale;
			}
			matchedPointCloudParticleSystem.SetParticles(matchedPointCloud, matchedPointCloud.Length);
		}
		if (Input.GetKeyDown(KeyCode.M)) {
			//Iterate Scan matching
			//Correct new point cloud using ICP
			Vector3 translation;
			float[,] rotation;
			//matchingPosition = Mapping.ICP.ICPUsingSVD(matchingPosition, refrencePosition, 1, out translation, out rotation);
			matchingPosition = Mapping.ICP.ICPUsingPointToPoint(matchingPosition, refrencePosition, 1, out translation, out rotation);

			matchedPointCloud = new ParticleSystem.Particle[matchingPosition.Length];
			for (int i = 0; i < matchingPosition.Length; i++) {
				matchedPointCloud[i].position = matchingPosition[i];
				matchedPointCloud[i].startColor = matchedPointCloudColor;
				matchedPointCloud[i].startSize = pointCloudScale;
			}
			matchedPointCloudParticleSystem.SetParticles(matchedPointCloud, matchedPointCloud.Length);
		}

		if (laserScanUpdate == true) {
			//Update Laser Scan Lines
			//for (int v = 0; v < 8; v++) {
			//	for (int h = 0; h < laserScanMatrix.x; h++) {
			//		if (laserScanRanges[v] != null && h < laserScanRanges[v].Length) {
			//			LineRenderer lineRendererUpdate = laserScanLines[v][h].GetComponent<LineRenderer>();

			//			if (showLaserScanLines == true) {
			//				//Set start and end position
			//				lineRendererUpdate.SetPosition(0, Vector3.zero);
			//				if (laserScanRanges[v][h] < laserScanMaxRange && laserScanRanges[v][h] > laserScanMinRange) {
			//					Vector3 lineEnd = Vector3.zero + Vector3.forward * laserScanRanges[v][h];
			//					lineRendererUpdate.SetPosition(1, lineEnd);
			//					lineRendererUpdate.enabled = true;
			//				}
			//				else {
			//					lineRendererUpdate.SetPosition(1, Vector3.zero);
			//					lineRendererUpdate.enabled = false;
			//				}
			//			}
			//			else {
			//				lineRendererUpdate.SetPosition(1, Vector3.zero);
			//				lineRendererUpdate.enabled = false;
			//			}

			//			//Debug.Log("Laser Scan: Index = " + h + " : " + laserScanLines[v][h].transform.rotation.eulerAngles.y + " deg : " + laserScanRanges[v][h] + "m");

			//			//Set start and end width, shape as triangle
			//			//lineRendererUpdate.startWidth = 0.0f;
			//			//lineRendererUpdate.endWidth = robotLaserScan.ranges[4][i] * Mathf.Tan(collisionSensorAngle * Mathf.Deg2Rad) * 2.0f;
			//		}
			//	}
			//}

			if(showLaserScanMesh == true) {
				Vector3[] meshPoints = LaserScanGraphics.LaserScanToPointCloud(laserScanRanges, laserScanStart.x, laserScanStep.x, laserScanStart.y, laserScanStep.y); //LaserScanToPointCloud(laserScanRanges, 0, 10);
				laserScanMesh.vertices = meshPoints;

				//Create Laser Scan Mesh Lines
				//for (int x = 0; x < (laserScanMatrix.x - 1); x++) {
				//	for (int y = 0; y < (laserScanMatrix.y - 1); y++) {
				//		//Upper Line
				//		int p1 = (int)(y * laserScanMatrix.x + x);
				//		int p2 = (int)(y * laserScanMatrix.x + x + 1);
				//		Debug.DrawLine(meshPoints[p1] + transform.position, meshPoints[p2] + transform.position, Color.white, 0.1f);
				//		//Left Line
				//		p1 = (int)(y * laserScanMatrix.x + x + 1);
				//		p2 = (int)((y + 1) * laserScanMatrix.x + x + 1);
				//		Debug.DrawLine(meshPoints[p1] + transform.position, meshPoints[p2] + transform.position, Color.white, 0.1f);
				//		//Lower Line
				//		p1 = (int)((y + 1) * laserScanMatrix.x + x);
				//		p2 = (int)((y + 1) * laserScanMatrix.x + x + 1);
				//		Debug.DrawLine(meshPoints[p1] + transform.position, meshPoints[p2] + transform.position, Color.white, 0.1f);
				//		//Right Line
				//		p1 = (int)(y * laserScanMatrix.x + x);
				//		p2 = (int)((y + 1) * laserScanMatrix.x + x);
				//		Debug.DrawLine(meshPoints[p1] + transform.position, meshPoints[p2] + transform.position, Color.white, 0.1f);
				//	}
				//}
			}

			if (showLaserScanPointCloud == true) {
				refrencePointCloudParticleSystem.SetParticles(refrencePointCloud, refrencePointCloud.Length);
				//newPointCloudParticleSystem.SetParticles(newPointCloud, newPointCloud.Length);
				//matchedPointCloudParticleSystem.SetParticles(matchedPointCloud, matchedPointCloud.Length);

				//showLaserScanPointCloud = false;
			}
			else {
				refrencePointCloudParticleSystem.Clear();
				//newPointCloudParticleSystem.Clear();
			}

			//laserScanUpdate = false;
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

	//Vector3[] LaserScanToPointCloud(float[][] ranges, float minRange, float maxRange) {
	//	List<Vector3> points = new List<Vector3>();

	//	int cnt = 0;
	//	for (int v = 0; v < ranges.Length; v++) {
	//		for (int h = 0; h < ranges[0].Length; h++) {
	//			if(ranges[v][h] < minRange || ranges[v][h] > maxRange) {
	//				continue;
	//			}

	//			//Convert from spherical to cartesian
	//			Vector2 pointCloudStart = laserScanStart;
	//			float angleH = (90 - (pointCloudStart.x + laserScanStep.x * h)) * Mathf.Deg2Rad;    //Horizontal Angle (around Vector point up), in radians
	//			float angleV = (pointCloudStart.y + laserScanStep.y * v) * Mathf.Deg2Rad;			//Vertical Angle, in radians
	//			points.Add(SphericalToCartesian(ranges[v][h], angleH, angleV));

	//			cnt++;
	//		}
	//	}

	//	return points.ToArray();
	//}

	public void UpdateLaserScan(float[][] ranges) {
		if(ranges == null) {
			return;
		}

		//Change laser scan do 2D
		laserScanRanges = new float[1][];
		laserScanRanges[0] = new float[ranges[0].Length];
		for (int h = 0; h < ranges[0].Length; h++) {
			if(ranges[3][h] < ranges[4][h]) {
				laserScanRanges[0][h] = ranges[3][h];
			}
			else {
				laserScanRanges[0][h] = ranges[4][h];
			}
			//laserScanRanges[0][h] = (ranges[3][h] + ranges[4][h]) / 2.0f;
		}
		laserScanRanges = ranges;

		Vector3[] positions = LaserScanGraphics.LaserScanToPointCloud(laserScanRanges, laserScanStart.x, laserScanStep.x, laserScanStart.y, laserScanStep.y); //LaserScanToPointCloud(laserScanRanges, 0.01f, 2.0f);
		refrencePointCloud = new ParticleSystem.Particle[positions.Length];

		for (int i = 0; i < positions.Length; i++) {
			refrencePointCloud[i].position = positions[i];
			refrencePointCloud[i].startColor = refrencePointCloudColor;
			refrencePointCloud[i].startSize = pointCloudScale;
		}

		////Translate and rotate point cloud some amount
		//Vector3[] newPositions = Mapping.ICP.TranslatePointCloud(positions, new Vector3(-0.1f, 0.0f, 0.2f));

		//float[,] rot = new float[3,3];
		//rot[0, 0] = Mathf.Cos(-25.0f * Mathf.Deg2Rad);
		//rot[0, 1] = 0;
		//rot[0, 2] = Mathf.Sin(-25.0f * Mathf.Deg2Rad);
		//rot[1, 0] = 0;
		//rot[1, 1] = 1;
		//rot[1, 2] = 0;
		//rot[2, 0] = -Mathf.Sin(-25.0f * Mathf.Deg2Rad);
		//rot[2, 1] = 0;
		//rot[2, 2] = Mathf.Cos(-25.0f * Mathf.Deg2Rad);

		//newPositions = Mapping.ICP.RotatePointCloud(newPositions, rot);
		//newPointCloud = new ParticleSystem.Particle[newPositions.Length];

		//for (int i = 0; i < positions.Length; i++) {
		//	newPointCloud[i].position = newPositions[i];
		//	newPointCloud[i].startColor = newPointCloudColor;
		//	newPointCloud[i].startSize = pointCloudScale;
		//}

		////Correct new point cloud using ICP
		//Vector3 translation;
		//float[,] rotation;
		//Vector3[] matchedPoints = Mapping.ICP.ICPUsingSVD(newPositions, positions, 5, out translation, out rotation);
		//matchedPointCloud = new ParticleSystem.Particle[matchedPoints.Length];

		//for (int i = 0; i < positions.Length; i++) {
		//	matchedPointCloud[i].position = matchedPoints[i];
		//	matchedPointCloud[i].startColor = matchedPointCloudColor;
		//	matchedPointCloud[i].startSize = pointCloudScale;
		//}

		laserScanUpdate = true;
	}

	private void OnNewLaserScan(double[] ranges, double[] signalQuality, int size) {
		Debug.Log("LiDAR New Scan: " + size.ToString());

		float scanStep = 360.0f / ranges.Length;
		Vector3[] positions = LaserScanGraphics.LaserScanToPointCloud(ranges, 0, scanStep);
		refrencePointCloud = new ParticleSystem.Particle[positions.Length];

		for (int i = 0; i < positions.Length; i++) {
			refrencePointCloud[i].position = positions[i];
			refrencePointCloud[i].startColor = LaserScanGraphics.ColorGradient(((double)i / positions.Length), 0, 1);
			refrencePointCloud[i].startSize = pointCloudScale;
		}

		laserScanUpdate = true;
	}

	private void OnNewRPM(int rpm) {
		Debug.Log("LiDAR RPM: " + rpm.ToString());
	}
}
