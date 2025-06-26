using System.Collections;
using System.Collections.Generic;
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
    [Range(1, 720)] // Batasi jumlah ray agar tidak terlalu berat
    public int numRays = 360; // Jumlah tembakan raycast dalam 360 derajat
    public float scanRateHz = 10f; // Seberapa sering sensor memindai (Hz)

    private ROSConnection ros;
    private float timeSinceLastScan = 0f;
    private float scanPeriod;
    private float[] ranges;

    void Start()
    {
        // Dapatkan koneksi ROS
        ros = ROSConnection.GetOrCreateInstance();
        // Daftarkan topik yang akan kita publikasikan
        ros.RegisterPublisher<LaserScanMsg>("/scan");

        scanPeriod = 1.0f / scanRateHz;
        // Inisialisasi ulang array jika numRays berubah di inspector
        if (ranges == null || ranges.Length != numRays)
        {
            ranges = new float[numRays];
        }
    }

    void Update()
    {
        // Hanya jalankan jika aplikasi sedang Play
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
        
        // Lakukan raycast untuk setiap sudut
        for (int i = 0; i < numRays; i++)
        {
            float currentAngle = angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, range))
            {
                // Jika raycast mengenai sesuatu, catat jaraknya
                ranges[i] = hit.distance;
            }
            else
            {
                // Jika tidak, catat sebagai jarak maksimum (atau tak terhingga)
                ranges[i] = range; // Lebih aman untuk SLAM daripada infinity
            }
        }
        
        // **PERBAIKAN TIMESTAMP:** Membuat timestamp secara manual tanpa Unity.Robots.Core
        var timeNow = Time.timeAsDouble;
        uint secs = (uint)timeNow;
        uint nsecs = (uint)((timeNow - secs) * 1e9);
        var timestamp = new TimeMsg
        {
            sec = secs,
            nanosec = nsecs
        };
        
        HeaderMsg header = new HeaderMsg
        {
            stamp = timestamp,
            frame_id = frameId
        };
        
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

    // **FITUR BARU: VISUALISASI GIZMOS**
    // Fungsi ini akan menggambar visualisasi di Scene Editor
    void OnDrawGizmosSelected()
    {
        // Inisialisasi ulang array jika numRays berubah di inspector
        if (ranges == null || ranges.Length != numRays)
        {
            ranges = new float[numRays];
        }

        float angleStep = 360.0f / numRays;

        // Gambar lingkaran jangkauan maksimum
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);

        // Gambar raycast-nya
        for (int i = 0; i < numRays; i++)
        {
            float currentAngle = angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, range))
            {
                // Jika kena, gambar garis hijau sampai titik kena
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, hit.point);
            }
            else
            {
                // Jika tidak kena, gambar garis merah sampai jarak maksimum
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, direction * range);
            }
        }
    }
}