using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;
// PENTING: Directive ini WAJIB ada
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class OdometryPublisher : MonoBehaviour
{
    // Variabel
    public string odomFrameId = "odom";
    public string baseFrameId = "base_link";
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
        if (!Application.isPlaying) return;

        if (Time.time >= lastPublishTime + publishPeriod)
        {
            PublishOdometry();
            lastPublishTime = Time.time;
        }
    }

    private void PublishOdometry()
    {
        var timeNow = Time.timeAsDouble;
        uint secs = (uint)timeNow;
        uint nsecs = (uint)((timeNow - secs) * 1e9);
        var timestamp = new TimeMsg { sec = (int)secs, nanosec = nsecs };
        var header = new HeaderMsg(timestamp, odomFrameId);

        // === PERBAIKAN UTAMA: Menghapus prefix 'ROSGeometry.' ===
        // Karena sudah ada 'using' di atas, kita panggil tipe datanya langsung.

        // --- Konversi POSE ---
        // Konversi posisi Unity ke tipe penerjemah 'Point'
        var rosPosition = transform.position.To<FLU>();
        
        // Konversi rotasi Unity ke tipe penerjemah 'Quaternion'
        var rosOrientation = transform.rotation.To<FLU>();

        var poseMsg = new PoseMsg
        {
            position = new PointMsg(rosPosition.x, rosPosition.y, rosPosition.z),
            orientation = new QuaternionMsg(rosOrientation.x, rosOrientation.y, rosOrientation.z, rosOrientation.w)
        };

        // --- Konversi TWIST ---
        // Konversi kecepatan linear Unity ke tipe penerjemah 'Vector3'
        var rosLinearVelocity = rb.velocity.To<FLU>();
        var rosAngularVelocity = rb.angularVelocity.To<FLU>();

        var twistMsg = new TwistMsg
        {
            linear = new Vector3Msg(rosLinearVelocity.x, rosLinearVelocity.y, rosLinearVelocity.z),
            angular = new Vector3Msg(rosAngularVelocity.x, rosAngularVelocity.y, rosAngularVelocity.z)
        };

        // --- Rakit Pesan Odometry Final ---
        var odomMsg = new OdometryMsg
        {
            header = header,
            child_frame_id = baseFrameId,
            pose = new PoseWithCovarianceMsg { pose = poseMsg },
            twist = new TwistWithCovarianceMsg { twist = twistMsg }
        };
        
        ros.Publish("/odom", odomMsg);
    }
}