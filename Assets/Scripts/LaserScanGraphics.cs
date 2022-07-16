using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserScanGraphics : MonoBehaviour {
	public static Vector3 SphericalToCartesian(float radius, float polar, float elevation) {
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

	public static Vector3[] LaserScanToPointCloud(float[][] ranges, float scanStartX, float scanStepX, float scanStartY, float scanStepY) {
		List<Vector3> points = new List<Vector3>();

		int cnt = 0;
		for (int v = 0; v < ranges.Length; v++) {
			for (int h = 0; h < ranges[0].Length; h++) {
				//Convert from spherical to cartesian
				float angleH = (90 - (scanStartX + scanStepX * h)) * Mathf.Deg2Rad;    //Horizontal Angle (around Vector point up), in radians
				float angleV = (scanStartY + scanStepY * v) * Mathf.Deg2Rad;           //Vertical Angle, in radians
				points.Add(SphericalToCartesian(ranges[v][h], angleH, angleV));

				cnt++;
			}
		}

		return points.ToArray();
	}

	public static Vector3[] LaserScanToPointCloud(double[] ranges, float scanStart, float scanStep) {
		List<Vector3> points = new List<Vector3>();

		int cnt = 0;
		for (int h = 0; h < ranges.Length; h++) {
			int v = 0;
			//Convert from spherical to cartesian
			float angleH = (scanStart - scanStep * h) * Mathf.Deg2Rad;      //Horizontal Angle (around Vector point up), in radians
			float angleV = 0 * Mathf.Deg2Rad;                               //Vertical Angle, in radians
			points.Add(SphericalToCartesian((float)ranges[h], angleH, angleV));

			cnt++;
		}

		return points.ToArray();
	}

	public static Color ColorGradient(double value, double rangeMin, double rangeMax) {
		//double[] colorGradientScale = new double[3];
		//Color[] colorGradient = new Color[3];

		////Set up the color scale
		//colorGradientScale[0] = 0;
		//colorGradient[0] = new Color(1, 0, 0);      //Start at Red
		//colorGradientScale[1] = 0.5;
		//colorGradient[1] = new Color(1, 1, 0);      //Middle is Yellow
		//colorGradientScale[2] = 1;
		//colorGradient[2] = new Color(0, 1, 0);      //End is Green

		double[] colorGradientScale = new double[6];
		Color[] colorGradient = new Color[6];

		//Set up the color scale
		colorGradientScale[0] = 0;
		colorGradient[0] = new Color(1, 0, 1);      //Purple
		colorGradientScale[1] = 0.2;
		colorGradient[1] = new Color(0, 0, 1);      //Blue
		colorGradientScale[2] = 0.4;
		colorGradient[2] = new Color(0, 1, 1);      //Aqua
		colorGradientScale[3] = 0.6;
		colorGradient[3] = new Color(0, 1, 0);      //Green
		colorGradientScale[4] = 0.8;
		colorGradient[4] = new Color(1, 1, 0);      //Yellow
		colorGradientScale[5] = 1;
		colorGradient[5] = new Color(1, 0, 0);      //Red


		//Get color based on "value"
		Color newColor = colorGradient[2];
		double scale = (value - rangeMin) / (rangeMax - rangeMin);  //Convert to decimal between 0 and 1
		for (int i = 1; i < colorGradient.Length; i++) {
			if (scale < colorGradientScale[i]) {
				double lerpValue = (scale - colorGradientScale[i - 1]) / (colorGradientScale[i] - colorGradientScale[i - 1]);
				newColor = Color.Lerp(colorGradient[i - 1], colorGradient[i], (float)lerpValue);
				break;
			}
		}

		return newColor;
	}

	public static ParticleSystem.Particle[] PointCloudToParticles(Vector3[] points, float particleScale, Color particleColor) {
		ParticleSystem.Particle[] particleSystem = new ParticleSystem.Particle[points.Length];

		for (int i = 0; i < points.Length; i++) {
			particleSystem[i].position = points[i];
			particleSystem[i].startColor = particleColor; // LaserScanGraphics.ColorGradient(signalQuality[i], 0, 255);
			particleSystem[i].startSize = particleScale;
		}

		return particleSystem;
	}
}
