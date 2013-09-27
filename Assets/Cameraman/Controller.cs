using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//[ExecuteInEditMode()]
[AddComponentMenu("Camera/Controller (CamOn)")]
public class Controller : MonoBehaviour
{
	public Shot Shot;
	public Transform[] SubjectsTransform;
	public Subject[] Subjects;
	
	public float MovementSpeed = 10f;
	public float RotationSpeed = 1f;
	
	public int EvaluationsPerSecond {
		get {return solver.EvaluationsPerSecond;}
	}
	
	private Solver solver = new HillClimber();
	Transform bestCamera;
	
	// Use this for initialization
	void Start ()
	{
		if (Shot != null)
			Shot.FixPropertyTypes ();
		
		if (SubjectsTransform != null)
			Subjects = new Subject[SubjectsTransform.Length];
		
		for (int i=0; i<SubjectsTransform.Length; i++)
			if (SubjectsTransform [i] != null)
				Subjects [i] = new Subject (SubjectsTransform [i], Shot.SubjectCenters [i], Shot.SubjectScales [i], Shot.SubjectBounds [i]);
		
		bestCamera = (Transform)GameObject.Instantiate(transform);
		GameObject.DestroyImmediate(bestCamera.GetComponent<Controller>());
		GameObject.DestroyImmediate(bestCamera.GetComponent<AudioListener>());
		bestCamera.gameObject.SetActive(false);
	}
	
	void FixedUpdate ()
	{
		if (Shot != null)
			solver.Update(bestCamera,Subjects,Shot,10);
		
		float distance = (transform.position-bestCamera.position).magnitude;
		
		transform.position = Vector3.Lerp(transform.position,bestCamera.position,distance/MovementSpeed);
		transform.rotation = Quaternion.Slerp(transform.rotation,bestCamera.rotation,Time.deltaTime*RotationSpeed);
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
		
		if (Shot != null) {
			Shot.UpdateSubjects (Subjects, camera);
			Shot.Evaluate ();
		}
		
		if (Subjects != null)
			foreach (Subject s in Subjects)
				if (s != null) 
					s.DrawGizmos ();
		
		solver.DrawGizmos();
	}
}

