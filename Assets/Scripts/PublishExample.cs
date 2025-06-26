using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;

public class PublishExample : MonoBehaviour
{
    private RosSocket rosSocket;
    private string topicName = "/example_topic";

    void Start()
    {
        rosSocket = GetComponent<RosConnector>().RosSocket;
        PublishExampleMessage();
    }

    private void PublishExampleMessage()
    {
        var message = new RosSharp.RosBridgeClient.MessageTypes.Std.String
        {
            data = "Hello from Unity!"
        };
        rosSocket.Publish(topicName, message);
    }
}
