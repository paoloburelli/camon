
using UnityEngine;

public static class PropertiesForces
{
	public static Vector3 PositionForce (this ProjectionSize property, Actor[] subjects, Transform currentCamera)
	{
		float direction = -1;
		if (property.DesiredSize < subjects [0].ProjectionSize)
			direction = 1;
		
		if (subjects [0].Visibility == 0)
			direction = 0;

		//Wiced Improvement
		return direction * (currentCamera.position - subjects [0].Position).normalized * (1 - property.Evaluate (subjects)) * (1-Mathf.Pow(subjects[property.MainSubjectIndex].Occlusion,10));
		//Previous: return direction * (currentCamera.position - subjects [0].Position).normalized * (1 - property.Evaluate (subjects));
	}
	
	public static Vector3 PositionForce (this VantageAngle property, Actor[] subjects, Transform currentCamera)
	{
		Vector3 relativeCamPos = (currentCamera.position - subjects [0].Position);
		Vector3 targetPosition = subjects [0].VantageDirection * relativeCamPos.magnitude;
		Vector3 nextPos = Vector3.RotateTowards (relativeCamPos.normalized, targetPosition, 1, 1);

		////Wiced Improvement
		return (nextPos - relativeCamPos) * (1-Mathf.Pow(subjects[property.MainSubjectIndex].Occlusion,10));
		//Previous: return (nextPos - relativeCamPos);
	}
	
	public static Vector3 OcclusionAvoidanceForce (Actor subject, Transform currentCamera){
		
		
		float h = 0;
		if (subject.Occluded(Actor.SamplePoint.Left))
			h++;
		if (subject.Occluded(Actor.SamplePoint.Right))
			h--;
		
		float v = 0;
		if (subject.Occluded(Actor.SamplePoint.Bottom))
			v++;
		if (subject.Occluded(Actor.SamplePoint.Top))
			v--;
		
		if (subject.Occlusion >= 1){
			h = Random.value;
			v = Random.value;
		}

		////Wiced Improvement
		return (currentCamera.up * v + currentCamera.right * h).normalized * subject.VolumeOfInterestSize.magnitude/5;
		//Previous: return (currentCamera.up * v + currentCamera.right * h).normalized;
	}
	
	public static Vector3 InFrustumForce (Actor subject, Transform currentCamera){
		float direction = 0;
		if (subject.InFrustum < 1)
			direction = 1;

		//Wiced Improvement
		return direction * (currentCamera.position - subject.Position).normalized * (1 - subject.InFrustum) * subject.VolumeOfInterestSize.magnitude/5;
		//Previous: return direction * (currentCamera.position - subject.Position).normalized * (1 - subject.InFrustum);
	}
}

public class ArtificialPotentialField : Solver
{
	Vector3 bestPosition, bestForward, tmpPos;
	float bestFitness = 0;
	Property.Type[] lookAtInfluencingProperties = {Property.Type.PositionOnScreen};
	
	protected override float update (Transform currentCamera, Actor[] subjects, Shot shot, float maxExecutionTime)
	{
		double maxMilliseconds = maxExecutionTime * 1000;
		double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;

		//Wiced Improvement
		setPosition(bestPosition + SubjectsVelocity*Time.deltaTime,currentCamera,shot);

		bestFitness = shot.GetQuality (subjects,currentCamera.GetComponent<Camera> ());
		float lookAtFitness = shot.InFrustum(subjects) * .5f + shot.GetQuality (lookAtInfluencingProperties,subjects) * .5f;
		bestPosition = currentCamera.transform.position;
		
		while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < maxMilliseconds) {
			
			Vector3 positionForce = Vector3.zero;
			foreach (Property p in shot.Properties) {
				if (p is ProjectionSize)
					positionForce += ((ProjectionSize)p).PositionForce (subjects, currentCamera);
				if (p is VantageAngle)
					positionForce += ((VantageAngle)p).PositionForce (subjects, currentCamera);
			}
			
			foreach (Actor s in subjects)
				positionForce += PropertiesForces.OcclusionAvoidanceForce(s,currentCamera);
			
			foreach (Actor s in subjects)
				positionForce += PropertiesForces.InFrustumForce(s,currentCamera);

			if (positionForce.magnitude >1)
				positionForce.Normalize();

			//Wiced Improvement
			setPosition(bestPosition + positionForce + Random.insideUnitSphere * (1 - Mathf.Pow (bestFitness, 2)) * CombinedSubjectsScale(subjects),currentCamera,shot);
			//Previous: setPosition(bestPosition + positionForce,currentCamera,shot);
			
			//Wiced Improvement
			Vector3 tmpLookAt = SubjectsCenter (subjects) + Random.insideUnitSphere * (1 - Mathf.Pow (lookAtFitness, 4)) * CombinedSubjectsScale(subjects);
			//Previous: Vector3 tmpLookAt = SubjectsCenter (subjects) + Random.insideUnitSphere * (1-lookAtFitness);

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
	
	public override void Start (Transform camera, Actor[] subjects, Shot shot)
	{
		base.Start (camera, subjects, shot);
		bestPosition = camera.position;
		bestForward = camera.forward;
	}

	//Wiced Improvement
	protected override void initBestCamera (Transform bestCamera, Actor[] subjects, Shot shot)
	{
		Vector3 rayCastPoint = subjects[0].Position;

		float radius = Solver.SubjectsRadius (subjects);
		Vector3 direction = Vector3.one * radius;
		Vector3 lookAtPoint = Solver.SubjectsCenter (subjects);
		
		
		foreach (Property p in shot.Properties) {
			if (p.PropertyType == Property.Type.VantageAngle) {
				VantageAngle va = (VantageAngle)p;
				subjects[va.MainSubjectIndex].CalculateRelativeCameraAngle(va.DesiredHorizontalAngle,va.DesiredVerticalAngle);
				direction = subjects[p.MainSubjectIndex].VantageDirection * radius;
				lookAtPoint = subjects [va.MainSubjectIndex].Position;
				rayCastPoint = subjects [va.MainSubjectIndex].Position;
				break;
			} else if (p.PropertyType == Property.Type.RelativePosition) {
				RelativePosition rp = (RelativePosition)p;
				if (rp.DesiredPosition == RelativePosition.Position.InFrontOf) {
					direction = ((subjects [rp.MainSubjectIndex].Position - subjects [rp.SecondaryActorIndex].Position) + subjects [rp.MainSubjectIndex].Right * subjects [rp.MainSubjectIndex].VolumeOfInterestSize.x);
					direction *= 1.1f + (subjects [rp.MainSubjectIndex].VolumeOfInterestSize.magnitude / direction.magnitude);
					lookAtPoint = subjects [rp.SecondaryActorIndex].Position;
					rayCastPoint = subjects [rp.MainSubjectIndex].Position;
					break;	
				}
			}
		}
		
		//I'm not sure if the initial camera position should be calculated or it should be manually set
		setPosition(lookAtPoint + direction,bestCamera,shot);
		//bestCamera.position = lookAtPoint + direction;
		bestCamera.LookAt (lookAtPoint);

		float rayDistance = (bestCamera.position-rayCastPoint).magnitude;
		Vector3 rayDirection = (bestCamera.position-rayCastPoint).normalized;

		RaycastHit hitInfo;
		Physics.Raycast (rayCastPoint,rayDirection, out hitInfo,rayDistance);
		
		bool obstacle = hitInfo.distance < rayDistance;
		foreach (Actor a in subjects)
			if (a != null && hitInfo.collider == a.GetComponent<Collider>())
				obstacle = false;
		
		if (obstacle)
			Solver.setPosition(bestCamera.position - rayDirection * (rayDistance-hitInfo.distance),bestCamera,shot);
	}
}

