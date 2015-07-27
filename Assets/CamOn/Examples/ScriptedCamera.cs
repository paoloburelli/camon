using UnityEngine;
using System.Collections;

public class ScriptedCamera : MonoBehaviour {

	public Transform actor;
	Actor ethan;

	// Use this for initialization
	void Start () {
		//Create an actor attached to a transform
		//The proxy geometry used is a capsule ofset upwards by (0,0.75f,0) and scaled by (0.5f,0.75f,0.5f) to match the object's geometry
		//Actors can be created both trhough script or by adding an actor component to a transform
		ethan = Actor.Create (actor,PrimitiveType.Capsule,new Vector3(0,0.75f,0),new Vector3(0.5f,0.75f,0.5f));

		//Select a closeup shot from the resources with a cut transition (i.e. the cameradirectly jumps to the new shot)
		CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-CloseUp"), CameraOperator.Transition.Cut, ethan);
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.frameCount == 100)
			//Select a long shot with a smooth transition (i.e. the camera smoothly mooves from its current position to the new one)
			CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-LongShot"), CameraOperator.Transition.Smooth, ethan);

		if (Time.frameCount == 200)
			//Select a left side shot with a cut
			CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-LeftSide"), CameraOperator.Transition.Cut, ethan);

		if (Time.frameCount == 300)
			//Finish with a smooth transition to a close-up
			CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-CloseUp"), CameraOperator.Transition.Smooth, ethan);
	}
}
