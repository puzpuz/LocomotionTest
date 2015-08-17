using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
[System.Serializable]
public class MySkeleton
{
	public MySkeleton(){}
	public int[] JointId { get; set; }
	[SerializeField]
	public Vector3Serializer[] Position { get; set; }
	[SerializeField]
	public QuaternionSerializer[] Rotation { get; set; }
}

