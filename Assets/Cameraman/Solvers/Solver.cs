using UnityEngine;
using System.Collections.Generic;

public abstract class Solver
{
	public const int TRACE_LENGHT = 100;
		
	Queue<Vector3> lookAtTrace = new Queue<Vector3>(TRACE_LENGHT);
	Queue<Vector3> positionTrace = new Queue<Vector3>(TRACE_LENGHT);
	Queue<float> fitnessTrace = new Queue<float>(TRACE_LENGHT);
	
	public Vector3 SubjectsCenter(Subject[] subjects){
			Vector3 center = Vector3.zero;
			foreach (Subject s in subjects)
				center += s.Position/subjects.Length;
			return center;
	}
	
	protected void logTrace(Vector3 position, Vector3 lookAt, float fitness){
		if (positionTrace.Count >= TRACE_LENGHT){
			positionTrace.Dequeue();
			fitnessTrace.Dequeue();
			lookAtTrace.Dequeue();
		}
		
		positionTrace.Enqueue(position);
		fitnessTrace.Enqueue(fitness);
		lookAtTrace.Enqueue(lookAt);
	}
	
	public int EvaluationsPerSecond {
		get {
			return (int)(1000/evaluationTime);
		}
	}
	protected double evaluationTime; 
	protected float bestFit = 0;
	abstract public float Update(Transform bestCamera, Subject[] subjects, Shot shot, int milliseconds);
	
	public void DrawGizmos(){
		for (int i=0;i<positionTrace.Count;i++){
			Gizmos.color = Color.red*fitnessTrace.ToArray()[i];
			Gizmos.DrawCube(positionTrace.ToArray()[i],0.1f*Vector3.one);
			
			Gizmos.color = Color.blue*fitnessTrace.ToArray()[i];
			Gizmos.DrawCube(lookAtTrace.ToArray()[i],0.1f*Vector3.one);
		}
	}
}

