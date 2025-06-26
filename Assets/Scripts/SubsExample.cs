using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Std;

public class SubsExample : MonoBehaviour
{
    private RosSocket rosSocket;
    private string topic = "/example_topic"; // Ganti dengan nama topik Anda

    void Start()
    {
        rosSocket = GetComponent<RosConnector>().RosSocket;

        // Berlangganan ke topik
        rosSocket.Subscribe<String>(topic, ReceiveMessage);
    }

    // Fungsi yang dipanggil setiap kali pesan diterima
    private void ReceiveMessage(String message)
    {
        Debug.Log("Pesan diterima: " + message.data);
    }
}
