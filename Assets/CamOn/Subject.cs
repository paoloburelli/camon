using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Subject
{
	public const int SAMPLES = 5;
	public enum Corner
	{
		Top,
		Bottom,
		Left,
		Right,
		Center
	};
	
	const int LAYER_MASK = ~6; //do not test transparent and ignore raycast;
	public const string PROXY_NAME = "[Camera Proxy]";
	public const string IGNORE_TAG = "IGNORE";

	public bool Ignore {
		get {
			return transform.tag == IGNORE_TAG;
		}
		set {
			transform.tag = value ? IGNORE_TAG : "Untagged";
		}
	}
	
	Transform transform;
	MeshRenderer proxyRenderer;
	Mesh proxyMesh;
	GameObject proxy;
	Vector3[] onScreenSamplePoints = new Vector3[SAMPLES];
	bool[] samplePointsVisibility = new bool[SAMPLES];
	float inFrustum;
	float projectionSize;
	Vector2 vantageAngle = new Vector2 (0, 0);
	Bounds screenSpaceBounds = new Bounds(Vector3.zero,Vector3.zero);
	Vector3 screenMin = Vector3.one;
	Vector3 screenMax = Vector3.zero;

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
	
	public float Visibility {
		get { 
			float occlusion = 0;
			foreach (bool c in samplePointsVisibility)
				occlusion += c ? 0 : 1.0f / SAMPLES;
			return inFrustum * (1 - occlusion);
		}
	}
	
	public bool Occlusion (Corner c)
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
	
	public Vector2 VantageAngle {
		get { return vantageAngle; }
	}
	
	public Subject (Transform transform, Vector3 center, Vector3 scale, PrimitiveType type)
	{
		if (transform == null)
			throw new System.ArgumentNullException ();
			
		this.transform = transform;
		DestroyProxies ();
		
		proxy = GameObject.CreatePrimitive (type);
		GameObject.DestroyImmediate (proxy.collider);
		proxy.renderer.sharedMaterial = new Material (Shader.Find ("Transparent/Diffuse"));
		proxy.renderer.sharedMaterial.color = new Color (1, 0, 1, 0.4f);


		proxy.transform.parent = transform;
		proxy.transform.localPosition = center;
		proxy.transform.localScale = scale;
		proxy.transform.localRotation = Quaternion.Euler(0,0,0);
		proxy.name = PROXY_NAME;
		proxyRenderer = proxy.GetComponent<MeshRenderer> ();
		proxyMesh = proxy.GetComponent<MeshFilter> ().sharedMesh;
		proxy.SetActive (transform.gameObject.activeInHierarchy);
		proxyRenderer.enabled = false;
	}
	
	public void DestroyProxies ()
	{
		proxyRenderer = null;
		proxyMesh = null;
		if (transform != null)
			while (transform.Find (PROXY_NAME) != null)
				GameObject.DestroyImmediate (transform.Find (PROXY_NAME).gameObject);
	}
	
	public void Update (Camera camera)
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
						onScreenSamplePoints [(int)Corner.Top] = tv;
					}
					if (sv.y < screenMin.y) {
						screenMin.y = sv.y;
						onScreenSamplePoints [(int)Corner.Bottom] = tv;
					}
					if (sv.x > screenMax.x) {
						screenMax.x = sv.x;
						onScreenSamplePoints [(int)Corner.Right] = tv;
					}
					if (sv.x < screenMin.x) {
						screenMin.x = sv.x;
						onScreenSamplePoints [(int)Corner.Left] = tv;
					}
					if (sv.z > screenMax.z)
						screenMax.z = sv.z;
					
					if (sv.z < screenMin.z)
						screenMin.z = sv.z;
				}
			}
			onScreenSamplePoints [(int)Corner.Center] = (onScreenSamplePoints [(int)Corner.Top] + onScreenSamplePoints [(int)Corner.Bottom] + onScreenSamplePoints [(int)Corner.Right]+ onScreenSamplePoints [(int)Corner.Left])/4 + (onScreenSamplePoints [(int)Corner.Right] - onScreenSamplePoints [(int)Corner.Left]) * Random.value * 0.2f;
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
			Vector3 relativeCameraPos = proxy.transform.InverseTransformPoint(camera.transform.position).normalized;
			vantageAngle.y = Mathf.Asin(relativeCameraPos.y) * Mathf.Rad2Deg;
			vantageAngle.x = -Mathf.Atan2(relativeCameraPos.x,relativeCameraPos.z) * Mathf.Rad2Deg;
			
			transform.gameObject.layer = originalLayer;
		}
	}
	
	public void DrawGizmos ()
	{
		if (inFrustum > 0) {
			for (int i =0; i<SAMPLES; i++)
				if (samplePointsVisibility [i]) {
					Gizmos.color = Color.green;
					Gizmos.DrawSphere (onScreenSamplePoints [i], 0.05f * proxy.renderer.bounds.size.y);
				} else {
					Gizmos.color = Color.red;
					Gizmos.DrawWireSphere (onScreenSamplePoints [i], 0.05f * proxy.renderer.bounds.size.y);
				}
		}
	}
}
