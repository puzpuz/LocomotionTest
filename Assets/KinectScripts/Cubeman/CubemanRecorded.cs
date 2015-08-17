
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class CubemanRecorded : MonoBehaviour 
{


	//added
	public GameObject BodySourceManager;
	private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
	private BodySourceManager _BodyManager;
	private BoxCollider boxCollider;
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

	private bool isPlaying = false;
	void OnGUI(){
		Event e = Event.current;
		if (e.isKey) {
			if(e.keyCode==KeyCode.P){
				Debug.Log("Replay start");
				bodyFrames.Clear();
				bodyFrames.AddRange(deserialize());
				double height=calculateHeight(bodyFrames[0],Kinect.JointType.Head,Kinect.JointType.FootLeft);
				Debug.Log("Height trainer:"+height);
				Vector3 size = boxCollider.size;
				size.y=(float)(height+ 0.8f);
				boxCollider.size = size;
				isPlaying = true;
			}
		}
	}
	void Start () 
	{
		counter = 0;
		//bodyFrames = new List<Kinect.Body[]>();
		bodyFrames = new List<MySkeleton> ();
		boxCollider=this.gameObject.AddComponent<BoxCollider>();
		//boxCollider.center = new Vector3 (Hip_Center.position.x ,Hip_Center.position.y,Hip_Center.position.z);
		boxCollider.size = new Vector3 (1f, 1.87f, 1f);
	}
	//private List<Kinect.Body[]> bodyFrames;
	private List<MySkeleton> bodyFrames;
	private int counter = 0;
	void Update () 
	{
		if (isPlaying) {
			//counter
			MySkeleton body=bodyFrames[counter];
			RefreshBodyObject(body);
			double height=calculateHeight(body,Kinect.JointType.Head,Kinect.JointType.FootLeft);
			//Debug.Log("Height trainer:"+height);
			Vector3 size = boxCollider.size;
			size.y=(float)(height + 0.8f);
			boxCollider.size = size;
			counter++;
			//terminator and reset counter
			if(counter == bodyFrames.Count){
				isPlaying = false;
				counter = 0;
			}
		}


	}
	private void RefreshBodyObject(MySkeleton skeleton)
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
			Debug.Log(Application.persistentDataPath+fileLocation);
			bFormat.Serialize(f,datas);

		}catch(System.Exception e){
			Debug.Log(e);
		}finally{
			if (f != null) {
				f.Close();
			}
		}
		
	}
	private List<MySkeleton> deserialize() {
		List<MySkeleton> data = null;
		FileStream f = null;
		
		BinaryFormatter formater=new BinaryFormatter();
		f = File.Open(Application.persistentDataPath+fileLocation, FileMode.Open);
		Debug.Log(Application.persistentDataPath+fileLocation);
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
