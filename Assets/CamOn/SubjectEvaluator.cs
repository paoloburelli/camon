using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A SubjectEvaluator is atomatically assigned to a transform by the <see cref="CameraOperator"/> behaviour when a shot is selected or a subject transforms are assigned.
/// The subject evaluator class handles the estimation of all the parameters of a subect in a shot.
/// </summary>
public class SubjectEvaluator
{
	const int SAMPLES = 5;
	const int LAYER_MASK = ~6; //do not test transparent and ignore raycast;
	const string PROXY_NAME = "[Proxy Goemetry]";
	const string VANTAGE_ANGLE_PROXY_NAME = "[Vantage Angle Direction]";
	const string IGNORE_TAG = "IGNORE";

	Transform vantageDirectionTransform;
	Transform transform;
	MeshRenderer proxyRenderer;
	Mesh proxyMesh;
	GameObject proxy;
	Vector3[] onScreenSamplePoints = new Vector3[SAMPLES];
	bool[] samplePointsVisibility = new bool[SAMPLES];
	float inFrustum;
	float projectionSize;
	Bounds screenSpaceBounds = new Bounds(Vector3.zero,Vector3.zero);
	Vector3 screenMin = Vector3.one;
	Vector3 screenMax = Vector3.zero;
	Vector3 cameraPosition;

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="SubjectEvaluator"/> is ignored.
	/// </summary>
	/// <value><c>true</c> if ignored; otherwise, <c>false</c>.</value>
	public bool Ignored {
		get {
			return transform.tag == IGNORE_TAG;
		}
		set {
			transform.tag = value ? IGNORE_TAG : "Untagged";
		}
	}

	/// <summary>
	/// Destroies all proxies associated to each subject evaluator.
	/// </summary>
	/// <param name="subjects">A list of subjects to be cleared; by default, all subjects are cleared.</param>
	public static void DestroyAllProxies(SubjectEvaluator[] subjects=null){
		Transform t;
		GameObject o;

		if (subjects == null)
			while ((o = GameObject.Find (SubjectEvaluator.PROXY_NAME)) != null)
				GameObject.DestroyImmediate (o);
		else
			foreach (SubjectEvaluator a in subjects) {
				while ((t = a.transform.Find (PROXY_NAME)) != null)
					GameObject.DestroyImmediate (t.gameObject);

				a.proxy = null;
				a.proxyMesh = null;
				a.proxyRenderer = null;
			}
	}

	/// <summary>
	/// Calculates angle between the current camera direction and a desired camera direction.
	/// </summary>
	/// <returns>The relative horizontal an vertical camera angle in degrees.</returns>
	/// <param name="desiredHorizontalAngle">Desired horizontal angle.</param>
	/// <param name="desiredVerticalAngle">Desired vertical angle.</param>
	public Vector2 CalculateRelativeCameraAngle(float desiredHorizontalAngle, float desiredVerticalAngle){
		vantageDirectionTransform.localRotation = Quaternion.Euler (-desiredVerticalAngle, -desiredHorizontalAngle, 0);
		Vector3 relativeCameraDirection = vantageDirectionTransform.InverseTransformPoint(cameraPosition).normalized;
		float v = Mathf.Asin(relativeCameraDirection.y) * Mathf.Rad2Deg;
		float h = -Mathf.Atan2(relativeCameraDirection.x,relativeCameraDirection.z) * Mathf.Rad2Deg;
		return new Vector2 (h, v);
	}

	enum SamplePoint
	{
		Top,
		Bottom,
		Left,
		Right,
		Center
	};

	bool Occlusion (SamplePoint c)
	{
		return samplePointsVisibility [(int)c];
	}

	/// <summary>
	/// Gets the collider associated to this subject.
	/// </summary>
	/// <value>The collider.</value>
	public Collider collider {
		get {
			return proxy.GetComponent<Collider>(); 
		}
	}

	/// <summary>
	/// Returns the direction from which the camera should frame this subject
	/// </summary>
	/// <value>The vantage direction.</value>
	public Vector3 VantageDirection {
		get {
			return vantageDirectionTransform.forward; 
		}
	}

	/// <summary>
	/// Gets the orientation.
	/// </summary>
	/// <value>The orientation.</value>
	public Quaternion Orientation {
		get {
			return proxy.transform.rotation;
		}
	}

	/// <summary>
	/// Returns the fraction of this subject which is included in the vew frustum
	/// </summary>
	/// <value>[0,1] The fraction in frustum.</value>
	public float InFrustum {
		get {
			return inFrustum;
		}
	}

	/// <summary>
	/// Gets and sets the scale mdifier for this subject
	/// </summary>
	/// <value>The scale.</value>
	public Vector3 Scale {
		get {
			return proxy.transform.localScale;
		}
		set {
			proxy.transform.localScale = value;
		}
	}

	/// <summary>
	/// Gets or sets the offset for ths subject.
	/// </summary>
	/// <value>The offset.</value>
	public Vector3 Offset {
		get {
			return proxy.transform.localPosition;
		}
		set {
			proxy.transform.localPosition = value;
		}
	}

	/// <summary>
	/// Returns the name of the game object associated to this subject
	/// </summary>
	/// <value>The name.</value>
	public string Name {
		get {
			return transform.name;
		}
	}

	/// <summary>
	/// Gets the position of this subject in world coordinates.
	/// </summary>
	/// <value>The position.</value>
	public Vector3 Position {
		get { return proxy.transform.position; }
	}

	/// <summary>
	/// Gets the forward vector of this subject.
	/// </summary>
	/// <value>The forward vector.</value>
	public Vector3 Forward {
		get { return proxy.transform.forward; }
	}

	/// <summary>
	/// Gets the right vector of this subject.
	/// </summary>
	/// <value>The right vector.</value>
	public Vector3 Right {
		get { return proxy.transform.right; }
	}

	/// <summary>
	/// Gets the level of visibility of this subject.
	/// </summary>
	/// <value>[0,1] The fraction of the subject visible.</value>
	public float Visibility {
		get { 
			float occlusion = 0;
			foreach (bool c in samplePointsVisibility)
				occlusion += c ? 0 : 1.0f / SAMPLES;
			return inFrustum * (1 - occlusion);
		}
	}

	/// <summary>
	/// Gets the size of the projection of this subject on screen.
	/// The size is defined as the largest dimension (i.e. height or width) of the character on the screen devided by the respective screen size
	/// </summary>
	/// <value>[0,1] The size of the projection.</value>
	public float ProjectionSize {
		get { return projectionSize; }
	}

	/// <summary>
	/// Gets the screen bounds.
	/// </summary>
	/// <value>The screen bounds.</value>
	public Bounds ScreenBounds {
		get {
			return screenSpaceBounds;
		}
	}

	/// <summary>
	/// Gets the position on screen of this ubject.
	/// The position is defined in 2D coordinates with the lower left corner corresponding to (0,0) and the upper right to (1,1)
	/// </summary>
	/// <value>[0,1] The position on screen.</value>
	public Vector3 PositionOnScreen {
		get { return screenSpaceBounds.center; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SubjectEvaluator"/> class.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="center">Center.</param>
	/// <param name="scale">Scale.</param>
	/// <param name="type">Type.</param>
	public SubjectEvaluator (Transform transform, Vector3 center, Vector3 scale, PrimitiveType type)
	{
		if (transform == null)
			throw new System.ArgumentNullException ();

		this.transform = transform;

		Transform t;
		if (transform != null)
			while ((t = transform.Find (PROXY_NAME)) != null)
				GameObject.DestroyImmediate (t.gameObject);
				
		proxy = GameObject.CreatePrimitive (type);
		GameObject.DestroyImmediate (proxy.GetComponent<Collider>());
		proxy.transform.parent = transform;
		proxy.transform.localPosition = center;
		proxy.transform.localScale = scale*0.9f;
		proxy.transform.localRotation = Quaternion.Euler(0,0,0);
		proxy.name = PROXY_NAME;
		proxyRenderer = proxy.GetComponent<MeshRenderer> ();
		proxyMesh = proxy.GetComponent<MeshFilter> ().sharedMesh;
		proxy.SetActive (transform.gameObject.activeInHierarchy);
		proxyRenderer.enabled = false;


		vantageDirectionTransform = (new GameObject ()).transform;
		vantageDirectionTransform.gameObject.name = VANTAGE_ANGLE_PROXY_NAME;
		vantageDirectionTransform.parent = proxy.transform;
		vantageDirectionTransform.localPosition = Vector3.zero;
		vantageDirectionTransform.localRotation = Quaternion.Euler(0,0,0);
	}

	/// <summary>
	/// Calculates all subject screen values given a specific camera.
	/// </summary>
	/// <param name="camera">Camera.</param>
	public void Reevaluate (Camera camera)
	{
		if (!Ignored) {
			
			int originalLayer = transform.gameObject.layer;
			transform.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			
			inFrustum = 0;
			screenMin = Vector3.one;
			screenMin.z = float.PositiveInfinity;
			screenMax = Vector3.zero;

			foreach (Vector3 v in proxyMesh.vertices) {
				Vector3 tv = proxy.transform.TransformPoint (v);
				Vector3 sv = camera.WorldToViewportPoint (tv);
				if (sv.z > 0 && sv.y < 1 && sv.y > 0 && sv.x > 0 && sv.x < 1) {
					inFrustum++;
					if (sv.y > screenMax.y) {
						screenMax.y = sv.y;
						onScreenSamplePoints [(int)SamplePoint.Top] = tv;
					}
					if (sv.y < screenMin.y) {
						screenMin.y = sv.y;
						onScreenSamplePoints [(int)SamplePoint.Bottom] = tv;
					}
					if (sv.x > screenMax.x) {
						screenMax.x = sv.x;
						onScreenSamplePoints [(int)SamplePoint.Right] = tv;
					}
					if (sv.x < screenMin.x) {
						screenMin.x = sv.x;
						onScreenSamplePoints [(int)SamplePoint.Left] = tv;
					}
					if (sv.z > screenMax.z)
						screenMax.z = sv.z;
					
					if (sv.z < screenMin.z)
						screenMin.z = sv.z;
				}
			}
			onScreenSamplePoints [(int)SamplePoint.Center] = (onScreenSamplePoints [(int)SamplePoint.Top] + onScreenSamplePoints [(int)SamplePoint.Bottom] + onScreenSamplePoints [(int)SamplePoint.Right]+ onScreenSamplePoints [(int)SamplePoint.Left])/4 + (onScreenSamplePoints [(int)SamplePoint.Right] - onScreenSamplePoints [(int)SamplePoint.Left]) * Random.value * 0.2f;
			inFrustum *= 1.0f / proxyMesh.vertices.LongLength;
			
			if (inFrustum > 0) {
				for (int i=0; i<SAMPLES; i++) {
					Vector3 direction = camera.transform.position - onScreenSamplePoints [i];
					samplePointsVisibility [i] = !Physics.Raycast (onScreenSamplePoints [i], direction.normalized, direction.magnitude, LAYER_MASK);
				}
				
				float height = screenMax.y - screenMin.y;
				float width = screenMax.x - screenMin.x;
				
				projectionSize = height > width ? height : width;
				projectionSize *= 1.1f; //this gives some borders around the object on the screen
				
			} else {
				for (int i=0; i<SAMPLES; i++) {
					onScreenSamplePoints [i] = Vector3.zero;
					samplePointsVisibility [i] = false;
				}

				projectionSize = float.NegativeInfinity;
				
				screenSpaceBounds.SetMinMax(Vector3.zero,Vector3.zero);
						
				Vector3 viewPortSpaceCenter = camera.WorldToViewportPoint (Position);
					
				screenMax.x = viewPortSpaceCenter.x > 0 ? viewPortSpaceCenter.x > 1 ? float.PositiveInfinity : float.NaN : float.NegativeInfinity;
				screenMax.y = viewPortSpaceCenter.y > 0 ? viewPortSpaceCenter.y > 1 ? float.PositiveInfinity : float.NaN : float.NegativeInfinity;
				screenMax.z = 0;
				screenMin.x = screenMax.x;
				screenMin.y = screenMax.y;
				screenMin.z = screenMax.z;
			}
			
			//If it is too small it is not visible;
			//if (projectionSize < 0.01)
			//	onScreenFraction = 0;
			
			screenSpaceBounds.SetMinMax(screenMin,screenMax);
			transform.gameObject.layer = originalLayer;

			cameraPosition = camera.transform.position;
		}
	}

	/// <summary>
	/// Reevaluates a list of subjects.
	/// </summary>
	/// <param name="subjects">The list fo subjects.</param>
	/// <param name="camera">Camera.</param>
	public static void ReevaluateAll (SubjectEvaluator[] subjects, Camera camera)
	{
		if (subjects != null && camera != null)
			foreach (SubjectEvaluator s in subjects)
				if (s != null)
					s.Reevaluate (camera);
	}

	/// <summary>
	/// Draws debug info.
	/// </summary>
	public void DrawGizmos ()
	{
		if (inFrustum > 0) {
			for (int i =0; i<SAMPLES; i++)
				if (samplePointsVisibility [i]) {
					Gizmos.color = Color.green;
					Gizmos.DrawSphere (onScreenSamplePoints [i], 0.05f * proxy.GetComponent<Renderer>().bounds.size.y);
				} else {
					Gizmos.color = Color.red;
					Gizmos.DrawWireSphere (onScreenSamplePoints [i], 0.05f * proxy.GetComponent<Renderer>().bounds.size.y);
				}
		}
	}
}
