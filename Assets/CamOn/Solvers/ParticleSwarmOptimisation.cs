using UnityEngine;
using System.Collections.Generic;

public class Particle
{
	Vector3 position,lookAtPosition;
	Vector3 positionVelocity = Vector3.zero,lookVelocity = Vector3.zero;
	float fitness = 0;
	Particle localOptimum;
	long frameId = 0;
	
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
		localOptimum = new Particle(this);
	}

	public Particle (Particle p) {
		this.position = p.position;
		this.lookAtPosition = p.lookAtPosition;
		this.fitness = p.fitness;
		this.lookVelocity = p.lookVelocity;
		this.positionVelocity = p.positionVelocity;
	}


	public void Refresh (Camera camera, Shot shot, Subject[] subjects)
	{
		if (Time.frameCount != frameId) {
			//Update the local bests at each new frame
			camera.transform.position = localOptimum.position;
			camera.transform.LookAt(localOptimum.lookAtPosition);
			shot.UpdateSubjects (subjects, camera);
			localOptimum.fitness = shot.Evaluate ();

			//Re-initialise 10% of the population at each new frame
			if (Random.value <= 0.1f) {
				//Debug.Log("Init");

				Vector3 c = Solver.SubjectsCenter(subjects);
				float r = Solver.SubjectsRadius(subjects);

				this.position = c+r*Random.insideUnitSphere;
				this.lookAtPosition = c+Random.insideUnitSphere;

				this.fitness = 0;
				this.lookVelocity = Vector3.zero;
				this.positionVelocity = Vector3.zero;
			}
		}

		frameId = Time.frameCount;
	}

	public float Evaluate (Camera camera, Shot shot, Subject[] subjects)
	{
		camera.transform.position = position;
		camera.transform.LookAt(lookAtPosition);
		shot.UpdateSubjects (subjects, camera);
		fitness = shot.Evaluate ();
		
		if (localOptimum != null && fitness >= localOptimum.fitness)
			localOptimum = new Particle(this);
		
		return fitness;
	}
	
	public void UpdateVelocity (float inertia,float cognitiveFactor, float socialFactor, Particle globalOptimum)
	{
		lookVelocity += inertia * (cognitiveFactor * Random.value * (localOptimum.LookAt - LookAt) + socialFactor * Random.value * (globalOptimum.LookAt - LookAt)) - (1-inertia)*lookVelocity;
		positionVelocity += inertia * (cognitiveFactor * Random.value * (localOptimum.Position - Position) + socialFactor * Random.value * (globalOptimum.Position - Position)) - (1-inertia)*positionVelocity;
	}
	
	public void Move ()
	{
		position += positionVelocity;
		lookAtPosition += lookVelocity;
	}
}

public class ParticleSwarmOptimisation : Solver
{

	public float inertia;
	public float cognitiveFactor;
	public float socialFactor;
	public int populationSize;
	
	Particle globalOptimum;
	List<Particle> particles;
	IEnumerator<Particle> enumerator=null;

	public ParticleSwarmOptimisation(float inertia, float cognitiveFactor, float socialFactor, int popSize){
		this.inertia = inertia;
		this.socialFactor = socialFactor;
		this.cognitiveFactor = cognitiveFactor;
		this.populationSize = popSize;
	}
	
	protected override float update (Transform currentCamera, Subject[] subjects, Shot shot, float maxExecutionTime)
	{
		double maxMilliseconds = maxExecutionTime * 1000;
		double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;

		globalOptimum.Evaluate(currentCamera.camera,shot,subjects);

		while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < maxMilliseconds) {
			if (!enumerator.MoveNext ()) {
				enumerator = particles.GetEnumerator ();
				enumerator.MoveNext ();
			}

			enumerator.Current.Refresh(currentCamera.camera,shot,subjects);
			enumerator.Current.Move ();
			enumerator.Current.Evaluate (currentCamera.camera,shot,subjects);
			enumerator.Current.UpdateVelocity (inertia,cognitiveFactor,socialFactor,globalOptimum);

			if (enumerator.Current.Fitness > globalOptimum.Fitness) {
				globalOptimum = new Particle(enumerator.Current);
			}

			logTrace (currentCamera.position, currentCamera.forward, enumerator.Current.Fitness);
		}
		
		currentCamera.position = globalOptimum.Position;
		currentCamera.LookAt(globalOptimum.LookAt);
		
		return bestFitness;
	}
	
	public override void Start (Transform camera, Subject[] subjects)
	{
		base.Start (camera, subjects);
		if (camera == null)
			throw new MissingReferenceException ("camera not initilised");

		particles = new List<Particle> ();
		particles.Add(new Particle(camera.position,camera.forward));

		Vector3 c = SubjectsCenter(subjects);
		float r = SubjectsRadius(subjects);

		for (int i=0;i<populationSize-1;i++){
			Vector3 pos = c+r*Random.insideUnitSphere;
			Vector3 lookAt = c+Random.insideUnitSphere;
			particles.Add(new Particle(pos,(lookAt-pos).normalized));
		}

		enumerator = particles.GetEnumerator ();
		globalOptimum = new Particle(particles[0]);
	}

}