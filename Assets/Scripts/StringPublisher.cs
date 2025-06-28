using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class StringPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/chatter";  // Pakai slash agar sesuai dengan ROS

    private float timeElapsed;
    public float publishInterval = 2.0f;  // Kirim setiap 2 detik

    void Start()
    {
        // Ambil instance ROS dan daftarkan publisher-nya
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>(topicName);
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= publishInterval)
        {
            timeElapsed = 0f;

            // Buat pesan
            StringMsg msg = new StringMsg("Hello from Unity at " + Time.time.ToString("F2"));
            
            // Kirim ke ROS
            ros.Publish(topicName, msg);
            Debug.Log("Sent message to ROS: " + msg.data);
        }
    }
}
