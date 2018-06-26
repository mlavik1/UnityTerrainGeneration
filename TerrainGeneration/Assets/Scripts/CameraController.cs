using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	void FixedUpdate()
    {
        Vector3 movementDir = Vector3.zero;
		if(Input.GetKey(KeyCode.W))
        {
            movementDir += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movementDir += transform.forward * -1.0f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movementDir += transform.right * -1.0f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movementDir += transform.right;
        }
        movementDir = movementDir.normalized;
        float movementSpeed = Input.GetKey(KeyCode.LeftShift) ? 550.0f : 70.0f;
        GetComponent<Rigidbody>().velocity = Vector3.up * GetComponent<Rigidbody>().velocity.y + movementDir * movementSpeed;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 10.0f, Space.World);

    }
}
