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
	Actor[] actors;

	readonly Solver solver = new ArtificialPotentialField();
	Transform bestCamera;
	Vector3 velocity = Vector3.zero;
	bool started = false;
	Transition transition = Transition.Cut;

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
	/// Returns the current list of actors evaluated
	/// </summary>
	/// <value>The subjects.</value>
	public Actor[] Actors {
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
				shot.FixPropertiesType();
				actors = new Actor[shot.NumberOfActors];
			}	
		}
		get {
			return shot;
		}
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


			foreach (Actor s in actors)
				if (s == null)
					return false;
			
			return true;
		}
	}
		
	void Start ()
	{
		if (!started){
			bestCamera = (Transform)GameObject.Instantiate(transform,transform.position,transform.rotation);
			GameObject.DestroyImmediate (bestCamera.GetComponent<CameraOperator> ());
			GameObject.DestroyImmediate (bestCamera.GetComponent<AudioListener> ());
			bestCamera.gameObject.SetActive (false);
			started = true;
		}

		if (shot != null && actors != null){
			shot.FixPropertiesType();
			solver.Stop ();
			for (int i=0;i<actors.Length;i++)
				if (actors[i] != null)
					actors[i].SetAreaOfInterest(shot.VolumesOfInterestSize[i],shot.VolumesOfInterestPosition[i]);
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

		if (transition == Transition.Cut) {
			transform.position = bestCamera.position;
			transform.rotation = bestCamera.rotation;
			transition = Transition.Smooth;
		} else {
			transform.position = Vector3.SmoothDamp(transform.position, bestCamera.position, ref velocity, 1.05f-MovementResponsiveness*dampening);
			transform.rotation = Quaternion.Slerp(transform.rotation, bestCamera.rotation, Time.deltaTime * (0.1f + RotationResponsiveness*dampening*0.9f)*2);
		}
	}

	void OnDrawGizmos ()
	{				
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
	public void SelectShot(Shot shot, Transition transition, Actor [] actors){
		this.Shot = shot;
		for (int i=0;i<Mathf.Min (actors.Length,this.actors.Length);i++)
			this.actors[i] = actors[i];
		this.transition = transition;
		Start();
	}

	/// <summary>
	/// Returns the camera operator assigned to a specific camera.
	/// If no operator exists, a new one is assigned.
	/// </summary>
	/// <param name="camera">Camera.</param>
	public static CameraOperator On(Camera camera){
		CameraOperator co = camera.GetComponent<CameraOperator> ();

		if (co == null)
			co = camera.gameObject.AddComponent<CameraOperator> ();

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

