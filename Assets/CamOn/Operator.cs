using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

[AddComponentMenu("CamOn/Camera Operator")]
public class Operator : MonoBehaviour
{
	public float MovementResponsiveness = 0.95f;
	public float RotationResponsiveness = 0.95f;
	private Vector3 velocity = Vector3.zero;
	
	[SerializeField]
	Shot shot;
	[SerializeField]
	Transform[] subjectsTransform;

	[SerializeField]
	Vector3[] subjectsCenter;

	[SerializeField]
	Vector3[] subjectsScale;
	
    Subject[] subjects;
	readonly Solver solver = new ArtificialPotentialField();
	//readonly Solver solver = new GreedyPSO (0.7298f,2.05f,2.05f,30);
	//readonly Solver solver = new GeneticAlgorithm (0.6f,0.7f,0.7f,30);
	Transform bestCamera;

	bool firstRun = true;

	public Transform EvaluationCamera {
		get {
			return bestCamera;
		}
	}
	
	
	public Subject[] Subjects {
		get {
			return subjects;
		}
	}
	
	public Shot Shot {
		set {
			if (value != shot) {
				shot = value;
				if (shot != null){
					subjectsTransform = new Transform[shot.NumberOfSubjects];
					subjectsScale = new Vector3[shot.NumberOfSubjects];
					subjectsCenter = new Vector3[shot.NumberOfSubjects];

					for (int i = 0; i<shot.NumberOfSubjects;i++){
						subjectsCenter[i] = shot.SubjectCenters[i];
						subjectsScale[i] = shot.SubjectScales[i];
					}
				}
				Reset ();
			}	
		}
		get {
			return shot;
		}
	}
	
	public int SubjectsCount {
		get {
			return subjectsTransform.Length;
		}
	}
	
	public void SetSubjectTransform(int i, Transform f){
		if (f != subjectsTransform[i]){
			subjectsTransform[i] = f;
			Reset();
		}
	}
	
	public Transform GetSubjectTransform(int i){
		return subjectsTransform[i];
	}

	public void SetSubjectScale(int i, Vector3 f){
		if (f != subjectsScale[i]){
			subjectsScale[i] = f;
			Reset();
		}
	}
	
	public Vector3 GetSubjectScale(int i){
		return subjectsScale[i];
	}

	public void SetSubjectCenter(int i, Vector3 f){
		if (f != subjectsCenter[i]){
			subjectsCenter[i] = f;
			Reset();
		}
	}
	
	public Vector3 GetSubjectCenter(int i){
		return subjectsCenter[i];
	}

	public bool ReadyForEvaluation {
		get {
			if (Shot == null || Shot.Properties == null)
				return false;
			
			if (subjects == null)
				return false;
			
			foreach (Subject s in subjects)
				if (s == null)
					return false;
			
			return true;
		}
	}
	

	
	// Use this for initialization
	void Start ()
	{
		bestCamera = (Transform)GameObject.Instantiate (transform);
		GameObject.DestroyImmediate (bestCamera.GetComponent<Operator> ());
		GameObject.DestroyImmediate (bestCamera.GetComponent<AudioListener> ());
		bestCamera.gameObject.SetActive (false);
		
		Reset ();
	}
	
	public void  Reset ()
	{
		//Stop the solver
		solver.Stop ();
		
		//Clean all the proxies in the scene
		while (GameObject.Find(Subject.PROXY_NAME) != null)
			GameObject.DestroyImmediate (GameObject.Find (Subject.PROXY_NAME));
		
		if (shot == null){
			subjectsTransform = null;
			subjects = null;
		} else {	
			shot.FixPropertyTypes ();
			subjects = new Subject[subjectsTransform.Length];
		
			for (int i=0; i<subjectsTransform.Length; i++)
				if (subjectsTransform [i] != null)
					subjects [i] = new Subject (subjectsTransform [i], subjectsCenter [i], subjectsScale [i], shot.SubjectBounds [i]);
		
			if (ReadyForEvaluation && Application.isPlaying)
				solver.Start (bestCamera, subjects);
		}
	}

    float timeLimit = 0.01f;
	void Update ()
	{
        if (Time.deltaTime < 1.0f/60)
            timeLimit *= 1.1f;
        else
            timeLimit *= 0.9f;

        timeLimit = Mathf.Max(timeLimit, 0.01f);

		if (ReadyForEvaluation) 
            solver.Update(bestCamera, subjects, shot,timeLimit);

		float dampening = Mathf.Pow(solver.Satisfaction,4);

		transform.position = Vector3.SmoothDamp(transform.position, bestCamera.position, ref velocity, 1.05f-MovementResponsiveness*dampening);
		transform.rotation = Quaternion.Slerp(transform.rotation, bestCamera.rotation, Time.deltaTime * (0.1f + RotationResponsiveness*dampening*0.9f)*2);
	}

	void OnDrawGizmos ()
	{
		//if (!Application.isPlaying)
		//	while (GameObject.Find(Subject.PROXY_NAME) != null)
		//		GameObject.DestroyImmediate (GameObject.Find (Subject.PROXY_NAME));
		
		if (ReadyForEvaluation) {
			shot.UpdateSubjects (subjects, camera);
			shot.Evaluate ();
		}
		
		if (subjects != null)
			foreach (Subject s in subjects)
				if (s != null) 
					s.DrawGizmos ();
		
		solver.DrawGizmos ();
	}

	public void SelectShot(Shot shot, Transform [] actors, Vector3[] offsets=null, Vector3[] scales=null){
		Shot = shot;
		for (int i=0;i<actors.Length;i++)
			SetSubjectTransform(i,actors[i]);

		if (offsets != null)
			for (int i=0;i<offsets.Length;i++)
				SetSubjectCenter(i,offsets[i]);

		if (scales != null)
			for (int i=0;i<scales.Length;i++)
				SetSubjectScale(i,scales[i]);

		if (firstRun) {
			transform.position = bestCamera.position;
			transform.forward = bestCamera.forward;
			firstRun = false;
		}
	}
}

