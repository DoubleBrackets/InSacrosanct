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
    }
    
    public Vector3 TransformDirection(Vector3 vector)
    {
        return _cameraTransform.TransformDirection(vector);
    }
    
    
    private void Look(Vector2 lookInput)
    {

        float mouseX = lookInput.x * _cameraSettings.Sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * _cameraSettings.Sensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, _cameraSettings.MinTiltDegrees, _cameraSettings.MaxTiltDegrees);

        _yRotation += mouseX;

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
    }
    

    
}