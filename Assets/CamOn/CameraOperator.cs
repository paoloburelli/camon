using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;


/// <summary>
/// Main component of CamOn.
/// </summary>
[AddComponentMenu("CamOn/Camera Operator")]
public class CameraOperator : MonoBehaviour
{
	public enum Transition {Cut, Smooth};
	public float MovementResponsiveness = 0.95f;
	public float RotationResponsiveness = 0.95f;
	
	[SerializeField]
	Shot shot;
	[SerializeField]
	Transform[] subjectsTransform;

	[SerializeField]
	Vector3[] subjectsCenter;

	[SerializeField]
	Vector3[] subjectsScale;

	readonly Solver solver = new ArtificialPotentialField();
	Actor[] actors;
	Transform bestCamera;
	Vector3 velocity = Vector3.zero;
	bool started = false;

	public Transform EvaluationCamera {
		get {
			return bestCamera;
		}
	}

	public Actor[] Actors {
		get {
			return actors;
		}
	}
	
	public Shot Shot {
		set {
			if (value != shot) {
				shot = value;
				if (shot != null){
					subjectsTransform = new Transform[shot.NumberOfActors];
					subjectsScale = new Vector3[shot.NumberOfActors];
					subjectsCenter = new Vector3[shot.NumberOfActors];

					for (int i = 0; i<shot.NumberOfActors;i++){
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
	
	public int ActorsCount {
		get {
			return subjectsTransform.Length;
		}
	}
	
	public void AssignActorTransform(int actorIndex, Transform f){
		if (f != subjectsTransform[actorIndex]){
			subjectsTransform[actorIndex] = f;
			Reset();
		}
	}
	
	public Transform GetActorTransform(int actorIndex){
		return subjectsTransform[actorIndex];
	}

	public void ModifyActorScale(int i, Vector3 f){
		if (f != subjectsScale[i]){
			subjectsScale[i] = f;
			Reset();
		}
	}
	
	public Vector3 GetActorScale(int i){
		return subjectsScale[i];
	}

	public void ModifyActorOffest(int i, Vector3 f){
		if (f != subjectsCenter[i]){
			subjectsCenter[i] = f;
			Reset();
		}
	}
	
	public Vector3 GetActorOffset(int i){
		return subjectsCenter[i];
	}

	public bool ReadyForEvaluation {
		get {
			if (Shot == null || Shot.Properties == null)
				return false;
			
			if (actors == null)
				return false;
			
			foreach (Actor s in actors)
				if (s == null)
					return false;
			
			return true;
		}
	}

	// Use this for initialization
	void Start ()
	{
		if (!started) {
			bestCamera = (Transform)GameObject.Instantiate (transform);
			GameObject.DestroyImmediate (bestCamera.GetComponent<CameraOperator> ());
			GameObject.DestroyImmediate (bestCamera.GetComponent<AudioListener> ());
			bestCamera.gameObject.SetActive (false);
			
			Reset ();

			transform.position = bestCamera.position;
			transform.forward = bestCamera.forward;

			started = true;
		}
	}
	
	public void  Reset ()
	{
		//Stop the solver
		solver.Stop ();
		
		//Clean all the proxies in the scene
		Actor.DestroyAllProxies ();
		
		if (shot == null){
			subjectsTransform = null;
			actors = null;
		} else {	
			shot.FixPropertyTypes ();
			actors = new Actor[subjectsTransform.Length];
		
			for (int i=0; i<subjectsTransform.Length; i++)
				if (subjectsTransform [i] != null)
					actors [i] = new Actor (subjectsTransform [i], subjectsCenter [i], subjectsScale [i], shot.SubjectBounds [i]);
		
			if (ReadyForEvaluation && Application.isPlaying)
				solver.Start (bestCamera, actors, shot);
		}
	}

    float timeLimit = 0.1f;
	void Update ()
	{
        if (Time.deltaTime < 1.0f/60)
            timeLimit *= 1.1f;
        else
            timeLimit *= 0.9f;

        timeLimit = Mathf.Max(timeLimit, 0.016f);

		if (ReadyForEvaluation) 
            solver.Update(bestCamera, actors, shot,timeLimit);

		float dampening = Mathf.Pow(solver.Satisfaction,4);

		transform.position = Vector3.SmoothDamp(transform.position, bestCamera.position, ref velocity, 1.05f-MovementResponsiveness*dampening);
		transform.rotation = Quaternion.Slerp(transform.rotation, bestCamera.rotation, Time.deltaTime * (0.1f + RotationResponsiveness*dampening*0.9f)*2);
	}

	void OnDrawGizmos ()
	{	
		if (ReadyForEvaluation) {
			shot.GetQuality (actors,GetComponent<Camera>());
		}
		
		if (actors != null)
			foreach (Actor s in actors)
				if (s != null) 
					s.DrawGizmos ();
		
		solver.DrawGizmos ();
	}

	public void SelectShot(Shot shot, Transition transition, Transform [] actors, Vector3[] offsets=null, Vector3[] scales=null){
		Shot = shot;
		for (int i=0;i<actors.Length;i++)
			AssignActorTransform(i,actors[i]);

		if (offsets != null)
			for (int i=0;i<offsets.Length;i++)
				ModifyActorOffest(i,offsets[i]);

		if (scales != null)
			for (int i=0;i<scales.Length;i++)
				ModifyActorScale(i,scales[i]);

		if (transition == Transition.Cut){
			transform.position = bestCamera.position;
			transform.forward = bestCamera.forward;
		}
	}
		
	public static CameraOperator On(Camera camera){
		CameraOperator co = camera.GetComponent<CameraOperator> ();

		if (co == null) {
			co = camera.gameObject.AddComponent<CameraOperator> ();
			co.Start ();
		}

		return co;
	}

	public static CameraOperator OnMainCamera {
		get {
			return On (Camera.main);
		}
	}
}

