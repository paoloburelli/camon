
using UnityEngine;

public static class PropertiesForces
{
	public static Vector3 PositionForce (this ProjectionSize property, Subject[] subjects, Camera currentCamera)
	{
		float direction = 1;
		if (property.DesiredValue < subjects[0].Visibility)
			direction = -1;
		
		if (subjects[0].Visibility == 0)
			direction = 0;
		
		return direction * (currentCamera.transform.position - subjects[0].Position).normalized * (1 - property.Evaluate(subjects));
	}
	
	public static Vector3 PositionForce (this VantageAngle property, Subject[] subjects, Camera currentCamera)
	{
		Vector3 relativeCamPos = (currentCamera.transform.position - subjects[0].Position);
		
		Vector3 direction =  subjects[0].Orientation * (Quaternion.Euler(-property.DesiredVerticalAngle,property.DesiredHorizontalAngle,0) * Vector3.forward);
		
		
		Vector3 targetPosition = direction * relativeCamPos.magnitude;
		Vector3 nextPos = Vector3.RotateTowards(relativeCamPos.normalized,targetPosition,1,1);
		return (nextPos-relativeCamPos);
	}
}

public class ArtificialPotentialField : Solver
{
		Vector3 bestPosition, bestForward, lastCenter;
		float bestFitness = 0;
		Property.Type[] lookAtInfluencingProperties = {Property.Type.PositionOnScreen};
	
		protected override float update (Transform currentCamera, Subject[] subjects, Shot shot, float maxExecutionTime)
		{
				double maxMilliseconds = maxExecutionTime * 1000;
				double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;

				Vector3 newCenter = SubjectsCenter (subjects);
				currentCamera.transform.position = bestPosition + newCenter - lastCenter;
				shot.UpdateSubjects (subjects, currentCamera.GetComponent<Camera>());
				bestFitness = shot.Evaluate ();
				float lookAtFitness = shot.InFrustum*.5f + shot.Evaluate(lookAtInfluencingProperties)*.5f;
				bestPosition = currentCamera.transform.position;
				lastCenter = newCenter;

				while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < maxMilliseconds) {

						Vector3 positionForce = Vector3.zero;
						foreach(Property p in shot.Properties){
							if (p is ProjectionSize)
								positionForce += ((ProjectionSize)p).PositionForce(subjects,currentCamera.GetComponent<Camera>());
							if (p is VantageAngle)
								positionForce += ((VantageAngle)p).PositionForce(subjects,currentCamera.GetComponent<Camera>());
						}
			
						currentCamera.position = bestPosition + positionForce*Random.value + Random.insideUnitSphere * (1-Mathf.Pow(bestFitness,2)) * SubjectsRadius(subjects);
						Vector3 tmpLookAt = SubjectsCenter (subjects) + Random.insideUnitSphere * (1-Mathf.Pow(lookAtFitness,4)) * SubjectsRadius(subjects);

						currentCamera.LookAt (tmpLookAt);
						shot.UpdateSubjects (subjects, currentCamera.GetComponent<Camera>());
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

		public override void Start (Transform camera, Subject[] subjects)
		{
				base.Start (camera, subjects);
				bestPosition = camera.position;
				bestForward = camera.forward;
				lastCenter = SubjectsCenter(subjects);
		}
}

