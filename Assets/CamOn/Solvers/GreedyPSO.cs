using UnityEngine;
using System.Collections;

public class GreedyPSO : ParticleSwarmOptimisation {

	public GreedyPSO(float inertia, float cognitiveFactor, float socialFactor, int popSize) : base(inertia, cognitiveFactor, socialFactor, popSize){

	}

	protected override float update (Transform currentCamera, Subject[] subjects, Shot shot, float maxExecutionTime)
	{
		if (IsStagnating)
			return base.update( currentCamera, subjects, shot, maxExecutionTime);
		else{
			return updateAPF( currentCamera, subjects, shot, maxExecutionTime);
		}
	}

	float lastBestFitness = 0;
	Vector3 lastCenter;
	Particle perticle = new Particle(Vector3.forward,-Vector3.forward);
	protected float updateAPF(Transform currentCamera, Subject[] subjects, Shot shot, float maxExecutionTime){
		double maxMilliseconds = maxExecutionTime * 1000;
		double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
		
		Vector3 newCenter = SubjectsCenter (subjects);
		globalOptimum.Position += newCenter - lastCenter;
		lastBestFitness = globalOptimum.Evaluate(currentCamera.camera, shot, subjects);
		lastCenter = newCenter;
		
		while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < maxMilliseconds) {
			
			Vector3 positionForce = Vector3.zero;
			foreach(Property p in shot.Properties){
				if (p is ProjectionSize)
					positionForce += ((ProjectionSize)p).PositionForce(subjects,currentCamera.camera);
				if (p is VantageAngle)
					positionForce += ((VantageAngle)p).PositionForce(subjects,currentCamera.camera);
			}
			
			perticle.Position = globalOptimum.Position + positionForce*Random.value + Random.insideUnitSphere * (1-globalOptimum.Fitness);
			perticle.LookAt = SubjectsCenter (subjects) + Random.insideUnitSphere * (1 - globalOptimum.Fitness) * SubjectsRadius(subjects)*.5f;
			perticle.Evaluate(currentCamera.camera, shot, subjects);
			
			logTrace (currentCamera.position, currentCamera.forward, perticle.Fitness);
			
			if (perticle.Fitness > globalOptimum.Fitness)
				globalOptimum = new Particle(perticle);
		}
		
		currentCamera.position = globalOptimum.Position;
		currentCamera.LookAt(globalOptimum.LookAt);
		return globalOptimum.Fitness;
	}
	
	public bool IsStagnating {
		get {
			bool rVal = globalOptimum.Fitness <= lastBestFitness;
			return rVal;
		}
	}
}
