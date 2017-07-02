using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SnapToGrid : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 curPos = transform.position;
        curPos.x = Mathf.Round(curPos.x);
        curPos.y = Mathf.Round(curPos.y);
        curPos.z = 0; // 2D, not using z-axis. 
        transform.position = curPos;
	}
}
