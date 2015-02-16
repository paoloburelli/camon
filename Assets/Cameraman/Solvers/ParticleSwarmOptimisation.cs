using UnityEngine;

public class Particle
{
	Vector3 position,lookAtPosition;
	Vector3 positionVelocity = Vector3.zero,lookVelocity = Vector3.zero;
	float fitness = 0;
	Particle localOptimum;
	
	public float Fitness {
		get {
			return fitness;	
		}
	}
	
	public Vector3 Position {
		get {
			return position;
		}
	}
	
	public Vector3 LookAt {
		get {
			return lookAtPosition;
		}
	}
	
	public Particle (Vector3 position, Vector3 forward)
	{
		this.position = position;
		this.lookAtPosition = position + forward;
		localOptimum = this.Clone();
	}
	
	public float Evaluate (Camera camera, Shot shot, Subject[] subjects)
	{
		camera.transform.position = position;
		camera.transform.LookAt(lookAtPosition);
		shot.UpdateSubjects (subjects, camera);
		fitness = shot.Evaluate ();
		
		if (fitness >= localOptimum.fitness)
			localOptimum = this.Clone ();
		
		return fitness;
	}
	
	public void UpdateVelocity (float inertia,float cognitiveFactor, float socialFactor, Particle globalOptimum)
	{
		lookVelocity = inertia * lookVelocity + cognitiveFactor * UnityEngine.Random.value * (globalOptimum.LookAt - LookAt) + socialFactor * UnityEngine.Random.value * (localOptimum.LookAt - LookAt);
		positionVelocity = inertia * positionVelocity + cognitiveFactor * UnityEngine.Random.value * (globalOptimum.Position - Position) + socialFactor * UnityEngine.Random.value * (localOptimum.Position - Position);
	}
	
	public void Move ()
	{
		position += positionVelocity;
		lookAtPosition += lookVelocity;
	}
	
	public Particle Clone ()
	{
		Particle tmp = new Particle (position, (lookAtPosition - position).normalized);
		tmp.fitness = fitness;
		return tmp;
	}
	
//	public static Particle Random (Vector3 min, Vector3 max, ParticleSwarmOptimisation optimizer)
//	{
//		Vector3 mean = (max + min) / 2;
//		Vector3 size = (max - mean) / 2;
//		Vector3 position = GeometryUtilityExtra.RandomValidPosition (mean, size);
//		Vector2 rotation = new Vector2 (UnityEngine.Random.value * 360 - 180,
//		                                UnityEngine.Random.value * 180 - 90);
//		return new Particle (position, rotation, optimizer);
//	}
}

public class ParticleSwarmOptimisation : Solver
{
	Vector3 bestPosition, bestForward, lastCenter;
	
	
	protected override float update (Transform currentCamera, Subject[] subjects, Shot shot, float maxExecutionTime)
	{
		double maxMilliseconds = maxExecutionTime * 1000;
		double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
		
		
		Vector3 newCenter = SubjectsCenter (subjects);
		currentCamera.transform.position = bestPosition + newCenter - lastCenter;
		shot.UpdateSubjects (subjects, currentCamera.camera);
		bestFitness = shot.Evaluate ();
		bestPosition = currentCamera.transform.position;
		lastCenter = newCenter;
		
		
		
		while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < maxMilliseconds) {
			currentCamera.position = bestPosition + Random.onUnitSphere * (1 - bestFitness);
			Vector3 tmpLookAt = SubjectsCenter (subjects) + Random.insideUnitSphere * (1 - bestFitness) * SubjectsRadius (subjects);
			currentCamera.LookAt (tmpLookAt);
			
			shot.UpdateSubjects (subjects, currentCamera.camera);
			float tmpFit = shot.Evaluate ();
			
			logTrace (currentCamera.position, currentCamera.forward, tmpFit);
			
			if (tmpFit > bestFitness) {
				bestFitness = tmpFit;
				bestPosition = currentCamera.position;
				bestForward = currentCamera.forward;
			}
		}
		
		currentCamera.position = bestPosition;
		currentCamera.forward = bestForward;
		
		return bestFitness;
	}
	
	public override void Start (Transform camera, Subject[] subjects, Shot shot)
	{
		base.Start (camera, subjects, shot);
		if (camera == null)
			throw new MissingReferenceException ("camera not initilised");
		bestPosition = camera.position;
		bestForward = (SubjectsCenter (subjects) - bestPosition).normalized;
		camera.position = bestPosition;
		camera.forward = bestForward;
		lastCenter = SubjectsCenter (subjects);
	}

}