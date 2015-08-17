using UnityEngine;
[System.Serializable]
public struct Vector3Serializer
{
	//public float w;
	public float x;
	public float y;
	public float z;
	
	public Vector3Serializer(Vector3 v){
		this.x = v.x;
		this.y = v.y;
		this.z = v.z;
	}
	public Vector3 getVector(){
		return new Vector3 (this.x, this.y, this.z);
	}
	
}