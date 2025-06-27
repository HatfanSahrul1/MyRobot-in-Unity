using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class LidarSensor : MonoBehaviour
{
    // Pengaturan Sensor
    public string frameId = "laser_link";
    public float range = 10f; // Jarak maksimum sensor
    [Range(1, 720)]
    public int numRays = 360; // Jumlah tembakan raycast dalam 360 derajat
    public float scanRateHz = 10f; // Seberapa sering sensor memindai (Hz)

    private ROSConnection ros;
    private float timeSinceLastScan = 0f;
    private float scanPeriod;
    private float[] ranges;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<LaserScanMsg>("/scan");

        scanPeriod = 1.0f / scanRateHz;
        if (ranges == null || ranges.Length != numRays)
        {
            ranges = new float[numRays];
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;

        timeSinceLastScan += Time.deltaTime;

        if (timeSinceLastScan >= scanPeriod)
        {
            Scan();
            timeSinceLastScan = 0;
        }
    }

    private void Scan()
    {
        float angleStep = 360.0f / numRays;
        
        for (int i = 0; i < numRays; i++)
        {
            float currentAngle = angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, range))
            {
                ranges[i] = hit.distance;
            }
            else
            {
                ranges[i] = range;
            }
        }
        
        var timeNow = Time.timeAsDouble;
        uint secs = (uint)timeNow;
        uint nsecs = (uint)((timeNow - secs) * 1e9);
        
        // **PERBAIKAN UTAMA:** 'nanosec' diisi langsung dengan 'nsecs' (tipe uint)
        var timestamp = new TimeMsg
        {
            sec = (int)secs,
            nanosec = nsecs // <-- Dihapus cast (int) yang salah
        };
        
        HeaderMsg header = new HeaderMsg(timestamp, frameId);
        
        LaserScanMsg scanMessage = new LaserScanMsg
        {
            header = header,
            angle_min = 0,
            angle_max = (360.0f - angleStep) * Mathf.Deg2Rad,
            angle_increment = angleStep * Mathf.Deg2Rad,
            time_increment = 0,
            scan_time = scanPeriod,
            range_min = 0,
            range_max = range,
            ranges = ranges,
            intensities = new float[numRays]
        };
        
        ros.Publish("/scan", scanMessage);
    }

    void OnDrawGizmosSelected()
    {
        if (ranges == null || ranges.Length != numRays)
        {
            ranges = new float[numRays];
        }

        float angleStep = 360.0f / numRays;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);

        for (int i = 0; i < numRays; i++)
        {
            float currentAngle = angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, range))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, hit.point);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, direction * range);
            }
        }
    }
}