using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Actor
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

	public enum SamplePoint
	{
		Top,
		Bottom,
		Left,
		Right,
		Center
	};

	public bool Ignore {
		get {
			return transform.tag == IGNORE_TAG;
		}
		set {
			transform.tag = value ? IGNORE_TAG : "Untagged";
		}
	}

	public static void DestroyAllProxies(Actor[] actors=null){
		Transform t;
		GameObject o;

		if (actors == null)
			while ((o = GameObject.Find (Actor.PROXY_NAME)) != null)
				GameObject.DestroyImmediate (o);
		else
			foreach (Actor a in actors) {
				while ((t = a.transform.Find (PROXY_NAME)) != null)
					GameObject.DestroyImmediate (t.gameObject);

				a.proxy = null;
				a.proxyMesh = null;
				a.proxyRenderer = null;
			}
	}
		
	public Vector2 CalculateRelativeCameraAngle(float desiredHorizontalAngle, float desiredVerticalAngle){
		vantageDirectionTransform.localRotation = Quaternion.Euler (-desiredVerticalAngle, -desiredHorizontalAngle, 0);
		Vector3 relativeCameraDirection = vantageDirectionTransform.InverseTransformPoint(cameraPosition).normalized;
		float v = Mathf.Asin(relativeCameraDirection.y) * Mathf.Rad2Deg;
		float h = -Mathf.Atan2(relativeCameraDirection.x,relativeCameraDirection.z) * Mathf.Rad2Deg;
		return new Vector2 (h, v);
	}

	public Collider collider {
		get {
			return proxy.GetComponent<Collider>(); 
		}
	}


	public Vector3 VantageDirection {
		get {
			return vantageDirectionTransform.forward; 
		}
	}

	public Quaternion Orientation {
		get {
			return proxy.transform.rotation;
		}
	}

	public float InFrustum {
		get {
			return inFrustum;
		}
	}

	public Vector3 Scale {
		get {
			return proxy.transform.localScale;
		}
		set {
			proxy.transform.localScale = value;
		}
	}

	public Vector3 Offset {
		get {
			return proxy.transform.localPosition;
		}
		set {
			proxy.transform.localPosition = value;
		}
	}

	public string Name {
		get {
			return transform.name;
		}
	}
	
	public Vector3 Position {
		get { return proxy.transform.position; }
	}

	public Vector3 Forward {
		get { return proxy.transform.forward; }
	}
		
	public Vector3 Right {
		get { return proxy.transform.right; }
	}
	
	public float Visibility {
		get { 
			float occlusion = 0;
			foreach (bool c in samplePointsVisibility)
				occlusion += c ? 0 : 1.0f / SAMPLES;
			return inFrustum * (1 - occlusion);
		}
	}
	
	public bool Occlusion (SamplePoint c)
	{
		return samplePointsVisibility [(int)c];
	}
	
	public float ProjectionSize {
		get { return projectionSize; }
	}
	
	public Bounds ScreenBounds {
		get {
			return screenSpaceBounds;
		}
	}
	
	public Vector3 PositionOnScreen {
		get { return screenSpaceBounds.center; }
	}
	
	public Actor (Transform transform, Vector3 center, Vector3 scale, PrimitiveType type)
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

	public void Reevaluate (Camera camera)
	{
		if (!Ignore) {
			
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

	public static void ReevaluateAll (Actor[] actors, Camera camera)
	{
		if (actors != null && camera != null)
			foreach (Actor s in actors)
				if (s != null)
					s.Reevaluate (camera);
	}

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
