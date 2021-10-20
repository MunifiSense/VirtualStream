# VirtualStream
This repository contains my final project for the Game Development bachelor's degree at the British Columbia Institute of Technology. </br>
It was developed by me from January 2020 - October 2020.

The project was an application for streaming with an avatar with the following tracking hardware:
* SteamVR devices/HTC Vive Trackers
* Webcam
* Tobii EyeTracker
* Leap Motion

The project had two main parts:
* VirtualStream application
* VirtualStream SDK (For custom avatars, environments, props)

Assets used:
* <a href="https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647">SteamVR Plugin</a>
* <a href="https://assetstore.unity.com/packages/tools/animation/final-ik-14290">FinalIK (Paid asset, not included in this repository)</a>
* <a href="https://assetstore.unity.com/packages/tools/integration/opencv-for-unity-21088">OpenCV for Unity (Paid asset, not included in this repository)</a>
* <a href="https://assetstore.unity.com/packages/tools/integration/dlib-facelandmark-detector-64314">Dlib Face Landmark Detector (Paid asset, not included in this repository)</a>
* <a href="https://developer.tobii.com/pc-gaming/unity-sdk/ ">Tobii SDK</a>
* <a href="https://developer.leapmotion.com/unity">Leap Motion SDK</a>
* <a href="https://github.com/gkngkc/UnityStandaloneFileBrowser">Unity Standalone File Browser</a>
* <a href="https://github.com/HiddenMonk/Unity3DRuntimeTransformGizmo">Unity3DRuntimeTransformGizmo</a>

Webcam tracking was done by using OpenCV with the Faster RCNN Inception V2 model trained using the Tensorflow Object Detection API with:
* <a href="http://shuoyang1213.me/WIDERFACE/">WIDER FACE dataset</a>
* <a href="http://vision.soic.indiana.edu/projects/egohands/">Egohands dataset</a>

## Main Menu
<p>
  <img src="https://i.imgur.com/vyXMEFK.png" width="500">
</p>

## Webcam Tracking with Debug Info
<p>
  <img src="https://i.imgur.com/ii5CfbJ.png" width="500">
</p>

## VirtualStream SDK Avatar Exporter
<p>
  <img src="https://i.imgur.com/qAQO1rs.png" width="500">
</p>

## References
The following were used for reference and development:
1. <a href="https://github.com/tensorflow/models/blob/master/research/object_detection/g3doc/tf1_detection_zoo.md">TensorFlow Model Detection Zoo</a>
2. <a href="https://www.pyimagesearch.com/2018/02/26/face-detection-with-opencv-and-deep-learning/">Face Detection with OpenCV and deep learning</a>
3. <a href="https://www.pyimagesearch.com/2017/04/03/facial-landmarks-dlib-opencv-python/">Facial landmarks with dlib, OpenCV, and Python</a>
4. <a href="https://www.learnopencv.com/head-pose-estimation-using-opencv-and-dlib/">Head Pose Estimation using OpenCV and Dlib</a>
5. <a href="https://medium.com/@soffritti.pierfrancesco/handy-hands-detection-with-opencv-ac6e9fb3cec1">Handy, hand detection with OpenCV</a>
6. <a href="https://www.pyimagesearch.com/2017/04/24/eye-blink-detection-opencv-python-dlib/">Eye blink detection with OpenCV, Python, and dlib</a>
7. <a href="https://stackoverflow.com/questions/36561593/opencv-rotation-rodrigues-and-translation-vectors-for-positioning-3d-object-in">OpenCV rotation (Rodrigues) and translation vectors for position 3D object in Unity3D</a> 
8. <a href="https://github.com/victordibia/handtracking">Real-time Hand-Detection using Neural Networks (SSD) on Tensorflow</a>
9. <a href="http://vision.soic.indiana.edu/projects/egohands/">EgoHands: A Dataset for Hands in Complex Egocentric Interactions</a>
10. <a href="https://towardsdatascience.com/how-to-train-a-tensorflow-face-object-detection-model-3599dcd0c26f">How to train a Tensorflow face object detection model</a>
11. <a href="http://shuoyang1213.me/WIDERFACE/ ">WIDER FACE: A Face Detection Benchmark</a>
