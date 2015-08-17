using UnityEngine;
[System.Serializable]
public struct QuaternionSerializer
{
	public float w;
	public float x;
	public float y;
	public float z;
	
	public QuaternionSerializer(Quaternion q){
		this.w = q.w;
		this.x = q.x;
		this.y = q.y;
		this.z = q.z;
	}
	public Quaternion getQuaternion(){
		return new Quaternion (this.x, this.y, this.z, this.w);
	}
	
}