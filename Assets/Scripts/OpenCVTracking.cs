/*
 *  Head tracking following this tutorial: https://www.pyimagesearch.com/2018/02/26/face-detection-with-opencv-and-deep-learning/
 *  Facial landmark tracking following this tutorial: https://www.pyimagesearch.com/2017/04/03/facial-landmarks-dlib-opencv-python/
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.ImgprocModule;
using UnityEngine.UI;

public class OpenCVTracking : MonoBehaviour
{
    public string chosenWebcam = "Logitech QuickCam Pro 9000";
    public string caffePrototxt = "deploy.prototxt";
    public string caffeModel = "res10_300x300_ssd_iter_140000.caffemodel";
    public float minConfidence = 0.5f;
    public RawImage image;
    WebCamTexture webcamTexture;
    Mat webcamMat;
    Net net;
    Color32[] colors;
    bool isPlaying = false;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            // Webcam texture to OpenCV Mat
            webcamMat = new Mat(webcamTexture.height, webcamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
            colors = new Color32[webcamTexture.width * webcamTexture.height];
            Utils.webCamTextureToMat(webcamTexture, webcamMat, colors, false);
            Texture2D texture = new Texture2D(webcamMat.cols(), webcamMat.rows(), TextureFormat.RGBA32, false);
            Utils.fastMatToTexture2D(webcamMat, texture);

            // DNN
            // Load image and construct blob
            Mat bgrMat = new Mat(webcamMat.rows(), webcamMat.cols(), CvType.CV_8UC3);
            Imgproc.cvtColor(webcamMat, bgrMat, Imgproc.COLOR_RGBA2BGR);
            Mat blob = Dnn.blobFromImage(bgrMat, 1, new Size(300, 300), new Scalar(104.0, 177.0, 123.0, 0));
            net.setInput(blob);
            
            // Pass blob through the network
            List<Mat> detections = new List<Mat>();
            net.forward(detections);
            detections[0] = detections[0].reshape(1, (int)detections[0].total() / 7);
            float[] data = new float[7];
            for (int i=0; i< detections[0].rows(); i++)
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
                    Imgproc.putText(webcamMat, confidence.ToString(), new Point(left, top-10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar(255, 0, 0, 255), 2);
                    Utils.fastMatToTexture2D(webcamMat, texture);
                }
            }
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
