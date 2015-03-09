using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;


/// <summary>
/// Main component of CamOn. 
/// A camera operator can be asigned to a camera using the "On" method or by adding the component to a camera
/// A shot can be selected using the SelectShot method or manually in the unity editor
/// </summary>
[AddComponentMenu("CamOn/Camera Operator")]
public class CameraOperator : MonoBehaviour
{
	/// <summary>
	/// Types of transition:
	/// </summary>
	public enum Transition {
		/// <summary>
		/// The camre switches directly from one shot to the next one
		/// </summary>
		Cut,
		/// <summary>
		/// The camera smoothy animates from the current shot to the new one
		/// </summary>
		Smooth
	};

	/// <summary>
	/// The movement responsiveness.
	/// </summary>
	public float MovementResponsiveness = 0.95f;
	/// <summary>
	/// The rotation responsiveness.
	/// </summary>
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
	SubjectEvaluator[] actors;
	Transform bestCamera;
	Vector3 velocity = Vector3.zero;
	bool started = false;
	Transition transition = Transition.Smooth;

	/// <summary>
	/// Return the camera used internally for the solver computations
	/// </summary>
	/// <value>The evaluation camera.</value>
	public Transform EvaluationCamera {
		get {
			return bestCamera;
		}
	}

	/// <summary>
	/// Returns the current list of subjects evaluated
	/// </summary>
	/// <value>The subjects.</value>
	public SubjectEvaluator[] Subjects {
		get {
			return actors;
		}
	}

	/// <summary>
	/// Gets or sets the shot that will drive the camera.
	/// </summary>
	/// <value>The shot.</value>
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

	/// <summary>
	/// Gets the subjects count.
	/// </summary>
	/// <value>The subjects count.</value>
	public int SubjectsCount {
		get {
			return subjectsTransform.Length;
		}
	}

	/// <summary>
	/// Assigns a transform to a subject in the current shot.
	/// </summary>
	/// <param name="subjectIndex">Subjects index.</param>
	/// <param name="f">The tranform.</param>
	public void AssignSubjectTransform(int subjectIndex, Transform f){
		if (f != subjectsTransform[subjectIndex]){
			subjectsTransform[subjectIndex] = f;
			Reset();
		}
	}

	/// <summary>
	/// Gets a subject's transform.
	/// </summary>
	/// <returns>The subject's transform.</returns>
	/// <param name="subjectIndex">Subject's index.</param>
	public Transform GetSubjectTransform(int subjectIndex){
		return subjectsTransform[subjectIndex];
	}

	/// <summary>
	/// Modifies a subject's scale.
	/// </summary>
	/// <param name="subjectIndex">Subject's index.</param>
	/// <param name="f">The new scale.</param>
	public void ModifySubjectScale(int i, Vector3 f){
		if (f != subjectsScale[i]){
			subjectsScale[i] = f;
			Reset();
		}
	}

	/// <summary>
	/// Gets a subject's scale.
	/// </summary>
	/// <returns>The subject scale.</returns>
	/// <param name="i">The subject's index.</param>
	public Vector3 GetSubjectScale(int i){
		return subjectsScale[i];
	}

	/// <summary>
	/// Modifies a subject's offest.
	/// </summary>
	/// <param name="i">The subject's index.</param>
	/// <param name="f">The new offset.</param>
	public void ModifySubjectOffest(int i, Vector3 f){
		if (f != subjectsCenter[i]){
			subjectsCenter[i] = f;
			Reset();
		}
	}

	/// <summary>
	/// Gets a subject's offset.
	/// </summary>
	/// <returns>The subject offset.</returns>
	/// <param name="i">The subject's index.</param>
	public Vector3 GetSubjectOffset(int i){
		return subjectsCenter[i];
	}

	/// <summary>
	/// Gets a value indicating whether this <see cref="CameraOperator"/> ready for evaluation.
	/// </summary>
	/// <value><c>true</c> if ready for evaluation; otherwise, <c>false</c>.</value>
	public bool ReadyForEvaluation {
		get {
			if (Shot == null || Shot.Properties == null)
				return false;
			
			if (actors == null)
				return false;
			
			foreach (SubjectEvaluator s in actors)
				if (s == null)
					return false;
			
			return true;
		}
	}
		
	void Start ()
	{
		if (!started) {
			bestCamera = (Transform)GameObject.Instantiate(transform,transform.position,transform.rotation);
			GameObject.DestroyImmediate (bestCamera.GetComponent<CameraOperator> ());
			GameObject.DestroyImmediate (bestCamera.GetComponent<AudioListener> ());
			bestCamera.gameObject.SetActive (false);
			
			Reset ();

			transform.position = bestCamera.position;
			transform.forward = bestCamera.forward;

			started = true;
		}
	}

	/// <summary>
	/// Reset this instance and rebilds the subject evaluators.
	/// </summary>
	public void  Reset ()
	{
		//Stop the solver
		solver.Stop ();
		
		//Clean all the proxies in the scene
		SubjectEvaluator.DestroyAllProxies ();
		
		if (shot == null){
			subjectsTransform = null;
			actors = null;
		} else {	
			shot.FixPropertiesType ();
			actors = new SubjectEvaluator[subjectsTransform.Length];
		
			for (int i=0; i<subjectsTransform.Length; i++)
				if (subjectsTransform [i] != null)
					actors [i] = new SubjectEvaluator (subjectsTransform [i], subjectsCenter [i], subjectsScale [i], shot.SubjectBounds [i]);
		
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

		Solver.setPosition(transform.position+solver.SubjectsVelocity*Time.deltaTime,transform,shot);

		float dampening = Mathf.Pow(solver.Satisfaction,4);

		transform.position = Vector3.SmoothDamp(transform.position, bestCamera.position, ref velocity, 1.05f-MovementResponsiveness*dampening);
		transform.rotation = Quaternion.Slerp(transform.rotation, bestCamera.rotation, Time.deltaTime * (0.1f + RotationResponsiveness*dampening*0.9f)*2);


		if (transition == Transition.Cut) {
			transform.position = bestCamera.position;
			transform.rotation = bestCamera.rotation;
			transition = Transition.Smooth;
		}
	}

	void OnDrawGizmos ()
	{	
		if (ReadyForEvaluation) {
			shot.GetQuality (actors,GetComponent<Camera>());
		}
		
		if (actors != null)
			foreach (SubjectEvaluator s in actors)
				if (s != null) 
					s.DrawGizmos ();
		
		solver.DrawGizmos ();
	}

	/// <summary>
	/// Selects a new shot and initiates a transition.
	/// </summary>
	/// <param name="shot">Shot.</param>
	/// <param name="transition">The type fo transition.</param>
	/// <param name="subjectsTransform">A list of the transforms of the subjects of this shot.</param>
	/// <param name="subjectsOffset">An optional list of offset modifiers for the subjects.</param>
	/// <param name="subjectsScale">An optional list of scale modifiers for the subjects.</param>
	public void SelectShot(Shot shot, Transition transition, Transform [] subjectsTransform, Vector3[] subjectsOffset=null, Vector3[] subjectsScale=null){

		Shot = shot;
		for (int i=0;i<subjectsTransform.Length;i++)
			AssignSubjectTransform(i,subjectsTransform[i]);

		if (subjectsOffset != null)
			for (int i=0;i<subjectsOffset.Length;i++)
				ModifySubjectOffest(i,subjectsOffset[i]);

		if (subjectsScale != null)
			for (int i=0;i<subjectsScale.Length;i++)
				ModifySubjectScale(i,subjectsScale[i]);

		this.transition = transition;
	}

	/// <summary>
	/// Returns the camera operator assigned to a specific camera.
	/// If no operator exists, a new one is assigned.
	/// </summary>
	/// <param name="camera">Camera.</param>
	public static CameraOperator On(Camera camera){
		CameraOperator co = camera.GetComponent<CameraOperator> ();

		if (co == null) {
			co = camera.gameObject.AddComponent<CameraOperator> ();
			co.Start ();
		}

		return co;
	}

	/// <summary>
	/// Returns the camera operator assigned to the main camera.
	/// If no operator exists, a new one is assigned.
	/// </summary>
	public static CameraOperator OnMainCamera {
		get {
			return On (Camera.main);
		}
	}
}

