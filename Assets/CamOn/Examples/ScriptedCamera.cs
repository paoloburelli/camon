using UnityEngine;
using System.Collections;

public class ScriptedCamera : MonoBehaviour {
	
	Actor ethan;

	// Use this for initialization
	void Start () {
		ethan = Actor.Create (GameObject.Find("Ethan").transform,PrimitiveType.Capsule,new Vector3(0,0.75f,0),new Vector3(0.5f,0.75f,0.5f));

		CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-CloseUp"), CameraOperator.Transition.Cut, new Actor[] {ethan});
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.frameCount == 100)
			CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-LongShot"), CameraOperator.Transition.Smooth, new Actor[] {ethan});

		if (Time.frameCount == 200)
			CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-LeftSide"), CameraOperator.Transition.Cut, new Actor[] {ethan});

		if (Time.frameCount == 300)
			CameraOperator.OnMainCamera.SelectShot(Resources.Load<Shot>("OneActor-CloseUp"), CameraOperator.Transition.Smooth, new Actor[] {ethan});
	}
}
