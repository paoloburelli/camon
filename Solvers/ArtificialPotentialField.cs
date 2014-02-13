
using UnityEngine;
using ArtificialPotentialFieldForces;

public class ArtificialPotentialField : Solver
{
		Vector3 bestPosition, bestForward;
	
		public Vector3 SubjectsCenter (Subject[] subjects)
		{
				Vector3 center = Vector3.zero;
				foreach (Subject s in subjects)
						if (s != null)
								center += s.Position / subjects.Length;
				return center;
		}

		public float SubjectsRadius (Subject[] subjects)
		{
				Vector3 center = SubjectsCenter (subjects);
				float radius = 1;
				foreach (Subject s in subjects) {
						if (s != null) {
								float distance = (s.Position - center).magnitude;
								if (distance > radius)
										radius = distance;
						}
				}
				
				return radius;
		}
	
		protected override float update (Transform currentCamera, Subject[] subjects, Shot shot, float maxExecutionTime)
		{
				double maxMilliseconds = maxExecutionTime * 1000;
				double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
		
				while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < maxMilliseconds) {

						currentCamera.position = bestPosition;
						currentCamera.forward = bestForward;
						shot.UpdateSubjects (subjects, currentCamera.camera);
						bestFitness = shot.Evaluate();

						Vector3 positionForce = Vector3.zero;
						foreach(Property p in shot.Properties){
							if (p is ProjectionSize)
								positionForce += ((ProjectionSize)p).PositionForce(subjects,currentCamera.camera);
							if (p is VantageAngle)
								positionForce += ((VantageAngle)p).PositionForce(subjects,currentCamera.camera);
						}
			
						currentCamera.position = bestPosition + positionForce*Random.value + Random.insideUnitSphere * (1-bestFitness);
						Vector3 tmpLookAt = SubjectsCenter (subjects) + Random.insideUnitSphere * (1 - bestFitness);
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
		}
}

