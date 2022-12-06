using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
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

    [SerializeField] public float MForce = 0f;
    [SerializeField] public float AForce = 10f;
    [SerializeField] private float MaxForce = 4000f;
    [SerializeField] public float MAngle = 10f;
    [SerializeField] public float BForce = 3000f;

    private float SteerAngle;


    private float S1RotY = 10;
    private float S2RotY = 45;
    private float S3RotY = 90;
    private float S2RotX = 10;
    private float S3RotX = 10;

    private void Start()
    {
        SL1.transform.Rotate(0, -S1RotY, 0);
        SL2.transform.Rotate(S2RotX, -S2RotY, 0);
        SL3.transform.Rotate(S3RotX, -S3RotY, 0);
        SR1.transform.Rotate(0, S1RotY, 0);
        SR2.transform.Rotate(S2RotX, S2RotY, 0);
        SR3.transform.Rotate(S3RotX, S3RotY, 0);

        StayOnTheRoad();
        HandleMotor();
        IncreaseMForce();
    }

    private void FixedUpdate()
    {
        Sense(SFR, 10);
        Sense(SL1, 5);
        Sense(SL2, 6);
        Sense(SL3, 5.5f);
        Sense(SR1, 5);
        Sense(SR2, 6);
        Sense(SR3, 5.5f);
        Sense(SOR, 1);
    }

    private bool Sense(Transform sensor, float dist)
    { 
        if (Physics.Raycast(sensor.position, sensor.TransformDirection(Vector3.forward), dist))
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.forward) * dist, Color.green);
            return true;
        }
        else
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.forward) * dist, Color.red);
            return false;
        }
    }

    private void UpdateWheelPosition(WheelCollider wheelCollider, Transform trans)
    {
        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion quat);
        trans.rotation = quat;
        trans.position = pos;
    }

    private void UpdateWheels()
    {
        UpdateWheelPosition(FLWC, FLWT);
        UpdateWheelPosition(FRWC, FRWT);
        UpdateWheelPosition(BLWC, BLWT);
        UpdateWheelPosition(BRWC, BRWT);
    }

    private void HandleMotor()
    {
        FLWC.motorTorque = MForce * Time.deltaTime;
        FRWC.motorTorque = MForce * Time.deltaTime;
        BLWC.motorTorque = MForce * Time.deltaTime;
        BRWC.motorTorque = MForce * Time.deltaTime;
    }

    private void IncreaseMForce()
    {
        MForce += AForce;
    }

    private void StayOnTheRoad()
    {
        if (Sense(SL3, 5.5f))
        {
            SteerAngle = MAngle;
        }
        if (Sense(SL2, 6))
        {
            SteerAngle = MAngle;
        }
        
        else
        {
            SteerAngle = 0f;
        }

        FLWC.steerAngle = SteerAngle;
        FRWC.steerAngle = SteerAngle;
    }
}

// SFR Detects the road directly in front, and it should be the longest. It will also only control the speed
// Once it detects a turn or object, it starts reducing speed until the SL1 or SR1 detects the turn or object and triggers the steering handle to evade it

// SL2 and SR2 will be used for curved bends, while SL3 and SR3 will keep the car in the middle of the road.
// Their lengths should decrease accordingly from SFR to 1 to 2 to 3.

// SORwould be used for climbing hills and coming down slopes

//Ask Luke if I can write a C# script for FollowPlayer so that I can attach it to the camera