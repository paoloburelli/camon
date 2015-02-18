using UnityEngine;

public class HillClimber : Solver
{
		Vector3 bestPosition, bestForward, lastCenter;
		
		protected override float update (Transform currentCamera, Subject[] subjects, Shot shot, float maxExecutionTime)
		{
				double maxMilliseconds = maxExecutionTime * 1000;
				double begin = System.DateTime.Now.TimeOfDay.TotalMilliseconds;

				
				Vector3 newCenter = SubjectsCenter (subjects);
				currentCamera.transform.position = bestPosition + newCenter - lastCenter;
				shot.UpdateSubjects (subjects, currentCamera.camera);
				bestFitness = shot.Evaluate ();
				bestPosition = currentCamera.transform.position;
				lastCenter = newCenter;
				
				

				while (System.DateTime.Now.TimeOfDay.TotalMilliseconds - begin < maxMilliseconds) {
						currentCamera.position = bestPosition + Random.onUnitSphere * (1 - bestFitness);
						Vector3 tmpLookAt = SubjectsCenter (subjects) + Random.insideUnitSphere * (1 - bestFitness) * SubjectsRadius (subjects);
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

		public override void Start (Transform camera, Subject[] subjects)
		{
				base.Start (camera, subjects);
				if (camera == null)
						throw new MissingReferenceException ("camera not initilised");
				bestPosition = camera.position;
				bestForward = (SubjectsCenter (subjects) - bestPosition).normalized;
				camera.position = bestPosition;
				camera.forward = bestForward;
				lastCenter = SubjectsCenter (subjects);
		}
}

