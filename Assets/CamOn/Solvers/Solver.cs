using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class Solver
{
	public const int TRACE_LENGHT = 100;
		
	private Queue<Vector3> forwardTrace = new Queue<Vector3>(TRACE_LENGHT);
	private Queue<Vector3> positionTrace = new Queue<Vector3>(TRACE_LENGHT);
	private Queue<float> fitnessTrace = new Queue<float>(TRACE_LENGHT);
	
	private bool running = true;
	private float satisfaction = 0;

	public float Satisfaction {
		get {
			return satisfaction;
		}
	}
	
	protected void logTrace(Vector3 position, Vector3 forward, float fitness){
		if (positionTrace.Count >= TRACE_LENGHT){
			positionTrace.Dequeue();
			fitnessTrace.Dequeue();
			forwardTrace.Dequeue();
		}
		
		positionTrace.Enqueue(position);
		fitnessTrace.Enqueue(fitness);
		forwardTrace.Enqueue(forward);
	}
	
	public virtual void DrawGizmos(){
		for (int i=0;i<positionTrace.Count;i++){
			Gizmos.color = Color.red*fitnessTrace.ElementAt(i);
			Gizmos.DrawCube(positionTrace.ElementAt(i),0.1f*Vector3.one);
			
			Gizmos.color = Color.blue*fitnessTrace.ElementAt(i);
			Gizmos.DrawLine(positionTrace.ElementAt(i),positionTrace.ElementAt(i)+forwardTrace.ElementAt(i));
		}
	}

	public float Update (Transform bestCamera, Subject[] subjects, Shot shot, float maxExecutionTime){
		if (running){
			satisfaction = update (bestCamera,subjects,shot,maxExecutionTime);
		}else 
			satisfaction = 0;

		return satisfaction;
	}

	public virtual void Start(Transform bestCamera, Subject[] subjects){
		running = true;
	}

	public virtual void Stop() {
		running = false;
	}

	public static Vector3 SubjectsCenter (Subject[] subjects)
	{
		Vector3 center = Vector3.zero;
		foreach (Subject s in subjects)
			if (s != null)
				center += s.Position / subjects.Length;
		return center;
	}
	
	public static float SubjectsRadius (Subject[] subjects)
	{
		Vector3 center = SubjectsCenter (subjects);
		float radius = 1;
		foreach (Subject s in subjects) {
			if (s != null) {
				float distance = (s.Position - center).magnitude;
				if (distance > radius)
					radius = distance;
			}
		}
		
		return radius;
	}

	abstract protected float update(Transform bestCamera, Subject[] subjects, Shot shot, float maxExecutionTime);
}

