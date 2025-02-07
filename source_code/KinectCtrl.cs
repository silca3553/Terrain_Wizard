using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//azure
using Microsoft.Azure.Kinect.Sensor;
public class KinectCtrl : MonoBehaviour
{
    Device kinect;
    // Start is called before the first frame update
    void Start()
    {
        initKinect();
    }

    private void initKinect()
    {
        kinect = Device.Open(0);

        kinect.StartCameras(new DeviceConfiguration
        {
            CameraFPS = FPS.FPS30,
            ColorResolution = ColorResolution.R720p,
            ColorFormat = ImageFormat.ColorBGRA32,
            DepthMode = DepthMode.WFOV_2x2Binned,
            SynchronizedImagesOnly = true
        });


    }
    // Update is called once per frame
    void Update()
    {
    }
}
