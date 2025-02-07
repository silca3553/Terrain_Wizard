using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using OpenCvSharp;
using System.Collections;

public class GroundCtrl3 : MonoBehaviour
{
    public Terrain terrain;
    public float depthMin = 50.0f;
    public float depthMax = 500.0f;
    public float ratio = 0.01f;
    public float viscosity = 0.95f;

    private Device kinect;
    private Transformation transformation;
    private CancellationTokenSource cancellationTokenSource;
    private Size groundSIze;
    private Texture2D kinectColorTexture;
    private bool terrainMode = false;
    private Mat preDepthmask = new Mat();

    [SerializeField]
    UnityEngine.UI.RawImage rawColorImg;

    int terrainWidth;
    int terrainHeight;
    float convertDepthMax;
    float convertDepthMin;
    float[,] pastHights;


    void Start()
    {
        terrainWidth = terrain.terrainData.heightmapResolution;
        terrainHeight = terrain.terrainData.heightmapResolution;
        convertDepthMax = depthMax * ratio;
        convertDepthMin = depthMin * ratio;
        pastHights  = new float[terrainWidth, terrainHeight];
        initKinect();

        int width = kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
        int height = kinect.GetCalibration().DepthCameraCalibration.ResolutionHeight;
        preDepthmask = Mat.Zeros(width, height, MatType.CV_8UC1);
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
        transformation = kinect.GetCalibration().CreateTransformation();

        groundSIze = new Size(terrainWidth, terrainHeight);

        int width = kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
        int height = kinect.GetCalibration().DepthCameraCalibration.ResolutionHeight;
        kinectColorTexture = new Texture2D(width, height);
    }

    private void Update()
    {
        //updateColorImage();
        updateDepthImage();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            terrainMode = !terrainMode;
            if (terrainMode)
            {
                Task t1 = updateDepthMask();
                Task t2 = updateTerrein();
            }

            else if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                Material terrainMaterial = terrain.materialTemplate;
                terrainMaterial.EnableKeyword("_EMISSION");
                StartCoroutine(Blink(terrainMaterial, 0.3f));
            }
        }

    }

    void updateColorImage()
    {
        Capture capture = kinect.GetCapture();

        //Image colorImage = transformation.ColorImageToDepthCamera(capture);
        Image colorImage = capture.Color;
        Color32[] colorArray = colorImage.GetPixels<Color32>().ToArray();
        for (int i = 0; i < colorArray.Length; i++)
        {
            var d = colorArray[i].b;
            var r = colorArray[i].r;
            colorArray[i].r = d;
            colorArray[i].b = r;
        }
        kinectColorTexture.SetPixels32(colorArray);
        kinectColorTexture.Apply();

        rawColorImg.texture = kinectColorTexture;
    }

    void updateDepthImage()
    {
        rawColorImg.texture = OpenCvSharp.Unity.MatToTexture(preDepthmask);
    }
 
    void getCountourMask(Mat depthMap, Mat depthMask, ref Mat cntMask)
    {
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(depthMask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        double minArea = 3000;
        double minAvg = 500;
        Point[] objectCont = new Point[] { };

        foreach (var contour in contours)
        {
            double area = Cv2.ContourArea(contour);
            if (area < minArea)
            {
                continue;
            }

            double depthSum = 0.0;
            int pointCount = contour.Length;

            foreach (var point in contour)
            {
                depthSum += depthMap.At<float>(point.Y, point.X);
            }

            // 평균 depth 계산
            double averageDepth = depthSum / pointCount;

            if(minAvg > averageDepth)
            {
                objectCont = contour;
                minAvg = averageDepth;
            }
        }

        Cv2.FillPoly(cntMask, new Point[][] { objectCont }, Scalar.White);
    }

    void getEdgeMask(Mat depthMap, ref Mat result)
    {
        Mat[] kernels = new Mat[4];
        kernels[0] = new Mat(3, 3, MatType.CV_32F, new float[] {
                0, 0, 1,
                0, -1, 0,
                0, 0, 0
            });

        kernels[1] = new Mat(3, 3, MatType.CV_32F, new float[] {
                0, 0, 0,
                0, -1, 1,
                0, 0, 0
            });

        kernels[2] = new Mat(3, 3, MatType.CV_32F, new float[] {
                0, 0, 0,
                0, -1, 0,
                0, 0, 1
            });

        kernels[3] = new Mat(3, 3, MatType.CV_32F, new float[] {
                0, 0, 0,
                0, -1, 0,
                0, 1, 0
            });

        for (int i = 0; i < 4; i++)
        {
            Mat edgeMask = new Mat();
            getSubEdgeMask(depthMap, ref edgeMask, kernels[i], 10);
            Cv2.BitwiseOr(result, edgeMask, result);
        }

        result *= 255;
    }

    void getSubEdgeMask(Mat depthMap, ref Mat mask,  Mat kernel, int delta)
    {
        Cv2.Filter2D(depthMap, mask, -1, kernel);
        mask = mask.ConvertScaleAbs();
        Cv2.Threshold(mask, mask, delta, 1, ThresholdTypes.Tozero);
        mask.ConvertTo(mask, MatType.CV_8UC1);
    }

    void getZeroMask(Mat depthMap, ref Mat mask)
    {
        Cv2.Threshold(depthMap, mask, 0, 255, ThresholdTypes.Binary);
        mask.ConvertTo(mask, MatType.CV_8UC1);
        Cv2.BitwiseNot(mask, mask); //(0,255)
    }


    private async Task updateDepthMask()
    {
        cancellationTokenSource = new CancellationTokenSource();

        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            using (Capture capture = await Task.Run(() => kinect.GetCapture()).ConfigureAwait(true))
            {
                Image depthImage = capture.Depth;
                int width = kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
                int height = kinect.GetCalibration().DepthCameraCalibration.ResolutionHeight;

                // Depth processing
                Mat depthMap = new Mat(depthImage.HeightPixels, depthImage.WidthPixels, MatType.CV_16UC1, depthImage.GetPixels<ushort>().ToArray());
                depthMap.ConvertTo(depthMap, MatType.CV_32FC1);

                Mat zeroMask = Mat.Zeros(width, height, MatType.CV_8UC1);
                getZeroMask(depthMap, ref zeroMask);

                Cv2.Threshold(depthMap, depthMap, 500, 1, ThresholdTypes.TozeroInv);

                Mat edgeMask = Mat.Zeros(width, height, MatType.CV_8UC1);
                getEdgeMask(depthMap, ref edgeMask);
                Cv2.BitwiseOr(edgeMask, zeroMask, edgeMask);

                Mat depthMask = new Mat();
                Cv2.Threshold(depthMap, depthMask, 0, 255, ThresholdTypes.Binary);
                depthMask.ConvertTo(depthMask, MatType.CV_8UC1);

                Cv2.BitwiseAnd(depthMask, ~edgeMask, depthMask);

                Mat cntMask = Mat.Zeros(width, height, MatType.CV_8UC1);
                getCountourMask(depthMap, depthMask, ref cntMask);

                Cv2.BitwiseAnd(depthMask, cntMask, depthMask);

                preDepthmask = depthMask;
            }
        }
    }

    private async Task updateTerrein()
    {
        cancellationTokenSource = new CancellationTokenSource();

        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            using (Capture capture = await Task.Run(() => kinect.GetCapture()).ConfigureAwait(true))
            {
                Image depthImage = capture.Depth;
                // Depth 이미지를 OpenCVSharp Mat으로 변환
                Mat depthMat = new Mat(depthImage.HeightPixels, depthImage.WidthPixels, MatType.CV_16UC1, depthImage.GetPixels<ushort>().ToArray());

                Mat depthMap = new Mat();
                depthMat.CopyTo(depthMap, preDepthmask);

                Mat resizedDepthMat = new Mat();
                Cv2.Resize(depthMap, resizedDepthMat, groundSIze, interpolation: InterpolationFlags.Nearest);

                //Cv2.Threshold(resizedDepthMat, resizedDepthMat, 10.0f, 1.0f, ThresholdTypes.Tozero);
                resizedDepthMat.ConvertTo(resizedDepthMat, MatType.CV_32FC1, -ratio, convertDepthMax);
                Cv2.Threshold(resizedDepthMat, resizedDepthMat, 0, 1, ThresholdTypes.Tozero);
                Cv2.Threshold(resizedDepthMat, resizedDepthMat, convertDepthMax - convertDepthMin, 1, ThresholdTypes.TozeroInv);

                float[,] heights = new float[terrainWidth, terrainHeight];

                float margin = 0.001f;
                for (int i = 0; i < groundSIze.Width; i ++)
                {
                    for (int j = 0; j < groundSIze.Height; j ++)
                    {
                        float depth = resizedDepthMat.At<float>(i , groundSIze.Height - 1- j) / convertDepthMax;
                        float distance = depth - pastHights[i,j];

                        //currentPosition.y = newY;
                        if ((margin > 0.2f * distance) && (-margin < 0.2f * distance))
                        {
                            heights[i, j] = depth;
                        }
                        else
                        {
                            heights[i, j] = pastHights[i, j] + (1 - viscosity) * distance;
                        }

                        pastHights[i, j] = heights[i, j];
                    }
                }
                terrain.terrainData.SetHeights(0, 0, heights);
            }
        }
    }

    IEnumerator Blink(Material terrainMaterial, float time)
    {
        Color startColor = Color.black; // 시작 색상 (검정색)
        Color endColor = Color.white / 3; // 끝 색상 (하얀색)

        yield return StartCoroutine(ChangeColor(terrainMaterial, startColor, endColor, time));

        // 점점 검정색으로
        yield return StartCoroutine(ChangeColor(terrainMaterial, endColor, startColor, time));
    }

    IEnumerator ChangeColor(Material terrainMaterial, Color fromColor, Color toColor, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 색상을 보간하여 적용
            terrainMaterial.SetColor("_EmissionColor", Color.Lerp(fromColor, toColor, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        // 색상을 최종값으로 설정
        terrainMaterial.SetColor("_EmissionColor", toColor);
    }

    private void OnApplicationQuit()
    {
        // 작업 취소
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
        }

        // Kinect 종료
        if (kinect != null)
        {
            kinect.StopCameras();
            kinect.Dispose();
        }
    }
}