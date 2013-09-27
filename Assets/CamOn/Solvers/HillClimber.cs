using UnityEngine;

public class HillClimber : Solver
{
	float posStep;
	float lookStep;
	
	Property.PropertyType[] positionProps = {Property.PropertyType.ProjectionSize,Property.PropertyType.VantageAngle};
	Property.PropertyType[] orientationProps = {Property.PropertyType.Visibility,Property.PropertyType.PositionOnScreen};
	
	
	public override float Update (Transform bestCamera, Subject[] subjects, Shot shot, int milliseconds)
	{
			float distance = (bestCamera.position-SubjectsCenter(subjects)).magnitude;
			Vector3 cameraPos = bestCamera.position;
			Vector3 lookAt = bestCamera.position+bestCamera.forward*distance;
		
			shot.UpdateSubjects (subjects, bestCamera.camera);
			
			bestFit = shot.Evaluate ();
			float posFit = shot.Evaluate (positionProps);
			float orFit = shot.Evaluate(orientationProps);
			
			double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
			while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < milliseconds) {
				double evalBegin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
				posStep = (cameraPos-SubjectsCenter(subjects)).magnitude/2;
				lookStep = (lookAt-SubjectsCenter(subjects)).magnitude/2;
			
				Pair<Vector3> random = CameraDistribution.GetRandomDirection(subjects,shot.Properties,bestCamera);
			
				bestCamera.position = cameraPos + random.a * posStep*(1-posFit)*Random.value;
				Vector3 bestLookAt  = lookAt + random.b * lookStep*(1-orFit)*Random.value;
				bestCamera.LookAt(bestLookAt);
				

			
				shot.UpdateSubjects (subjects, bestCamera.camera);
				float newFit = shot.Evaluate ();
			
				logTrace(bestCamera.position,cameraPos+bestCamera.forward,newFit);
								
				if (newFit >= bestFit) {
					bestFit = newFit;
					posFit = shot.Evaluate(positionProps);
					orFit = shot.Evaluate(orientationProps);
					
					cameraPos = bestCamera.position;
					lookAt = bestLookAt;
				}
				evaluationTime=System.DateTime.Now.TimeOfDay.TotalMilliseconds-evalBegin;
			}
			
			bestCamera.position = cameraPos;
			bestCamera.LookAt(lookAt);
		
			return bestFit;
	}
}

