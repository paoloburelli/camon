using UnityEngine; 

namespace ArtificialPotentialFieldForces
{
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
}

