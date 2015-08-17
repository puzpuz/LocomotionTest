using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class AvatarControllerKinect : MonoBehaviour {
	//added
	public GameObject BodySourceManager;
	private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
	private BodySourceManager _BodyManager;
	private Dictionary<JointType, JointType> _BoneMap = new Dictionary<JointType, JointType>()
	{
		{ JointType.FootLeft, JointType.AnkleLeft },
		{ JointType.AnkleLeft, JointType.KneeLeft },
		{ JointType.KneeLeft, JointType.HipLeft },
		{ JointType.HipLeft, JointType.SpineBase },
		
		{ JointType.FootRight, JointType.AnkleRight },
		{ JointType.AnkleRight, JointType.KneeRight },
		{ JointType.KneeRight, JointType.HipRight },
		{ JointType.HipRight, JointType.SpineBase },
		
		{ JointType.HandTipLeft, JointType.HandLeft },
		{ JointType.ThumbLeft, JointType.HandLeft },
		{ JointType.HandLeft, JointType.WristLeft },
		{ JointType.WristLeft, JointType.ElbowLeft },
		{ JointType.ElbowLeft, JointType.ShoulderLeft },
		{ JointType.ShoulderLeft, JointType.SpineShoulder },
		
		{ JointType.HandTipRight, JointType.HandRight },
		{ JointType.ThumbRight, JointType.HandRight },
		{ JointType.HandRight, JointType.WristRight },
		{ JointType.WristRight, JointType.ElbowRight },
		{ JointType.ElbowRight, JointType.ShoulderRight },
		{ JointType.ShoulderRight, JointType.SpineShoulder },
		
		{ JointType.SpineBase, JointType.SpineMid },
		{ JointType.SpineMid, JointType.SpineShoulder },
		{ JointType.SpineShoulder, JointType.Neck },
		{ JointType.Neck, JointType.Head },
	};

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
	
//	private Vector3 initialPosition;
	private Quaternion initialRotation;
	private Vector3 initialPosOffset = Vector3.zero;
	private uint initialPosUserID = 0;
	private String fileLocation = "/skeletonData.mot1";
	private String lengthFileLocation = "/model.metadata";
	private KinectSensor kinectSensor;
	private List<MySkeleton> bodyFrames;
	private List<double> boneLengths; 

	private static Vector3 GetVector3FromJoint(Windows.Kinect.Joint joint)
		
	{
		return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
	}
	
	private bool isSaving=false;
	private bool isSavingMetadata = false;
	private bool Metadata = false;
	private bool Recording = false;
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
	//ADJUSTED QUATERNION
	Quaternion hipAdjust;
	Quaternion spineAdjust;
	Quaternion shoulderCenterAdjust;
	Quaternion hipLeftAdjust;
	Quaternion kneeLeftAdjust;
	Quaternion hipRightAdjust;
	Quaternion kneeRightAdjust;
	Quaternion shoulderRightAdjust;
	Quaternion shoulderLeftAdjust;
	Quaternion elbowLeftAdjust;
	Quaternion elbowRightAdjust;
	//BIND QUATERNION
	Quaternion hipBind;
	Quaternion spineBind;
	Quaternion shoulderCenterBind;
	Quaternion rightShoulderBind;
	Quaternion hipLeftBind;
	Quaternion kneeLeftBind;
	Quaternion hipRightBind;
	Quaternion kneeRightBind;
	Quaternion shoulderRightBind;
	Quaternion shoulderLeftBind;
	Quaternion elbowLeftBind;
	Quaternion elbowRightBind;
	Vector3 positionBind;
	void Start () 
	{
		//INITIATE BINDING POSE
		rightShoulderBind = Shoulder_Right.transform.rotation;
		spineBind = Spine.transform.rotation;
		hipBind = Hip_Center.transform.rotation;
		shoulderCenterBind = Shoulder_Center.rotation;
		hipLeftBind = Hip_Left.rotation;
		hipRightBind = Hip_Right.rotation;
		shoulderRightBind = Shoulder_Right.rotation;
		kneeRightBind = Knee_Right.rotation;
		kneeLeftBind = Knee_Left.rotation;
		shoulderLeftBind = Shoulder_Left.rotation;
		elbowLeftBind = Elbow_Left.rotation;
		elbowRightBind = Elbow_Right.rotation;
		positionBind = Hip_Center.position;
		//INITIATE ADJUSTED POSE
		spineAdjust = Quaternion.FromToRotation(Spine.transform.position-Shoulder_Center.transform.position,Vector3.down);
		hipAdjust = Quaternion.FromToRotation(Hip_Center.transform.position-Spine.transform.position,Vector3.down);
		shoulderCenterAdjust = Quaternion.FromToRotation(Shoulder_Center.transform.position-Head.transform.position,Vector3.down);
		hipLeftAdjust = Quaternion.FromToRotation(Hip_Left.transform.position-Knee_Left.transform.position,Vector3.down);
		kneeLeftAdjust = Quaternion.FromToRotation(Knee_Left.transform.position-Ankle_Left.transform.position,Vector3.down);
		hipRightAdjust = Quaternion.FromToRotation(Hip_Right.transform.position-Knee_Right.transform.position,Vector3.down);
		kneeRightAdjust = Quaternion.FromToRotation(Knee_Right.transform.position-Ankle_Right.transform.position,Vector3.down);
		shoulderRightAdjust = Quaternion.FromToRotation(Shoulder_Right.transform.position-Elbow_Right.transform.position,Vector3.down);
		shoulderLeftAdjust = Quaternion.FromToRotation (Shoulder_Left.transform.position - Elbow_Left.transform.position, Vector3.down);
		elbowLeftAdjust = Quaternion.FromToRotation (Elbow_Left.transform.position - Wrist_Left.transform.position, Vector3.down);
		elbowRightAdjust = Quaternion.FromToRotation (Elbow_Right.transform.position - Wrist_Right.transform.position, Vector3.down);
		Hip_Center.rotation = Quaternion.Euler (new Vector3(0,180,0));
		//bodyFrames = new List<Body[]>();
		bodyFrames = new List<MySkeleton> ();
		boneLengths = new List<double> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (BodySourceManager == null)
		{
			return;
		}
		
		_BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
		if (_BodyManager == null)
		{
			return;
		}
		
		Body[] data = _BodyManager.GetData();
		if (data == null)
		{
			return;
		}
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
			if (data [i] == null) {
				continue;
			}
			
			if (data [i].IsTracked) {
				if(ctr==0){
					initialPosition = data[i].Joints[JointType.SpineBase].Position
				}
				RefreshBodyObjectOrientation(data[i]);
				ctr++;
			}	
		}
	}
	private int ctr = 0;
	private Windows.Kinect.CameraSpacePoint initialPosition;
	public static Vector3 QuaternionToEuler(Quaternion q){
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
		v.x = Mathf.Rad2Deg*v.x;
		v.y = Mathf.Rad2Deg*v.y;
		v.z = Mathf.Rad2Deg*v.z;
		return v;
	}
	private Quaternion convertCoordinateSystem (Quaternion input){
		return new Quaternion (input.x, input.y, -input.z, -input.w);
	}

	private void RefreshBodyObjectOrientation(Body body){
		Windows.Kinect.Vector4 spineBase = body.JointOrientations [JointType.SpineBase].Orientation;
		Windows.Kinect.Vector4 spineMid = body.JointOrientations [JointType.SpineMid].Orientation;
		Windows.Kinect.Vector4 spineShoulder = body.JointOrientations [JointType.SpineShoulder].Orientation;
		Windows.Kinect.Vector4 neck = body.JointOrientations [JointType.Neck].Orientation;
		Windows.Kinect.Vector4 kneeRight = body.JointOrientations [JointType.KneeRight].Orientation;
		Windows.Kinect.Vector4 kneeLeft = body.JointOrientations [JointType.KneeLeft].Orientation;
		Windows.Kinect.Vector4 ankleRight = body.JointOrientations [JointType.AnkleRight].Orientation;
		Windows.Kinect.Vector4 ankleLeft = body.JointOrientations [JointType.AnkleLeft].Orientation;
		Windows.Kinect.Vector4 elbowRight = body.JointOrientations [JointType.ElbowRight].Orientation;
		Windows.Kinect.Vector4 elbowLeft = body.JointOrientations [JointType.ElbowLeft].Orientation;
		Windows.Kinect.Vector4 wristRight = body.JointOrientations [JointType.WristRight].Orientation;
		Windows.Kinect.Vector4 wristLeft = body.JointOrientations [JointType.WristLeft].Orientation;
		Windows.Kinect.CameraSpacePoint position = body.Joints [JointType.SpineBase].Position - initialPosition;


//		Quaternion spineBaseRotation = Quaternion.Inverse (new Quaternion(spineBase.X,spineBase.Y,spineBase.Z,spineBase.W))*
//			new Quaternion(spineMid.X,spineMid.Y,spineMid.Z,spineMid.W);
//
//		Quaternion spineMidRotation = Quaternion.Euler(0,180,0) * Quaternion.Inverse (spineBaseRotation)*
//			new Quaternion(spineShoulder.X,spineShoulder.Y,spineShoulder.Z,spineShoulder.W);
//
//		Quaternion spineShoulderRotation = Quaternion.Euler(0,180,0) * Quaternion.Inverse (spineMidRotation)*
//			new Quaternion(neck.X,neck.Y,neck.Z,neck.W);
//		Quaternion shoulderRightRotation = Quaternion.Euler(0,180,0) * Quaternion.Inverse (spineShoulderRotation)*
//			new Quaternion(elbowRight.X,elbowRight.Y,elbowRight.Z,elbowRight.W);

//		USING CONVERT COORDINATE SYSTEN
//		Spine.transform.localRotation = convertCoordinateSystem(spineMidRotation);
//		Shoulder_Center.transform.localRotation = convertCoordinateSystem(spineShoulderRotation);
//		Shoulder_Left.transform.localRotation = convertCoordinateSystem(shoulderRightRotation);
//		Debug.Log ("SpineMidRotation" + QuaternionToEuler(convertCoordinateSystem(shoulderRightRotation)));


//		USING KINECT QUATERNION
//		Hip_Center.transform.rotation = new Quaternion (spineMid.X, spineMid.Y, spineMid.Z, spineMid.W) * hipAdjust * hipBind;
		Spine.transform.rotation = new Quaternion (spineShoulder.X, spineShoulder.Y, spineShoulder.Z, spineShoulder.W) * spineAdjust * spineBind;
		Shoulder_Center.transform.rotation = new Quaternion (neck.X, neck.Y, neck.Z, neck.W) * shoulderCenterAdjust * shoulderCenterBind;
		Shoulder_Left.transform.rotation = new Quaternion (elbowRight.X, elbowRight.Y, elbowRight.Z, elbowRight.W) * shoulderLeftAdjust * 
			shoulderLeftBind;
		Shoulder_Right.transform.rotation = new Quaternion (elbowLeft.X, elbowLeft.Y, elbowLeft.Z, elbowLeft.W) * shoulderRightAdjust * 
			shoulderRightBind;
		Elbow_Left.transform.rotation = new Quaternion (wristRight.X, wristRight.Y, wristRight.Z, wristRight.W) * elbowLeftAdjust * 
			elbowLeftBind;
		Elbow_Right.transform.rotation = new Quaternion (wristLeft.X, wristLeft.Y, wristLeft.Z, wristLeft.W) * elbowRightAdjust * 
			elbowRightBind;


		Hip_Left.transform.rotation = new Quaternion (kneeRight.X, kneeRight.Y, kneeRight.Z, kneeRight.W) * hipLeftAdjust * hipLeftBind * 
			Quaternion.Euler(new Vector3(0,0,180));
		Knee_Left.transform.rotation = new Quaternion (ankleRight.X, ankleRight.Y, ankleRight.Z, ankleRight.W) * kneeLeftAdjust * kneeLeftBind;

		Hip_Right.transform.rotation = new Quaternion (kneeLeft.X, kneeLeft.Y, kneeLeft.Z, kneeLeft.W) * hipRightAdjust * hipRightBind* 
			Quaternion.Euler(new Vector3(0,0,-90));
		Knee_Right.transform.rotation = new Quaternion (ankleLeft.X, ankleLeft.Y, ankleLeft.Z, ankleLeft.W) * kneeRightAdjust * kneeRightBind* 
			Quaternion.Euler(new Vector3(0,0,90));
		Hip_Center.transform.position = positionBind + new Vector3 (position.X,position.Y,position.Z);
		//		Hip_Left.transform.rotation = new Quaternion (kneeLeft.X, kneeLeft.Y, kneeLeft.Z, kneeLeft.W) * hipLeftAdjust * hipLeftBind;
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
	private double calculateBoneLength(MySkeleton skeleton,JointType jointType1, JointType jointType2) {
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
			
			spine += calculateBoneLength(datas[i], JointType.SpineBase, JointType.SpineMid);
			shoulderCenter += calculateBoneLength(datas[i], JointType.SpineMid, JointType.SpineShoulder);
			head += calculateBoneLength(datas[i], JointType.SpineShoulder, JointType.Head);
			
			shoulderLeft += calculateBoneLength(datas[i], JointType.SpineShoulder, JointType.ShoulderLeft);
			elbowLeft += calculateBoneLength(datas[i], JointType.ShoulderLeft, JointType.ElbowLeft);
			wristLeft += calculateBoneLength(datas[i], JointType.ElbowLeft, JointType.WristLeft);
			handLeft += calculateBoneLength(datas[i], JointType.WristLeft, JointType.HandLeft);
			
			shoulderRight += calculateBoneLength(datas[i], JointType.SpineShoulder, JointType.ShoulderRight);
			elbowRight += calculateBoneLength(datas[i], JointType.ShoulderRight, JointType.ElbowRight);
			wristRight += calculateBoneLength(datas[i], JointType.ElbowRight, JointType.WristRight);
			handRight += calculateBoneLength(datas[i], JointType.WristRight, JointType.HandRight);
			
			hipLeft += calculateBoneLength(datas[i], JointType.SpineBase, JointType.HipLeft);
			kneeLeft += calculateBoneLength(datas[i], JointType.HipLeft, JointType.KneeLeft);
			ankleLeft += calculateBoneLength(datas[i], JointType.KneeLeft, JointType.AnkleLeft);
			footLeft += calculateBoneLength(datas[i], JointType.AnkleLeft, JointType.FootLeft);
			
			hipRight += calculateBoneLength(datas[i], JointType.SpineBase, JointType.HipRight);
			kneeRight += calculateBoneLength(datas[i], JointType.HipRight, JointType.KneeRight);
			ankleRight += calculateBoneLength(datas[i], JointType.KneeRight, JointType.AnkleRight);
			footRight += calculateBoneLength(datas[i], JointType.AnkleRight, JointType.FootRight);
			
		}
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
