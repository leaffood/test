using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Example Behaviour which applies the measured OpenZen sensor orientation to a 
 * Unity object.
 */
public class OpenZenMoveObject : MonoBehaviour
{
    ZenClientHandle_t mZenHandle = new ZenClientHandle_t();
    ZenSensorHandle_t mSensorHandle = new ZenSensorHandle_t();

    // Use this for initialization
    void Start()
    {
        // create OpenZen
        OpenZen.ZenInit(mZenHandle);

        // Hint: to get the io type and identifer for all connected sensor,
        // you cant start the DiscoverSensorScene. The information of all 
        // found sensors is printed in the debug console of Unity after
        // the search is complete.
        string ioType = "SiUsb";
        string identifier = "lpmscu2000573";

        var sensorInitError = OpenZen.ZenObtainSensorByName(mZenHandle,
            ioType,
            identifier,
            0,
            mSensorHandle);
        if (sensorInitError != ZenSensorInitError.ZenSensorInitError_None)
        {
            print("Error while connecting to sensor.");
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        // run as long as there are new OpenZen events to process
        while (true)
        {
            ZenEvent zenEvent = new ZenEvent();
            // read all events which are waiting for us
            // use the rotation from the newest IMU event
            if (!OpenZen.ZenPollNextEvent(mZenHandle, zenEvent))
                break;

            // if compontent handle = 0, this is a OpenZen wide event,
            // like sensor search
            if (zenEvent.component.handle != 0)
            {
                switch (zenEvent.eventType)
                {
                    case (int)ZenImuEvent.ZenImuEvent_Sample:
                        // read quaternion
                        OpenZenFloatArray fq = OpenZenFloatArray.frompointer(zenEvent.data.imuData.q);
                        // Unity Quaternion constructor has order x,y,z,w
                        // Furthermore, y and z axis need to be flipped to 
                        // convert between the LPMS and Unity coordinate system
                        Quaternion sensorOrientation = new Quaternion(fq.getitem(1),
                                                                    fq.getitem(3),
                                                                    fq.getitem(2),
                                                                    fq.getitem(0));
                        transform.rotation = sensorOrientation;
                        break;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (mSensorHandle != null)
        {
            OpenZen.ZenReleaseSensor(mZenHandle, mSensorHandle);
        }
        OpenZen.ZenShutdown(mZenHandle);
    }

}
