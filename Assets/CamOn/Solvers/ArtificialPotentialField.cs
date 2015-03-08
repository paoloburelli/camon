
using UnityEngine;

public static class PropertiesForces
{
	public static Vector3 PositionForce (this ProjectionSize property, SubjectEvaluator[] subjects, Camera currentCamera)
	{
		float direction = 1;
		if (property.DesiredSize < subjects [0].Visibility)
			direction = -1;
		
		if (subjects [0].Visibility == 0)
			direction = 0;
		
		return direction * (currentCamera.transform.position - subjects [0].Position).normalized * (1 - property.Evaluate (subjects));
	}
	
	public static Vector3 PositionForce (this VantageAngle property, SubjectEvaluator[] subjects, Camera currentCamera)
	{
		Vector3 relativeCamPos = (currentCamera.transform.position - subjects [0].Position);
		Vector3 targetPosition = subjects [0].VantageDirection * relativeCamPos.magnitude;
		Vector3 nextPos = Vector3.RotateTowards (relativeCamPos.normalized, targetPosition, 1, 1);
		return (nextPos - relativeCamPos);
	}
}

public class ArtificialPotentialField : Solver
{
	Vector3 bestPosition, bestForward, lastCenter, tmpPos;
	float bestFitness = 0;
	Property.Type[] lookAtInfluencingProperties = {Property.Type.PositionOnScreen};
	
	protected override float update (Transform currentCamera, SubjectEvaluator[] subjects, Shot shot, float maxExecutionTime)
	{
		double maxMilliseconds = maxExecutionTime * 1000;
		double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;

		Vector3 newCenter = SubjectsCenter (subjects);
		setPosition(bestPosition + newCenter - lastCenter,currentCamera,shot);
		bestFitness = shot.GetQuality (subjects,currentCamera.GetComponent<Camera> ());
		float lookAtFitness = shot.InFrustum(subjects) * .5f + shot.GetQuality (lookAtInfluencingProperties,subjects) * .5f;
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
			float tmpFit = shot.GetQuality (subjects,currentCamera.GetComponent<Camera> ());
			
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

	public override void Start (Transform camera, SubjectEvaluator[] subjects, Shot shot)
	{
		base.Start (camera, subjects, shot);
		bestPosition = camera.position;
		bestForward = camera.forward;
		lastCenter = SubjectsCenter (subjects);
	}

	protected override void initBestCamera (Transform bestCamera, SubjectEvaluator[] subjects, Shot shot)
	{
		float radius = Solver.SubjectsRadius (subjects);
		Vector3 center = Solver.SubjectsCenter (subjects);

		Vector3 direction = Vector3.one * radius;
		Vector3 lookAtPoint = center;


		foreach (Property p in shot.Properties) {
			if (p.PropertyType == Property.Type.VantageAngle) {
				VantageAngle va = (VantageAngle)p;
				direction = (Quaternion.Euler (va.DesiredHorizontalAngle, 0, 0) * subjects [va.MainSubjectIndex].VantageDirection) * radius;
				lookAtPoint = subjects [va.MainSubjectIndex].Position;
				break;
			} else if (p.PropertyType == Property.Type.RelativePosition) {
				RelativePosition rp = (RelativePosition)p;
				if (rp.DesiredPosition == RelativePosition.Position.InFrontOf) {
					direction = ((subjects [rp.MainSubjectIndex].Position - subjects [rp.SecondaryActorIndex].Position) + subjects [rp.MainSubjectIndex].Right * subjects [rp.MainSubjectIndex].Scale.x);
					direction *= 1.1f + (subjects [rp.MainSubjectIndex].Scale.magnitude / direction.magnitude);
					lookAtPoint = subjects [rp.SecondaryActorIndex].Position;
					break;	
				}
			}
		}

		//I'm not sure if the initial camera position should be calculated or it should be manually set
		setPosition(lookAtPoint + direction,bestCamera,shot);
		//bestCamera.position = lookAtPoint + direction;
		bestCamera.LookAt (lookAtPoint);

		RaycastHit hitInfo;
		Physics.Raycast (lookAtPoint,-bestCamera.forward, out hitInfo,direction.magnitude);

		bool obstacle = hitInfo.distance < direction.magnitude;
		foreach (SubjectEvaluator a in subjects)
			if (hitInfo.collider == a.collider)
				obstacle = false;

		if (obstacle)
			bestCamera.position += bestCamera.forward * (direction.magnitude-hitInfo.distance);
	}
}

