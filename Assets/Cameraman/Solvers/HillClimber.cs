using UnityEngine;

public class HillClimber : Solver
{
	Vector3 bestPosition,bestLookAtPoint;
	float bestFitness;
	
	Property.PropertyType[] positionProps = {Property.PropertyType.ProjectionSize,Property.PropertyType.VantageAngle};
	Property.PropertyType[] orientationProps = {Property.PropertyType.PositionOnScreen};
	
	public Vector3 SubjectsCenter(Subject[] subjects){
			Vector3 center = Vector3.zero;
			foreach (Subject s in subjects)
				center += s.Position/subjects.Length;
			return center;
	}
	
	public override float Update (Transform bestCamera, Subject[] subjects, Shot shot, int milliseconds)
	{
			shot.UpdateSubjects (subjects, bestCamera.camera);
		
			if (shot.Visibility == 0)
				SnapToSubjects(bestCamera,subjects);
		
			bestFitness = shot.Evaluate();
			float bestPosFit = shot.Evaluate (positionProps);
			float bestOrFit = shot.Evaluate(orientationProps);
			
			double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
			while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < milliseconds) {
				double evalBegin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
				
				float posStep = (bestPosition-SubjectsCenter(subjects)).magnitude/2;
				float lookStep = (bestLookAtPoint-SubjectsCenter(subjects)).magnitude/2;
			
				Vector3 tmpPos = bestPosition + Random.insideUnitSphere * posStep*(1-bestPosFit);
				Vector3 tmpLook  = bestLookAtPoint + Random.insideUnitSphere * lookStep*(1-bestOrFit);
			
				bestCamera.position = tmpPos;
				bestCamera.LookAt(tmpLook);
				shot.UpdateSubjects (subjects, bestCamera.camera);
				float tmpFit = shot.Evaluate ();
			
				logTrace(bestCamera.position,bestCamera.forward,tmpFit);
								
				if (tmpFit >= bestFitness) {
					bestFitness = tmpFit;
					bestPosFit = shot.Evaluate(positionProps);
					bestOrFit = shot.Evaluate(orientationProps);
					
					bestPosition = tmpPos;
					bestLookAtPoint = tmpLook;
				}
				evaluationTime=System.DateTime.Now.TimeOfDay.TotalMilliseconds-evalBegin;
			}
			
			bestCamera.position = bestPosition;
			bestCamera.LookAt(bestLookAtPoint);
		
			return bestFitness;
	}

	public override void Start (Transform camera, Subject[] subjects, Shot shot)
	{
		bestLookAtPoint = camera.position;
		SnapToSubjects(camera, subjects);
		SetCamera(camera);
	}
	
	protected void SnapToSubjects(Transform camera, Subject[] subjects){
		bestLookAtPoint = SubjectsCenter(subjects);
	}
	
	protected void SetCamera(Transform camera){
		camera.position = bestPosition;
		camera.LookAt(bestLookAtPoint);
	}
}

