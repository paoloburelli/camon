using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class Solver
{
	public const int TRACE_LENGHT = 100;
		
	private Queue<Vector3> forwardTrace = new Queue<Vector3>(TRACE_LENGHT);
	private Queue<Vector3> positionTrace = new Queue<Vector3>(TRACE_LENGHT);
	private Queue<float> fitnessTrace = new Queue<float>(TRACE_LENGHT);
	
	protected double evaluationTime; 
	protected float bestFit = 0;
	protected bool running = true;
	
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
	
	public int EvaluationsPerSecond {
		get {
			return (int)(1000/evaluationTime);
		}
	}
	
	public void DrawGizmos(){
		for (int i=0;i<positionTrace.Count;i++){
			Gizmos.color = Color.red*fitnessTrace.ElementAt(i);
			Gizmos.DrawCube(positionTrace.ElementAt(i),0.1f*Vector3.one);
			
			Gizmos.color = Color.blue*fitnessTrace.ElementAt(i);
			Gizmos.DrawLine(positionTrace.ElementAt(i),positionTrace.ElementAt(i)+forwardTrace.ElementAt(i));
		}
	}
	
	abstract public float Update(Transform bestCamera, Subject[] subjects, Shot shot, float maxExecutionTime);
	abstract public void Start(Transform bestCamera, Subject[] subjects, Shot shot);
	abstract public void Stop();
}

