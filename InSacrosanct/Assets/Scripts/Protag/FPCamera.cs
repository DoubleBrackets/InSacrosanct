using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class FpCamera : MonoBehaviour
{
    [Serializable]
    public struct CameraSettings
    {
        [FormerlySerializedAs("sensitivity")]
        [Header("Look")]
        public float Sensitivity;

        [FormerlySerializedAs("minTiltDegrees")]
        public float MinTiltDegrees;

        [FormerlySerializedAs("maxTiltDegrees")]
        public float MaxTiltDegrees;
    }

    public static float Sensitivity = 20f;

    [Header("Dependencies")]

    [SerializeField]
    private Transform _cameraTransform;

    [SerializeField]
    private CinemachineCamera _cinemachineCamera;

    [Header("Cam Settings")]

    [SerializeField]
    private CameraSettings _cameraSettings;

    public Vector3 CameraForward => _cameraTransform.forward;
    public Vector3 CameraRight => _cameraTransform.right;
    public Vector3 Position => _cameraTransform.position;

    private float _xRotation;
    private float _yRotation;

    private float _desiredNormalizedFOV;

    private float _desiredDutchTilt;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void TickCamera(Vector2 lookInput)
    {
        Look(lookInput);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Sensitivity -= 2.5f;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Sensitivity += 2.5f;
        }

        Sensitivity = Mathf.Clamp(Sensitivity, 0f, 1000f);
    }

    public Vector3 TransformDirection(Vector3 vector)
    {
        return _cameraTransform.TransformDirection(vector);
    }

    private void Look(Vector2 lookInput)
    {
        float dTime = Mathf.Clamp(Time.deltaTime, 0, 0.05f);
        float mouseX = lookInput.x * Sensitivity * dTime;
        float mouseY = lookInput.y * Sensitivity * dTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, _cameraSettings.MinTiltDegrees, _cameraSettings.MaxTiltDegrees);

        _yRotation += mouseX;

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
    }
}