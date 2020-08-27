/*
 *  Head tracking following this tutorial: https://www.pyimagesearch.com/2018/02/26/face-detection-with-opencv-and-deep-learning/
 *  Facial landmark tracking following this tutorial: https://www.pyimagesearch.com/2017/04/03/facial-landmarks-dlib-opencv-python/
 *  Head pose estimation reference: https://www.learnopencv.com/head-pose-estimation-using-opencv-and-dlib/
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.ImgprocModule;
using UnityEngine.UI;
using DlibFaceLandmarkDetector;
using Rect = UnityEngine.Rect;
using OpenCVForUnity.Calib3dModule;

public class OpenCVTracking : MonoBehaviour
{
    public string chosenWebcam = "Logitech QuickCam Pro 9000";
    public string caffePrototxt = "deploy.prototxt";
    public string caffeModel = "res10_300x300_ssd_iter_140000.caffemodel";
    public string handCaffePrototxt = "pose_deploy.prototxt";
    public string handCaffeModel = "pose_iter_102000.caffemodel";
    public string dlibModel = "sp_human_face_68.dat";
    public float minConfidence = 0.5f;
    public float eyeClosedThreshold = 0.1f;
    public float mouthClosedThreshold = 0.1f;
    public RawImage image;
    public GameObject head;
    public GameObject avatar;
    private WebCamTexture webcamTexture;
    private Mat webcamMat;
    private Net net;
    private Net handNet;
    private Color32[] colors;
    private FaceLandmarkDetector faceDetector;
    private bool isPlaying = false;
    private MatOfPoint3f model_points;
    private Mat cameraMatrix;
    private MatOfDouble distCoeffs;
    private Mat rotationVector;
    private Mat translationVector;
    // Start is called before the first frame update
    void Start()
    {
        // Webcam Texture setup
        webcamTexture = new WebCamTexture();
        webcamTexture.requestedFPS = 30f;
        webcamTexture.deviceName = chosenWebcam;

        // DNN setup

        // Load model
        net = Dnn.readNetFromCaffe(Utils.getFilePath("dnn/" + caffePrototxt), Utils.getFilePath("dnn/" + caffeModel));
        

        // Load Dlib Model
        faceDetector = new FaceLandmarkDetector(Utils.getFilePath("dlib/" + dlibModel));

        // Head Pose Estimation 3d points from: https://www.learnopencv.com/head-pose-estimation-using-opencv-and-dlib/
        Point3 nose = new Point3(0.0, 0.0, 0.0);                            // Nose (Tip)
        Point3 chin = new Point3(0.0, -330.0, -65.0);                       // Chin
        Point3 leftEyeLeftCorner = new Point3(-225.0, 170.0, -135.0);       // Left eye left corner
        Point3 rightEyeRightCorner = new Point3(225.0, 170.0, -135.0);      // Right eye right corner
        Point3 mouthLeftCorner = new Point3(-150.0, -150.0, -125.0);        // Mouth left corner
        Point3 mouthRightCorner = new Point3(150.0, -150.0, -125.0);        // Mouth right corner

        model_points = new MatOfPoint3f(
            nose, chin, leftEyeLeftCorner, rightEyeRightCorner, mouthLeftCorner, mouthRightCorner
            );

        // Camera matrix
        cameraMatrix = new Mat(3, 3, CvType.CV_64FC1);

        avatar = avatar.GetComponentInChildren<SkinnedMeshRenderer>().gameObject;

        distCoeffs = new MatOfDouble(0,0,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying && webcamTexture.didUpdateThisFrame)
        {
            // Webcam texture to OpenCV Mat
            webcamMat = new Mat(webcamTexture.height, webcamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
            colors = new Color32[webcamTexture.width * webcamTexture.height];
            Utils.webCamTextureToMat(webcamTexture, webcamMat, colors, false);
            Texture2D texture = new Texture2D(webcamMat.cols(), webcamMat.rows(), TextureFormat.RGBA32, false);
            Utils.fastMatToTexture2D(webcamMat, texture, true);

            // DNN
            // Load image and construct blob
            Mat bgrMat = new Mat(webcamMat.rows(), webcamMat.cols(), CvType.CV_8UC3);
            Imgproc.cvtColor(webcamMat, bgrMat, Imgproc.COLOR_RGBA2BGR);
            Mat blob = Dnn.blobFromImage(bgrMat, 1, new Size(300, 300), new Scalar(104.0, 177.0, 123.0, 0));
            net.setInput(blob);
            // Pass blob through the network
            List<Mat> detections = new List<Mat>();
            net.forward(detections);
            if (detections.Count > 0)
            {
                //Debug.Log("Face detected");
                detections[0] = detections[0].reshape(1, (int)detections[0].total() / 7);
                float[] data = new float[7];
                for (int i = 0; i < detections[0].rows(); i++)
                {
                    detections[0].get(i, 0, data);

                    // Get the confidence
                    float confidence = data[2];

                    // Filter out weak detections
                    if (confidence > minConfidence)
                    {
                        int class_id = (int)(data[1]);
                        int left = (int)(data[3] * webcamMat.cols());
                        int top = (int)(data[4] * webcamMat.rows());
                        int right = (int)(data[5] * webcamMat.cols());
                        int bottom = (int)(data[6] * webcamMat.rows());
                        int width = right - left + 1;
                        int height = bottom - top + 1;

                        // Add rectangle and text
                        Imgproc.rectangle(webcamMat, new Point(left, top), new Point(right, bottom), new Scalar(255, 0, 0, 255), 2);
                        //Debug.Log("x: " + left + " " + top + " y: " + right + " " + bottom + " width: " + width + " height: " + height);
                        Imgproc.putText(webcamMat, confidence.ToString(), new Point(left, top - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar(255, 0, 0, 255), 2);

                        Utils.fastMatToTexture2D(webcamMat, texture, true);
                        webcamTexture.GetPixels32(colors);

                        faceDetector.SetImage<Color32>(colors, texture.width, texture.height, 4, true);

                        // Detect face
                        List<Vector2> facePoints = faceDetector.DetectLandmark(new Rect(left, top, width, height));

                        Color32[] texColor = texture.GetPixels32();
                        // Draw face
                        faceDetector.DrawDetectLandmarkResult<Color32>(texColor, texture.width, texture.height, 4, true, 0, 255, 0, 255);

                        texture.SetPixels32(texColor);
                        texture.Apply(false);

                        Utils.fastTexture2DToMat(texture, webcamMat, true);

                        // Detect direction user is facing (Head pose estimation)

                        // Coordinates of some facial points
                        Point nose = new Point(facePoints[30].x, facePoints[30].y);                 // Nose (Tip)
                        Point chin = new Point(facePoints[8].x, facePoints[8].y);                   // Chin
                        Point leftEyeLeftCorner = new Point(facePoints[36].x, facePoints[36].y);    // Left eye left corner
                        Point rightEyeRightCorner = new Point(facePoints[45].x, facePoints[45].y);  // Right eye right corner
                        Point mouthLeftCorner = new Point(facePoints[48].x, facePoints[48].y);      // Mouth left corner
                        Point mouthRightCorner = new Point(facePoints[54].x, facePoints[54].y);     // Mouth right corner

                        // Face Aspect ratios
                        // Reference: https://www.pyimagesearch.com/2017/04/24/eye-blink-detection-opencv-python-dlib/
                        float leftEyeAspectRatio = (Vector2.Distance(facePoints[37], facePoints[41]) +
                            Vector2.Distance(facePoints[38], facePoints[40])) / (2 * Vector2.Distance(facePoints[36], facePoints[39]));
                        float rightEyeAspectRatio = (Vector2.Distance(facePoints[43], facePoints[47]) +
                            Vector2.Distance(facePoints[44], facePoints[46])) / (2 * Vector2.Distance(facePoints[42], facePoints[45]));
                        float mouthAspectRatio = (Vector2.Distance(facePoints[61], facePoints[67]) +
                            Vector2.Distance(facePoints[63], facePoints[65])) / (2 * Vector2.Distance(facePoints[60], facePoints[64]));

                        // Camera matrix
                        double fx = webcamTexture.width;
                        double fy = webcamTexture.width;
                        double cx = webcamTexture.width / 2.0;
                        double cy = webcamTexture.height / 2.0;
                        double[] cameraArray = new double[] { fx, 0, cx, 0, fy, cy, 0, 0, 1.0 };
                        cameraMatrix.put(0, 0, cameraArray);

                        MatOfPoint2f image_points = new MatOfPoint2f(new Point[] {
                            nose, chin, leftEyeLeftCorner, rightEyeRightCorner, mouthLeftCorner, mouthRightCorner
                        });

                        if (rotationVector == null || translationVector == null)
                        {
                            rotationVector = new Mat(3, 1, CvType.CV_64FC1);
                            translationVector = new Mat(3, 1, CvType.CV_64FC1);
                            Calib3d.solvePnP(model_points, image_points, cameraMatrix, distCoeffs, rotationVector, translationVector);
                        }

                        //Debug.Log("Rotation Vector: " + rotationVector.dump());
                        //Debug.Log("Translation Vector: " + translationVector.dump());

                        Calib3d.solvePnP(model_points, image_points, cameraMatrix, distCoeffs, rotationVector, translationVector, true, Calib3d.SOLVEPNP_ITERATIVE);

                        MatOfPoint3f nose_end_point3d = new MatOfPoint3f(new Point3(0, 0, 1000));
                        MatOfPoint2f nose_end_point2d = new MatOfPoint2f(new Point(0,0));
                        Calib3d.projectPoints(nose_end_point3d, rotationVector, translationVector, cameraMatrix, distCoeffs, nose_end_point2d);

                        //Debug.Log(nose_end_point2d.dump());

                        Imgproc.line(webcamMat, new Point(image_points.get(0,0)), new Point(nose_end_point2d.get(0,0)), new Scalar(0, 0, 255, 255), 2);

                        Mat rotationMatrix = new Mat();
                        Calib3d.Rodrigues(rotationVector, rotationMatrix);

                        // OpenGl to Unity coordinates https://stackoverflow.com/questions/36561593/opencv-rotation-rodrigues-and-translation-vectors-for-positioning-3d-object-in

                        Mat up = rotationMatrix.row(1);
                        Mat forward = rotationMatrix.row(2);

                        double[] u = new double[3];
                        rotationMatrix.get(1, 0, u);
                        double[] f = new double[3];
                        rotationMatrix.get(2, 0, f);
                        Quaternion rot = Quaternion.LookRotation(new Vector3((float)f[0], (float)f[1], (float)f[2]), new Vector3((float)u[0], (float)-u[1], (float)u[2]));


                        head.gameObject.transform.rotation = rot;

                        //Debug.Log("Left:" + leftEyeAspectRatio + " Right:" + rightEyeAspectRatio + " Mouth: " + mouthAspectRatio);
                        if(leftEyeAspectRatio > 0.3f)
                        {
                            leftEyeAspectRatio = 0;
                        }
                        else
                        {
                            leftEyeAspectRatio = 100;
                        }

                        if (rightEyeAspectRatio > 0.3f)
                        {
                            rightEyeAspectRatio = 0;
                        }
                        else
                        {
                            rightEyeAspectRatio = 100;
                        }

                        if (mouthAspectRatio > 0.4f)
                        {
                            mouthAspectRatio = 100;
                        }
                        else if(mouthAspectRatio > 0.2f)
                        {
                            mouthAspectRatio = 50;
                        }
                        else
                        {
                            mouthAspectRatio = 0;
                        }

                        // Set left eye
                        avatar.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, leftEyeAspectRatio);

                        // Set right eye
                        avatar.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(1, rightEyeAspectRatio);

                        // Set mouth
                        avatar.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(4, mouthAspectRatio);
                    }
                }
            }
            Utils.fastMatToTexture2D(webcamMat, texture, true);
            image.texture = texture;
            blob.Dispose();
        }
    }

    // Get the webcam devices
    WebCamDevice[] GetWebcamDevices()
    {
        return WebCamTexture.devices;
    }

    // Set the device
    void SetWebcamDevice(string webcam)
    {
        webcamTexture.deviceName = chosenWebcam;
    }

    // Start the webcam tracking
    public void StartWebcamTracking()
    {
        webcamTexture.Play();
        isPlaying = true;
    }

    // Stops the webcam tracking
    public void StopWebcamTracking()
    {
        webcamTexture.Stop();
        isPlaying = false;
    }


}
