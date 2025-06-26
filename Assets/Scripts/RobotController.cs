using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

// Skrip untuk mengontrol pergerakan robot berdasarkan perintah dari topik /cmd_vel
public class RobotController : MonoBehaviour
{
    // Variabel untuk kecepatan gerak
    public float linearSpeed = 0.2f; // m/s
    public float angularSpeed = 50.0f; // degrees/s

    // Komponen Rigidbody untuk pergerakan fisika
    private Rigidbody rb;

    void Start()
    {
        // Dapatkan komponen Rigidbody
        rb = GetComponent<Rigidbody>();

        // Mulai berlangganan (subscribe) ke topik /cmd_vel
        ROSConnection.GetOrCreateInstance().Subscribe<TwistMsg>("/cmd_vel", MoveRobot);
    }

    // Fungsi yang akan dipanggil setiap kali ada pesan masuk di /cmd_vel
    private void MoveRobot(TwistMsg twistMsg)
    {
        // Atur kecepatan linear berdasarkan pesan
        // twistMsg.linear.x > 0 artinya maju, < 0 artinya mundur
        Vector3 linearVelocity = transform.forward * (float)twistMsg.linear.x * linearSpeed;
        rb.velocity = new Vector3(linearVelocity.x, rb.velocity.y, linearVelocity.z);

        // Atur kecepatan angular (rotasi) berdasarkan pesan
        // twistMsg.angular.z > 0 artinya berputar ke kiri, < 0 ke kanan
        Vector3 angularVelocity = -transform.up * (float)twistMsg.angular.z * angularSpeed * Mathf.Deg2Rad;
        rb.angularVelocity = new Vector3(0, angularVelocity.y, 0);
    }
}