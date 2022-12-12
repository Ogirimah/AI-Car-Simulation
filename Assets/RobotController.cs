using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
/////////////////////////////
// VERSION 1
////////////////////////////
{
    // naming constraints do not change
    [SerializeField] private WheelCollider FLWC;
    [SerializeField] private WheelCollider FRWC;
    [SerializeField] private WheelCollider BLWC;
    [SerializeField] private WheelCollider BRWC;

    [SerializeField] private Transform FLWT;
    [SerializeField] private Transform FRWT;
    [SerializeField] private Transform BLWT;
    [SerializeField] private Transform BRWT;

    [SerializeField] private Transform SFR;
    [SerializeField] private Transform SL1;
    [SerializeField] private Transform SL2;
    [SerializeField] private Transform SL3;
    [SerializeField] private Transform SR1;
    [SerializeField] private Transform SR2;
    [SerializeField] private Transform SR3;
    [SerializeField] private Transform SOR;

    [SerializeField] private float angle_x, angle_z, velocity;

    private float MAngle = 10f;
    [SerializeField] private float Mforce = f;
    [SerializeField] private float BForce = 0f;

    private Rigidbody rb;

    private float steerAngle;
    private bool isBreaking;

    void Start()
    {
        // x and y rotations of the sensors
        float S1RX = 0, S2RX = 10, S3RX = 12;
        float S1RY = 10, S2RY = 45, S3RY = 90;

        AdjustSensors(SL1, S1RX, -S1RY, 0);
        AdjustSensors(SL2, S2RX, -S2RY, 0);
        AdjustSensors(SL3, S3RX, -S3RY, 0);
        AdjustSensors(SR1, S1RX, S1RY, 0);
        AdjustSensors(SR2, S2RX, S2RY, 0);
        AdjustSensors(SR3, S3RX, S3RY, 0);

        //Orientation sensor at an angle -- to be able to know the orientation
        AdjustSensors(SOR, 50, 180, 0);

        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        HandleSteering();
        AdjustSpeed();
        HandleMotor();
        UpdateWheels();
        sense(SFR, 10);
        sense(SL1, 5);
        sense(SL2, 6);
        sense(SL3, 5);
        sense(SR1, 5);
        sense(SR2, 6);
        sense(SR3, 5.3f);
        sense(SOR, 3);

        velocity = rb.velocity.magnitude;
    }

    private void AdjustSensors(Transform sensor, float angle_x, float angle_y, float angle_z)
    {
        sensor.transform.Rotate(angle_x, angle_y, angle_z);
    }

    private void HandleSteering()
    {
        steerAngle = MAngle;
        if (sense(SL1, 5) || sense(SR1, 5))
        {
            if (sense(SL1, 5))
            {
                steerAngle = MAngle;
            }
            if (sense(SR1, 5))
            {
                steerAngle = -MAngle;
            }
        }
        else if (!sense(SL2, 6) || !sense(SR2, 6))
        {
            if (!sense(SL2, 6))
            {
                steerAngle = 2 * MAngle;
            }
            if (!sense(SR2, 6))
            {
                steerAngle = -2 * MAngle;
            }
        }
        else if (!sense(SL3, 5) || !sense(SR3, 5.3f))
        {
            if (!sense(SL3, 5))
            {
                steerAngle = 3 * MAngle;
            }
            if (!sense(SR3, 5.3f))
            {
                steerAngle = -3 * MAngle;
            }
        }
        else
        {
            isBreaking = false;
            steerAngle = 0;
        }
        FLWC.steerAngle = steerAngle;
        FRWC.steerAngle = steerAngle;
    }
    private void HandleMotor()
    {
        FLWC.motorTorque = Mforce;
        FRWC.motorTorque = Mforce;
        BLWC.motorTorque = Mforce;
        BRWC.motorTorque = Mforce;
        BForce = isBreaking ? 3000f : 0f;
        FLWC.brakeTorque = BForce;
        FRWC.brakeTorque = BForce;
        BLWC.brakeTorque = BForce;
        BRWC.brakeTorque = BForce;
    }
    private void AdjustSpeed()
    {
        if (velocity < 0.5 & Mforce < 10)
        {
            Mforce = 50f;
        }
        if (velocity < 2 & Mforce < 50)
        {
            Mforce = Mforce + 0.5f;
        }
        if (velocity > 4 & Mforce > 0)
        {
            Mforce = Mforce - 0.5f;
        }
    }
    private void UpdateWheelPos(WheelCollider wheelCollider, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        trans.rotation = rot;
        trans.position = pos;
    }
    private void UpdateWheels()
    {
        UpdateWheelPos(FLWC, FLWT);
        UpdateWheelPos(FRWC, FRWT);
        UpdateWheelPos(BLWC, BLWT);
        UpdateWheelPos(BRWC, BRWT);
    }
    private bool sense(Transform sensor, float dist)
    {
        RaycastHit hit;
        if (Physics.Raycast(sensor.position,
            sensor.TransformDirection(Vector3.forward), out hit, dist))
        {
            Debug.DrawRay(sensor.position,
                sensor.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            return true;
        }
        else
        {
            Debug.DrawRay(sensor.position,
                sensor.TransformDirection(Vector3.forward) * dist, Color.white);
            return false;
        }
    }
}
