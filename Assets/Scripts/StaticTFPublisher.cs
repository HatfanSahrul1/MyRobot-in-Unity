using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Tf2;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class StaticTFPublisher : MonoBehaviour
{
    public string parentFrame = "base_link";
    public string childFrame = "laser_link";

    private ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        // ✅ Register dulu sebelum publish
        ros.RegisterPublisher<TFMessageMsg>("tf_static");

        // Ambil waktu ROS (pakai waktu lokal Unity)
        double timeNow = Time.timeAsDouble;
        uint secs = (uint)timeNow;
        uint nsecs = (uint)((timeNow - secs) * 1e9);

        var tfStamped = new TransformStampedMsg
        {
            header = new HeaderMsg(new TimeMsg((int)secs, nsecs), parentFrame),
            child_frame_id = childFrame,
            transform = new TransformMsg
            {
                translation = new Vector3Msg(0f, 0f, 0f),
                rotation = new QuaternionMsg(0f, 0f, 0f, 1f)
            }
        };

        var tfMsg = new TFMessageMsg(new TransformStampedMsg[] { tfStamped });

        ros.Publish("tf_static", tfMsg);
        Debug.Log("Published static TF: " + parentFrame + " → " + childFrame);
    }
}
