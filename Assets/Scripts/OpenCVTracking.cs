/*
 *  Head tracking following this tutorial: https://www.pyimagesearch.com/2018/02/26/face-detection-with-opencv-and-deep-learning/
 *  Facial landmark tracking following this tutorial: https://www.pyimagesearch.com/2017/04/03/facial-landmarks-dlib-opencv-python/
 *  Head pose estimation reference: https://www.learnopencv.com/head-pose-estimation-using-opencv-and-dlib/
 *  Hand gesture detection reference: https://medium.com/@soffritti.pierfrancesco/handy-hands-detection-with-opencv-ac6e9fb3cec1
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
using Rect = OpenCVForUnity.CoreModule.Rect;
using OpenCVForUnity.Calib3dModule;

public class OpenCVTracking : MonoBehaviour
{
    //public string chosenWebcam = "c922 Pro Stream Webcam";
    public string tfPrototxt = "opencv_face_detector.pbtxt";
    public string tfModel = "opencv_face_detector_uint8.pb";
    public string tfHandModelPrototxt = "hand_label_map.pbtxt";
    public string tfHandModel = "hands_detector.pb";
    public string dlibModel = "sp_human_face_68.dat";
    public float minConfidence = 0.5f;
    public float minConfidenceHands = 0.99f;
    public float eyeClosedThreshold = 0.1f;
    public float mouthClosedThreshold = 0.1f;
    public RawImage image;
    public GameObject head;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject avatar;
    public float timeBetweenChecks = 1000;
    public double lowH;
    public double lowS;
    public double lowV;
    public double highH;
    public double highS;
    public double highV;
    public int bgRemovalThreshold = 10;
    public int handBoxPadding = 20;
    public double fingerScaling = 0.3;
    public double neighborScaling = 0.05;
    public int limitAngleSup = 60;
    public int limitAngleInf = 5;
    public int offsetLowThreshold = 80;
    public int offsetHighThreshold = 30;
    public int sampleSize = 20;
    public int sampleDistance = 20;
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
    private float timeSinceLastCheck;
    private Texture2D texture;
    private Mat firstFrame;
    private bool firstFrameTaken;
    private bool webcamLeftHandDetected;
    private bool webcamRightHandDetected;
    private float timeSinceLeftHandDetection;
    private float timeSinceRightHandDetection;
    private float leftHandLerpTime;
    private float rightHandLerpTime;
    private bool testing;
    // Start is called before the first frame update
    void Start()
    {
        testing = false;
        timeSinceLastCheck = 0;
        timeSinceLeftHandDetection = 0;
        timeSinceRightHandDetection = 0;
        webcamLeftHandDetected = false;
        webcamRightHandDetected = false;
        // Webcam Texture setup
        webcamTexture = new WebCamTexture();
        webcamTexture.requestedFPS = 30f;
        webcamTexture.deviceName = Settings.Instance.webcam;

        // DNN setup

        // Load model
        //net = Dnn.readNetFromCaffe(Utils.getFilePath("dnn/" + caffePrototxt), Utils.getFilePath("dnn/" + caffeModel));
        net  = Dnn.readNetFromTensorflow(Utils.getFilePath("dnn/" + tfModel), Utils.getFilePath("dnn/" + tfPrototxt));
        // Load hand model
        handNet = Dnn.readNetFromTensorflow(Utils.getFilePath("dnn/" + tfHandModel), Utils.getFilePath("dnn/" + tfHandModelPrototxt));

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

        avatar = GameObject.FindGameObjectWithTag("Player");

        distCoeffs = new MatOfDouble(0,0,0,0);

        firstFrame = new Mat(webcamTexture.height, webcamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));

        // Manually setting slider values for my own hand
        /*
        lowH = 0.0f;
        lowS = 15.5f;
        lowV = 165.3f;
        highH = 180.0f;
        highS = 104.7f;
        highV = 255.0f;*/

        Utils.setDebugMode(true);
        firstFrameTaken = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (Settings.Instance.webcam == "None")
        {
            return;
        }
        webcamTexture.deviceName = Settings.Instance.webcam;
        timeSinceLastCheck += Time.deltaTime;
        if(isPlaying && webcamTexture.didUpdateThisFrame && Mathf.FloorToInt(timeSinceLastCheck * 1000) >= timeBetweenChecks)
        {
            // Webcam texture to OpenCV Mat
            webcamMat = new Mat(webcamTexture.height, webcamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
            colors = new Color32[webcamTexture.width * webcamTexture.height];
            Utils.webCamTextureToMat(webcamTexture, webcamMat, colors, true);
            texture = new Texture2D(webcamMat.cols(), webcamMat.rows(), TextureFormat.RGBA32, false);
            if (Settings.Instance.headTracker == "Webcam")
            {
                Utils.fastMatToTexture2D(webcamMat, texture, false);
                

                // DNN
                // Load image and construct blob
                Mat bgrMat = new Mat(webcamMat.rows(), webcamMat.cols(), CvType.CV_8UC3);
                Imgproc.cvtColor(webcamMat, bgrMat, Imgproc.COLOR_RGBA2BGR);
                Mat blob = Dnn.blobFromImage(bgrMat, 1.0, new Size(300, 300), new Scalar(127.5, 127.5, 127.5), false, false);
                net.setInput(blob);
                // Pass blob through the network
                //List<Mat> detections = new List<Mat>();
                Mat detections = net.forward();
                if (!detections.empty())
                {
                    //Debug.Log("Face detected");
                    detections = detections.reshape(1, (int)detections.total() / 7);
                    float[] data = new float[7];
                    for (int i = 0; i < detections.rows(); i++)
                    {
                        detections.get(i, 0, data);
                        // Get the confidence
                        float confidence = data[2];

                        // Filter out weak detections
                        if (confidence > minConfidence)
                        {
                            //Debug.Log("Aaaaaaaa");
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

                            Utils.fastMatToTexture2D(webcamMat, texture, false);
                            webcamTexture.GetPixels32(colors);

                            faceDetector.SetImage<Color32>(colors, texture.width, texture.height, 4, true);

                            // Detect face
                            List<Vector2> facePoints = faceDetector.DetectLandmark(new UnityEngine.Rect(left, top, width, height));

                            Color32[] texColor = texture.GetPixels32();
                            // Draw face
                            faceDetector.DrawDetectLandmarkResult<Color32>(texColor, texture.width, texture.height, 4, false, 0, 255, 0, 255);

                            texture.SetPixels32(texColor);
                            texture.Apply(false);

                            Utils.fastTexture2DToMat(texture, webcamMat, false);

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

                            Calib3d.solvePnP(model_points, image_points, cameraMatrix, distCoeffs, rotationVector, translationVector, true, Calib3d.SOLVEPNP_ITERATIVE);

                            MatOfPoint3f nose_end_point3d = new MatOfPoint3f(new Point3(0, 0, 1000));
                            MatOfPoint2f nose_end_point2d = new MatOfPoint2f(new Point(0, 0));
                            Calib3d.projectPoints(nose_end_point3d, rotationVector, translationVector, cameraMatrix, distCoeffs, nose_end_point2d);

                            Imgproc.line(webcamMat, new Point(image_points.get(0, 0)), new Point(nose_end_point2d.get(0, 0)), new Scalar(0, 0, 255, 255), 2);

                            if (!testing)
                            {
                                // OpenGl to Unity coordinates https://stackoverflow.com/questions/36561593/opencv-rotation-rodrigues-and-translation-vectors-for-positioning-3d-object-in
                                Mat rotationMatrix = new Mat();
                                Calib3d.Rodrigues(rotationVector, rotationMatrix);
                                Mat up = rotationMatrix.row(1);
                                Mat forward = rotationMatrix.row(2);

                                double[] u = new double[3];
                                rotationMatrix.get(1, 0, u);
                                double[] f = new double[3];
                                rotationMatrix.get(2, 0, f);
                                Quaternion rot = Quaternion.LookRotation(new Vector3(-(float)f[0], (float)f[1], (float)f[2]), new Vector3((float)u[0], -(float)u[1], (float)u[2]));


                                head.gameObject.transform.rotation = rot;

                                //Debug.Log("Left:" + leftEyeAspectRatio + " Right:" + rightEyeAspectRatio + " Mouth: " + mouthAspectRatio);
                                if (leftEyeAspectRatio > 0.3f)
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
                                else if (mouthAspectRatio > 0.2f)
                                {
                                    mouthAspectRatio = 50;
                                }
                                else
                                {
                                    mouthAspectRatio = 0;
                                }

                                GameObject avatarMesh = null;
                                foreach (Transform child in avatar.transform)
                                {
                                    if (child.name == "Body")
                                    {
                                        avatarMesh = child.gameObject;
                                        break;
                                    }
                                }
                                // Set left eye
                                avatarMesh.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, leftEyeAspectRatio);

                                // Set right eye
                                avatarMesh.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(1, rightEyeAspectRatio);

                                // Set mouth
                                avatarMesh.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(4, mouthAspectRatio);
                            }

                                                     
                        }
                    }
                }
                bgrMat.Dispose();
                blob.Dispose();
            }
            if (Settings.Instance.handTracker == "Webcam")
            {
                if (webcamLeftHandDetected)
                {
                    timeSinceLeftHandDetection += Time.deltaTime;
                    leftHandLerpTime += Time.deltaTime;
                    Vector3 newLeftHandPosition = new Vector3(leftHand.transform.localPosition.x, (float)Settings.Instance.webcamHandHeight, leftHand.transform.localPosition.z);
                    Vector3 newLeftHandRotation = new Vector3(0, 0, 0);
                    leftHand.transform.localEulerAngles = newLeftHandRotation;/*Vector3.Lerp(leftHand.transform.localEulerAngles, newLeftHandRotation, leftHandLerpTime / 1.0f);*/
                    leftHand.transform.localPosition = Vector3.Lerp(leftHand.transform.localPosition, newLeftHandPosition, leftHandLerpTime / 1.0f);
                }
                else
                {
                    leftHandLerpTime += Time.deltaTime;
                    Vector3 newLeftHandPosition = new Vector3(leftHand.transform.localPosition.x, (float)Settings.Instance.handHeight, leftHand.transform.localPosition.z);
                    Vector3 newLeftHandRotation = new Vector3(-90, 0, 0);
                    leftHand.transform.localEulerAngles = newLeftHandRotation; /*Vector3.Lerp(leftHand.transform.localEulerAngles, newLeftHandRotation, rightHandLerpTime / 1.0f);*/
                    leftHand.transform.localPosition = Vector3.Lerp(leftHand.transform.localPosition, newLeftHandPosition, rightHandLerpTime / 1.0f);
                }

                if (webcamRightHandDetected)
                {
                    timeSinceRightHandDetection += Time.deltaTime;
                    rightHandLerpTime += Time.deltaTime;
                    Vector3 newRightHandPosition = new Vector3(rightHand.transform.localPosition.x, (float)Settings.Instance.webcamHandHeight, rightHand.transform.localPosition.z);
                    Vector3 newRightHandRotation = new Vector3(0, 0, 0);
                    rightHand.transform.localEulerAngles = newRightHandRotation; /*Vector3.Lerp(rightHand.transform.localEulerAngles, newRightHandRotation, rightHandLerpTime / 1.0f);*/
                    rightHand.transform.localPosition = Vector3.Lerp(rightHand.transform.localPosition, newRightHandPosition, rightHandLerpTime / 1.0f);
                }
                else
                {
                    rightHandLerpTime += Time.deltaTime;
                    Vector3 newRightHandPosition = new Vector3(rightHand.transform.localPosition.x, (float)Settings.Instance.handHeight, rightHand.transform.localPosition.z);
                    Vector3 newRightHandRotation = new Vector3(-90, 0, 0);
                    rightHand.transform.localEulerAngles = newRightHandRotation; /*Vector3.Lerp(rightHand.transform.localEulerAngles, newRightHandRotation, rightHandLerpTime / 1.0f);*/
                    rightHand.transform.localPosition = Vector3.Lerp(rightHand.transform.localPosition, newRightHandPosition, rightHandLerpTime / 1.0f);
                }

                if (!firstFrameTaken && webcamTexture.didUpdateThisFrame)
                {
                    firstFrame = new Mat(webcamTexture.height, webcamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
                    colors = new Color32[webcamTexture.width * webcamTexture.height];
                    Utils.webCamTextureToMat(webcamTexture, firstFrame, colors, true);
                    Imgproc.cvtColor(firstFrame, firstFrame, Imgproc.COLOR_RGBA2GRAY);
                    firstFrameTaken = true;
                }

                // Webcam texture to OpenCV Mat
                Mat webcamMat2 = new Mat(webcamTexture.height, webcamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
                Utils.webCamTextureToMat(webcamTexture, webcamMat2, colors, true);
               
                //texture = new Texture2D(webcamMat2.cols(), webcamMat2.rows(), TextureFormat.RGBA32, false);
                //Utils.fastMatToTexture2D(webcamMat2, texture, true);
                // DNN
                // Load image and construct blob
                Mat bgrMat = new Mat(webcamMat2.rows(), webcamMat2.cols(), CvType.CV_8UC3);
                Imgproc.cvtColor(webcamMat2, bgrMat, Imgproc.COLOR_RGBA2BGR);
                Mat blob = Dnn.blobFromImage(bgrMat, 1, new Size(320, 240));
                handNet.setInput(blob);
                // Pass blob through the network
                Mat detections = handNet.forward();
                if (!detections.empty())
                {
                    detections = detections.reshape(1, (int)detections.total() / 7);
                    float[] data = new float[7];
                    for (int i = 0; i < detections.rows(); i++)
                    {
                        detections.get(i, 0, data);
                        //Debug.Log(detections[0].dump());
                        // Get the confidence
                        float confidence = data[2];

                        // Filter out weak detections
                        if (confidence > minConfidenceHands)
                        {
                            int class_id = (int)(data[1]);
                            int left = (int)(data[3] * webcamMat2.cols());
                            int top = (int)(data[4] * webcamMat2.rows());
                            int right = (int)(data[5] * webcamMat2.cols());
                            int bottom = (int)(data[6] * webcamMat2.rows());

                            // Add rectangle and text
                            Imgproc.rectangle(webcamMat, new Point(left, bottom), new Point(right, top), new Scalar(255, 0, 0, 255), 2);
                            Imgproc.putText(webcamMat, confidence.ToString(), new Point(left, top - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar(255, 0, 0, 255), 2);

                            // Finding hand gesture
                            // Making sure that the hand detection is in the picture
                            if(left + Mathf.Abs(right - left) < webcamMat.cols() && top + Mathf.Abs(top - bottom) < webcamMat.rows())
                            {
                                int leftBeforeClamp = left;
                                // Add some padding to the hand detection box
                                left = Mathf.Clamp(left-handBoxPadding, 0, int.MaxValue);
                                right = Mathf.Clamp(right + handBoxPadding, int.MinValue, webcamTexture.width);
                                top = Mathf.Clamp(top - handBoxPadding, 0, int.MaxValue);
                                bottom = Mathf.Clamp(bottom + handBoxPadding, int.MinValue, webcamTexture.height);

                                // Get mask to cut out as much of the background as possible
                                // Get the mask for cutting out background
                                Mat webcamMask = Mat.zeros(webcamTexture.height, webcamTexture.width, CvType.CV_8UC1);
                                Imgproc.cvtColor(webcamMat2, webcamMask, Imgproc.COLOR_RGBA2GRAY);

                                Mat diff = new Mat();
                                Core.absdiff(webcamMask, firstFrame, diff);
                                byte[] pixels = new byte[webcamMask.cols() * webcamMask.rows()];
                                diff.get(0, 0, pixels);
                                for (int k = 0; k < webcamMask.cols() * webcamMask.rows(); k++)
                                {
                                    if ((float)pixels[k] <= bgRemovalThreshold)
                                    {
                                        pixels[k] = 0;
                                    }
                                    else
                                    {
                                        pixels[k] = 255;
                                    }
                                }
                                webcamMask.put(0, 0, pixels);
                                Imgproc.dilate(webcamMask, webcamMask, Mat.ones(5, 5, CvType.CV_8UC1), new Point(-1, -1), 2);
                                Imgproc.erode(webcamMask, webcamMask, Mat.ones(3, 3, CvType.CV_8UC1), new Point(-1, -1), 3);
                                Imgproc.GaussianBlur(webcamMask, webcamMask, new Size(3, 3), 0);

                                // Apply mask
                                Imgproc.cvtColor(webcamMask, webcamMask, Imgproc.COLOR_GRAY2RGBA);
                                Core.bitwise_and(webcamMat2, webcamMask, webcamMat2);
                                diff.Dispose();
                                webcamMask.Dispose();

                                Mat tempMat = new Mat(webcamMat2,
                                new Rect(new Point(left, top), new Size(Mathf.Abs(right - left), Mathf.Abs(top - bottom))));

                                Mat tempMat2 = new Mat(Mathf.Abs(top - bottom), Mathf.Abs(right - left), CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
                                Imgproc.cvtColor(tempMat, tempMat2, Imgproc.COLOR_RGB2HSV);

                                // Sampling detected hand for HSV color values
                                Rect scanRect1 = new Rect(new Point(tempMat2.cols() / 2, tempMat2.rows() / 2), new Size(sampleSize, sampleSize));
                                Rect scanRect2 = new Rect(new Point(tempMat2.cols() / 2, tempMat2.rows() / 2 + sampleDistance), new Size(sampleSize, sampleSize));
                                Mat pixel1 = new Mat(tempMat2, scanRect1);
                                Mat pixel2 = new Mat(tempMat2, scanRect2);

                                // Segmenting by skin color
                                Scalar hsvMeanSample1 = Core.mean(pixel1);
                                Scalar hsvMeanSample2 = Core.mean(pixel2);
                                pixel1.Dispose();
                                pixel2.Dispose();
                                lowH = Mathf.Min((float)hsvMeanSample1.val[0], (float)hsvMeanSample2.val[0]) - offsetLowThreshold;
                                highH = Mathf.Max((float)hsvMeanSample1.val[0], (float)hsvMeanSample2.val[0]) + offsetHighThreshold;
                                lowS = Mathf.Min((float)hsvMeanSample1.val[1], (float)hsvMeanSample2.val[1]) - offsetLowThreshold;
                                highS = Mathf.Max((float)hsvMeanSample1.val[1], (float)hsvMeanSample2.val[1]) + offsetHighThreshold;
                                lowV = Mathf.Min((float)hsvMeanSample1.val[2], (float)hsvMeanSample2.val[2]) - offsetLowThreshold;
                                //lowV = 0;
                                highV = Mathf.Max((float)hsvMeanSample1.val[2], (float)hsvMeanSample2.val[2]) + offsetHighThreshold;
                                Scalar skinColorUpper = new Scalar(highH, highS, highV);
                                Scalar skinColorLower = new Scalar(lowH, lowS, lowV);
                                Core.inRange(tempMat2, skinColorLower, skinColorUpper, tempMat2);
                                Imgproc.blur(tempMat2, tempMat2, new Size(10, 10));
                                Imgproc.threshold(tempMat2, tempMat2, 200, 255, Imgproc.THRESH_BINARY);

                                //texture = new Texture2D(Mathf.Abs(right - left), Mathf.Abs(top - bottom), TextureFormat.RGBA32, false);
                                //Utils.matToTexture2D(tempMat2, texture);

                                
                                // Find the hand contours
                                List<MatOfPoint> contours = new List<MatOfPoint>();
                                Mat hierachy = new Mat();
                                Imgproc.findContours(tempMat2, contours, hierachy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);

                                // Get the biggest contour
                                int biggestContourIndex = -1;
                                double biggestContourArea = 0.0;

                                for(int j =0; j < contours.Count; j++)
                                {
                                    double area = Imgproc.contourArea(contours[j], false);
                                    if(area > biggestContourArea)
                                    {
                                        biggestContourArea = area;
                                        biggestContourIndex = j;
                                    }
                                }

                                if (contours.Count > 0 && biggestContourIndex != -1)
                                {

                                    // Find the convex hull
                                    List<MatOfInt> hullList = new List<MatOfInt>();
                                    List<MatOfPoint> hullPointList = new List<MatOfPoint>();
                                    MatOfInt hullInts = new MatOfInt();
                                    Imgproc.convexHull(contours[biggestContourIndex], hullInts, false);
                                    hullList.Add(hullInts);

                                    Point[] contourArray = contours[biggestContourIndex].toArray();
                                    Point[] hullPointFromInts = new Point[hullInts.rows()];
                                    List<int> hullContourIdxList = hullInts.toList();
                                    for (int j = 0; j < hullContourIdxList.Count; j++)
                                    {
                                        hullPointFromInts[j] = contourArray[hullContourIdxList[j]];
                                    }
                                    MatOfPoint hullPoints = new MatOfPoint(hullPointFromInts);
                                    hullPointList.Add(hullPoints);

                                    // Find the convexity defects
                                    MatOfInt4 defect = new MatOfInt4();
                                    Imgproc.convexityDefects(contours[biggestContourIndex], hullInts, defect);

                                    // Find the bounding box of the hull
                                    Rect boundingRect = Imgproc.boundingRect(hullPoints);

                                    // Find the center of the hull bounding box
                                    Point boundingRectCenter = new Point((boundingRect.tl().x + boundingRect.br().x) / 2,
                                        (boundingRect.tl().y + boundingRect.br().y) / 2);

                                    List<Point> startPoints = new List<Point>();
                                    List<Point> farPoints = new List<Point>();
                                    
                                    
                                    for(int j = 0; j < defect.rows(); j++)
                                    {
                                        startPoints.Add(contourArray[(int)defect.get(j, 0)[0]]);
                                        if(GetPointsDistance(contourArray[(int)defect.get(j, 0)[2]], boundingRectCenter) < boundingRect.height * fingerScaling)
                                        {
                                            farPoints.Add(contourArray[(int)defect.get(j, 0)[2]]);
                                        }
                                    }

                                    // Compact the point on their medians
                                    List<Point> fingerTipPointsFiltered = CompactOnNeighborhoodMedian(startPoints, boundingRect.height * neighborScaling);
                                    List<Point> defectPointsFiltered = CompactOnNeighborhoodMedian(farPoints, boundingRect.height * neighborScaling);

                                    // Find the fingers
                                    List<Point> fingerPointsFiltered = new List<Point>();
                                    if(defectPointsFiltered.Count > 1)
                                    {
                                        List<Point> fingerPoints = new List<Point>();
                                        for (int j = 0; j < fingerTipPointsFiltered.Count; j++)
                                        {
                                            List<Point> closestPoints = new List<Point>(2) {new Point(), new Point()};
                                            int indexFound = 0;
                                            double distance1x = double.MaxValue;
                                            double distance1 = double.MaxValue;

                                            for (int k = 0; k < defectPointsFiltered.Count; k++)
                                            {
                                                double distancex = GetPointsDistanceOnX(fingerTipPointsFiltered[j], defectPointsFiltered[k]);
                                                double distance = GetPointsDistance(fingerTipPointsFiltered[j], defectPointsFiltered[k]);

                                                // If the distance on the x axis is less
                                                // and the distance is less
                                                if (distancex < distance1x && distancex != 0 && distance <= distance1)
                                                {
                                                    distance1x = distancex;
                                                    distance1 = distance;
                                                    indexFound = k;
                                                }
                                            }

                                            closestPoints[0] = defectPointsFiltered[indexFound];

                                            double distance2x = double.MaxValue;
                                            double distance2 = double.MaxValue;

                                            for (int k = 0; k < defectPointsFiltered.Count; k++)
                                            {
                                                double distancex = GetPointsDistanceOnX(fingerTipPointsFiltered[j], defectPointsFiltered[k]);
                                                double distance = GetPointsDistance(fingerTipPointsFiltered[j], defectPointsFiltered[k]);

                                                // If the distance on the x axis is less
                                                // and the distance is less
                                                // and is not the previous point
                                                if (distancex < distance2x && distancex != 0
                                                    && distance <= distance2 && distancex != distance1x)
                                                {
                                                    distance2x = distancex;
                                                    distance2 = distance;
                                                    indexFound = k;
                                                }
                                            }

                                            closestPoints[1] = defectPointsFiltered[indexFound];
                                            if (IsFinger(closestPoints[0], fingerTipPointsFiltered[j], closestPoints[1], limitAngleInf, limitAngleSup, boundingRectCenter, boundingRect.height * fingerScaling))
                                            {
                                                fingerPoints.Add(fingerTipPointsFiltered[j]);
                                            }
                                        }
                                        if(fingerPoints.Count > 0)
                                        {
                                            // Shouldn't have more than 5 fingers...
                                            while (fingerPoints.Count > 5)
                                            {
                                                fingerPoints.RemoveAt(fingerPoints.Count - 1);
                                            }

                                            // Filter out points too close to each other
                                            for(int k = 0; k < fingerPoints.Count - 1; k++)
                                            {
                                                if (GetPointsDistanceOnX(fingerPoints[k], fingerPoints[k + 1]) > boundingRect.height * neighborScaling * 1.5)
                                                {
                                                    fingerPointsFiltered.Add(fingerPoints[k]);
                                                }
                                            }

                                            if (fingerPoints.Count > 2)
                                            {
                                                if (GetPointsDistanceOnX(fingerPoints[0], fingerPoints[fingerPoints.Count - 1]) > boundingRect.height * neighborScaling * 1.5)
                                                {
                                                    fingerPointsFiltered.Add(fingerPoints[fingerPoints.Count - 1]);
                                                }
                                            }
                                            else
                                            {
                                                fingerPointsFiltered.Add(fingerPoints[fingerPoints.Count - 1]);
                                            }
                                        }
                                    }
                                    
                                    Imgproc.drawContours(tempMat, contours, biggestContourIndex, new Scalar(0, 0, 255, 255));
                                    Imgproc.polylines(tempMat, hullPointList, true, new Scalar(0, 255, 0, 255));
                                    Imgproc.rectangle(tempMat, boundingRect.tl(), boundingRect.br(), new Scalar(255, 0, 0, 255), 2, 8, 0);
                                    Imgproc.circle(tempMat, boundingRectCenter, 5, new Scalar(255, 0, 0, 255), 2, 8);
                                    DrawFingerPoints(tempMat, fingerTipPointsFiltered, new Scalar(Color.magenta.r*255, Color.magenta.g*255, Color.magenta.b*255, 255), false);
                                    DrawFingerPoints(tempMat, defectPointsFiltered, new Scalar(255, 0, 0, 255), false);
                                    DrawFingerPoints(tempMat, fingerPointsFiltered, new Scalar(Color.yellow.r*255, Color.yellow.g * 255, Color.yellow.b * 255, 255), false);
                                    Imgproc.putText(tempMat, "Fingers: " + fingerPointsFiltered.Count.ToString(), new Point(boundingRect.x, boundingRect.y-10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar(255, 0, 0, 255), 2); ;
                                    Rect roi = new Rect(new Point(left, top), new Size(Mathf.Abs(right - left), Mathf.Abs(top - bottom)));
                                    Mat webcamMatTemp = webcamMat.submat(roi);
                                    tempMat2.copyTo(webcamMatTemp);
                                    //texture = new Texture2D(Mathf.Abs(right - left), Mathf.Abs(top - bottom), TextureFormat.RGBA32, false);
                                    //Utils.matToTexture2D(tempMat2, texture);
                                    for (int y = 0; y < tempMat.rows(); y++)
                                    {
                                        //Width of image
                                        for (int x = 0; x < tempMat.cols(); x++)
                                        {
                                            double[] matData = tempMat.get(y, x);
                                            webcamMat.put(top + y, left + x, matData);
                                        }
                                    }

                                    if (!testing)
                                    {
                                        // Change avatar gesture
                                        // Is this the left or right hand (Mirrored)?
                                        // Left side

                                        if (leftBeforeClamp >= webcamMat.cols() / 2)
                                        {
                                            avatar.GetComponent<Animator>().SetInteger("RGesture", fingerPointsFiltered.Count);
                                            if (!webcamLeftHandDetected)
                                            {
                                                webcamLeftHandDetected = true;
                                                leftHandLerpTime = 0;
                                            }
                                            timeSinceLeftHandDetection = 0;
                                        }
                                        // Right side
                                        else
                                        {
                                            avatar.GetComponent<Animator>().SetInteger("LGesture", fingerPointsFiltered.Count);
                                            if (!webcamRightHandDetected)
                                            {
                                                webcamRightHandDetected = true;
                                                rightHandLerpTime = 0;
                                            }
                                            timeSinceRightHandDetection = 0;
                                        }
                                    }

                                    
                                }
                                tempMat.Dispose();
                                tempMat2.Dispose();
                            }
                        }
                        else
                        {
                            // No detections...
                            if(timeSinceLeftHandDetection > 5 && webcamLeftHandDetected)
                            {
                                webcamLeftHandDetected = false;
                                leftHandLerpTime = 0;
                                avatar.GetComponent<Animator>().SetInteger("LGesture", -1);
                            }

                            if (timeSinceRightHandDetection > 5 && webcamRightHandDetected)
                            {
                                webcamRightHandDetected = false;
                                rightHandLerpTime = 0;
                                avatar.GetComponent<Animator>().SetInteger("RGesture", -1);
                            }
                        }
                    }
                }
                detections.Dispose();
                webcamMat2.Dispose();
                blob.Dispose();
                bgrMat.Dispose();
            }
            Utils.fastMatToTexture2D(webcamMat, texture, true);
            image.texture = texture;
            webcamMat.Dispose();
            timeSinceLastCheck = 0;
        }
    }

    double GetPointsDistance(Point a, Point b)
    {
        Point difference = a - b;
        return Mathf.Sqrt((float)difference.dot(difference));
    }

    double GetPointsDistanceOnX(Point a, Point b)
    {
        return Mathf.Abs((float)a.x- (float)b.x);
    }

    double FindAngle(Point a, Point b, Point c)
    {
        double ab = GetPointsDistance(a, b);
        double bc = GetPointsDistance(b, c);
        double ac = GetPointsDistance(a, c);
        return Mathf.Acos((float)((ab * ab + bc * bc - ac * ac) / (2 * ab * bc))) * 180/Mathf.PI;
    }

    bool IsFinger(Point a, Point b, Point c, double limitAngleInf, double limitAngleSup, Point palmCenter, double minDistanceFromPalm)
    {
        double angle = FindAngle(a, b, c);
        if (angle <= limitAngleSup && angle >= limitAngleInf)
        {
            // Finger should not be under the two defect points
            double deltay1 = b.y - a.y;
            double deltay2 = b.y - c.y;
            if(deltay1 <= 0 || deltay2 <= 0)
            {
                // The two defect points should not be both under the center of the hand
                double deltay3 = palmCenter.y - a.y;
                double deltay4 = palmCenter.y - c.y;
                if(deltay3 >= 0 || deltay4 >= 0)
                {
                    double distanceFromPalm = GetPointsDistance(b, palmCenter);
                    if(distanceFromPalm >= minDistanceFromPalm)
                    {
                        // No fingers up
                        double distanceFromPalmFar1 = GetPointsDistance(a, palmCenter);
                        double distanceFromPalmFar2 = GetPointsDistance(c, palmCenter);
                        if(distanceFromPalmFar1 >= minDistanceFromPalm/4 && distanceFromPalmFar2 >= minDistanceFromPalm / 4)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    void DrawFingerPoints(Mat image, List<Point> points, Scalar color, bool withNumbers)
    {
        for(int i = 0; i < points.Count; i++)
        {
            Imgproc.circle(image, points[i], 5, color, 2, 8);
            if (withNumbers)
            {
                Imgproc.putText(image, i.ToString(), points[i], Imgproc.FONT_HERSHEY_PLAIN, 3, color);
            }
        }
    }

    List<Point> CompactOnNeighborhoodMedian(List<Point> points, double maxNeighborDistance)
    {
        List<Point> medianPoints = new List<Point>();
        if(points.Count == 0 || maxNeighborDistance <= 0)
        {
            return medianPoints;
        }

        Point reference = points[0];
        Point median = points[0];
        for (int i = 1; i < points.Count; i++)
        {
            if (GetPointsDistance(reference, points[i]) > maxNeighborDistance)
            {
                medianPoints.Add(median);

                reference = points[i];
                median = points[i];
            }
            else
            {
                median = (points[i] + median) / 2;
            }
        }

        medianPoints.Add(median);

        return medianPoints;
    }

    // Set the device
    public void SetWebcamDevice(string webcam)
    {
        webcamTexture.deviceName = webcam;
    }

    // Start the webcam tracking
    public void StartWebcamTracking()
    {
        webcamTexture.Play();
        isPlaying = true;
        firstFrameTaken = false;
    }

    // Stops the webcam tracking
    public void StopWebcamTracking()
    {
        //webcamTexture.Stop();
        webcamTexture.Pause();
        isPlaying = false;
    }

    public void TakeBackgroundFrame()
    {
        firstFrameTaken = false;
    }

    public void SetTestingMode(bool testingMode)
    {
        testing = testingMode;
    }
}
