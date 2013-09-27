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
	public const string BOUNDS_NAME = "[Camera Proxy]";
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
	Vector2 onScreenPosition = new Vector2 (0, 0);
	Vector2 vantageAngle = new Vector2 (0, 0);
	
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
	
	public Vector2 PositionOnScreen {
		get { return onScreenPosition; }
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
		proxy.transform.localPosition = Vector3.Scale (transform.renderer.bounds.size, center - Vector3.one * 0.5f);
		proxy.transform.localScale = scale;
		proxy.name = BOUNDS_NAME;
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
			while (transform.Find (BOUNDS_NAME) != null)
				GameObject.DestroyImmediate (transform.Find (BOUNDS_NAME).gameObject);
	}

	public void Update (Camera camera)
	{
		if (!Ignore) {
			
			int originalLayer = transform.gameObject.layer;
			transform.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			
			onScreenFraction = 0;
			float screenTop = 0, screenBottom = 1, screenRight = 0, screenLeft = 1;
		
			foreach (Vector3 v in proxyMesh.vertices) {
				Vector3 tv = proxy.transform.TransformPoint (v);
				Vector3 sv = camera.WorldToViewportPoint (tv);
				if (sv.z > 0 && sv.y < 1 && sv.y > 0 && sv.x > 0 && sv.x < 1) {
					onScreenFraction++;
					if (sv.y > screenTop) {
						screenTop = sv.y;
						onScreenCorners [(int)Corner.Top] = tv;
					}
					if (sv.y < screenBottom) {
						screenBottom = sv.y;
						onScreenCorners [(int)Corner.Bottom] = tv;
					}
					if (sv.x > screenRight) {
						screenRight = sv.x;
						onScreenCorners [(int)Corner.Right] = tv;
					}
					if (sv.x < screenLeft) {
						screenLeft = sv.x;
						onScreenCorners [(int)Corner.Left] = tv;
					}
				}
			}
			onScreenFraction *= 1.0f / proxyMesh.vertices.LongLength;
			
			if (onScreenFraction > 0) {
				for (int i=0; i<CORNERS_COUNT; i++) {
					Vector3 direction = camera.transform.position - onScreenCorners [i];
					onScreenCornersVisibility [i] = !Physics.Raycast (onScreenCorners [i], direction.normalized, direction.magnitude, LAYER_MASK);
				}
				
				float height = (screenTop - screenBottom);
				float width = (screenRight - screenLeft);
				projectionSize = height > width ? height : width;
				projectionSize *= 1.1f; //this gives some borders around the object on the screen
					
				onScreenPosition.x = screenLeft + width / 2;
				onScreenPosition.y = screenBottom + height / 2;
				
			} else {
				for (int i=0; i<CORNERS_COUNT; i++) {
					onScreenCorners [i] = Vector3.zero;
					onScreenCornersVisibility [i] = false;
				}

				projectionSize = float.NegativeInfinity;
						
				Vector3 viewPortSpaceCenter = camera.WorldToViewportPoint (Position);
					
				onScreenPosition.x = viewPortSpaceCenter.x > 0 ? viewPortSpaceCenter.x > 1 ? float.PositiveInfinity : float.NaN : float.NegativeInfinity;
				onScreenPosition.y = viewPortSpaceCenter.y > 0 ? viewPortSpaceCenter.y > 1 ? float.PositiveInfinity : float.NaN : float.NegativeInfinity;
			}
			
			Vector3 relativeCameraPos = proxy.transform.InverseTransformPoint(camera.transform.position).normalized;
			vantageAngle.y = Mathf.Asin(relativeCameraPos.y) * Mathf.Rad2Deg;
			vantageAngle.x = -Mathf.Atan2(relativeCameraPos.x,-relativeCameraPos.z) * Mathf.Rad2Deg;
			
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
