using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Microsoft.Azure.Kinect.Sensor;
using System.Threading.Tasks;


public class KinectPointCloudCtrl : MonoBehaviour
{

    Device kinect;
    int num;
    Mesh mesh;
    Vector3[] vertices;
    Color32[] colors;
    int[] indices;

    Transformation transformation;

    // Start is called before the first frame update
    void Start()
    {
        initKinect();
        initMesh();

        Task t = KinectLoop();
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

        //color-depth calibration or depth-xyz tramsformation info
        transformation = kinect.GetCalibration().CreateTransformation();
    }

    private void initMesh()
    {
        int width = kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
        int height = kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
        num = width * height;


        Debug.Log("width: "+width + "height: " + height);

        mesh = new Mesh();

        //extend limit of point count
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // Depth 
        vertices = new Vector3[num];
        colors = new Color32[num];
        indices = new int[num];


        for (int i=0;i<num;i++)
        {
            indices[i] = i;
        }

        mesh.vertices = vertices;
        mesh.colors32 = colors;
        mesh.SetIndices(indices, MeshTopology.Points, 0);

        //apply meshfilter
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }


    private async Task KinectLoop()
    {   
        //kinect에서 데이터 취득
        while(true)
        {   
            using (Capture capture = await Task.Run(() => kinect.GetCapture()).ConfigureAwait(true))
            {
                Image colorImage = transformation.ColorImageToDepthCamera(capture);
                BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray();

                Image xyzImage = transformation.DepthImageToPointCloud(capture.Depth);

                Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray();

                for (int i=0;i<num;i++)
                {
                    vertices[i].x = xyzArray[i].X * 0.001f;
                    vertices[i].y = -xyzArray[i].Y * 0.001f;
                    vertices[i].z = xyzArray[i].Z * 0.001f;


                    colors[i].b = colorArray[i].B;
                    colors[i].g = colorArray[i].G;
                    colors[i].r = colorArray[i].R;
                }

                mesh.vertices = vertices;
                mesh.colors32 = colors;
                mesh.RecalculateBounds();
            }
        }
    }

    private void OnDestroy()
    {
        kinect.StopCameras();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
