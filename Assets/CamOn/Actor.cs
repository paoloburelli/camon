using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The Actor is one of the two main component of CamOn, it handles the estimation of all the parameters of a subect in a shot.
/// It supports a number of custom shapes used to represent the target object
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("CamOn/Actor")]
public class Actor : MonoBehaviour
{
	const int SAMPLES = 5;
	const int LAYER_MASK = ~6; //do not test transparent and ignore raycast;
	const string PROXY_NAME = "[Proxy Goemetry]";
	const string VANTAGE_ANGLE_PROXY_NAME = "[Vantage Angle Direction]";
	const string IGNORE_TAG = "IGNORE";
	
	Transform _vantageDirectionProxy;
	GameObject _proxy;
	Mesh _proxyMesh;
	
	Transform vantageDirectionProxy {
		get {
			if (_vantageDirectionProxy == null)
				CreateProxy();
			return _vantageDirectionProxy;
		}
	}
	
	GameObject proxy {
		get {
			if (_proxy == null)
				CreateProxy();
			return _proxy;
		}
	}
	
	Mesh proxyMesh {
		get {
			if (_proxyMesh == null)
				CreateProxy();
			return _proxyMesh;
		}
	}
	
	Vector3[] onScreenSamplePoints = new Vector3[SAMPLES];
	bool[] samplePointsVisibility = new bool[SAMPLES];
	float inFrustum;
	float projectionSize;
	Bounds screenSpaceBounds = new Bounds(Vector3.zero,Vector3.zero);
	Vector3 screenMin = Vector3.one;
	Vector3 screenMax = Vector3.zero;
	Vector3 cameraPosition;
	
	[SerializeField]
	PrimitiveType shape;
	[SerializeField]
	Vector3 scale= new Vector3(.95f,.95f,.95f);
	[SerializeField]
	Vector3 offset=Vector3.zero;
	
	Vector3 scaleModifier = Vector3.one;
	Vector3 offsetMofidier = Vector3.zero;
	
	/// <summary>
	/// Sets the area of interest of an Actor.
	/// Used by CameraOperator to qhen a shot is selected
	/// </summary>
	/// <param name="scale">Size of the are of interest.</param>
	/// <param name="offset">Position of the area of interest.</param>
	public void SetAreaOfInterest(Vector3 scale,Vector3 offset) {
		this.scaleModifier = scale;
		this.offsetMofidier = offset;
		proxy.transform.localScale = Vector3.Scale(this.scale,this.scaleModifier);
		proxy.transform.localPosition = this.offset+this.offsetMofidier;
	}
	
	/// <summary>
	/// Gets or sets the shape of the actor.
	/// </summary>
	/// <value>The actor's shape.</value>
	public PrimitiveType Shape {
		get {
			return shape;
		}
		set {
			if (shape != value) {
				shape = value;
				CreateProxy();
			}
		}
	}
	
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
	/// If true the actors does not follow the transform rotation
	/// </summary>
	public bool IgnoreRotation = false;
	
	/// <summary>
	/// Calculates angle between the current camera direction and a desired camera direction.
	/// </summary>
	/// <returns>The relative horizontal an vertical camera angle in degrees.</returns>
	/// <param name="desiredHorizontalAngle">Desired horizontal angle.</param>
	/// <param name="desiredVerticalAngle">Desired vertical angle.</param>
	public Vector2 CalculateRelativeCameraAngle(float desiredHorizontalAngle, float desiredVerticalAngle){
		vantageDirectionProxy.localRotation = Quaternion.Euler (-desiredVerticalAngle, -desiredHorizontalAngle, 0);
		Vector3 relativeCameraDirection = vantageDirectionProxy.InverseTransformPoint(cameraPosition).normalized;
		float v = Mathf.Asin(relativeCameraDirection.y) * Mathf.Rad2Deg;
		float h = -Mathf.Atan2(relativeCameraDirection.x,relativeCameraDirection.z) * Mathf.Rad2Deg;
		return new Vector2 (h, v);
	}
	
	void Update() {
		if (IgnoreRotation)
			proxy.transform.localRotation = Quaternion.Inverse(transform.rotation);
	}
	
	///<summary>
	///Points used to sample actor's visibility
	///</summary>
	public enum SamplePoint
	{
		Top,
		Bottom,
		Left,
		Right,
		Center
	};
	
	/// <summary>
	/// Check if a specific sample point is occluded
	/// </summary>
	/// <param name="c">C.</param>
	public bool Occluded (SamplePoint c)
	{
		return samplePointsVisibility [(int)c];
	}
	
	/// <summary>
	/// Returns the direction from which the camera should frame this subject
	/// </summary>
	/// <value>The vantage direction.</value>
	public Vector3 VantageDirection {
		get {
			return vantageDirectionProxy.forward; 
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
			return scale;
		}
		set {
			scale = value;
			proxy.transform.localScale = Vector3.Scale(value,scaleModifier);
		}
	}

	/// <summary>
	/// Gets the size of the volume of interest.
	/// </summary>
	/// <value>The size of the volume of interest.</value>
	public Vector3 VolumeOfInterestSize {
		get{
			return proxy.transform.localScale;
		}
	}
	
	/// <summary>
	/// Gets or sets the offset for ths subject.
	/// </summary>
	/// <value>The offset.</value>
	public Vector3 Offset {
		get {
			return offset;
		}
		set {
			offset = value;
			proxy.transform.localPosition = value+offsetMofidier;
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
			return inFrustum * (1 - Occlusion);
		}
	}
	
	/// <summary>
	/// Returns the fraction of the subject which is hidden by other geometry
	/// </summary>
	/// <value>[0,1] The occluded fraction.</value>
	public float Occlusion {
		get {
			float occlusion = 0;
			foreach (bool c in samplePointsVisibility)
				occlusion += c ? 0 : 1.0f / SAMPLES;
			return occlusion;
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
	
	public void CreateProxy() {
		Transform t;
		if (transform != null)
			while ((t = transform.Find (PROXY_NAME)) != null)
				GameObject.DestroyImmediate (t.gameObject);
		
		_proxy = GameObject.CreatePrimitive (shape);
		GameObject.DestroyImmediate (_proxy.GetComponent<Collider>());
		_proxy.transform.parent = transform;
		_proxy.transform.localPosition = offset;
		_proxy.transform.localScale = scale;
		_proxy.transform.localRotation = Quaternion.Euler(0,0,0);
		_proxy.name = PROXY_NAME;
		_proxyMesh = proxy.GetComponent<MeshFilter> ().sharedMesh;
		_proxy.SetActive (transform.gameObject.activeInHierarchy);
		_proxy.GetComponent<MeshRenderer> ().enabled = false;
		
		
		_vantageDirectionProxy = (new GameObject ()).transform;
		_vantageDirectionProxy.gameObject.name = VANTAGE_ANGLE_PROXY_NAME;
		_vantageDirectionProxy.parent = _proxy.transform;
		_vantageDirectionProxy.localPosition = Vector3.zero;
		_vantageDirectionProxy.localRotation = Quaternion.Euler(0,0,0);
	}
	
	/// <summary>
	/// Calculates all actor's screen values given a specific camera.
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
	/// Reevaluates a list of actors.
	/// </summary>
	/// <param name="actors">The list fo actors.</param>
	/// <param name="camera">Camera.</param>
	public static void ReevaluateAll (Actor[] actors, Camera camera)
	{
		if (actors != null && camera != null)
			foreach (Actor s in actors)
				if (s != null)
					s.Reevaluate (camera);
	}
	
	/// <summary>
	/// Draws debug info.
	/// </summary>
	void OnDrawGizmos ()
	{
		if (proxy != null) {
			Gizmos.color = Color.white;
			Vector3 scale = proxy.transform.localScale;
			Transform t = proxy.transform.parent;
			while ((t = t.parent) != null)
				scale.Scale(t.localScale);
			
			Gizmos.DrawWireMesh(proxyMesh,Position,Orientation,scale);
			
			for (int i =0; i<SAMPLES; i++)
			if (samplePointsVisibility [i]) {
				Gizmos.color = Color.green;
				Gizmos.DrawSphere (onScreenSamplePoints [i], 0.05f * proxy.GetComponent<Renderer>().bounds.size.y);
			} else {
				Gizmos.color = Color.red;
				Gizmos.DrawSphere (onScreenSamplePoints [i], 0.05f * proxy.GetComponent<Renderer>().bounds.size.y);
			}
		}
	}
	
	/// <summary>
	/// Creates and actor for a specifi transform
	/// </summary>
	/// <param name="t">The transform on which to attach the actor.</param>
	/// <param name="shape">The shape of the actor.</param>
	/// <param name="offset">The offset of the actor within the transfrom.</param>
	/// <param name="scale">The scale of the actor within the transfrom.</param>
	public static Actor Create(Transform t, PrimitiveType shape, Vector3 offset, Vector3 scale){
		Actor rVal = t.gameObject.AddComponent<Actor>();
		rVal.Shape = shape;
		rVal.Offset = offset;
		rVal.Scale = scale;
		return rVal;
	}
}
