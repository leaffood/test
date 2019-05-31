using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Example Behaviour which applies the measured OpenZen sensor orientation to a 
 * Unity object.
 */
public class MoveSensor : MonoBehaviour {

	// Use this for initialization
	void Start() {

        OpenZen.ZenInit(mZenHandle);
        OpenZen.ZenListSensorsAsync(mZenHandle);

        bool mSearchDone = false;
        List<ZenSensorDesc> mFoundSensors = new List<ZenSensorDesc>();

        print("Starting sensor search");
        while (mSearchDone == false)
        {
            ZenEvent zenEvent = new ZenEvent();
            if (OpenZen.ZenWaitForNextEvent(mZenHandle, zenEvent))
            {
                if (zenEvent.component.handle == 0)
                {
                    // if the handle is on, its not a sensor event but a system wide 
                    // event
                    switch (zenEvent.eventType)
                    {
                        case (int)ZenSensorEvent.ZenSensorEvent_SensorFound:
                            print("ZenSensorEvent_SensorFound, sensor name: " + zenEvent.data.sensorFound.name);
                            mFoundSensors.Add(zenEvent.data.sensorFound);
                            break;
                        case (int)ZenSensorEvent.ZenSensorEvent_SensorListingProgress:
                            if (zenEvent.data.sensorListingProgress.complete > 0)
                            {
                                mSearchDone = true;
                            }
                            break;
                    }
                }
            }
        }
        print("Sensor search complete");

        if (mFoundSensors.Count == 0)
        {
            print("No sensors to connect to found");
            return;
        }

        ZenSensorInitError sensorInitError = ZenSensorInitError.ZenSensorInitError_Max;
        // try three connection attempts
        for (int i = 0; i < 3; i++)
        {
            mSensorHandle = new ZenSensorHandle_t();
            // connect to the first available sensor in the list of found sensors
            sensorInitError = OpenZen.ZenObtainSensor(mZenHandle, mFoundSensors[0], mSensorHandle);
            if (sensorInitError == ZenSensorInitError.ZenSensorInitError_None)
            {
                print("Succesfully connected to sensor");
                break;
            }
        }

        if (sensorInitError != ZenSensorInitError.ZenSensorInitError_None)
        {
            mSensorHandle = null;
            print("Could not connect to sensor");
        }
    }

    // Update is called once per frame
    void Update () {
		if (mSensorHandle != null) {
            while (true)
            {
                ZenEvent zenEvent = new ZenEvent();
                // read all events which are waiting for us
                // use the rotation from the newest IMU event
                if (!OpenZen.ZenPollNextEvent(mZenHandle, zenEvent))
                    break;
                if (zenEvent.component.handle != 0) {
                    switch (zenEvent.eventType) {
                        case (int)ZenImuEvent.ZenImuEvent_Sample:
                            // read acceleration
                            OpenZenFloatArray fa = OpenZenFloatArray.frompointer(zenEvent.data.imuData.a);
                            // read euler angles
                            OpenZenFloatArray fr = OpenZenFloatArray.frompointer(zenEvent.data.imuData.r);
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
    }

    void OnDestroy()
    {
        OpenZen.ZenShutdown(mZenHandle);
    }

    Quaternion mInitialRotation;
    ZenClientHandle_t mZenHandle = new ZenClientHandle_t();
    ZenSensorHandle_t mSensorHandle;
}
