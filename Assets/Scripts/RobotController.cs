using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class RobotController : MonoBehaviour
{
    public float linearSpeed = 0.2f;
    public float angularSpeed = 50.0f;

    private Rigidbody rb;
    private float targetLinear = 0f;
    private float targetAngular = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ROSConnection.GetOrCreateInstance().Subscribe<TwistMsg>("/cmd_vel", MoveRobot);
    }

    void MoveRobot(TwistMsg twistMsg)
    {
        targetLinear = (float)twistMsg.linear.x;
        targetAngular = (float)twistMsg.angular.z;
    }

    void FixedUpdate()
    {
        // Gerakan translasi
        Vector3 move = transform.forward * targetLinear * linearSpeed;
        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);

        // Gerakan rotasi
        Quaternion turn = Quaternion.Euler(0, targetAngular * angularSpeed * Time.fixedDeltaTime, 0);
        rb.MoveRotation(rb.rotation * turn);
    }
}
