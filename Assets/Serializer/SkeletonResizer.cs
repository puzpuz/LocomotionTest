using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using Kinect = Windows.Kinect;

public class SkeletonResizer
{
	List<double> referenceBoneLengths;
	public SkeletonResizer() {
		referenceBoneLengths = new List<double>();
		
	}
	public void deserializer(){
		referenceBoneLengths = deserializeLength();
	}
	public MySkeleton resizeSkeleton(MySkeleton skel) {
		MySkeleton newSkeleton = new MySkeleton ();
		newSkeleton = skel;
		
		//spine
		newSkeleton.Position[(int)Kinect.JointType.SpineMid] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.SpineMid, Kinect.JointType.SpineMid, 0));
		newSkeleton.Position[(int)Kinect.JointType.Neck] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.SpineMid, Kinect.JointType.Neck, 0));
		newSkeleton.Position[(int)Kinect.JointType.Head] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.SpineMid, Kinect.JointType.Head, 0));
		newSkeleton.Position[(int)Kinect.JointType.ShoulderLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.SpineMid, Kinect.JointType.ShoulderLeft, 0));
		newSkeleton.Position[(int)Kinect.JointType.ElbowLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.SpineMid, Kinect.JointType.ElbowLeft, 0));
		newSkeleton.Position[(int)Kinect.JointType.WristLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.SpineMid, Kinect.JointType.WristLeft, 0));
		newSkeleton.Position[(int)Kinect.JointType.HandLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.SpineMid, Kinect.JointType.HandLeft, 0));
		newSkeleton.Position[(int)Kinect.JointType.ShoulderRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.SpineMid, Kinect.JointType.ShoulderRight, 0));
		newSkeleton.Position[(int)Kinect.JointType.ElbowRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.SpineMid, Kinect.JointType.ElbowRight, 0));
		newSkeleton.Position[(int)Kinect.JointType.WristRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.SpineMid, Kinect.JointType.WristRight, 0));
		newSkeleton.Position[(int)Kinect.JointType.HandRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.SpineMid, Kinect.JointType.HandRight, 0));
		
		
		//shoulder center
		newSkeleton.Position[(int)Kinect.JointType.Neck] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineMid, (int)Kinect.JointType.Neck, Kinect.JointType.Neck, 1));
		newSkeleton.Position[(int)Kinect.JointType.Head] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineMid, (int)Kinect.JointType.Neck, Kinect.JointType.Head, 1));
		newSkeleton.Position[(int)Kinect.JointType.ShoulderLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineMid, (int)Kinect.JointType.Neck, Kinect.JointType.ShoulderLeft, 1));
		newSkeleton.Position[(int)Kinect.JointType.ElbowLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineMid, (int)Kinect.JointType.Neck, Kinect.JointType.ElbowLeft, 1));
		newSkeleton.Position[(int)Kinect.JointType.WristLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineMid, (int)Kinect.JointType.Neck, Kinect.JointType.WristLeft, 1));
		newSkeleton.Position[(int)Kinect.JointType.HandLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineMid, (int)Kinect.JointType.Neck, Kinect.JointType.HandLeft, 1));
		newSkeleton.Position[(int)Kinect.JointType.ShoulderRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineMid, (int)Kinect.JointType.Neck, Kinect.JointType.ShoulderRight, 1));
		newSkeleton.Position[(int)Kinect.JointType.ElbowRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineMid, (int)Kinect.JointType.Neck, Kinect.JointType.ElbowRight, 1));
		newSkeleton.Position[(int)Kinect.JointType.WristRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineMid, (int)Kinect.JointType.Neck, Kinect.JointType.WristRight, 1));
		newSkeleton.Position[(int)Kinect.JointType.HandRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineMid, (int)Kinect.JointType.Neck, Kinect.JointType.HandRight, 1));
		
		//head
		newSkeleton.Position[(int)Kinect.JointType.Head] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.Neck, (int)Kinect.JointType.Head, Kinect.JointType.Head, 2));
		
		
		//shoulder left
		newSkeleton.Position[(int)Kinect.JointType.ShoulderLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.Neck, (int)Kinect.JointType.ShoulderLeft, Kinect.JointType.ShoulderLeft, 3));
		newSkeleton.Position[(int)Kinect.JointType.ElbowLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.Neck, (int)Kinect.JointType.ShoulderLeft, Kinect.JointType.ElbowLeft, 3));
		newSkeleton.Position[(int)Kinect.JointType.WristLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.Neck, (int)Kinect.JointType.ShoulderLeft, Kinect.JointType.WristLeft, 3));
		newSkeleton.Position[(int)Kinect.JointType.HandLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.Neck, (int)Kinect.JointType.ShoulderLeft, Kinect.JointType.HandLeft, 3));
		
		
		//elbow left
		newSkeleton.Position[(int)Kinect.JointType.ElbowLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.ShoulderLeft, (int)Kinect.JointType.ElbowLeft, Kinect.JointType.ElbowLeft, 4));
		newSkeleton.Position[(int)Kinect.JointType.WristLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.ShoulderLeft, (int)Kinect.JointType.ElbowLeft, Kinect.JointType.WristLeft, 4));
		newSkeleton.Position[(int)Kinect.JointType.HandLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.ShoulderLeft, (int)Kinect.JointType.ElbowLeft, Kinect.JointType.HandLeft, 4));
		
		//wrist left
		newSkeleton.Position[(int)Kinect.JointType.WristLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.ElbowLeft, (int)Kinect.JointType.WristLeft, Kinect.JointType.WristLeft, 5));
		newSkeleton.Position[(int)Kinect.JointType.HandLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.ElbowLeft, (int)Kinect.JointType.WristLeft, Kinect.JointType.HandLeft, 5));
		
		//hand left
		newSkeleton.Position[(int)Kinect.JointType.HandLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.WristLeft, (int)Kinect.JointType.HandLeft, Kinect.JointType.HandLeft, 6));
		
		
		//shoulder right
		newSkeleton.Position[(int)Kinect.JointType.ShoulderRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.Neck, (int)Kinect.JointType.ShoulderRight, Kinect.JointType.ShoulderRight, 7));
		newSkeleton.Position[(int)Kinect.JointType.ElbowRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.Neck, (int)Kinect.JointType.ShoulderRight, Kinect.JointType.ElbowRight, 7));
		newSkeleton.Position[(int)Kinect.JointType.WristRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.Neck, (int)Kinect.JointType.ShoulderRight, Kinect.JointType.WristRight, 7));
		newSkeleton.Position[(int)Kinect.JointType.HandRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.Neck, (int)Kinect.JointType.ShoulderRight, Kinect.JointType.HandRight, 7));
		
		
		//elbow right
		newSkeleton.Position[(int)Kinect.JointType.ElbowRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.ShoulderRight, (int)Kinect.JointType.ElbowRight, Kinect.JointType.ElbowRight, 8));
		newSkeleton.Position[(int)Kinect.JointType.WristRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.ShoulderRight, (int)Kinect.JointType.ElbowRight, Kinect.JointType.WristRight, 8));
		newSkeleton.Position[(int)Kinect.JointType.HandRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.ShoulderRight, (int)Kinect.JointType.ElbowRight, Kinect.JointType.HandRight, 8));
		
		//wrist right
		newSkeleton.Position[(int)Kinect.JointType.WristRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.ElbowRight, (int)Kinect.JointType.WristRight, Kinect.JointType.WristRight, 9));
		newSkeleton.Position[(int)Kinect.JointType.HandRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.ElbowRight, (int)Kinect.JointType.WristRight, Kinect.JointType.HandRight, 9));
		
		//hand right
		newSkeleton.Position[(int)Kinect.JointType.HandRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.WristRight, (int)Kinect.JointType.HandRight, Kinect.JointType.HandRight, 10));
		
		//hip left;
		newSkeleton.Position[(int)Kinect.JointType.HipLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.HipLeft, Kinect.JointType.HipLeft, 11));
		newSkeleton.Position[(int)Kinect.JointType.KneeLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.HipLeft, Kinect.JointType.KneeLeft, 11));
		newSkeleton.Position[(int)Kinect.JointType.AnkleLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.HipLeft, Kinect.JointType.AnkleLeft, 11));
		newSkeleton.Position[(int)Kinect.JointType.FootLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.HipLeft, Kinect.JointType.FootLeft, 11));
		//knee left
		newSkeleton.Position[(int)Kinect.JointType.KneeLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.HipLeft, (int)Kinect.JointType.KneeLeft, Kinect.JointType.KneeLeft, 12));
		newSkeleton.Position[(int)Kinect.JointType.AnkleLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.HipLeft, (int)Kinect.JointType.KneeLeft, Kinect.JointType.AnkleLeft, 12));
		newSkeleton.Position[(int)Kinect.JointType.FootLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.HipLeft, (int)Kinect.JointType.KneeLeft, Kinect.JointType.FootLeft, 12));
		//ankle left
		newSkeleton.Position[(int)Kinect.JointType.AnkleLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.KneeLeft, (int)Kinect.JointType.AnkleLeft, Kinect.JointType.AnkleLeft, 13));
		newSkeleton.Position[(int)Kinect.JointType.FootLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.KneeLeft, (int)Kinect.JointType.AnkleLeft, Kinect.JointType.FootLeft, 13));
		//foot left           
		newSkeleton.Position[(int)Kinect.JointType.FootLeft] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.AnkleLeft, (int)Kinect.JointType.FootLeft, Kinect.JointType.FootLeft, 14));
		
		//hip right;
		newSkeleton.Position[(int)Kinect.JointType.HipRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.HipRight, Kinect.JointType.HipRight, 15));
		newSkeleton.Position[(int)Kinect.JointType.KneeRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.HipRight, Kinect.JointType.KneeRight, 15));
		newSkeleton.Position[(int)Kinect.JointType.AnkleRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.HipRight, Kinect.JointType.AnkleRight, 15));
		newSkeleton.Position[(int)Kinect.JointType.FootRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.SpineBase, (int)Kinect.JointType.HipRight, Kinect.JointType.FootRight, 15));
		//knee right
		newSkeleton.Position[(int)Kinect.JointType.KneeRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.HipRight, (int)Kinect.JointType.KneeRight, Kinect.JointType.KneeRight, 16));
		newSkeleton.Position[(int)Kinect.JointType.AnkleRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.HipRight, (int)Kinect.JointType.KneeRight, Kinect.JointType.AnkleRight, 16));
		newSkeleton.Position[(int)Kinect.JointType.FootRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.HipRight, (int)Kinect.JointType.KneeRight, Kinect.JointType.FootRight, 16));
		//ankle right
		newSkeleton.Position[(int)Kinect.JointType.AnkleRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.KneeRight, (int)Kinect.JointType.AnkleRight, Kinect.JointType.AnkleRight, 17));
		newSkeleton.Position[(int)Kinect.JointType.FootRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.KneeRight, (int)Kinect.JointType.AnkleRight, Kinect.JointType.FootRight, 17));
		//foot right            
		newSkeleton.Position[(int)Kinect.JointType.FootRight] = new Vector3Serializer(createJoint(newSkeleton, (int)Kinect.JointType.AnkleRight, (int)Kinect.JointType.FootRight, Kinect.JointType.FootRight, 18));
		
		return newSkeleton;
	}
	public Vector3 createJoint(MySkeleton skel, int firstJoint, int secondJoint, Kinect.JointType jointToMove, int index) {
		
		float Xtrans = translateX(skel, firstJoint, secondJoint, index);
		float Ytrans = translateY(skel, firstJoint, secondJoint, index);
		float Ztrans = translateZ(skel, firstJoint, secondJoint, index);
//		Debug.Log ("x transition: " + Xtrans+" ytansitiion: "+ Ytrans+" z transition: "+Ztrans);

		Vector3 Position = new Vector3 ((float)(skel.Position[(int)jointToMove].x - Xtrans), (float)(skel.Position[(int)jointToMove].y - Ytrans), 
		                                (float)(skel.Position[(int)jointToMove].z - Ztrans));
		return Position;
	}
	public float translateX(MySkeleton skel, int anchor, int moved,int index) {
		double A = calculateBoneLength(skel, anchor, moved);
		double B = referenceBoneLengths[index];
		//Debug.Log ("A:" + A + "B:" + B);
		float Xresized = skel.Position[(int)moved].x;
		float Xanchor = skel.Position[(int)anchor].x;
		float translateX = Xresized - (((Xresized - Xanchor) / (float)A) * (float)B + Xanchor);
		return translateX;
	}
	public float translateY(MySkeleton skel, int anchor, int moved, int index) {
		double A = calculateBoneLength(skel, anchor, moved);
		double B = referenceBoneLengths[index];
		float Yresized = skel.Position[moved].y;
		float Yanchor = skel.Position[anchor].y;
		float translateY = Yresized - (((Yresized - Yanchor) / (float)A) * (float)B + Yanchor);
		return translateY;
	}
	public float translateZ(MySkeleton skel, int anchor, int moved, int index) {
		double A = calculateBoneLength(skel, anchor, moved);
		double B = referenceBoneLengths[index];
		float Zresized = skel.Position[(int)moved].z;
		float Zanchor = skel.Position[(int)anchor].z;
		float translateZ = Zresized - (((Zresized - Zanchor) / (float)A) * (float)B + Zanchor);
		return translateZ;
	}
	private double calculateBoneLength(MySkeleton skeleton,
	                                   int jointType1, int jointType2) {

		double length = Math.Round(Math.Sqrt(
			(Math.Pow(skeleton.Position[(int)jointType1].x - skeleton.Position[(int)jointType2].x, 2)) +
			(Math.Pow(skeleton.Position[(int)jointType1].y - skeleton.Position[(int)jointType2].y, 2)) +
			(Math.Pow(skeleton.Position[(int)jointType1].z - skeleton.Position[(int)jointType2].z, 2))), 3);
		return length;
	}
	private String lengthFileLocation = "/model.metadata";
	public List<double> deserializeLength() {
		List<double> lengths = null;
		FileStream stream = null;
		lengths = new List<double>();
		BinaryFormatter formater = new BinaryFormatter();
		stream = File.Open(Application.persistentDataPath+lengthFileLocation, FileMode.Open);
		lengths = (List<double>)formater.Deserialize(stream);
		stream.Close();
		return lengths;
	}
}