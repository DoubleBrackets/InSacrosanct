using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class FPCameraEffects : MonoBehaviour
{
    [System.Serializable]
    private struct CameraEffectSettings
    {
        [Header("Dynamic FOV")]
        public float MinFOV;
        public float MaxFOV;
        public AnimationCurve FOVCurve;
        public float FOVLerpSpeed;
        
        [Header("Dutch Tilt")]
        public float MaxDutchTilt;
        public float DutchTiltLerpSpeed;
    }
    
    [Header("Dependencies")]
        
    [SerializeField]
    private CinemachineCamera _cinemachineCamera;
        
    [SerializeField]
    private CinemachineRecomposer _cinemachineRecomposer;

    [Header("Impulse Sources")]

    [SerializeField]
    private CinemachineImpulseSource _wallrideImpulse;
    
    [Header("Settings")]
    [SerializeField]
    private CameraEffectSettings _settings;
    
    private float _desiredDutchTilt;
    
    private float _desiredNormalizedFOV;
    
    private bool _wallRideImpulseActive;
    
    private float _wallRideImpulseTimer = 0;

    public void Tick()
    {
        DynamicFOV();
        UpdateDutch();
        _wallRideImpulseTimer -= Time.deltaTime;
        if (_wallRideImpulseActive && _wallRideImpulseTimer <= 0)
        {
            _wallRideImpulseTimer = _wallrideImpulse.ImpulseDefinition.ImpulseDuration;
            _wallrideImpulse.GenerateImpulse();
        }
    }
    
    public void SetFOV(float normalizedFOV)
    {
        _desiredNormalizedFOV = normalizedFOV;
    }
    
    public void SetDutchTilt(float factor)
    {
        _desiredDutchTilt = _settings.MaxDutchTilt * factor;
    }
    
    public void SetWallRideImpulse(bool active)
    {
        _wallRideImpulseActive = active;
    }
    
    private void UpdateDutch()
    {
        float currentDutch = _cinemachineRecomposer.Dutch;
        
        float t = 1 - Mathf.Pow(0.01f, Time.deltaTime * _settings.DutchTiltLerpSpeed);
        
        _cinemachineRecomposer.Dutch = Mathf.Lerp(currentDutch, _desiredDutchTilt, t);
    }
    
    private void DynamicFOV()
    {
        float currentFOV = _cinemachineCamera.Lens.FieldOfView;
        float targetFOV = Mathf.Lerp(_settings.MinFOV, 
            _settings.MaxFOV, 
            _settings.FOVCurve.Evaluate(_desiredNormalizedFOV));
        
        float t = 1 - Mathf.Pow(0.01f, Time.deltaTime * _settings.FOVLerpSpeed);
        _cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(currentFOV, targetFOV, t);
    }

}
