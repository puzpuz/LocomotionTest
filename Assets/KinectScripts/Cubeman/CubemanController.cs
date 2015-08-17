
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Windows.Kinect;

public class CubemanController : MonoBehaviour 
{


	//added
	public GameObject BodySourceManager;
	private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
	private BodySourceManager _BodyManager;
	
	private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
	{
		{ Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
		{ Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
		{ Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
		{ Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
		
		{ Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
		{ Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
		{ Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
		{ Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
		
		{ Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
		{ Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
		{ Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
		{ Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
		{ Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
		{ Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
		
		{ Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
		{ Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
		{ Kinect.JointType.HandRight, Kinect.JointType.WristRight },
		{ Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
		{ Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
		{ Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
		
		{ Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
		{ Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
		{ Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
		{ Kinect.JointType.Neck, Kinect.JointType.Head },
	};
	//

	public bool MoveVertically = false;
	public bool MirroredMovement = false;

	//public GameObject debugText;
	
	public Transform Hip_Center;
	public Transform Spine;
	public Transform Shoulder_Center;
	public Transform Neck;
	public Transform Head;
	public Transform Shoulder_Left;
	public Transform Elbow_Left;
	public Transform Wrist_Left;
	public Transform Hand_Left;
	public Transform Shoulder_Right;
	public Transform Elbow_Right;
	public Transform Wrist_Right;
	public Transform Hand_Right;
	public Transform Hip_Left;
	public Transform Knee_Left;
	public Transform Ankle_Left;
	public Transform Foot_Left;
	public Transform Hip_Right;
	public Transform Knee_Right;
	public Transform Ankle_Right;
	public Transform Foot_Right;

	public LineRenderer SkeletonLine;
	
	private GameObject[] bones; 
	private LineRenderer[] lines;
	private int[] parIdxs;
	
	private Vector3 initialPosition;
	private Quaternion initialRotation;
	private Vector3 initialPosOffset = Vector3.zero;
	private uint initialPosUserID = 0;
	private String fileLocation = "/skeletonData.mot1";
	private String lengthFileLocation = "/model.metadata";
	private Kinect.KinectSensor kinectSensor;
	private static Vector3 GetVector3FromJoint(Kinect.Joint joint)

	{
		return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
	}

	private bool isSaving=false;
	private bool isSavingMetadata = false;
	private bool Metadata = false;
	private bool Recording = false;
	private CoordinateMapper coordinateMapper = null;
	void OnGUI(){
		Event e = Event.current;
		if (e.isKey) {
			if(e.keyCode==KeyCode.R){
				if(!Recording){
					Recording=true;
					//Debug.Log("recording pressed");
				}
				
			}else if(e.keyCode==KeyCode.S){
				Debug.Log("saving pressed");
				if(Recording==true){
					//Debug.Log("recording stopped");
					//Debug.Log("saving started");
					Recording=false;
					isSaving=true;
					
				}else if(Metadata == true){
					Metadata = false;
					isSavingMetadata = true;
				}
			}else if(e.keyCode==KeyCode.M){
				if(!Metadata){
					Metadata = true;
					Debug.Log("metadata pressed");
				}
			}
		}
	}
	void Start () 
	{

		//bodyFrames = new List<Kinect.Body[]>();
		bodyFrames = new List<MySkeleton> ();
		boneLengths = new List<double> ();
		hipStart = Hip_Center.transform.localRotation;
		spineStart = Spine.transform.localRotation;
		shoulderCenterStart = Shoulder_Center.transform.localRotation;
		headStart = Head.transform.localRotation; 
		shoulderRightStart = Shoulder_Right.localRotation;
	}
	//private List<Kinect.Body[]> bodyFrames;
	private Quaternion hipStart;
	private Quaternion spineStart;
	private Quaternion shoulderCenterStart;
	private Quaternion neckStart;
	private Quaternion headStart;
	private Quaternion shoulderRightStart;
	private List<MySkeleton> bodyFrames;
	private List<double> boneLengths; 
	private long frameCount=0;
	private Windows.Kinect.Body firstBody;
	// Update is called once per frame
	void Update () 
	{
		if (BodySourceManager == null)
		{
			return;
		}
		
		_BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
		if (_BodyManager == null)
		{
			return;
		}
		
		Kinect.Body[] data = _BodyManager.GetData();
		if (data == null)
		{
			return;
		}
		if (frameCount == 0)
			firstBody = data [0];



		List<ulong> trackedIds = new List<ulong>();
		foreach(var body in data)
		{
			if (body == null)
			{
				continue;
			}
			
			if(body.IsTracked)
			{
				trackedIds.Add (body.TrackingId);
			}
		}
		
		List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
		
		// First delete untracked bodies
		foreach(ulong trackingId in knownIds)
		{
			if(!trackedIds.Contains(trackingId))
			{
				Destroy(_Bodies[trackingId]);
				_Bodies.Remove(trackingId);
			}
		}
		for (int i=0; i<data.Length; i++) {
		
		//foreach(var body in data)
		//{
			if (data[i] == null)
			{
				continue;
			}
			
			if(data[i].IsTracked)
			{
				//data[i] = NormalizedBody(data[i],firstBody);
				//RefreshBodyObject(data[i],firstBody);
				RefreshBodyObjectOrientation (data[i]);
				/*Debug.Log("ELBOW RIGHT-> "+
				          "W: "+data[i].JointOrientations [Kinect.JointType.ElbowRight].Orientation.W *Mathf.Rad2Deg +
				          "X: "+data[i].JointOrientations [Kinect.JointType.ElbowRight].Orientation.X *Mathf.Rad2Deg+
				          "Y: "+data[i].JointOrientations [Kinect.JointType.ElbowRight].Orientation.Y *Mathf.Rad2Deg+
				          "Z: "+data[i].JointOrientations [Kinect.JointType.ElbowRight].Orientation.Z *Mathf.Rad2Deg
				          );*/
			}
		}
		/*
		//record skeleton and store to memory first
		if (Recording) {
			//bodyFrames.Clear();
			Debug.Log("Recording...");
			MySkeleton skel = new MySkeleton();
			skel.JointId = new int[data[0].Joints.Count];
			skel.Position = new Vector3Serializer[data[0].Joints.Count];
			skel.Rotation = new QuaternionSerializer[data[0].Joints.Count];
			//GetVector3FromJoint(body.Joints[Kinect.JointType.SpineBase]);
			for(int i=0;i<data[0].Joints.Count;i++){
				//Vector3 a=GetVector3FromJoint(data[0].Joints[(Kinect.JointType)i]);
				skel.Position[i]= new Vector3Serializer(GetVector3FromJoint(data[0].Joints[(Kinect.JointType)i]));
			}
			bodyFrames.Add(skel);
		}
		if (Metadata) {
			//bodyFrames.Clear();
			MySkeleton skel = new MySkeleton ();
			Debug.Log ("recording metadata");
			skel.JointId = new int[data[0].Joints.Count];
			skel.Position = new Vector3Serializer[data[0].Joints.Count];
			skel.Rotation = new QuaternionSerializer[data[0].Joints.Count];
			for(int i=0;i<data[0].Joints.Count;i++){
				skel.Position[i] = new Vector3Serializer(GetVector3FromJoint(data[0].Joints[(Kinect.JointType)i]));
			}
			Debug.Log("asu"+skel.Position[0].getVector());
			bodyFrames.Add(skel);

		}
		//serialize (store recorded skeleton as binary)
		if (isSaving) {
			Debug.Log("saving...");
			isSaving = false;
			serialize(bodyFrames);
			bodyFrames.Clear();
		}
		if(isSavingMetadata){
			serializeLength(bodyFrames);
			Debug.Log("Skeletons count:"+boneLengths.Count);
			Debug.Log("saving metadata");
			isSavingMetadata = false;
			bodyFrames.Clear();
		}
		frameCount++;
*/
	}
	/*private Kinect.Body NormalizedBody(Kinect.Body body, Kinect.Body firstBody){
		Kinect.Body newBody=null;

		newBody.Joints [Kinect.JointType.SpineBase].Position.X = body.Joints [Kinect.JointType.SpineBase].Position.X - 
			firstBody.Joints [Kinect.JointType.SpineBase].Position.X;
		newBody.Joints [Kinect.JointType.SpineBase].Position.Y = body.Joints [Kinect.JointType.SpineBase].Position.Y - 
			firstBody.Joints [Kinect.JointType.SpineBase].Position.Y;
		newBody.Joints [Kinect.JointType.SpineBase].Position.Z = body.Joints [Kinect.JointType.SpineBase].Position.Z - 
			firstBody.Joints [Kinect.JointType.SpineBase].Position.Z;

		newBody.Joints [Kinect.JointType.SpineMid] = body.Joints [Kinect.JointType.SpineMid] - 
			firstBody.Joints [Kinect.JointType.SpineMid];
		newBody.Joints [Kinect.JointType.SpineShoulder] = body.Joints [Kinect.JointType.SpineShoulder] - 
			firstBody.Joints [Kinect.JointType.SpineShoulder];
		newBody.Joints [Kinect.JointType.Head] = body.Joints [Kinect.JointType.Head] - 
			firstBody.Joints [Kinect.JointType.Head];

		newBody.Joints [Kinect.JointType.ShoulderLeft] = body.Joints [Kinect.JointType.ShoulderLeft] - 
			firstBody.Joints [Kinect.JointType.ShoulderLeft];
		newBody.Joints [Kinect.JointType.ElbowLeft] = body.Joints [Kinect.JointType.ElbowLeft] - 
			firstBody.Joints [Kinect.JointType.ElbowLeft];
		newBody.Joints [Kinect.JointType.WristLeft] = body.Joints [Kinect.JointType.WristLeft] - 
			firstBody.Joints [Kinect.JointType.WristLeft];
		newBody.Joints [Kinect.JointType.HandLeft] = body.Joints [Kinect.JointType.HandLeft] - 
			firstBody.Joints [Kinect.JointType.HandLeft];

		newBody.Joints [Kinect.JointType.ShoulderRight] = body.Joints [Kinect.JointType.ShoulderRight] - 
			firstBody.Joints [Kinect.JointType.ShoulderRight];
		newBody.Joints [Kinect.JointType.ElbowRight] = body.Joints [Kinect.JointType.ElbowRight] - 
			firstBody.Joints [Kinect.JointType.ElbowRight];
		newBody.Joints [Kinect.JointType.WristRight] = body.Joints [Kinect.JointType.WristRight] - 
			firstBody.Joints [Kinect.JointType.WristRight];
		newBody.Joints [Kinect.JointType.HandRight] = body.Joints [Kinect.JointType.HandRight] - 
			firstBody.Joints [Kinect.JointType.HandRight];

		newBody.Joints [Kinect.JointType.HipLeft] = body.Joints [Kinect.JointType.HipLeft] - 
			firstBody.Joints [Kinect.JointType.HipLeft];
		newBody.Joints [Kinect.JointType.KneeLeft] = body.Joints [Kinect.JointType.KneeLeft] - 
			firstBody.Joints [Kinect.JointType.KneeLeft];
		newBody.Joints [Kinect.JointType.AnkleLeft] = body.Joints [Kinect.JointType.AnkleLeft] - 
			firstBody.Joints [Kinect.JointType.AnkleLeft];
		newBody.Joints [Kinect.JointType.FootLeft] = body.Joints [Kinect.JointType.FootLeft] - 
			firstBody.Joints [Kinect.JointType.FootLeft];

		newBody.Joints [Kinect.JointType.HipRight] = body.Joints [Kinect.JointType.HipRight] - 
			firstBody.Joints [Kinect.JointType.HipRight];
		newBody.Joints [Kinect.JointType.KneeRight] = body.Joints [Kinect.JointType.KneeRight] - 
			firstBody.Joints [Kinect.JointType.KneeRight];
		newBody.Joints [Kinect.JointType.AnkleRight] = body.Joints [Kinect.JointType.AnkleRight] - 
			firstBody.Joints [Kinect.JointType.AnkleRight];
		newBody.Joints [Kinect.JointType.FootRight] = body.Joints [Kinect.JointType.FootRight] - 
			firstBody.Joints [Kinect.JointType.FootRight];
		return newBody;
	}*/


	private void RefreshBodyObjectOrientation(Kinect.Body body){
		//POSITIONS
		/*Vector3 spineBasePosition = GetVector3FromJoint(body.Joints[Kinect.JointType.SpineBase]);
		Vector3 spineMidPosition = GetVector3FromJoint(body.Joints[Kinect.JointType.SpineMid]);
		Vector3 spineShoulderPosition = GetVector3FromJoint(body.Joints[Kinect.JointType.SpineShoulder]);
		Vector3 neckPosition = GetVector3FromJoint(body.Joints[Kinect.JointType.Neck]);
		Vector3 headPosition = GetVector3FromJoint(body.Joints[Kinect.JointType.Head]);
		Vector3 shoulderRight = GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderRight]);

		//ROTATIONS
		Quaternion spineBaseRotation = QuaternionFromVector4 (body.JointOrientations [Kinect.JointType.SpineBase].Orientation);
		Quaternion spineMidRotation = QuaternionFromVector4 (body.JointOrientations [Kinect.JointType.SpineMid].Orientation);
		Quaternion spineShoulderRotation = QuaternionFromVector4 (body.JointOrientations [Kinect.JointType.SpineShoulder].Orientation);
		Quaternion neckRotation = QuaternionFromVector4 (body.JointOrientations [Kinect.JointType.Neck].Orientation);
		Quaternion headRotation = QuaternionFromVector4 (body.JointOrientations [Kinect.JointType.Head].Orientation);
		Quaternion shoulderRightRotation = QuaternionFromVector4 (body.JointOrientations [Kinect.JointType.ShoulderRight].Orientation);
		//HIPS
		Quaternion hipRotation = spineBaseRotation;
		Quaternion hipAngle = Quaternion.AngleAxis(hipRotation.eulerAngles.y,new Vector3(0.0f,1.0f,0.0f));
		Hip_Center.transform.localRotation = Quaternion.Inverse (hipStart) * hipAngle;

		//SPINE
		Quaternion spineAngle = Quaternion.AngleAxis(hipRotation.eulerAngles.z,new Vector3(0.0f,0.0f,1.0f));
		Spine.transform.localRotation = Quaternion.Inverse (spineStart) * Quaternion.Inverse (hipStart) * spineAngle;

		//SHOULDER CENTER
		Quaternion shoulderCenterAngle = Quaternion.Inverse(spineBaseRotation) * spineMidRotation;
		Shoulder_Center.transform.localRotation = Quaternion.Inverse (shoulderCenterStart)* 
			Quaternion.Inverse (spineStart) * Quaternion.Inverse (hipStart) * shoulderCenterAngle;

		//SHOULDER RIGHT
		Quaternion shoulderRightAngle =Quaternion.AngleAxis(shoulderRightRotation.eulerAngles.z,new Vector3(0.0f,0.0f,-1.0f)); 
		Shoulder_Right.transform.localRotation = Quaternion.Inverse (shoulderRightStart)* Quaternion.Inverse (shoulderCenterStart)* 
			Quaternion.Inverse (spineStart) * Quaternion.Inverse (hipStart) * shoulderRightAngle;*/

		//BONES


		//SPINE ROTATION


		//Quaternion spine = QuaternionFromVector4 (body.JointOrientations [Kinect.JointType.SpineMid].Orientation);
		double yaw=0; double pitch=0; double roll=0;


		//ExtractRotationInDegrees 
		//	(UnityV4fromKinectV4(body.JointOrientations [Kinect.JointType.ElbowRight].Orientation),out yaw,out pitch,out roll);
		//Debug.Log ("yaw:" + yaw + " pitch: "+pitch+" roll: "+roll);


		//Debug.Log ("Angle hand Left:" + QuaternionToEuler (UnityV4fromKinectV4(body.JointOrientations [Kinect.JointType.HandLeft].Orientation)));

		//Spine.localRotation = QuaternionFromVector4(body.JointOrientations [Kinect.JointType.SpineMid].Orientation);
		//Shoulder_Center.localRotation = QuaternionFromVector4(body.JointOrientations [Kinect.JointType.SpineShoulder].Orientation);
		//Head.localRotation = QuaternionFromVector4(body.JointOrientations [Kinect.JointType.Head].Orientation);
		//Shoulder_Right.localRotation = QuaternionFromVector4(body.JointOrientations [Kinect.JointType.ShoulderRight].Orientation);
		//Elbow_Right.localRotation = QuaternionFromVector4(body.JointOrientations [Kinect.JointType.ElbowRight].Orientation);
		//Wrist_Right.localRotation = QuaternionFromVector4(body.JointOrientations [Kinect.JointType.WristRight].Orientation);



		//GET CHILD JOINT POSITION
		Kinect.Vector4 vec = body.JointOrientations [Kinect.JointType.ElbowRight].Orientation;
		KinectMathHelper.Quaternion qOrientation = new KinectMathHelper.Quaternion (vec.W,vec.X,vec.Y,vec.Z);
		Kinect.CameraSpacePoint csX=CreateEndPoint(body.Joints[Kinect.JointType.ElbowRight].Position,
		                                           qOrientation.Rotate(0.1f, 0.0f, 0.0f));
		Kinect.CameraSpacePoint csY = CreateEndPoint(body.Joints[Kinect.JointType.ElbowRight].Position, 
		                                             qOrientation.Rotate(0.0f, 0.1f, 0.0f));
		Kinect.CameraSpacePoint csZ = CreateEndPoint(body.Joints[Kinect.JointType.ElbowRight].Position, 
		                                             qOrientation.Rotate(0.0f, 0.0f, 0.1f));



		Kinect.DepthSpacePoint dsX = _BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csX);
		Kinect.DepthSpacePoint dsY = _BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csY);
		Kinect.DepthSpacePoint dsZ = _BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csZ);

		//GET PARENT JOINT POSITION
		JointType parentJoint = KinectMathHelper.KinectHelpers.GetParentJoint(JointType.ElbowRight);
		double AngleBetweenParentChildY = 0;
		double AngleBetweenParentChildX = 0;
		double AngleBetweenParentChildZ = 0;
		//For each vector in the DepthSpacePoint, compute the angle between 
		//  parent and child (only if the joint has a parent)
		if (parentJoint != JointType.ElbowRight)
		{
			
			Windows.Kinect.Vector4 vecParent = body.JointOrientations[parentJoint].Orientation;
			KinectMathHelper.Quaternion qOrientationParent = 
				new KinectMathHelper.Quaternion(vecParent.W, vecParent.X, vecParent.Y, vecParent.Z);
			//(only compute if requested) 
			//if (DrawOrientationAnglesX == true)
			//{
				CameraSpacePoint csXParent = CreateEndPoint(body.Joints[parentJoint].Position, 
			                                            qOrientationParent.Rotate(0.1f, 0.0f, 0.0f));
				DepthSpacePoint dsXParent =_BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csXParent);
				AngleBetweenParentChildX = KinectMathHelper.MathHelpers.
					AngleBetweenPoints(new Vector2(dsX.X, dsX.Y), new Vector2(dsXParent.X, dsXParent.Y));
			//}
			//if (DrawOrientationAnglesY == true)
			//{
			CameraSpacePoint csYParent = CreateEndPoint(body.Joints[parentJoint].Position, qOrientationParent.Rotate(0.0f, 0.1f, 0.0f));
			DepthSpacePoint dsYParent = _BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csYParent);
			AngleBetweenParentChildY = KinectMathHelper.MathHelpers.
				AngleBetweenPoints(new Vector2(dsY.X, dsY.Y), new Vector2(dsYParent.X, dsYParent.Y));
			//}
			//if (DrawOrientationAnglesZ == true)
			//{
			CameraSpacePoint csZParent = CreateEndPoint(body.Joints[parentJoint].Position, qOrientationParent.Rotate(0.0f, 0.0f, 0.1f));
			DepthSpacePoint dsZParent = _BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csZParent);
			AngleBetweenParentChildZ = KinectMathHelper.MathHelpers.
				AngleBetweenPoints(new Vector2(dsZ.X, dsY.Y), new Vector2(dsZParent.X, dsZParent.Y));
			if(AngleBetweenParentChildY<90){
				
//				Shoulder_Right.transform.localRotation = Quaternion.Euler(90,
//				                                                          90,
//				                                                          0
//				                                                          );
//				Shoulder_Right.transform.localRotation = CreateFromYawPitchRoll(
//					Mathf.Deg2Rad*90,
////					0,
//					Mathf.Deg2Rad*180-(float)(Mathf.Deg2Rad*(AngleBetweenParentChildY)),
//					0);

//				Shoulder_Right.transform.localRotation = CreateFromYawPitchRoll(
//					body.JointOrientations [Kinect.JointType.ElbowRight].Orientation.X/
//					body.JointOrientations [Kinect.JointType.ElbowRight].Orientation.W,
//					Mathf.Deg2Rad*body.JointOrientations [Kinect.JointType.ElbowRight].Orientation.Y/
//					body.JointOrientations [Kinect.JointType.ElbowRight].Orientation.W,
//					body.JointOrientations [Kinect.JointType.ElbowRight].Orientation.Z/
//					body.JointOrientations [Kinect.JointType.ElbowRight].Orientation.W
//					);

			}

//			Debug.Log("Y kinect: "+
//			          body.JointOrientations [Kinect.JointType.ShoulderRight].Orientation.Y);
//			Debug.Log("X rot:"+AngleBetweenParentChildX);
			//Debug.Log("Y rot" +AngleBetweenParentChildY);
//			Debug.Log("Z rot" +AngleBetweenParentChildZ);
			//}
		}


//		Kinect.Vector4 vec2 = body.JointOrientations [Kinect.JointType.ShoulderRight].Orientation;
//		KinectMathHelper.Quaternion qOrientation2 = new KinectMathHelper.Quaternion (vec2.W,vec2.X,vec2.Y,vec2.Z);
//		Kinect.CameraSpacePoint csX2=CreateEndPoint(body.Joints[Kinect.JointType.ShoulderRight].Position,
//		                                            qOrientation2.Rotate(0.1f, 0.0f, 0.0f));
//		Kinect.CameraSpacePoint csY2 = CreateEndPoint(body.Joints[Kinect.JointType.ShoulderRight].Position, 
//		                                              qOrientation2.Rotate(0.0f, 0.1f, 0.0f));
//		Kinect.CameraSpacePoint csZ2 = CreateEndPoint(body.Joints[Kinect.JointType.ShoulderRight].Position, 
//		                                              qOrientation2.Rotate(0.0f, 0.0f, 0.1f));
//		
//		
//		
//		Kinect.DepthSpacePoint dsX2 = _BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csX2);
//		Kinect.DepthSpacePoint dsY2 = _BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csY2);
//		Kinect.DepthSpacePoint dsZ2 = _BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csZ2);
//
//
//		//GET PARENT JOINT POSITION
//		JointType parentJoint2 = KinectMathHelper.KinectHelpers.GetParentJoint(JointType.ShoulderRight);
//		double AngleBetweenParentChildY2 = 0;
//		double AngleBetweenParentChildX2 = 0;
//		double AngleBetweenParentChildZ2 = 0;
//		//For each vector in the DepthSpacePoint, compute the angle between 
//		//  parent and child (only if the joint has a parent)
//		if (parentJoint2 != JointType.ShoulderRight)
//		{
//			
//			Windows.Kinect.Vector4 vecParent2 = body.JointOrientations[parentJoint2].Orientation;
//			KinectMathHelper.Quaternion qOrientationParent2 = 
//				new KinectMathHelper.Quaternion(vecParent2.W, vecParent2.X, vecParent2.Y, vecParent2.Z);
//			//(only compute if requested) 
//			//if (DrawOrientationAnglesX == true)
//			//{
//			CameraSpacePoint csXParent2 = CreateEndPoint(body.Joints[parentJoint2].Position, 
//			                                            qOrientationParent2.Rotate(0.1f, 0.0f, 0.0f));
//			DepthSpacePoint dsXParent2 =_BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csXParent2);
//			AngleBetweenParentChildX2 = KinectMathHelper.MathHelpers.
//				AngleBetweenPoints(new Vector2(dsX2.X, dsX2.Y), new Vector2(dsXParent2.X, dsXParent2.Y));
//			//}
//			//if (DrawOrientationAnglesY == true)
//			//{
//			CameraSpacePoint csYParent2 = CreateEndPoint(body.Joints[parentJoint2].Position, qOrientationParent2.Rotate(0.0f, 0.1f, 0.0f));
//			DepthSpacePoint dsYParent2 = _BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csYParent2);
//			AngleBetweenParentChildY2 = KinectMathHelper.MathHelpers.
//				AngleBetweenPoints(new Vector2(dsY2.X, dsY2.Y), new Vector2(dsYParent2.X, dsYParent2.Y));
//			//}
//			//if (DrawOrientationAnglesZ == true)
//			//{
//			CameraSpacePoint csZParent2 = CreateEndPoint(body.Joints[parentJoint2].Position, qOrientationParent2.Rotate(0.0f, 0.0f, 0.1f));
//			DepthSpacePoint dsZParent2 = _BodyManager.getCoordinateMapper().MapCameraPointToDepthSpace(csZParent2);
//			AngleBetweenParentChildZ2 = KinectMathHelper.MathHelpers.
//				AngleBetweenPoints(new Vector2(dsZ2.X, dsY2.Y), new Vector2(dsZParent2.X, dsZParent2.Y));
//			
//			Shoulder_Right.transform.localRotation =Quaternion.Euler(
//													  0,
//													  -(float)AngleBetweenParentChildY2,
//                                                      0
//                                                      );
////			Debug.Log("X rot:"+AngleBetweenParentChildX2);
//			Debug.Log("Y rot" +AngleBetweenParentChildY2);
////			Debug.Log("Z rot" +AngleBetweenParentChildZ2);
//			//}
//		}


		Vector3 upperRightHand = new Vector3 (
			body.Joints [JointType.ShoulderRight].Position.X - body.Joints [JointType.ElbowRight].Position.X,
			body.Joints [JointType.ShoulderRight].Position.Y - body.Joints [JointType.ElbowRight].Position.Y,
			body.Joints [JointType.ShoulderRight].Position.Z - body.Joints [JointType.ElbowRight].Position.Z);

		Shoulder_Right.transform.localRotation = Quaternion.AngleAxis(
			Mathf.Rad2Deg*Mathf.Acos(Vector3.Dot (Vector3.left,Vector3.Normalize (upperRightHand))),
			Vector3.Normalize(Vector3.Cross(Vector3.left,Vector3.Normalize(upperRightHand))));
		Debug.Log ("angle: " + Mathf.Rad2Deg*Mathf.Acos(Vector3.Dot (Vector3.left,Vector3.Normalize (upperRightHand))));
		Debug.Log ("axis: " + Vector3.Normalize(Vector3.Cross(Vector3.left,Vector3.Normalize(upperRightHand))));

	}
	public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
	{
		float rollOver2 = roll * 0.5f;
		float sinRollOver2 = (float)Math.Sin((double)rollOver2);
		float cosRollOver2 = (float)Math.Cos((double)rollOver2);
		float pitchOver2 = pitch * 0.5f;
		float sinPitchOver2 = (float)Math.Sin((double)pitchOver2);
		float cosPitchOver2 = (float)Math.Cos((double)pitchOver2);
		float yawOver2 = yaw * 0.5f;
		float sinYawOver2 = (float)Math.Sin((double)yawOver2);
		float cosYawOver2 = (float)Math.Cos((double)yawOver2);
		Quaternion result;
		result.x = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
		result.y = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;
		result.z = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
		result.w = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
		return result;
	} 
	private Kinect.CameraSpacePoint CreateEndPoint(Kinect.CameraSpacePoint startP, float[] vec)
	{
		Kinect.CameraSpacePoint point = new Kinect.CameraSpacePoint();
		point.X = startP.X + vec[0];
		point.Y = startP.Y + vec[1];
		point.Z = startP.Z + vec[2];
		return point;
	}
	private static void ExtractRotationInDegrees(UnityEngine.Vector4 rotQuaternion, out double pitch, out double yaw, out double roll)
	{

		double x = rotQuaternion.x;
		double y = rotQuaternion.y;
		double z = rotQuaternion.z;
		double w = rotQuaternion.w;
		
		// convert rotation quaternion to Euler angles in degrees
		//double yawD, pitchD, rollD;
		pitch = Math.Atan2(2 * ((y * z) + (w * x)), (w * w) - (x * x) - (y * y) + (z * z)) / Math.PI * 180.0;
		yaw = Math.Asin(2 * ((w * y) - (x * z))) / Math.PI * 180.0;
		roll = Math.Atan2(2 * ((x * y) + (w * z)), (w * w) + (x * x) - (y * y) - (z * z)) / Math.PI * 180.0;
	}

	public static Vector3 QuaternionToEuler(UnityEngine.Vector4 q){
		Vector3 v = Vector3.zero;
		
		//If the Z, pitch, attitude is straight up or straight down, Y, roll, bank is zero
		//  X, Yaw, heading is + or - the ... umm... the angle computed from Atan2 of X,W
		// Basically, from what I understand, this choses Heading, Yaw or X
		//  when faced with gimbal lock and zeros Bank, Roll or Y
		if (q.x * q.y + q.z * q.w == 0.5)
		{
			//X Angle represents Yaw, Heading
			v.x =(float) (2 * Math.Atan2(q.x, q.w));
			//Y Angle represents Roll, Bank
			v.y = 0;
		}
		else if (q.x * q.y +q.z  * q.w == -0.5)
		{
			//X Angle represents Yaw, Heading
			v.x = (float)(-2 * Math.Atan2(q.x, q.w));
			//Y Angle represents Roll, Bank
			v.y = 0;
		}
		else
		{
			//X Angle represents Yaw, heading 
			v.x =(float) Math.Atan2(2 * q.y * q.w - 2 * q.w * q.z,
			                 1 - 2 * Math.Pow(q.y, 2) - 2 * Math.Pow(q.z, 2));
			
			//Y Angle represents Roll, bank
			v.y =(float) Math.Atan2(2 * q.x * q.w - 2 * q.y * q.z,
			                 1 - 2 * Math.Pow(q.x, 2) - 2 * Math.Pow(q.z, 2));
		}
		
		//Z Angle represents Pitch, attitude
		v.z =(float) Math.Asin(2 * q.x * q.y + 2 * q.z * q.w);
		
		
		//Convert the Euler angles from Radians to Degrees
		v.x = RadianToDegree(v.x);
		v.y = RadianToDegree(v.y);
		v.z = RadianToDegree(v.z);
		return v;
	}
	private static float RadianToDegree(float angle)
	{//Return degrees (0->360) from radians
		return (float)(angle * (180.0 / Math.PI) + 180);
	}
	private UnityEngine.Vector4 UnityV4fromKinectV4(Windows.Kinect.Vector4 inVector){
		return new UnityEngine.Vector4 (inVector.W, inVector.X, inVector.Y, inVector.Z);
	}
	private Quaternion QuaternionFromVector4(Windows.Kinect.Vector4 inVector)
	{
		return new Quaternion(inVector.X, inVector.Y, inVector.Z, inVector.W);
	}
	private void RefreshBodyObject(Kinect.Body body, Kinect.Body firstBody)
	{
		
		Hip_Center.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.SpineBase]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.SpineBase]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.SpineBase]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.SpineBase]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.SpineBase]).z);

		Spine.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.SpineMid]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.SpineMid]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.SpineMid]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.SpineMid]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.SpineMid]).z);

		Shoulder_Center.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.SpineShoulder]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.SpineShoulder]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.SpineShoulder]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.SpineShoulder]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.SpineShoulder]).z);

		Head.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.Head]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.Head]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.Head]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.Head]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.Head]).z);

		Shoulder_Left.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderLeft]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.ShoulderLeft]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderLeft]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderLeft]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.ShoulderLeft]).z);

		Elbow_Left.localPosition  = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.ElbowLeft]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.ElbowLeft]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.ElbowLeft]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.ElbowLeft]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.ElbowLeft]).z);

		Wrist_Left.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.WristLeft]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.WristLeft]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.WristLeft]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.WristLeft]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.WristLeft]).z);

		Hand_Left.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.HandLeft]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.HandLeft]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.HandLeft]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.HandLeft]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.HandLeft]).z);

		Shoulder_Right.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderRight]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.ShoulderRight]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderRight]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderRight]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.ShoulderRight]).z);

		Elbow_Right.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.ElbowRight]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.ElbowRight]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.ElbowRight]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.ElbowRight]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.ElbowRight]).z);

		Wrist_Right.localPosition  = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.WristRight]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.WristRight]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.WristRight]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.WristRight]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.WristRight]).z);

		Hand_Right.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.HandRight]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.HandRight]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.HandRight]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.HandRight]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.HandRight]).z);

		Hip_Left.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.HipLeft]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.HipLeft]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.HipLeft]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.HipLeft]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.HipLeft]).z);

		Knee_Left.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.KneeLeft]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.KneeLeft]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.KneeLeft]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.KneeLeft]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.KneeLeft]).z);

		Ankle_Left.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.AnkleLeft]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.AnkleLeft]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.AnkleLeft]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.AnkleLeft]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.AnkleLeft]).z);

		Foot_Left.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.FootLeft]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.FootLeft]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.FootLeft]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.FootLeft]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.FootLeft]).z);

		Hip_Right.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.HipRight]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.HipRight]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.HipRight]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.HipRight]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.HipRight]).z);

		Knee_Right.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.KneeRight]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.KneeRight]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.KneeRight]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.KneeRight]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.KneeRight]).z);

		Ankle_Right.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.AnkleRight]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.AnkleRight]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.AnkleRight]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.AnkleRight]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.AnkleRight]).z);

		Foot_Right.localPosition = new Vector3 (
			GetVector3FromJoint(body.Joints[Kinect.JointType.FootRight]).x - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.FootRight]).x,
			GetVector3FromJoint(body.Joints[Kinect.JointType.FootRight]).y,
			GetVector3FromJoint(body.Joints[Kinect.JointType.FootRight]).z - GetVector3FromJoint(firstBody.Joints[Kinect.JointType.FootRight]).z);

		
	}
	private void serialize(List<MySkeleton> datas) {
		FileStream f = null;
		try{
			BinaryFormatter bFormat = new BinaryFormatter();
			f=File.Create(Application.persistentDataPath+fileLocation);
			bFormat.Serialize(f,datas);
			Debug.Log(Application.persistentDataPath+fileLocation);
		}catch(System.Exception e){
			Debug.Log(e);
		}finally{
			if (f != null) {
				f.Close();
			}
		}

	}
	private double calculateBoneLength(MySkeleton skeleton,Kinect.JointType jointType1, Kinect.JointType jointType2) {
		//Debug.Log (jointType1.ToString () + skeleton.Position [(int)jointType1].getVector ().x);
		double length = Math.Round(Math.Sqrt(
			(Math.Pow(skeleton.Position[(int)jointType1].getVector().x - skeleton.Position[(int)jointType2].getVector().x, 2)) +
			(Math.Pow(skeleton.Position[(int)jointType1].getVector().y - skeleton.Position[(int)jointType2].getVector().y, 2)) +
			(Math.Pow(skeleton.Position[(int)jointType1].getVector().z - skeleton.Position[(int)jointType2].getVector().z, 2))), 3);
		return length;
	}
	public void serializeLength(List<MySkeleton> datas) {
		double spine = 0, shoulderCenter = 0, head = 0;
		double shoulderLeft = 0, elbowLeft = 0, wristLeft = 0, handLeft = 0;
		double shoulderRight = 0, elbowRight = 0, wristRight = 0, handRight = 0;
		double hipLeft = 0, kneeLeft = 0, ankleLeft = 0, footLeft = 0;
		double hipRight = 0, kneeRight = 0, ankleRight = 0, footRight = 0;

		for (int i = 0; i < datas.Count; i++) {

			spine += calculateBoneLength(datas[i], Kinect.JointType.SpineBase, Kinect.JointType.SpineMid);
			shoulderCenter += calculateBoneLength(datas[i], Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder);
			head += calculateBoneLength(datas[i], Kinect.JointType.SpineShoulder, Kinect.JointType.Head);
			
			shoulderLeft += calculateBoneLength(datas[i], Kinect.JointType.SpineShoulder, Kinect.JointType.ShoulderLeft);
			elbowLeft += calculateBoneLength(datas[i], Kinect.JointType.ShoulderLeft, Kinect.JointType.ElbowLeft);
			wristLeft += calculateBoneLength(datas[i], Kinect.JointType.ElbowLeft, Kinect.JointType.WristLeft);
			handLeft += calculateBoneLength(datas[i], Kinect.JointType.WristLeft, Kinect.JointType.HandLeft);
			
			shoulderRight += calculateBoneLength(datas[i], Kinect.JointType.SpineShoulder, Kinect.JointType.ShoulderRight);
			elbowRight += calculateBoneLength(datas[i], Kinect.JointType.ShoulderRight, Kinect.JointType.ElbowRight);
			wristRight += calculateBoneLength(datas[i], Kinect.JointType.ElbowRight, Kinect.JointType.WristRight);
			handRight += calculateBoneLength(datas[i], Kinect.JointType.WristRight, Kinect.JointType.HandRight);
			
			hipLeft += calculateBoneLength(datas[i], Kinect.JointType.SpineBase, Kinect.JointType.HipLeft);
			kneeLeft += calculateBoneLength(datas[i], Kinect.JointType.HipLeft, Kinect.JointType.KneeLeft);
			ankleLeft += calculateBoneLength(datas[i], Kinect.JointType.KneeLeft, Kinect.JointType.AnkleLeft);
			footLeft += calculateBoneLength(datas[i], Kinect.JointType.AnkleLeft, Kinect.JointType.FootLeft);
			
			hipRight += calculateBoneLength(datas[i], Kinect.JointType.SpineBase, Kinect.JointType.HipRight);
			kneeRight += calculateBoneLength(datas[i], Kinect.JointType.HipRight, Kinect.JointType.KneeRight);
			ankleRight += calculateBoneLength(datas[i], Kinect.JointType.KneeRight, Kinect.JointType.AnkleRight);
			footRight += calculateBoneLength(datas[i], Kinect.JointType.AnkleRight, Kinect.JointType.FootRight);
			
		}
		/*Debug.Log ("spine" + spine);
		Debug.Log ("shoulderCenter" + shoulderCenter);
		Debug.Log ("shoulderLeft" + shoulderLeft);
		Debug.Log ("elbowLeft" + elbowLeft);
		Debug.Log ("wristLeft" + wristLeft);
		Debug.Log ("handLeft" + handLeft);
		Debug.Log ("shoulderRight" + shoulderRight);
		Debug.Log ("elbowRight" + elbowRight);
		Debug.Log ("wristRight" + wristRight);
		Debug.Log ("handRight" + handRight);
		Debug.Log ("hipLeft" + hipLeft);
		Debug.Log ("kneeLeft" + kneeLeft);
		Debug.Log ("ankleLeft" + ankleLeft);
		Debug.Log ("footLeft" + footLeft);
		Debug.Log ("hipRight" + hipRight);
		Debug.Log ("kneeRight" + kneeRight);
		Debug.Log ("ankleRight" + ankleRight);
		Debug.Log ("footRight" + footRight);*/
		spine /= datas.Count;
		shoulderCenter /= datas.Count;
		head /= datas.Count;
		shoulderLeft /= datas.Count;
		elbowLeft /= datas.Count;
		wristLeft /= datas.Count;
		handLeft /= datas.Count;
		shoulderRight /= datas.Count;
		elbowRight /= datas.Count;
		wristRight /= datas.Count;
		handRight /= datas.Count;
		hipLeft /= datas.Count;
		kneeLeft /= datas.Count;
		ankleLeft /= datas.Count;
		footLeft /= datas.Count;
		hipRight /= datas.Count;
		kneeRight /= datas.Count;
		ankleRight /= datas.Count;
		footRight /= datas.Count;



		boneLengths.Add(spine);
		boneLengths.Add(shoulderCenter);
		boneLengths.Add(head);
		
		boneLengths.Add(shoulderLeft);
		boneLengths.Add(elbowLeft);
		boneLengths.Add(wristLeft);
		boneLengths.Add(handLeft);
		
		boneLengths.Add(shoulderRight);
		boneLengths.Add(elbowRight);
		boneLengths.Add(wristRight);
		boneLengths.Add(handRight);
		
		boneLengths.Add(hipLeft);
		boneLengths.Add(kneeLeft);
		boneLengths.Add(ankleLeft);
		boneLengths.Add(footLeft);
		
		boneLengths.Add(hipRight);
		boneLengths.Add(kneeRight);
		boneLengths.Add(ankleRight);
		boneLengths.Add(footRight);
		
		FileStream f = null;
		BinaryFormatter bFormat = new BinaryFormatter();
		f=File.Create(Application.persistentDataPath+lengthFileLocation);
		Debug.Log ("saved at: " + Application.persistentDataPath + lengthFileLocation);

		bFormat.Serialize(f, boneLengths);
		if (f != null) {
			f.Close();
		}
		boneLengths.Clear ();
	}
	private List<MySkeleton> deserialize() {
		List<MySkeleton> data = null;
		FileStream f = null;

		BinaryFormatter formater=new BinaryFormatter();
		f = File.Open(Application.persistentDataPath+fileLocation, FileMode.Open);
		data = (List<MySkeleton>)formater.Deserialize(f);
		f.Close();
		return data;
	}

}
