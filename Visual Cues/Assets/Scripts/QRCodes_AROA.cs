using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using Microsoft.MixedReality.QR;
using Microsoft.MixedReality.Toolkit.Audio;

namespace QRTracking
{
    public class QRCodes_AROA : MonoBehaviour
    {
        public GameObject qrCodePrefab;
        public GameObject obstLow;
        public GameObject obstMid;
        public GameObject obstHigh;
        public GameObject obstWide;
        public GameObject obstacleCollection;
        public Experiment_Logger experimentLogger;
        public string layout = "Default";

        private System.Collections.Generic.SortedDictionary<System.Guid, GameObject> qrCodesObjectsList;
        private bool clearExisting = false;

        //AROA Edit
        public TextToSpeech textToSpeech;
        public ObstacleManager obstacleManager;

        struct ActionData
        {
            public enum Type
            {
                Added,
                Updated,
                Removed
            };
            public Type type;
            public Microsoft.MixedReality.QR.QRCode qrCode;

            public ActionData(Type type, Microsoft.MixedReality.QR.QRCode qRCode) : this()
            {
                this.type = type;
                qrCode = qRCode;
            }
        }

        private System.Collections.Generic.Queue<ActionData> pendingActions = new Queue<ActionData>();
        void Awake()
        {

        }

        // Use this for initialization
        void Start()
        {
            Debug.Log("QRCodesVisualizer start");
            qrCodesObjectsList = new SortedDictionary<System.Guid, GameObject>();

            QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;

            if (qrCodePrefab == null || obstLow == null || obstMid == null || obstHigh == null || obstWide == null || obstacleCollection == null)
            {
                throw new System.Exception("Prefab or obstacles not assigned");
            }
        }
        private void Instance_QRCodesTrackingStateChanged(object sender, bool status)
        {
            if (!status)
            {
                clearExisting = true;
            }
        }

        private void Instance_QRCodeAdded(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeAdded");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Added, e.Data));
            }
        }

        private void Instance_QRCodeUpdated(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeUpdated");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, e.Data));
            }
        }

        private void Instance_QRCodeRemoved(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeRemoved");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, e.Data));
            }
        }

        private void HandleEvents()
        {
            lock (pendingActions)
            {
                while (pendingActions.Count > 0)
                {
                    var action = pendingActions.Dequeue();
                    if (action.type == ActionData.Type.Added)
                    {
                        GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                        qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                        qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);

                        //AROA EDIT - Assign object to QRCode script
                        Debug.Log("Action.qrCode.Data = " + action.qrCode.Data);
                        qrCodeObject.GetComponent<QRCode>().textToSpeech = textToSpeech;
                        qrCodeObject.GetComponent<QRCode>().obstacleManager = obstacleManager;
                                               
                        if (action.qrCode.Data == "QR Code 1")
                        {
                            layout = "Layout 1";
                        }

                        else if (action.qrCode.Data == "QR Code 2")
                        {
                            layout = "Layout 2";
                        }

                        else if (action.qrCode.Data == "QR Code 3")
                        {
                            layout = "Layout 3";
                        }

                        else if (action.qrCode.Data == "QR Code 4")
                        {
                            layout = "Layout 4";
                        }

                        else if (action.qrCode.Data == "QR Code 5")
                        {
                            layout = "Layout 5";
                        }

                        else if (action.qrCode.Data == "QR Code 6")
                        {
                            layout = "Layout 6";
                        }

                        else if (action.qrCode.Data == "Demo")
                        {
                            layout = "Demo Layout";
                        }

                        experimentLogger.layout = layout;
                        qrCodeObject.GetComponent<QRCode>().layout = layout;
                        qrCodeObject.GetComponent<QRCode>().trackedObject = obstacleCollection;
                        qrCodeObject.GetComponent<QRCode>().experimentLogger = experimentLogger;

                        //Assign obstacles to QR Code object
                        qrCodeObject.GetComponent<QRCode>().obstLow = obstLow;
                        qrCodeObject.GetComponent<QRCode>().obstMid = obstMid;
                        qrCodeObject.GetComponent<QRCode>().obstHigh = obstHigh;
                        qrCodeObject.GetComponent<QRCode>().obstWide = obstWide;


                    }
                    else if (action.type == ActionData.Type.Updated)
                    {
                        if (!qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                            qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                            qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                            qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);

                            //AROA EDIT
                            Debug.Log("Action.qrCode.Data = " + action.qrCode.Data);
                            qrCodeObject.GetComponent<QRCode>().textToSpeech = textToSpeech;
                            qrCodeObject.GetComponent<QRCode>().obstacleManager = obstacleManager;

                            if (action.qrCode.Data == "QR Code 1")
                            {
                                layout = "Layout 1";
                            }

                            else if (action.qrCode.Data == "QR Code 2")
                            {
                                layout = "Layout 2";                            
                            }

                            else if (action.qrCode.Data == "QR Code 3")
                            {
                                layout = "Layout 3";
                            }

                            else if (action.qrCode.Data == "QR Code 4")
                            {
                                layout = "Layout 4";             
                            }

                            else if (action.qrCode.Data == "QR Code 5")
                            {
                                layout = "Layout 5";
                            }

                            else if (action.qrCode.Data == "QR Code 6")
                            {
                                layout = "Layout 6";
                            }

                            else if (action.qrCode.Data == "Demo")
                            {
                                layout = "Demo Layout";
                            }

                            qrCodeObject.GetComponent<QRCode>().layout = layout;
                            qrCodeObject.GetComponent<QRCode>().trackedObject = obstacleCollection;
                            qrCodeObject.GetComponent<QRCode>().experimentLogger = experimentLogger;

                            //Assign obstacles to QR Code object
                            qrCodeObject.GetComponent<QRCode>().obstLow = obstLow;
                            qrCodeObject.GetComponent<QRCode>().obstMid = obstMid;
                            qrCodeObject.GetComponent<QRCode>().obstHigh = obstHigh;
                            qrCodeObject.GetComponent<QRCode>().obstWide = obstWide;

                        }
                    }
                    else if (action.type == ActionData.Type.Removed)
                    {
                        if (qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            Destroy(qrCodesObjectsList[action.qrCode.Id]);
                            qrCodesObjectsList.Remove(action.qrCode.Id);
                        }
                    }
                }
            }
            if (clearExisting)
            {
                clearExisting = false;
                foreach (var obj in qrCodesObjectsList)
                {
                    Destroy(obj.Value);
                }
                qrCodesObjectsList.Clear();

            }
        }

        // Update is called once per frame
        void Update()
        {
            HandleEvents();
        }
    }

}