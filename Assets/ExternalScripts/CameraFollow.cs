using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;                                //Testing smoothness of camera follow rather than cinemachine
    private Vector3 offset;

    private void Start()
    {
        offset = transform.position - target.position;
    }
    void Update()
    {
        transform.position = target.position + offset;
    }
}
