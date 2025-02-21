using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointGroup : MonoBehaviour {

    public List<Transform> waypoints = new List<Transform>();

    private int childWays;
	
	void Update () {
        if (transform.childCount < waypoints.Count)
        {
            waypoints.Clear();
            childWays = 0;
        }

        if (transform.childCount > childWays)
        {
            for(int i = childWays; i < transform.childCount; i++)
            {
                waypoints.Add(transform.GetChild(i));
                childWays++;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform t in waypoints)
            {
                Gizmos.DrawCube(t.position, new Vector3(0.5f, 0.5f, 0.5f));
            }
        }
    }
}
