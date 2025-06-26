using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class OdometryPublisher : MonoBehaviour
{
    // Variabel
    public string odomFrameId = "odom";
    public string baseFrameId = "base_link"; // Nama frame robot
    public float publishRateHz = 20f;

    private ROSConnection ros;
    private Rigidbody rb;
    private float lastPublishTime = 0f;
    private float publishPeriod;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        rb = GetComponent<Rigidbody>();
        
        ros.RegisterPublisher<OdometryMsg>("/odom");
        
        publishPeriod = 1.0f / publishRateHz;
    }

    void FixedUpdate()
    {
        // Hanya jalankan jika aplikasi sedang Play
        if (!Application.isPlaying) return;

        if (Time.time - lastPublishTime > publishPeriod)
        {
            PublishOdometry();
            lastPublishTime = Time.time;
        }
    }

    private void PublishOdometry()
    {
        // **PERBAIKAN TIMESTAMP:** Membuat timestamp secara manual
        var timeNow = Time.timeAsDouble;
        uint secs = (uint)timeNow;
        uint nsecs = (uint)((timeNow - secs) * 1e9);
        var timestamp = new TimeMsg
        {
            sec = secs,
            nanosec = nsecs
        };
        
        // Isi Header
        HeaderMsg header = new HeaderMsg
        {
            stamp = timestamp,
            frame_id = odomFrameId
        };

        // Buat pesan Odometry
        OdometryMsg odomMsg = new OdometryMsg
        {
            header = header,
            child_frame_id = baseFrameId
        };
        
        // **PERBAIKAN KONVERSI:** Manual conversion tanpa extension methods
        // Isi Pose (Posisi & Rotasi)
        odomMsg.pose.pose.position = new PointMsg
        {
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z
        };
        
        odomMsg.pose.pose.orientation = new QuaternionMsg
        {
            x = transform.rotation.x,
            y = transform.rotation.y,
            z = transform.rotation.z,
            w = transform.rotation.w
        };
        
        // Isi Twist (Kecepatan Linear & Angular)
        odomMsg.twist.twist.linear = new Vector3Msg
        {
            x = rb.velocity.x,
            y = rb.velocity.y,
            z = rb.velocity.z
        };
        
        odomMsg.twist.twist.angular = new Vector3Msg
        {
            x = rb.angularVelocity.x,
            y = rb.angularVelocity.y,
            z = rb.angularVelocity.z
        };
        
        ros.Publish("/odom", odomMsg);
    }
}