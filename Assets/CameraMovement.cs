using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject followObject;
    public int speed;

    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector3.MoveTowards(gameObject.transform.position, followObject.transform.position, speed * Time.deltaTime);
        //transform.eulerAngles = followObject.transform.eulerAngles;
        transform.LookAt(followObject.transform);
    }
}
