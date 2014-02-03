using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

[AddComponentMenu("Camera/Cameraman")]
public class Cameraman : MonoBehaviour
{
	[SerializeField]
	protected Shot shot;

	public Shot Shot {
		set {
			if (value != shot) {
				shot = value;
				subjectsTransform = new Transform[shot.NumberOfSubjects];
				if (Application.isPlaying)
					Reset ();
			}	
		}
		get {
			return shot;
		}
	}
	
	[SerializeField]
	protected Transform[] subjectsTransform;
	
	public IEnumerator SubjectTrasnforms {
		get {
			return subjectsTransform.GetEnumerator();
		}
	}
	
	public int SubjectTransformsCount {
		get {
			return subjectsTransform.Length;
		}
	}
	
	public void SetSubjectTransform(int i, Transform f){
		subjectsTransform[i] = f;
	}
	
	public Transform GetSubjectTransform(int i){
		return subjectsTransform[i];
	}
	
	public Subject[] Subjects;
	public float MovementSpeed = 10f;
	public float RotationSpeed = 1f;
	
	public int EvaluationsPerSecond {
		get { return solver.EvaluationsPerSecond;}
	}
	
	private Solver solver = new HillClimber ();
	Transform bestCamera;
	
	// Use this for initialization
	void Start ()
	{
		if (shot != null) {
			shot.FixPropertyTypes ();
			
			if (subjectsTransform != null)
				Subjects = new Subject[subjectsTransform.Length];
			
			for (int i=0; i<subjectsTransform.Length; i++)
				if (subjectsTransform [i] != null)
					Subjects [i] = new Subject (subjectsTransform [i], shot.SubjectCenters [i], shot.SubjectScales [i], shot.SubjectBounds [i]);
			
			bestCamera = (Transform)GameObject.Instantiate (transform);
			GameObject.DestroyImmediate (bestCamera.GetComponent<Cameraman> ());
			GameObject.DestroyImmediate (bestCamera.GetComponent<AudioListener> ());
			bestCamera.gameObject.SetActive (false);
			
			solver.Start (bestCamera, Subjects, shot);
		}
	}
	
	public void  Reset ()
	{
		solver.Stop ();
		
		if (Subjects != null)
			foreach (Subject s in Subjects)
				if (s != null)
					s.DestroyProxies ();
		
		if (shot != null) {
			shot.FixPropertyTypes ();
			
			if (subjectsTransform != null)
				Subjects = new Subject[subjectsTransform.Length];
		
			bool ready = true;
			for (int i=0; i<subjectsTransform.Length; i++)
				if (subjectsTransform [i] != null)
					Subjects [i] = new Subject (subjectsTransform [i], shot.SubjectCenters [i], shot.SubjectScales [i], shot.SubjectBounds [i]);
				else
					ready = false;
		
			if (ready)
				solver.Start (bestCamera, Subjects, shot);
		}
	}
	
	void FixedUpdate ()
	{
		if (shot != null) {
			bool eval = Subjects != null;
			if (eval)
				foreach (Subject s in Subjects)
					if (s == null)
						eval = false;
			
			if (eval) {
				solver.Update (bestCamera, Subjects, shot, 10);
				
				float distance = (transform.position - bestCamera.position).magnitude;
				transform.position = Vector3.Lerp (transform.position, bestCamera.position, distance / MovementSpeed);
				transform.rotation = Quaternion.Slerp (transform.rotation, bestCamera.rotation, 0.01f * RotationSpeed);
			}
		}
	}
	
	void OnApplicationQuit ()
	{
		if (Subjects != null)
			foreach (Subject s in Subjects)
				if (s != null)
					s.DestroyProxies ();
	}
	
	void OnDrawGizmos ()
	{
		if (!Application.isPlaying)
			while (GameObject.Find(Subject.PROXY_NAME) != null)
				GameObject.DestroyImmediate (GameObject.Find (Subject.PROXY_NAME));
		
		if (shot != null) {
			shot.UpdateSubjects (Subjects, camera);
			shot.Evaluate ();
		}
		
		if (Subjects != null)
			foreach (Subject s in Subjects)
				if (s != null) 
					s.DrawGizmos ();
		
		solver.DrawGizmos ();
	}
}

