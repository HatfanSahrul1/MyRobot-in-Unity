using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Tf2;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Std;

public class OdomTfPublisher : MonoBehaviour
{
    public string parentFrame = "odom";
    public string childFrame = "base_link";

    private ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TFMessageMsg>("/tf");
    }

    void FixedUpdate()
    {
        double timeNow = Time.timeAsDouble;
        uint secs = (uint)timeNow;
        uint nsecs = (uint)((timeNow - secs) * 1e9);

        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        var tfMsg = new TFMessageMsg(new TransformStampedMsg[]
        {
            new TransformStampedMsg
            {
                header = new HeaderMsg(new TimeMsg((int)secs, nsecs), parentFrame),
                child_frame_id = childFrame,
                transform = new TransformMsg
                {
                    translation = new Vector3Msg(position.x, position.y, position.z),
                    rotation = new QuaternionMsg(rotation.x, rotation.y, rotation.z, rotation.w)
                }
            }
        });

        ros.Publish("/tf", tfMsg);
    }
}
