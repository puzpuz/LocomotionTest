
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class CubeManResized : MonoBehaviour 
{
	
	
	//added
	public GameObject BodySourceManager;
	private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
	private BodySourceManager _BodyManager;
	//private BoxCollider boxCollider;
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
	private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
	{
		return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
	}
	
	private bool isPlaying=false;
	private SkeletonResizer resizer;
	private int counter = 0;
	void OnGUI(){
		Event e = Event.current;
		if (e.isKey) {
			if(e.keyCode==KeyCode.P){
				Debug.Log("Replay start");
				bodyFrames.Clear();
				bodyFrames.AddRange(deserialize());
				resizer.deserializer();
				for(int i=0;i<bodyFrames.Count;i++){
					bodyFrames[i] = resizer.resizeSkeleton(bodyFrames[i]);
				}
				double height=calculateHeight(bodyFrames[0],Kinect.JointType.Head,Kinect.JointType.FootLeft);
				Debug.Log("Height trainee:"+height);
				//Vector3 size = boxCollider.size;
				//size.y=(float)(height+ 0.8f);
				//boxCollider.size = size;
				isPlaying=true;
			}
			
		}
	}
	void Start () 
	{
		//bodyFrames = new List<Kinect.Body[]>();
		bodyFrames = new List<MySkeleton> ();
		boneLengths = new List<double> ();
		resizer = new SkeletonResizer ();
		//boxCollider=this.gameObject.AddComponent<BoxCollider>();
		//boxCollider.center = new Vector3 (Hip_Center.position.x ,Hip_Center.position.y,Hip_Center.position.z);
		//boxCollider.size = new Vector3 (1f, 1.87f, 1f);
	}
	//private List<Kinect.Body[]> bodyFrames;
	private List<MySkeleton> bodyFrames;
	private List<double> boneLengths; 
	// Update is called once per frame
	void Update (){
		if (isPlaying) {
			//counter
			MySkeleton firstBody=bodyFrames[0];
			MySkeleton body=bodyFrames[counter];
			
			RefreshBodyObject(body,firstBody);
			double height=calculateHeight(body,Kinect.JointType.Head,Kinect.JointType.FootLeft);
			//Debug.Log("Height trainee:"+height);
			//Vector3 size = boxCollider.size;
			//size.y=(float)(height + 0.8f);
			//boxCollider.size = size;
			counter++;
			//terminator and reset counter
			if(counter == bodyFrames.Count){
				isPlaying = false;
				counter = 0;
			}
		}
	}
	private void RefreshBodyObject(MySkeleton skeleton, MySkeleton firstSkeleton)
	{
		//Debug.Log (skeleton.Position [(int)Kinect.JointType.SpineMid].getVector ());
		Hip_Center.localPosition = skeleton.Position[(int)Kinect.JointType.SpineBase].getVector();
		Spine.localPosition = skeleton.Position [(int)Kinect.JointType.SpineMid].getVector();
		Shoulder_Center.localPosition = skeleton.Position[(int)Kinect.JointType.SpineShoulder].getVector();
		Head.localPosition = skeleton.Position[(int)Kinect.JointType.Head].getVector();
		Shoulder_Left.localPosition = skeleton.Position[(int)Kinect.JointType.ShoulderLeft].getVector();
		Elbow_Left.localPosition = skeleton.Position[(int)Kinect.JointType.ElbowLeft].getVector();
		Wrist_Left.localPosition = skeleton.Position[(int)Kinect.JointType.WristLeft].getVector();
		Hand_Left.localPosition = skeleton.Position[(int)Kinect.JointType.HandLeft].getVector();
		Shoulder_Right.localPosition = skeleton.Position[(int)Kinect.JointType.ShoulderRight].getVector();
		Elbow_Right.localPosition = skeleton.Position[(int)Kinect.JointType.ElbowRight].getVector();
		Wrist_Right.localPosition = skeleton.Position[(int)Kinect.JointType.WristRight].getVector();
		Hand_Right.localPosition = skeleton.Position[(int)Kinect.JointType.HandRight].getVector();
		Hip_Left.localPosition = skeleton.Position[(int)Kinect.JointType.HipLeft].getVector();
		Knee_Left.localPosition = skeleton.Position[(int)Kinect.JointType.KneeLeft].getVector();
		Ankle_Left.localPosition = skeleton.Position[(int)Kinect.JointType.AnkleLeft].getVector();
		Foot_Left.localPosition = skeleton.Position[(int)Kinect.JointType.FootLeft].getVector();
		Hip_Right.localPosition = skeleton.Position[(int)Kinect.JointType.HipRight].getVector();
		Knee_Right.localPosition = skeleton.Position[(int)Kinect.JointType.KneeRight].getVector();
		Ankle_Right.localPosition = skeleton.Position[(int)Kinect.JointType.AnkleRight].getVector();
		Foot_Right.localPosition = skeleton.Position[(int)Kinect.JointType.FootRight].getVector();
		
	}
	private void serialize(List<MySkeleton> datas) {
		FileStream f = null;
		try{
			BinaryFormatter bFormat = new BinaryFormatter();
			f=File.Create(Application.persistentDataPath+fileLocation);
			bFormat.Serialize(f,datas);

		}catch(System.Exception e){
			Debug.Log(e);
		}finally{
			if (f != null) {
				f.Close();
			}
		}
		
	}
	private double calculateBoneLength(MySkeleton skeleton,Kinect.JointType jointType1, Kinect.JointType jointType2) {
		double length = Math.Round(Math.Sqrt(
			(Math.Pow(skeleton.Position[(int)jointType1].x - skeleton.Position[(int)jointType2].x, 2)) +
			(Math.Pow(skeleton.Position[(int)jointType1].y - skeleton.Position[(int)jointType2].y, 2)) +
			(Math.Pow(skeleton.Position[(int)jointType1].z - skeleton.Position[(int)jointType2].z, 2))), 3);
		return length;
	}
	public void serializeLength() {
		double spine = 0, shoulderCenter = 0, head = 0;
		double shoulderLeft = 0, elbowLeft = 0, wristLeft = 0, handLeft = 0;
		double shoulderRight = 0, elbowRight = 0, wristRight = 0, handRight = 0;
		double hipLeft = 0, kneeLeft = 0, ankleLeft = 0, footLeft = 0;
		double hipRight = 0, kneeRight = 0, ankleRight = 0, footRight = 0;
		for (int i = 0; i < bodyFrames.Count; i++) {
			
			spine += calculateBoneLength(bodyFrames[i], Kinect.JointType.SpineBase, Kinect.JointType.SpineMid);
			shoulderCenter += calculateBoneLength(bodyFrames[i], Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder);
			head += calculateBoneLength(bodyFrames[i], Kinect.JointType.SpineShoulder, Kinect.JointType.Head);
			
			shoulderLeft += calculateBoneLength(bodyFrames[i], Kinect.JointType.SpineShoulder, Kinect.JointType.ShoulderLeft);
			elbowLeft += calculateBoneLength(bodyFrames[i], Kinect.JointType.ShoulderLeft, Kinect.JointType.ElbowLeft);
			wristLeft += calculateBoneLength(bodyFrames[i], Kinect.JointType.ElbowLeft, Kinect.JointType.WristLeft);
			handLeft += calculateBoneLength(bodyFrames[i], Kinect.JointType.WristLeft, Kinect.JointType.HandLeft);
			
			shoulderRight += calculateBoneLength(bodyFrames[i], Kinect.JointType.SpineShoulder, Kinect.JointType.ShoulderRight);
			elbowRight += calculateBoneLength(bodyFrames[i], Kinect.JointType.ShoulderRight, Kinect.JointType.ElbowRight);
			wristRight += calculateBoneLength(bodyFrames[i], Kinect.JointType.ElbowRight, Kinect.JointType.WristRight);
			handRight += calculateBoneLength(bodyFrames[i], Kinect.JointType.WristRight, Kinect.JointType.HandRight);
			
			hipLeft += calculateBoneLength(bodyFrames[i], Kinect.JointType.SpineBase, Kinect.JointType.HipLeft);
			kneeLeft += calculateBoneLength(bodyFrames[i], Kinect.JointType.HipLeft, Kinect.JointType.KneeLeft);
			ankleLeft += calculateBoneLength(bodyFrames[i], Kinect.JointType.KneeLeft, Kinect.JointType.AnkleLeft);
			footLeft += calculateBoneLength(bodyFrames[i], Kinect.JointType.AnkleLeft, Kinect.JointType.FootLeft);
			
			hipRight += calculateBoneLength(bodyFrames[i], Kinect.JointType.SpineBase, Kinect.JointType.HipRight);
			kneeRight += calculateBoneLength(bodyFrames[i], Kinect.JointType.HipRight, Kinect.JointType.KneeRight);
			ankleRight += calculateBoneLength(bodyFrames[i], Kinect.JointType.KneeRight, Kinect.JointType.AnkleRight);
			footRight += calculateBoneLength(bodyFrames[i], Kinect.JointType.AnkleRight, Kinect.JointType.FootRight);
			
		}
		spine /= bodyFrames.Count;
		shoulderCenter /= bodyFrames.Count;
		head /= bodyFrames.Count;
		shoulderLeft /= bodyFrames.Count;
		elbowLeft /= bodyFrames.Count;
		wristLeft /= bodyFrames.Count;
		handLeft /= bodyFrames.Count;
		shoulderRight /= bodyFrames.Count;
		elbowRight /= bodyFrames.Count;
		wristRight /= bodyFrames.Count;
		handRight /= bodyFrames.Count;
		hipLeft /= bodyFrames.Count;
		kneeLeft /= bodyFrames.Count;
		ankleLeft /= bodyFrames.Count;
		footLeft /= bodyFrames.Count;
		hipRight /= bodyFrames.Count;
		kneeRight /= bodyFrames.Count;
		ankleRight /= bodyFrames.Count;
		footRight /= bodyFrames.Count;
		
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
		f=File.Create(Application.persistentDataPath+lengthFileLocation);;
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
	private double calculateHeight(MySkeleton skeleton,
	                               Kinect.JointType jointType1, Kinect.JointType jointType2) {
		double length = Math.Round(Math.Sqrt(
			(Math.Pow(skeleton.Position[(int)jointType1].x - skeleton.Position[(int)jointType2].x, 2)) +
			(Math.Pow(skeleton.Position[(int)jointType1].y - skeleton.Position[(int)jointType2].y, 2)) +
			(Math.Pow(skeleton.Position[(int)jointType1].z - skeleton.Position[(int)jointType2].z, 2))), 3);
		return length;
	}
}
