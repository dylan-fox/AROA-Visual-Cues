﻿using Microsoft.MixedReality.Toolkit.Audio;
using System.Collections;

using System.Collections.Generic;
using UnityEngine;

namespace QRTracking
{
    [RequireComponent(typeof(QRTracking.SpatialGraphNodeTracker))]
    public class QRCode : MonoBehaviour
    {
        public Microsoft.MixedReality.QR.QRCode qrCode;
        private GameObject qrCodeCube;

        public float PhysicalSize { get; private set; }
        public string CodeText { get; private set; }

        private TextMesh QRID;
        private TextMesh QRNodeID;
        private TextMesh QRText;
        private TextMesh QRVersion;
        private TextMesh QRTimeStamp;
        private TextMesh QRSize;
        private GameObject QRInfo;
        private bool validURI = false;
        private bool launch = false;
        private System.Uri uriResult;
        private long lastTimeStamp = 0;
        //AROA EDIT
        private TextMesh QRPositionText;
        public GameObject trackedObject; //Object that will mimic position of QR Code
        public TextToSpeech textToSpeech;
        public ObstacleManager obstacleManager;

        // Use this for initialization
        void Start()
        {
            PhysicalSize = 0.1f;
            CodeText = "Dummy";
            if (qrCode == null)
            {
                throw new System.Exception("QR Code Empty");
            }

            PhysicalSize = qrCode.PhysicalSideLength;
            CodeText = qrCode.Data;

            qrCodeCube = gameObject.transform.Find("Cube").gameObject;
            QRInfo = gameObject.transform.Find("QRInfo").gameObject;
            QRID = QRInfo.transform.Find("QRID").gameObject.GetComponent<TextMesh>();
            QRNodeID = QRInfo.transform.Find("QRNodeID").gameObject.GetComponent<TextMesh>();
            QRText = QRInfo.transform.Find("QRText").gameObject.GetComponent<TextMesh>();
            QRVersion = QRInfo.transform.Find("QRVersion").gameObject.GetComponent<TextMesh>();
            QRTimeStamp = QRInfo.transform.Find("QRTimeStamp").gameObject.GetComponent<TextMesh>();
            QRSize = QRInfo.transform.Find("QRSize").gameObject.GetComponent<TextMesh>();
            //AROA EDIT
            QRPositionText = QRInfo.transform.Find("QRPositionText").gameObject.GetComponent<TextMesh>();

            QRID.text = "Id:" + qrCode.Id.ToString();
            QRNodeID.text = "NodeId:" + qrCode.SpatialGraphNodeId.ToString();
            QRText.text = CodeText;

            if (System.Uri.TryCreate(CodeText, System.UriKind.Absolute,out uriResult))
            {
                validURI = true;
                QRText.color = Color.blue;
            }

            QRVersion.text = "Ver: " + qrCode.Version;
            QRSize.text = "Size:" + qrCode.PhysicalSideLength.ToString("F04") + "m";
            QRTimeStamp.text = "Time:" + qrCode.LastDetectedTime.ToString("MM/dd/yyyy HH:mm:ss.fff");
            QRTimeStamp.color = Color.yellow;
            Debug.Log("Id= " + qrCode.Id + "NodeId= " + qrCode.SpatialGraphNodeId + " PhysicalSize = " + PhysicalSize + " TimeStamp = " + qrCode.SystemRelativeLastDetectedTime.Ticks + " QRVersion = " + qrCode.Version + " QRData = " + CodeText);
        }

        void UpdatePropertiesDisplay()
        {
            // Update properties that change
            if (qrCode != null && lastTimeStamp != qrCode.SystemRelativeLastDetectedTime.Ticks)
            {
                QRSize.text = "Size:" + qrCode.PhysicalSideLength.ToString("F04") + "m";

                QRTimeStamp.text = "Time:" + qrCode.LastDetectedTime.ToString("MM/dd/yyyy HH:mm:ss.fff");
                QRTimeStamp.color = QRTimeStamp.color == Color.yellow ? Color.white : Color.yellow;
                PhysicalSize = qrCode.PhysicalSideLength;
                Debug.Log("Id= " + qrCode.Id + "NodeId= " + qrCode.SpatialGraphNodeId + " PhysicalSize = " + PhysicalSize + " TimeStamp = " + qrCode.SystemRelativeLastDetectedTime.Ticks + " Time = " + qrCode.LastDetectedTime.ToString("MM/dd/yyyy HH:mm:ss.fff"));

                qrCodeCube.transform.localPosition = new Vector3(PhysicalSize / 2.0f, PhysicalSize / 2.0f, 0.0f);
                qrCodeCube.transform.localScale = new Vector3(PhysicalSize, PhysicalSize, 0.005f);
                lastTimeStamp = qrCode.SystemRelativeLastDetectedTime.Ticks;
                QRInfo.transform.localScale = new Vector3(PhysicalSize / 0.2f, PhysicalSize / 0.2f, PhysicalSize / 0.2f);

                //AROA EDIT
                QRPositionText.text = "Position: " + qrCodeCube.transform.position;
                Debug.Log("Position = " + qrCodeCube.transform.position);
                if (trackedObject != null) { 
                    if (trackedObject.name == "Collocated Cues") //Moving whole room
                    {
                        if (!textToSpeech.IsSpeaking())
                            textToSpeech.StartSpeaking("Room calibrated.");
                        obstacleManager.ResetPositions(); //Reset positions of obstacles
                        trackedObject.transform.eulerAngles = new Vector3(0f, 0f, 0f);
                        trackedObject.transform.Rotate(0f, qrCodeCube.transform.rotation.eulerAngles.y + 90f, 0f);//Rotate to match QR code, then 90 degrees 
                        trackedObject.transform.position = qrCodeCube.transform.position + new Vector3 (0.9f, 0f, 0f);  //Adjust position to QR code location; move based on width of hallway
                        //make local changes to adjust
                        trackedObject.transform.localPosition -= trackedObject.transform.forward * 0.86f;
                        trackedObject.transform.localPosition -= trackedObject.transform.up * 1.42f;
                        trackedObject.transform.localPosition += trackedObject.transform.right * 0.36f;
                        Debug.Log("Room position adjusted via QR code.");


                    }

                    else //Normal object
                    {
                        if (!textToSpeech.IsSpeaking())
                            textToSpeech.StartSpeaking("QR code detected.");
                        trackedObject.transform.localPosition = new Vector3(0f, 0f, 0f); //reset local position
                        trackedObject.transform.position = qrCodeCube.transform.position;
                        trackedObject.transform.rotation = qrCodeCube.transform.rotation;
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdatePropertiesDisplay();
            if (launch)
            {
                launch = false;
                LaunchUri();
            }
        }

        void LaunchUri()
        {
#if WINDOWS_UWP
            // Launch the URI
            UnityEngine.WSA.Launcher.LaunchUri(uriResult.ToString(), true);
#endif
        }

        public void OnInputClicked()
        {
            if (validURI)
            {
                launch = true;
            }
// eventData.Use(); // Mark the event as used, so it doesn't fall through to other handlers.
        }
    }
}