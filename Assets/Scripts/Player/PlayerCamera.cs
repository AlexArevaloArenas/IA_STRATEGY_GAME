using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float speed = 0.01f;
    [SerializeField] float zoomSpeed = 2f;
    //float rotationSpeed = 1f;

    [SerializeField] float maxHeight = 25f;
    [SerializeField] float minHeight = 2f;



    // Update is called once per frame
    void Update()
    {
        float hsp = transform.position.y * speed * Input.GetAxis("Horizontal"); //horizontalSpeed
        float vsp = transform.position.y * speed * Input.GetAxis("Vertical"); //verticalSpeed
        float scrollSpeed = Mathf.Log(transform.position.y) * -zoomSpeed * Input.GetAxis("Mouse ScrollWheel");


        //Movement Locks
        if (transform.position.y + scrollSpeed >= maxHeight && scrollSpeed > 0)
        {
            scrollSpeed = 0;
        }
        else if (transform.position.y - scrollSpeed <= minHeight && scrollSpeed < 0)
        {
            scrollSpeed = 0;
        }

        if (transform.position.y + scrollSpeed > maxHeight)
        {
            //scrollSpeed = maxHeight - scrollSpeed;
            scrollSpeed = 0;
        }
        else if (transform.position.y + scrollSpeed < minHeight)
        {
            //scrollSpeed = minHeight - scrollSpeed;
            scrollSpeed = 0;
        }

        //Movement
        Vector3 verticalMove = new Vector3(0, scrollSpeed, 0);
        Vector3 lateralMove = hsp * transform.right;
        Vector3 forwardMove = transform.forward;
        forwardMove.y = 0;
        forwardMove.Normalize();
        forwardMove *= vsp;

        Vector3 move = verticalMove + lateralMove + forwardMove;
        transform.position += move;
        //transform.position.y = Mathf.Clamp(minHeight, maxHeight);



    }
}
