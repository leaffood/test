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

    public enum  OpenZenIoTypes { SiUsb, Bluetooth };

    [Tooltip("IO Type which OpenZen should use to connect to the sensor.")]
    public OpenZenIoTypes OpenZenIoType = OpenZenIoTypes.SiUsb;
    [Tooltip("Idenfier which is used to connect to the sensor. The name depends on the IO type used and the configuration of the sensor.")]
    public string OpenZenIdentifier = "lpmscu2000573";

    // Use this for initialization
    void Start()
    {
        // create OpenZen
        OpenZen.ZenInit(mZenHandle);

        // Hint: to get the io type and identifer for all connected sensor,
        // you cant start the DiscoverSensorScene. The information of all 
        // found sensors is printed in the debug console of Unity after
        // the search is complete.

        print("Trying to connect to OpenZen Sensor on IO " + OpenZenIoType +
            " with sensor name " + OpenZenIdentifier);

        var sensorInitError = OpenZen.ZenObtainSensorByName(mZenHandle,
            OpenZenIoType.ToString(),
            OpenZenIdentifier,
            0,
            mSensorHandle);
        if (sensorInitError != ZenSensorInitError.ZenSensorInitError_None)
        {
            print("Error while connecting to sensor.");
        } else {
            ZenComponentHandle_t mComponent = new ZenComponentHandle_t();
            OpenZen.ZenSensorComponentsByNumber(mZenHandle, mSensorHandle, OpenZen.g_zenSensorType_Imu, 0, mComponent);

            // enable sensor streaming, normally on by default anyways
            OpenZen.ZenSensorComponentSetBoolProperty(mZenHandle, mSensorHandle, mComponent,
               (int)EZenImuProperty.ZenImuProperty_StreamData, true);

            // set the sampling rate to 100 Hz
            OpenZen.ZenSensorComponentSetInt32Property(mZenHandle, mSensorHandle, mComponent,
               (int)EZenImuProperty.ZenImuProperty_SamplingRate, 100);

            // filter mode using accelerometer & gyroscope & magnetometer
            OpenZen.ZenSensorComponentSetInt32Property(mZenHandle, mSensorHandle, mComponent,
               (int)EZenImuProperty.ZenImuProperty_FilterMode, 2);

            // Ensure the Orientation data is streamed out
            OpenZen.ZenSensorComponentSetBoolProperty(mZenHandle, mSensorHandle, mComponent,
               (int)EZenImuProperty.ZenImuProperty_OutputQuat, true);

            print("Sensor configuration complete");
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
