using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// This is a general solver class which can be extended to define custom solvers
/// </summary>
public abstract class Solver
{
	private Queue<Vector3> forwardTrace = new Queue<Vector3>(TRACE_LENGHT);
	private Queue<Vector3> positionTrace = new Queue<Vector3>(TRACE_LENGHT);
	private Queue<float> fitnessTrace = new Queue<float>(TRACE_LENGHT);
	private bool running = true;
	private float satisfaction = 0;
	
	/// <summary>
	/// Defines the length fo the debug trace (it is used only in editor mode)
	/// </summary>
	public const int TRACE_LENGHT = 100;
		
	/// <summary>
	/// Returns the quality of the current best found solution
	/// </summary>
	/// <value>[0,1] A value describing how much the current best solution satisfies the shot requirements.</value>
	public float Satisfaction {
		get {
			return satisfaction;
		}
	}
	
	protected void logTrace(Vector3 position, Vector3 forward, float fitness){
		if (Application.isEditor) {
			if (positionTrace.Count >= TRACE_LENGHT){
				positionTrace.Dequeue();
				fitnessTrace.Dequeue();
				forwardTrace.Dequeue();
			}
			
			positionTrace.Enqueue(position);
			fitnessTrace.Enqueue(fitness);
			forwardTrace.Enqueue(forward);
		}
	}
	
	public virtual void DrawGizmos(){
		for (int i=0;i<positionTrace.Count;i++){
			Gizmos.color = Color.red*fitnessTrace.ElementAt(i);
			Gizmos.DrawCube(positionTrace.ElementAt(i),0.1f*Vector3.one);
			
			Gizmos.color = Color.blue*fitnessTrace.ElementAt(i);
			Gizmos.DrawLine(positionTrace.ElementAt(i),positionTrace.ElementAt(i)+forwardTrace.ElementAt(i));
		}
	}

	/// <summary>
	/// Updates the solver for a given amount of time
	/// </summary>
	/// <param name="bestCamera">curret camera to be animated</param>
	/// <param name="subjects">subjects in the shot</param>
	/// <param name="shot">shot to be generated</param>
	/// <param name="maxExecutionTime">maximum execution time.</param>
	public float Update (Transform bestCamera, Actor[] subjects, Shot shot, float maxExecutionTime){
		if (running){
			satisfaction = update (bestCamera,subjects,shot,maxExecutionTime);
		}else 
			satisfaction = 0;

		return satisfaction;
	}

	/// <summary>
	/// Enabling of the solver
	/// </summary>
	/// <param name="bestCamera">curret camera to be animated</param>
	/// <param name="subjects">subjects in the shot</param>
	/// <param name="shot">shot to be generated</param>
	public virtual void Start(Transform bestCamera, Actor[] subjects, Shot shot){
		if (bestCamera == null)
			throw new MissingReferenceException ("camera not initilised");
			
		initBestCamera (bestCamera, subjects, shot);
		running = true;
	}

	/// <summary>
	/// Disabling the solver
	/// </summary>
	public virtual void Stop() {
		running = false;
	}

	/// <summary>
	/// Given a list of ubjects, it calculates a position in the middle.
	/// </summary>
	/// <returns>A position wich is central to the subjects passed.</returns>
	/// <param name="subjects">The subjects in the shot.</param>
	public static Vector3 SubjectsCenter (Actor[] subjects)
	{
		Vector3 center = Vector3.zero;
		foreach (Actor s in subjects)
			if (s != null)
				center += s.Position / subjects.Length;
		return center;
	}

	/// <summary>
	/// Given a list of subjects, it calculates a sphere that contains them all.
	/// </summary>
	/// <returns>The radius of the sphere.</returns>
	/// <param name="subjects">The subjects in the shot.</param>
	public static float SubjectsRadius (Actor[] subjects)
	{
		Vector3 center = SubjectsCenter (subjects);
		float radius = 0;
		foreach (Actor s in subjects) {
			if (s != null) {
				float distance = (s.Position - center).magnitude + s.Scale.magnitude;
				if (distance > radius)
					radius = distance;
			}
		}
		
		return radius;
	}

	/// <summary>
	/// Sets the position to the camera controlling if the camera is locked
	/// </summary>
	/// <param name="position">The position to be set.</param>
	/// <param name="bestCamera">The camera on which to set the position.</param>
	/// <param name="shot">The current shot.</param>
	protected void setPosition(Vector3 position, Transform bestCamera, Shot shot){
		if (shot.LockX)
			position.x = bestCamera.position.x;
		if (shot.LockY)
			position.y = bestCamera.position.y;
		if (shot.LockZ)
			position.z = bestCamera.position.z;
		
		bestCamera.position = position;
	}
	
	abstract protected float update(Transform bestCamera, Actor[] subjects, Shot shot, float maxExecutionTime);
	abstract protected void initBestCamera (Transform bestCamera, Actor[] subjects, Shot shot);
}

