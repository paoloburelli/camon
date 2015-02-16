using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Subject
{
	public const int CORNERS_COUNT = 4;
	public enum Corner
	{
		Top,
		Bottom,
		Left,
		Right
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
	Vector3[] onScreenCorners = new Vector3[CORNERS_COUNT];
	bool[] onScreenCornersVisibility = new bool[CORNERS_COUNT];
	float onScreenFraction;
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

	public Vector3 Scale {
		get {
			return proxy.transform.localScale;
		}
		set {
			proxy.transform.localScale = value;
		}
	}

	public Vector3 Center {
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
			foreach (bool c in onScreenCornersVisibility)
				occlusion += c ? 0 : 1.0f / CORNERS_COUNT;
			return onScreenFraction * (1 - occlusion);
		}
	}
	
	public bool Occlusion (Corner c)
	{
		return onScreenCornersVisibility [(int)c];
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
			
			onScreenFraction = 0;
			screenMin = Vector3.one;
			screenMin.z = float.PositiveInfinity;
			screenMax = Vector3.zero;

			foreach (Vector3 v in proxyMesh.vertices) {
				Vector3 tv = proxy.transform.TransformPoint (v);
				Vector3 sv = camera.WorldToViewportPoint (tv);
				if (sv.z > 0 && sv.y < 1 && sv.y > 0 && sv.x > 0 && sv.x < 1) {
					onScreenFraction++;
					if (sv.y > screenMax.y) {
						screenMax.y = sv.y;
						onScreenCorners [(int)Corner.Top] = tv;
					}
					if (sv.y < screenMin.y) {
						screenMin.y = sv.y;
						onScreenCorners [(int)Corner.Bottom] = tv;
					}
					if (sv.x > screenMax.x) {
						screenMax.x = sv.x;
						onScreenCorners [(int)Corner.Right] = tv;
					}
					if (sv.x < screenMin.x) {
						screenMin.x = sv.x;
						onScreenCorners [(int)Corner.Left] = tv;
					}
					if (sv.z > screenMax.z)
						screenMax.z = sv.z;
					
					if (sv.z < screenMin.z)
						screenMin.z = sv.z;
				}
			}
			onScreenFraction *= 1.0f / proxyMesh.vertices.LongLength;
			
			if (onScreenFraction > 0) {
				for (int i=0; i<CORNERS_COUNT; i++) {
					Vector3 direction = camera.transform.position - onScreenCorners [i];
					onScreenCornersVisibility [i] = !Physics.Raycast (onScreenCorners [i], direction.normalized, direction.magnitude, LAYER_MASK);
				}
				
				float height = screenMax.y - screenMin.y;
				float width = screenMax.x - screenMin.x;
				
				projectionSize = height > width ? height : width;
				projectionSize *= 1.1f; //this gives some borders around the object on the screen
				
			} else {
				for (int i=0; i<CORNERS_COUNT; i++) {
					onScreenCorners [i] = Vector3.zero;
					onScreenCornersVisibility [i] = false;
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
		if (onScreenFraction > 0) {
			for (int i =0; i<CORNERS_COUNT; i++)
				if (onScreenCornersVisibility [i]) {
					Gizmos.color = Color.green;
					Gizmos.DrawSphere (onScreenCorners [i], 0.05f * proxy.renderer.bounds.size.y);
				} else {
					Gizmos.color = Color.red;
					Gizmos.DrawWireSphere (onScreenCorners [i], 0.05f * proxy.renderer.bounds.size.y);
				}
		}
	}
}
