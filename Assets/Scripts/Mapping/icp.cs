using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mapping {
	public class ICP {

		//public static Vector3[] DirectSVP(Vector3[] p, Vector3[] q) {
		//	Vector3[] pCopy = (Vector3[])p.Clone();

		//	//Step 1: Align centers
		//	Vector3 cmP = GetCenterMass(pCopy);
		//	Vector3 cmQ = GetCenterMass(q);
		//	Vector3[] pT = TranslatePointCloud(pCopy, -cmP);
		//	Vector3[] pQ = TranslatePointCloud(q, -cmQ);

		//	//Step 2: Compute correspondences
		//	List<Vector2> correspondences = ClosestPointCorrespondence(pT, pQ);

		//	//Step 3: Compute cross covariance
		//	float[,] crossCov = CrossCovariance(pT, pQ, correspondences);

		//	//Step 4: Get Rotation from SVD Decomposition
		//	float[,] rotation = FindRotationWithSVD(crossCov);

		//	//Step 5: Calculate translation
		//	Vector3 rP = new Vector3();
		//	rP.x = cmP.x * rotation[0, 0] + cmP.y * rotation[0, 1] + cmP.z * rotation[0, 2];
		//	rP.y = cmP.x * rotation[1, 0] + cmP.y * rotation[1, 1] + cmP.z * rotation[1, 2];
		//	rP.z = cmP.x * rotation[2, 0] + cmP.y * rotation[2, 1] + cmP.z * rotation[2, 2];
		//	Vector3 translation = cmQ - rP;

		//	//Step 6: Rotate and translate point cloud
		//	pCopy = RotatePointCloud(pCopy, rotation);
		//	pCopy = TranslatePointCloud(pCopy, translation);

		//	return pCopy;
		//}

		//Shift point cloud p to best match the refrence point cloud q, returns the shifted point cloud and the translation and rotation used
		public static Vector3[] ICPUsingSVD(Vector3[] p, Vector3[] q, int maxIterations, out Vector3 translation, out float[,] rotation) {
			Vector3[] pCopy = (Vector3[])p.Clone();

			Vector3 cmQ = GetCenterMass(q);

			float error = float.MaxValue;

			int iteration = 0;
			do {
				//Step 0: Align centers
				//Vector3 cmP = GetCenterMass(pCopy);
				//Vector3 cmQ = GetCenterMass(q);
				//Vector3[] pT = TranslatePointCloud(pCopy, -cmP);
				//Vector3[] pQ = TranslatePointCloud(q, -cmQ);

				//Debug.Log("Iteration: " + iteration.ToString() + "/" + maxIterations.ToString());

				//Step 1: Compute correspondences
				List<Vector2> correspondences = ClosestPointCorrespondence(pCopy, q);

				//Step 2: Compute cross covariance
				float[,] crossCov = CrossCovariance(pCopy, q, correspondences);

				//Step 3: Get Rotation from SVD Decomposition
				rotation = FindRotationWithSVD(crossCov);

				//Step 4: Calculate translation
				Vector3 cmP = GetCenterMass(pCopy);

				Vector3 rP = new Vector3();
				rP.x = cmP.x * rotation[0, 0] + cmP.y * rotation[0, 1] + cmP.z * rotation[0, 2];
				rP.y = cmP.x * rotation[1, 0] + cmP.y * rotation[1, 1] + cmP.z * rotation[1, 2];
				rP.z = cmP.x * rotation[2, 0] + cmP.y * rotation[2, 1] + cmP.z * rotation[2, 2];
				translation = cmQ - rP;

				//Debug.Log("Translation: " + translation.ToString());

				//Step 5: Rotate and translate point cloud
				pCopy = RotatePointCloud(pCopy, rotation);
				pCopy = TranslatePointCloud(pCopy, translation);

				iteration++;
			} while(error > 0.1f && iteration < maxIterations);
			
			return pCopy;
		}

		//Returns a Vector2 list of point correspondences between point cloud p and point cloud q using euclidean closest points
		public static List<Vector2> ClosestPointCorrespondence(Vector3[] p, Vector3[] q) {
			List<Vector2> correspondences = new List<Vector2>();

			int sizeP = p.Length;
			int sizeQ = q.Length;
			for (int i = 0; i < sizeP; i++) {
				float minDist = float.MaxValue;
				int corrIndex = -1;
				for (int j = 0; j < sizeQ; j++) {
					float dist = Vector3.Distance(p[i], q[j]);
					if (dist < minDist) {
						minDist = dist;
						corrIndex = j;
					}
				}
				correspondences.Add(new Vector2(i, corrIndex));
			}

			return correspondences;
		}

		//Returns the center of mass of a point cloud
		public static Vector3 GetCenterMass(Vector3[] p) {
			Vector3 cm = new Vector3();

			int sizeP = p.Length;
			for (int i = 0; i < sizeP; i++) {
				cm.x += p[i].x;
				cm.y += p[i].y;
				cm.z += p[i].z;
			}

			cm.x = cm.x / sizeP;
			cm.y = cm.y / sizeP;
			cm.z = cm.z / sizeP;

			return cm;
		}

		static float Kernel(float th, float e) {
			float wh = 0;
			if (e > th) {
				wh = 0.0f;
			}
			else {
				wh = 1.0f;
			}
			return wh;
		}

		public static float[,] CrossCovariance(Vector3[] p, Vector3[] q, List<Vector2> correspondences) {
			float[,] cov = new float[3, 3];

			for (int i = 0; i < correspondences.Count; i++) {
				int indexP = (int)correspondences[i].x;
				int indexQ = (int)correspondences[i].y;

				float weight = Kernel(0.1f, Vector3.Distance(q[indexQ], p[indexP]));

				cov[0, 0] += weight * q[indexQ].x * p[indexP].x;
				cov[0, 1] += weight * q[indexQ].x * p[indexP].y;
				cov[0, 2] += weight * q[indexQ].x * p[indexP].z;
				cov[1, 0] += weight * q[indexQ].y * p[indexP].x;
				cov[1, 1] += weight * q[indexQ].y * p[indexP].y;
				cov[1, 2] += weight * q[indexQ].y * p[indexP].z;
				cov[2, 0] += weight * q[indexQ].z * p[indexP].x;
				cov[2, 1] += weight * q[indexQ].z * p[indexP].y;
				cov[2, 2] += weight * q[indexQ].z * p[indexP].z;
			}

			return cov;
		}

		public static Vector3[] TranslatePointCloud(Vector3[] p, Vector3 t) {
			Vector3[] pT = new Vector3[p.Length];

			for (int i = 0; i < p.Length; i++) {
				pT[i].x = p[i].x + t.x;
				pT[i].y = p[i].y + t.y;
				pT[i].z = p[i].z + t.z;
			}

			return pT;
		}

		public static Vector3[] RotatePointCloud(Vector3[] p, float[,] r) {
			Vector3[] pR = new Vector3[p.Length];

			for (int i = 0; i < pR.Length; i++) {
				pR[i].x = p[i].x * r[0, 0] + p[i].y * r[0, 1] + p[i].z * r[0, 2];
				pR[i].y = p[i].x * r[1, 0] + p[i].y * r[1, 1] + p[i].z * r[1, 2];
				pR[i].z = p[i].x * r[2, 0] + p[i].y * r[2, 1] + p[i].z * r[2, 2];
			}

			return pR;
		}

		private static AForge.Math.Matrix3x3 ArrayToMatrix3x3(float[,] a) {
			AForge.Math.Matrix3x3 m = new AForge.Math.Matrix3x3();
			m.V00 = a[0, 0];
			m.V01 = a[0, 1];
			m.V02 = a[0, 2];
			m.V10 = a[1, 0];
			m.V11 = a[1, 1];
			m.V12 = a[1, 2];
			m.V20 = a[2, 0];
			m.V21 = a[2, 1];
			m.V22 = a[2, 2];
			return m;
		}

		private static float[,] Matrix3x3ToArray(AForge.Math.Matrix3x3 m) {
			float[,] a = new float[3, 3];
			a[0, 0] = m.V00;
			a[0, 1] = m.V01;
			a[0, 2] = m.V02;
			a[1, 0] = m.V10;
			a[1, 1] = m.V11;
			a[1, 2] = m.V12;
			a[2, 0] = m.V20;
			a[2, 1] = m.V21;
			a[2, 2] = m.V22;
			return a;
		}

		public static float[,] FindRotationWithSVD(float[,] crossCov) {
			float[,] rot;

			AForge.Math.Matrix3x3 svdU;
			AForge.Math.Vector3 svdE;
			AForge.Math.Matrix3x3 svdV;

			AForge.Math.Matrix3x3 svdA = ArrayToMatrix3x3(crossCov);
			svdA.SVD(out svdU, out svdE, out svdV);

			AForge.Math.Matrix3x3 rotM = svdU * svdV.Transpose();

			if (rotM.Determinant < 0) {
				AForge.Math.Matrix3x3 diagM = new AForge.Math.Matrix3x3();

				diagM.V00 = 1;
				diagM.V11 = 1;
				diagM.V22 = -1;

				rotM = svdU * diagM * svdV.Transpose();
			}

			rot = Matrix3x3ToArray(rotM);

			return rot;
		}

		public static float[,] JacobianPointToPoint(Vector3 p) {
			//https://github.com/arntanguy/icp/blob/master/src/icp/error_point_to_point.cpp
			float[,] j = new float[3, 6];
			j[0, 0] = -1;
			j[0, 1] = 0;
			j[0, 2] = 0;
			j[0, 3] = 0;
			j[0, 4] = -p.z;
			j[0, 5] = p.y;
			j[1, 0] = 0;
			j[1, 1] = -1;
			j[1, 2] = 0;
			j[1, 3] = p.z;
			j[1, 4] = 0;
			j[1, 5] = -p.x;
			j[2, 0] = 0;
			j[2, 1] = 0;
			j[2, 2] = -1;
			j[2, 3] = -p.y;
			j[2, 4] = p.x;
			j[2, 5] = 0;
			return j;
		}

		public static Vector3 ErrorPointToPoint(Vector3 p, Vector3 q) {
			Vector3 e = new Vector3();
			e.x = p.x - q.x;
			e.y = p.y - q.y;
			e.z = p.z - q.z;
			return e;
		}

		public static float[,] HessianPointToPoint(Vector3[] p, Vector3[] q, List<Vector2> correspondences) {
			float[,] h = new float[6, 6];

			for (int i = 0; i < correspondences.Count; i++) {
				int indexP = (int)correspondences[i].x;
				int indexQ = (int)correspondences[i].y;

				float[,] j = JacobianPointToPoint(p[indexP]);

				//Compute H = sum(JnT * Jn)
				for (int l = 0; l < 6; l++) {
					h[l, 0] += j[0, l] * j[0, 0] + j[1, l] * j[1, 0] + j[2, l] * j[2, 0];
					h[l, 1] += j[0, l] * j[0, 1] + j[1, l] * j[1, 1] + j[2, l] * j[2, 1];
					h[l, 2] += j[0, l] * j[0, 2] + j[1, l] * j[1, 2] + j[2, l] * j[2, 2];
					h[l, 3] += j[0, l] * j[0, 3] + j[1, l] * j[1, 3] + j[2, l] * j[2, 3];
					h[l, 4] += j[0, l] * j[0, 4] + j[1, l] * j[1, 4] + j[2, l] * j[2, 4];
					h[l, 5] += j[0, l] * j[0, 5] + j[1, l] * j[1, 5] + j[2, l] * j[2, 5];
				}
			}

			return h;
		}

		public static float[] GradientPointToPoint(Vector3[] p, Vector3[] q, List<Vector2> correspondences) {
			float[] g = new float[6];
			for (int i = 0; i < correspondences.Count; i++) {
				int indexP = (int)correspondences[i].x;
				int indexQ = (int)correspondences[i].y;

				float[,] j = JacobianPointToPoint(p[indexP]);
				Vector3 e = ErrorPointToPoint(p[indexP], q[indexQ]);

				//Compute G = sum(JnT * e)
				g[0] += j[0, 0] * e.x + j[1, 0] * e.y + j[2, 0] * e.z;
				g[1] += j[0, 1] * e.x + j[1, 1] * e.y + j[2, 1] * e.z;
				g[2] += j[0, 2] * e.x + j[1, 2] * e.y + j[2, 2] * e.z;
				g[3] += j[0, 3] * e.x + j[1, 3] * e.y + j[2, 3] * e.z;
				g[4] += j[0, 4] * e.x + j[1, 4] * e.y + j[2, 4] * e.z;
				g[5] += j[0, 5] * e.x + j[1, 5] * e.y + j[2, 5] * e.z;
			}
			return g;
		}

		public static float[] GaussianElimination(float[,] A, float[] b) {
			int rowCnt = A.GetLength(0);
			int colCnt = A.GetLength(0) + 1;
			float[,] augA = new float[rowCnt, colCnt];

			//Create augmented matrix: [A | b]
			for (int r = 0; r < rowCnt; r++) {
				for (int c = 0; c < colCnt; c++) {
					if (c == (colCnt - 1)) {
						augA[r, c] = b[r];
					}
					else {
						augA[r, c] = A[r, c];
					}
				}
			}

			//Find row-echelon form
			//https://en.wikipedia.org/wiki/Gaussian_elimination
			int h = 0;  //Initialization of the pivot row
			int k = 0;  //Initialization of the pivot column
			while (h < rowCnt && k < colCnt) {
				//Find the k-th pivot
				float maxValue = 0;
				int maxIndex = 0;
				for (int i = h; i < rowCnt; i++) {
					if (Mathf.Abs(augA[i, k]) > maxValue) {
						maxValue = Mathf.Abs(augA[i, k]);
						maxIndex = i;
					}
				}
				if (augA[maxIndex, k] == 0) {
					//No pivot in this column, pass to next column
					k = k + 1;
				}
				else {
					//Swap row h with row maxIndex
					for (int c = 0; c < colCnt; c++) {
						float tmp = augA[h, c];
						augA[h, c] = augA[maxIndex, c];
						augA[maxIndex, c] = tmp;
					}
					//Do for all rows below pivot
					for (int i = h + 1; i < rowCnt; i++) {
						float f = augA[i, k] / augA[h, k];
						//Fill with zeros the lower part of pivot column
						augA[i, k] = 0;
						//Do for all remaining elements in current row
						for (int j = k + 1; j < colCnt; j++) {
							augA[i, j] = augA[i, j] - (augA[h, j] * f);
						}
					}
					//Increase pivot row and column
					h = h + 1;
					k = k + 1;
				}
			}

			//Back substitution
			float[] x = new float[rowCnt];
			for (int i = (rowCnt - 1); i >= 0; i--) {
				x[i] = augA[i, colCnt - 1];
				for (int j = (i + 1); j < rowCnt; j++) {
					x[i] = x[i] - augA[i, j] * x[j];
				}
				x[i] = x[i] / augA[i, i];
			}
			return x;
		}

		public static Vector3[] ICPUsingPointToPoint(Vector3[] p, Vector3[] q, int maxIterations, out Vector3 translation, out float[,] rotation) {
			Vector3[] pCopy = (Vector3[])p.Clone();

			Vector3 cmQ = GetCenterMass(q);

			float error = float.MaxValue;

			int iteration = 0;
			do {
				//Step 1: Compute correspondences
				List<Vector2> correspondences = ClosestPointCorrespondence(pCopy, q);

				//Step 2: Compute Hessian and Gradient
				float[,] h = HessianPointToPoint(pCopy, q, correspondences);
				float[] g = GradientPointToPoint(pCopy, q, correspondences);

				//Step 3: Get Rotation and Translation from Hx = -g
				float[] dX = GaussianElimination(h, g);		//Accord.Math.Matrix.Solve(h, g, true);

				Debug.Log("Translation: " + dX[0].ToString() + ";" + dX[1].ToString() + ";" + dX[2].ToString());
				Debug.Log("Rotation: " + dX[3].ToString() + ";" + dX[4].ToString() + ";" + dX[5].ToString());

				translation = new Vector3(dX[0], dX[1], dX[2]);
				float rotY = dX[4];

				rotation = new float[3, 3];
				rotation[0, 0] = Mathf.Cos(rotY);
				rotation[0, 1] = 0;
				rotation[0, 2] = Mathf.Sin(rotY);
				rotation[1, 0] = 0;
				rotation[1, 1] = 1;
				rotation[1, 2] = 0;
				rotation[2, 0] = -Mathf.Sin(rotY);
				rotation[2, 1] = 0;
				rotation[2, 2] = Mathf.Cos(rotY);

				//Debug.Log("Translation: " + translation.ToString());

				//Step 5: Rotate and translate point cloud
				pCopy = RotatePointCloud(pCopy, rotation);
				pCopy = TranslatePointCloud(pCopy, translation);

				iteration++;
			} while (error > 0.1f && iteration < maxIterations);

			return pCopy;
		}


		public static float[] JacobianPointToPlane(Vector3 p, Vector3 np) {
			//https://github.com/arntanguy/icp/blob/master/src/icp/error_point_to_plane.cpp
			float[] j = new float[6];
			j[0] = np.x;
			j[1] = np.y;
			j[2] = np.z;
			j[3] = p.y * np.z - p.z * np.y;
			j[4] = p.z * np.x - p.x * np.z;
			j[5] = p.y * np.y - p.y * np.x;
			return j;
		}

		public static float ErrorPointToPlane(Vector3 p, Vector3 np,Vector3 q) {
			float e;
			Vector3 ep = ErrorPointToPoint(p, q);
			e = np.x * ep.x + np.y * ep.y + np.z * ep.z;
			return e;
		}
	}
}
