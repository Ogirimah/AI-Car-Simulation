using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
/////////////////////////////
// MINE
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

    private Rigidbody carBody;
    public float carVelocity, maxCarVelocity;
    public int angle_x, angle_z;
    public int engineForce, maxEngineForce, gearEngineForce, breakForce, forceChangeValue;
    public bool isBreaking;
    public int steerAngle;
    private int maxSteerAngle;

    public int sorX = 50, sorY = 180;

    //Sensing distance for all sensors
    private float s1D, s2D, s3D, sorD, sfrD;

    int trackMask, obstacleMask;

    void Start()
    {
        carBody = GetComponent<Rigidbody>();

        //x, y, and z rotation angles of the sensors
        int s1X = 0, s2X = 16, s3X = 18, sfrX = 5;
        int s1Y = 10, s2Y = 40, s3Y = 70, sfrY = 0;
        int s1Z = 0, s2Z = 0, s3Z = 0, sorZ = 0, sfrZ = 0;

        trackMask = 1 << 9;
        obstacleMask = 1 << 10;

        //Sensing distance for all sensors
        s1D = 5; s2D = 8f; s3D = 7; sorD = 3; sfrD = 15;

        AdjustSensors(SFR, sfrX, sfrY, sfrZ);
        AdjustSensors(SL1, s1X, -s1Y, s1Z);
        AdjustSensors(SR1, s1X, s1Y, s1Z);
        AdjustSensors(SL2, s2X, -s2Y, s2Z);
        AdjustSensors(SR2, s2X, s2Y, s2Z);
        AdjustSensors(SL3, s3X, -s3Y, s3Z);
        AdjustSensors(SR3, s3X, s3Y, s3Z);
        AdjustSensors(SOR, sorX, sorY, sorZ);
    }

    private void FixedUpdate()
    {
        carVelocity = carBody.velocity.magnitude;
        angle_x = (int)SOR.eulerAngles.x;
        angle_z = (int)SOR.eulerAngles.z;

        UpdateWheels();

        
        Steer();

        DriveMode();
        EngineForceControl();
        MotorControl();
        DoNotExceedMaxVelocity();
    }

    private void LateUpdate()
    {
        StayOnTheRoad();
    }

    private void Drive1()
    {
        maxSteerAngle = 20;
        gearEngineForce = 500;
        forceChangeValue = 10;
        maxCarVelocity = 0.5f;
    }

    private void Drive2()
    {
        maxSteerAngle = 15;
        gearEngineForce = 300;
        forceChangeValue = 5;
        maxCarVelocity = 2;
    }

    private void Drive3()
    {
        maxSteerAngle = 6;
        gearEngineForce = 50;
        forceChangeValue = 1;
        maxCarVelocity = 4;
    }

    private void EngineForceControl()
    {
        if (carVelocity < maxCarVelocity & engineForce < gearEngineForce)
        {
            engineForce = engineForce + forceChangeValue;
        }
        else if (carVelocity > maxCarVelocity & engineForce > gearEngineForce)
        {
            engineForce = engineForce - forceChangeValue;
        }
    }

    private void DoNotExceedMaxVelocity()
    {
        if (carVelocity > maxCarVelocity)
        {
            isBreaking = true;
        }
        else
        {
            isBreaking = false;
        }
    }

    private void StayOnTheRoad()
    {
        if (Sense(SL1, obstacleMask, s1D) || Sense(SR1, obstacleMask, s1D))
        {
            AvoidObstacles();
        }
        else if (!Sense(SL2, trackMask, s2D, 1) || !Sense(SR2, trackMask, s2D, -1) || !Sense(SL3, trackMask, s3D, 1) || !Sense(SR3, trackMask, s3D, -1))
        {
            if (!Sense(SL2, trackMask, s2D, 1) & !Sense(SR2, trackMask, s2D, -1))
            {
                steerAngle = 0;
            }
            else if (!Sense(SL3, trackMask, s3D, 1) || !Sense(SR3, trackMask, s3D, -1))
            {
                if (!Sense(SL3, trackMask, s3D, 1))
                {
                    steerAngle = 3 * maxSteerAngle / 2;
                }
                else
                {
                    steerAngle = -3 * maxSteerAngle / 2;
                }
            }
            else if (!Sense(SL2, trackMask, s2D, 1) || !Sense(SR2, trackMask, s2D, -1))
            {
                if (!Sense(SL2, trackMask, s2D, 1))
                {
                    steerAngle = maxSteerAngle / 2;
                }
                else
                {
                    steerAngle = -maxSteerAngle / 2;
                }
            }
        }
        else
        {
            steerAngle = 0;
        }
    }

    private void MotorControl()
    {
        FLWC.motorTorque = engineForce;
        FRWC.motorTorque = engineForce;
        BLWC.motorTorque = engineForce;
        BRWC.motorTorque = engineForce;
        breakForce = isBreaking ? 5000 : 0;
        FLWC.brakeTorque = breakForce;
        FRWC.brakeTorque = breakForce;
        BLWC.brakeTorque = breakForce;
        BRWC.brakeTorque = breakForce;
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

    private void AdjustSensors(Transform sensor, int angle_x, int angle_y, int angle_z)
    {
        sensor.transform.Rotate(angle_x, angle_y, angle_z);
    }

    private bool Sense(Transform sensor, int layerMask, params float[] distanceAndDirection)
    {
        float dist = distanceAndDirection[0];
        Nullable<bool> sensing = null;
        RaycastHit hit;

        //for front sensors and orientation sensor
        if (distanceAndDirection.Length == 1)
        {
            if (Physics.Raycast(sensor.position,
                sensor.TransformDirection(Vector3.forward), out hit, dist, layerMask))
            {
                Debug.DrawRay(sensor.position,
                    sensor.TransformDirection(Vector3.forward) * hit.distance, Color.white);
                sensing = true;
            }
            else
            {
                Debug.DrawRay(sensor.position,
                    sensor.TransformDirection(Vector3.forward) * dist, Color.yellow);
                sensing = false;
            }
        }
        //for side sensors to keep the car on track
        else if (distanceAndDirection.Length == 2)
        {
            int dir = (int)distanceAndDirection[1];

            //Change sensor angle before sensing
            Vector3 sensorDirection = sensor.TransformDirection(Vector3.forward);
            Quaternion changeAngle = Quaternion.AngleAxis(dir * BalanceSensors(), new Vector3(1, 0, 0));
            Vector3 rayDirection = changeAngle * sensorDirection;

            if (Physics.Raycast(sensor.position,
                rayDirection, out hit, dist, layerMask))
            {
                Debug.DrawRay(sensor.position,
                    rayDirection * hit.distance, Color.white);
                sensing = true;
            }
            else
            {
                Debug.DrawRay(sensor.position,
                    rayDirection * dist, Color.yellow);
                sensing = false;
            }
        }
        return (bool)sensing;
    }

    //Returns the difference in the x-axis angle caused by the swaying of the car in the z-axis
    private int BalanceSensors()
    {
        int angleCorrection = 0;
        if (angle_z < 180)
        {
            angleCorrection = angle_z;
        }
        else if (angle_z > 180 & angle_z > 0)
        {
            angleCorrection = (360 - angle_z);
        }
        else if (angle_z == 0)
        {
            angleCorrection = 0;
        }
        return angleCorrection;
    }

    private void Steer()
    {
        FLWC.steerAngle = steerAngle;
        FRWC.steerAngle = steerAngle;
    }

    private void DriveMode()
    {
        if (!Sense(SFR, trackMask, sfrD) || Sense(SL1, obstacleMask, s1D) || Sense(SR1, obstacleMask, s1D) || Sense(SFR, obstacleMask, sfrD))
        {
            Drive1();
        }
        else if (angle_x < (sorX - 2) || angle_x > (sorX + 2))
        {
            Drive2();
        }
        else
        {
            Drive3();
        }
    }

    private void AvoidObstacles()
    {
        if (Sense(SL1, obstacleMask, s1D))
        {
            steerAngle = maxSteerAngle * 3;
        }
        else
        {
            steerAngle = -maxSteerAngle * 3;
        }
    }
}
