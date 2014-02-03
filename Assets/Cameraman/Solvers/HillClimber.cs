using UnityEngine;

public class HillClimber : Solver
{
	Vector3 bestPosition,bestForward;
	float bestFitness;
	
	Property.PropertyType[] positionProps = {Property.PropertyType.ProjectionSize,Property.PropertyType.VantageAngle};
	Property.PropertyType[] orientationProps = {Property.PropertyType.PositionOnScreen};
	
	public Vector3 SubjectsCenter(Subject[] subjects){
			Vector3 center = Vector3.zero;
			foreach (Subject s in subjects)
				if (s!=null)
					center += s.Position/subjects.Length;
			return center;
	}
	
	public override float Update (Transform currentCamera, Subject[] subjects, Shot shot, int milliseconds)
	{
		if (running) {
			double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
		
			shot.UpdateSubjects (subjects, currentCamera.camera);
		
			bestFitness = shot.Evaluate();
			float bestPosFit = shot.Evaluate (positionProps);
			float bestOrFit = shot.Evaluate (orientationProps);
			
			
			while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < milliseconds) {
				double evalBegin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
				
				float posDistance = (bestPosition-SubjectsCenter(subjects)).magnitude;
			
				if (shot.ProjectionSize < 0.01f)
					bestPosition = SubjectsCenter(subjects) + (bestPosition-SubjectsCenter(subjects)).normalized*10;
				if (shot.Visibility == 0) 
					bestForward = (SubjectsCenter(subjects) - bestPosition).normalized;
			
				currentCamera.position = bestPosition + Random.insideUnitSphere * (posDistance/10) *(1-bestPosFit);
				Vector3 tmpLookAt = bestPosition + bestForward*posDistance;
				currentCamera.LookAt(tmpLookAt);
				currentCamera.forward += Random.insideUnitSphere * Random.value *(1-bestOrFit) / posDistance;
			
				shot.UpdateSubjects (subjects, currentCamera.camera);
				float tmpFit = shot.Evaluate ();
			
				logTrace(currentCamera.position,currentCamera.forward,tmpFit);
								
				if (tmpFit > bestFitness) {
					bestFitness = tmpFit;
					bestPosFit = shot.Evaluate(positionProps);
					bestOrFit = shot.Evaluate(orientationProps);
					
					bestPosition = currentCamera.position;
					bestForward = currentCamera.forward;
				}
				evaluationTime=System.DateTime.Now.TimeOfDay.TotalMilliseconds-evalBegin;
			}
			
			currentCamera.position = bestPosition;
			currentCamera.forward = bestForward;
		
			return bestFitness;
		} else 
			return 0;
	}

	public override void Start (Transform camera, Subject[] subjects, Shot shot)
	{
		bestPosition = camera.position;
		bestForward = (SubjectsCenter(subjects) - bestPosition).normalized;
		camera.position = bestPosition;
		camera.forward = bestForward;
		running = true;
	}
	
	public override void Stop(){
		running = false;
	}
}

