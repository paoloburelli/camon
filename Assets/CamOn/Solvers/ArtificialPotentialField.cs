
using UnityEngine;

public static class PropertiesForces
{
	public static Vector3 PositionForce (this ProjectionSize property, Subject[] subjects, Camera currentCamera)
	{
		float direction = 1;
		if (property.DesiredValue < subjects [0].Visibility)
			direction = -1;
		
		if (subjects [0].Visibility == 0)
			direction = 0;
		
		return direction * (currentCamera.transform.position - subjects [0].Position).normalized * (1 - property.Evaluate (subjects));
	}
	
	public static Vector3 PositionForce (this VantageAngle property, Subject[] subjects, Camera currentCamera)
	{
		Vector3 relativeCamPos = (currentCamera.transform.position - subjects [0].Position);
		
		Vector3 direction = subjects [0].Orientation * (Quaternion.Euler (-property.DesiredVerticalAngle, property.DesiredHorizontalAngle, 0) * Vector3.forward);
		
		
		Vector3 targetPosition = direction * relativeCamPos.magnitude;
		Vector3 nextPos = Vector3.RotateTowards (relativeCamPos.normalized, targetPosition, 1, 1);
		return (nextPos - relativeCamPos);
	}
}

public class ArtificialPotentialField : Solver
{
	Vector3 bestPosition, bestForward, lastCenter, tmpPos;
	float bestFitness = 0;
	Property.Type[] lookAtInfluencingProperties = {Property.Type.PositionOnScreen};
	
	protected override float update (Transform currentCamera, Subject[] subjects, Shot shot, float maxExecutionTime)
	{
		double maxMilliseconds = maxExecutionTime * 1000;
		double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;

		Vector3 newCenter = SubjectsCenter (subjects);
		setPosition(bestPosition + newCenter - lastCenter,currentCamera,shot);
		shot.UpdateSubjects (subjects, currentCamera.GetComponent<Camera> ());
		bestFitness = shot.Evaluate ();
		float lookAtFitness = shot.InFrustum * .5f + shot.Evaluate (lookAtInfluencingProperties) * .5f;
		bestPosition = currentCamera.transform.position;
		lastCenter = newCenter;

		while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < maxMilliseconds) {

			Vector3 positionForce = Vector3.zero;
			foreach (Property p in shot.Properties) {
				if (p is ProjectionSize)
					positionForce += ((ProjectionSize)p).PositionForce (subjects, currentCamera.GetComponent<Camera> ());
				if (p is VantageAngle)
					positionForce += ((VantageAngle)p).PositionForce (subjects, currentCamera.GetComponent<Camera> ());
			}
			
			setPosition(bestPosition + positionForce * Random.value + Random.insideUnitSphere * (1 - Mathf.Pow (bestFitness, 2)) * SubjectsRadius (subjects),currentCamera,shot);
						
			Vector3 tmpLookAt = SubjectsCenter (subjects) + Random.insideUnitSphere * (1 - Mathf.Pow (lookAtFitness, 4)) * SubjectsRadius (subjects);

			currentCamera.LookAt (tmpLookAt);
			shot.UpdateSubjects (subjects, currentCamera.GetComponent<Camera> ());
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
		bestPosition = camera.position;
		bestForward = camera.forward;
		lastCenter = SubjectsCenter (subjects);
	}

	protected override void initBestCamera (Transform bestCamera, Subject[] subjects, Shot shot)
	{
		float radius = Solver.SubjectsRadius (subjects);
		Vector3 center = Solver.SubjectsCenter (subjects);

		Vector3 direction = Vector3.one * radius;
		Vector3 lookAtPoint = center;


		foreach (Property p in shot.Properties) {
			if (p.PropertyType == Property.Type.VantageAngle) {
				VantageAngle va = (VantageAngle)p;
				direction = (Quaternion.Euler (va.DesiredHorizontalAngle, 0, 0) * subjects [va.Subject].Forward) * radius;
				lookAtPoint = subjects [va.Subject].Position;
				break;
			} else if (p.PropertyType == Property.Type.RelativePosition) {
				RelativePosition rp = (RelativePosition)p;
				if ((RelativePosition.Position)rp.DesiredValue == RelativePosition.Position.InFrontOf) {
					direction = ((subjects [rp.Subject].Position - subjects [rp.SecondarySubject].Position) + subjects [rp.Subject].Right * subjects [rp.Subject].Scale.x);
					direction *= 1.1f + (subjects [rp.Subject].Scale.magnitude / direction.magnitude);
					lookAtPoint = subjects [rp.SecondarySubject].Position;
					break;	
				}
			}
		}

		//I'm not sure if the initial camera position should be calculated or it should be manually set
		//setPosition(lookAtPoint + direction,bestCamera,shot);
		bestCamera.position = lookAtPoint + direction;
		bestCamera.LookAt (lookAtPoint);
	}
}

