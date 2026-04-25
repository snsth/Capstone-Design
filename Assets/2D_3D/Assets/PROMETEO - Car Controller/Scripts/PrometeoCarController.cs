using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrometeoCarController : MonoBehaviour
{
    [Range(20, 190)] public int maxSpeed = 90;
    [Range(10, 120)] public int maxReverseSpeed = 45;
    [Range(1, 10)] public int accelerationMultiplier = 2;
    [Range(10, 45)] public int maxSteeringAngle = 27;
    [Range(0.1f, 1f)] public float steeringSpeed = 0.5f;
    [Range(100, 600)] public int brakeForce = 350;
    [Range(1, 10)] public int decelerationMultiplier = 2;
    [Range(1, 10)] public int handbrakeDriftMultiplier = 5;
    public Vector3 bodyMassCenter;

    public GameObject frontLeftMesh; public WheelCollider frontLeftCollider;
    public GameObject frontRightMesh; public WheelCollider frontRightCollider;
    public GameObject rearLeftMesh; public WheelCollider rearLeftCollider;
    public GameObject rearRightMesh; public WheelCollider rearRightCollider;

    public bool useEffects = false;
    public ParticleSystem RLWParticleSystem, RRWParticleSystem;
    public TrailRenderer RLWTireSkid, RRWTireSkid;

    public bool useUI = false;
    public Text carSpeedText;

    public bool useSounds = false;
    public AudioSource carEngineSound;
    public AudioSource tireScreechSound;
    float initialCarEngineSoundPitch;

    [HideInInspector] public float carSpeed;
    [HideInInspector] public bool isDrifting;
    [HideInInspector] public bool isTractionLocked;
    [HideInInspector] public bool brakeLockedOnly = false;

    Rigidbody carRigidbody;
    float steeringAxis;
    float throttleAxis;
    float driftingAxis;
    float localVelocityZ, localVelocityX;
    bool deceleratingCar;

    WheelFrictionCurve FLwheelFriction; float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction; float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction; float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction; float RRWextremumSlip;

    [Header("Minigame Override")]
    [Tooltip("�̴ϰ��� ��: �Է� ���� + �극��ũ �Ҵ� + ���� ����")]
    public bool minigameOverride = false;
    public int minigameMaxSpeed = 200;
    public float minigameTorqueMulMax = 1.5f;
    int originalMaxSpeed;
    float currentOverrideTorqueMul = 1f;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        FLwheelFriction = new WheelFrictionCurve();
        FLwheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;

        FRwheelFriction = new WheelFrictionCurve();
        FRwheelFriction.extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRWextremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = frontRightCollider.sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = frontRightCollider.sidewaysFriction.stiffness;

        RLwheelFriction = new WheelFrictionCurve();
        RLwheelFriction.extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLWextremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = rearLeftCollider.sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = rearLeftCollider.sidewaysFriction.stiffness;

        RRwheelFriction = new WheelFrictionCurve();
        RRwheelFriction.extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRWextremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = rearRightCollider.sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = rearRightCollider.sidewaysFriction.stiffness;

        if (carEngineSound != null) initialCarEngineSoundPitch = carEngineSound.pitch;

        if (useUI) InvokeRepeating(nameof(CarSpeedUI), 0f, 0.1f);
        else if (carSpeedText != null) carSpeedText.text = "0";

        if (useSounds) InvokeRepeating(nameof(CarSounds), 0f, 0.1f);
        else
        {
            if (carEngineSound != null) carEngineSound.Stop();
            if (tireScreechSound != null) tireScreechSound.Stop();
        }

        if (!useEffects)
        {
            if (RLWParticleSystem != null) RLWParticleSystem.Stop();
            if (RRWParticleSystem != null) RRWParticleSystem.Stop();
            if (RLWTireSkid != null) RLWTireSkid.emitting = false;
            if (RRWTireSkid != null) RRWTireSkid.emitting = false;
        }

        originalMaxSpeed = maxSpeed;
    }

    public void EnableMinigameOverride(bool enabled)
    {
        if (enabled)
        {
            minigameOverride = true;
            originalMaxSpeed = maxSpeed;
            maxSpeed = minigameMaxSpeed;
            currentOverrideTorqueMul = 1f;
            CancelInvoke(nameof(DecelerateCar));
            deceleratingCar = false;
            RecoverTraction();
        }
        else
        {
            minigameOverride = false;
            maxSpeed = originalMaxSpeed;
            currentOverrideTorqueMul = 1f;
            RecoverTraction();
            ThrottleOff();
            CancelInvoke(nameof(DecelerateCar));
            deceleratingCar = false;
        }
    }

    void Update()
    {
        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

        // �̴ϰ��� �������̵�: �극��ũ �Ҵ� + ���� ����(���� ��ƾ ���)
        if (minigameOverride)
        {
            // �극��ũ ���� 0
            frontLeftCollider.brakeTorque = 0;
            frontRightCollider.brakeTorque = 0;
            rearLeftCollider.brakeTorque = 0;
            rearRightCollider.brakeTorque = 0;

            // �׸� ����(�ڵ�극��ũ ���� ����)
            RecoverTraction();

            // ��ũ ���� ������ ���� �� ���� ����(�극��ũ ȣ�� ����)
            currentOverrideTorqueMul = Mathf.MoveTowards(currentOverrideTorqueMul, minigameTorqueMulMax, Time.deltaTime * 0.75f);
            ForceAccelerateNoBrakes(currentOverrideTorqueMul);

            // ���⸸ ���
            if (Input.GetKey(KeyCode.A)) TurnLeft();
            if (Input.GetKey(KeyCode.D)) TurnRight();
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && steeringAxis != 0f)
                ResetSteeringAngle();

            AnimateWheelMeshes();
            return;
        }

        // �Ϲ� �Է�
        if (Input.GetKey(KeyCode.W))
        {
            CancelInvoke(nameof(DecelerateCar));
            deceleratingCar = false;
            GoForward();
        }
        if (Input.GetKey(KeyCode.S))
        {
            CancelInvoke(nameof(DecelerateCar));
            deceleratingCar = false;
            GoReverse();
        }
        if (Input.GetKey(KeyCode.A)) TurnLeft();
        if (Input.GetKey(KeyCode.D)) TurnRight();

        if (Input.GetKey(KeyCode.Space) && !brakeLockedOnly)
        {
            CancelInvoke(nameof(DecelerateCar));
            deceleratingCar = false;
            Handbrake();
        }
        if (Input.GetKeyUp(KeyCode.Space)) RecoverTraction();

        if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W)) ThrottleOff();

        if ((!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W)) && !Input.GetKey(KeyCode.Space) && !deceleratingCar)
        {
            InvokeRepeating(nameof(DecelerateCar), 0f, 0.1f);
            deceleratingCar = true;
        }
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && steeringAxis != 0f) ResetSteeringAngle();

        AnimateWheelMeshes();
    }

    public void CarSpeedUI()
    {
        if (!useUI) return;
        try
        {
            carSpeedText.text = Mathf.RoundToInt(Mathf.Abs(carSpeed)).ToString();
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public void CarSounds()
    {
        if (!useSounds)
        {
            if (carEngineSound != null && carEngineSound.isPlaying) carEngineSound.Stop();
            if (tireScreechSound != null && tireScreechSound.isPlaying) tireScreechSound.Stop();
            return;
        }

        try
        {
            if (carEngineSound != null)
            {
                float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.linearVelocity.magnitude) / 25f);
                carEngineSound.pitch = engineSoundPitch;
            }

            // null ���� �߰�
            if (tireScreechSound != null)
            {
                if (isDrifting || (isTractionLocked && Mathf.Abs(carSpeed) > 12f))
                {
                    if (!tireScreechSound.isPlaying) tireScreechSound.Play();
                }
                else
                {
                    if (tireScreechSound.isPlaying) tireScreechSound.Stop();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public void TurnLeft()
    {
        steeringAxis -= Time.deltaTime * 10f * steeringSpeed;
        if (steeringAxis < -1f) steeringAxis = -1f;
        var angle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, angle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, angle, steeringSpeed);
    }
    public void TurnRight()
    {
        steeringAxis += Time.deltaTime * 10f * steeringSpeed;
        if (steeringAxis > 1f) steeringAxis = 1f;
        var angle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, angle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, angle, steeringSpeed);
    }
    public void ResetSteeringAngle()
    {
        if (steeringAxis < 0f) steeringAxis += Time.deltaTime * 10f * steeringSpeed;
        else if (steeringAxis > 0f) steeringAxis -= Time.deltaTime * 10f * steeringSpeed;
        if (Mathf.Abs(frontLeftCollider.steerAngle) < 1f) steeringAxis = 0f;
        var angle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, angle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, angle, steeringSpeed);
    }

    void AnimateWheelMeshes()
    {
        try
        {
            Quaternion rot; Vector3 pos;
            if (frontLeftCollider != null && frontLeftMesh != null)
            {
                frontLeftCollider.GetWorldPose(out pos, out rot);
                frontLeftMesh.transform.SetPositionAndRotation(pos, rot);
            }
            if (frontRightCollider != null && frontRightMesh != null)
            {
                frontRightCollider.GetWorldPose(out pos, out rot);
                frontRightMesh.transform.SetPositionAndRotation(pos, rot);
            }
            if (rearLeftCollider != null && rearLeftMesh != null)
            {
                rearLeftCollider.GetWorldPose(out pos, out rot);
                rearLeftMesh.transform.SetPositionAndRotation(pos, rot);
            }
            if (rearRightCollider != null && rearRightMesh != null)
            {
                rearRightCollider.GetWorldPose(out pos, out rot);
                rearRightMesh.transform.SetPositionAndRotation(pos, rot);
            }
        }
        catch (Exception ex) { Debug.LogWarning(ex); }
    }

    public void GoForward()
    {
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f; DriftCarPS();
        throttleAxis += Time.deltaTime * 3f;
        if (throttleAxis > 1f) throttleAxis = 1f;

        if (localVelocityZ < -1f) { Brakes(); }
        else
        {
            if (Mathf.RoundToInt(carSpeed) < maxSpeed)
            {
                ApplyMotorTorqueToAll((accelerationMultiplier * 50f) * throttleAxis);
            }
            else
            {
                ApplyMotorTorqueToAll(0f);
            }
        }
    }

    public void GoReverse()
    {
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f; DriftCarPS();
        throttleAxis -= Time.deltaTime * 3f;
        if (throttleAxis < -1f) throttleAxis = -1f;

        if (localVelocityZ > 1f) { Brakes(); }
        else
        {
            if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
            {
                ApplyMotorTorqueToAll((accelerationMultiplier * 50f) * throttleAxis);
            }
            else
            {
                ApplyMotorTorqueToAll(0f);
            }
        }
    }

    // �̴ϰ��ӿ�: �극��ũ ���� ���� ����(�극��ũ ���� ���� �� ��)
    void ForceAccelerateNoBrakes(float torqueMul)
    {
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f; DriftCarPS();

        throttleAxis += Time.deltaTime * 3f;
        if (throttleAxis > 1f) throttleAxis = 1f;

        frontLeftCollider.brakeTorque = 0;
        frontRightCollider.brakeTorque = 0;
        rearLeftCollider.brakeTorque = 0;
        rearRightCollider.brakeTorque = 0;

        if (Mathf.RoundToInt(carSpeed) < maxSpeed)
        {
            float torque = (accelerationMultiplier * 50f) * throttleAxis * torqueMul;
            ApplyMotorTorqueToAll(torque);
        }
        else
        {
            ApplyMotorTorqueToAll(0f);
        }
    }

    void ApplyMotorTorqueToAll(float torque)
    {
        frontLeftCollider.brakeTorque = 0; frontLeftCollider.motorTorque = torque;
        frontRightCollider.brakeTorque = 0; frontRightCollider.motorTorque = torque;
        rearLeftCollider.brakeTorque = 0; rearLeftCollider.motorTorque = torque;
        rearRightCollider.brakeTorque = 0; rearRightCollider.motorTorque = torque;
    }

    public void ThrottleOff()
    {
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
    }

    public void DecelerateCar()
    {
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f; DriftCarPS();

        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f) throttleAxis -= Time.deltaTime * 10f;
            else throttleAxis += Time.deltaTime * 10f;
            if (Mathf.Abs(throttleAxis) < 0.15f) throttleAxis = 0f;
        }

        carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.025f * decelerationMultiplier)));
        ThrottleOff();

        if (carRigidbody.linearVelocity.magnitude < 0.25f)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            CancelInvoke(nameof(DecelerateCar));
        }
    }

        public void Brakes()
        {
            if (brakeLockedOnly)
            {
            frontLeftCollider.brakeTorque = 0;
            frontRightCollider.brakeTorque = 0;
            rearLeftCollider.brakeTorque = 0;
            rearRightCollider.brakeTorque = 0;
            return;
            }
        frontLeftCollider.brakeTorque = brakeForce;
        frontRightCollider.brakeTorque = brakeForce;
        rearLeftCollider.brakeTorque = brakeForce;
        rearRightCollider.brakeTorque = brakeForce;
        }

        public void Handbrake()
        {
        if (brakeLockedOnly) return;

        CancelInvoke(nameof(RecoverTraction));
        driftingAxis += Time.deltaTime;
        float secureStartingPoint = driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;

        if (secureStartingPoint < FLWextremumSlip)
        {
            driftingAxis = FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier);
        }
        if (driftingAxis > 1f) driftingAxis = 1f;

        isDrifting = Mathf.Abs(localVelocityX) > 2.5f;

        if (driftingAxis < 1f)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis; frontLeftCollider.sidewaysFriction = FLwheelFriction;
            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis; frontRightCollider.sidewaysFriction = FRwheelFriction;
            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis; rearLeftCollider.sidewaysFriction = RLwheelFriction;
            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis; rearRightCollider.sidewaysFriction = RRwheelFriction;
        }

        isTractionLocked = true;
        DriftCarPS();
        }

    public void DriftCarPS()
    {
        if (!useEffects)
        {
            if (RLWParticleSystem != null) RLWParticleSystem.Stop();
            if (RRWParticleSystem != null) RRWParticleSystem.Stop();
            if (RLWTireSkid != null) RLWTireSkid.emitting = false;
            if (RRWTireSkid != null) RRWTireSkid.emitting = false;
            return;
        }

        try
        {
            if (RLWParticleSystem != null && RRWParticleSystem != null)
            {
                if (isDrifting)
                {
                    if (!RLWParticleSystem.isPlaying) RLWParticleSystem.Play();
                    if (!RRWParticleSystem.isPlaying) RRWParticleSystem.Play();
                }
                else
                {
                    RLWParticleSystem.Stop(); RRWParticleSystem.Stop();
                }
            }
        }
        catch (Exception ex) { Debug.LogWarning(ex); }

        try
        {
            if (RLWTireSkid != null && RRWTireSkid != null)
            {
                if ((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f)
                {
                    RLWTireSkid.emitting = true; RRWTireSkid.emitting = true;
                }
                else
                {
                    RLWTireSkid.emitting = false; RRWTireSkid.emitting = false;
                }
            }
        }
        catch (Exception ex) { Debug.LogWarning(ex); }
    }

    public void RecoverTraction()
    {
        isTractionLocked = false;
        driftingAxis -= Time.deltaTime / 1.5f;
        if (driftingAxis < 0f) driftingAxis = 0f;

        if (FLwheelFriction.extremumSlip > FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis; frontLeftCollider.sidewaysFriction = FLwheelFriction;
            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis; frontRightCollider.sidewaysFriction = FRwheelFriction;
            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis; rearLeftCollider.sidewaysFriction = RLwheelFriction;
            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis; rearRightCollider.sidewaysFriction = RRwheelFriction;

            Invoke(nameof(RecoverTraction), Time.deltaTime);
        }
        else if (FLwheelFriction.extremumSlip < FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip; frontLeftCollider.sidewaysFriction = FLwheelFriction;
            FRwheelFriction.extremumSlip = FRWextremumSlip; frontRightCollider.sidewaysFriction = FRwheelFriction;
            RLwheelFriction.extremumSlip = RLWextremumSlip; rearLeftCollider.sidewaysFriction = RLwheelFriction;
            RRwheelFriction.extremumSlip = RRWextremumSlip; rearRightCollider.sidewaysFriction = RRwheelFriction;
            driftingAxis = 0f;
        }
    }
}