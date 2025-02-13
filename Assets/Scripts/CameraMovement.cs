using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraMovement : MonoBehaviour
{
    private Transform target;
    public float followSpeed = 5f;
    public float rotationSpeed = 5f;
    public Vector3 offset = new Vector3(0, 5, -10);
    public Vector3 rotationOffset = new Vector3(15, 0, 0);
    public SnakeManager snakeManager;

    private Quaternion targetRotation;

    // Update is called once per frame

    void LateUpdate()
    {
        //transform.position = Vector3.MoveTowards(gameObject.transform.position, followObject.transform.position, speed * Time.deltaTime);
        ////transform.eulerAngles = followObject.transform.eulerAngles;
        //transform.LookAt(followObject.transform);
        var snakeMoveSpeed = snakeManager.moveSpeed;
        if(snakeMoveSpeed > 2 && snakeMoveSpeed < 4)
        {
            rotationSpeed = followSpeed = snakeManager.moveSpeed;
        }

        if (target == null) target = snakeManager.headNeck.transform;

        // Move towards the target position (without changing Y)
        Vector3 targetPosition = target.position + target.forward * offset.z + Vector3.up * offset.y;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Rotate to match player's forward direction
        targetRotation = Quaternion.LookRotation(target.forward) * Quaternion.Euler(rotationOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
