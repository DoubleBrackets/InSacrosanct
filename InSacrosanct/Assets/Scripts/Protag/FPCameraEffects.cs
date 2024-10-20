using System;
using Unity.Cinemachine;
using UnityEngine;

public class FPCameraEffects : MonoBehaviour
{
    [Serializable]
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

    [SerializeField]
    private CinemachineImpulseSource _wormImpulse;

    [SerializeField]
    private AnimationCurve _wormImpulseCurve;

    [Header("Settings")]

    [SerializeField]
    private CameraEffectSettings _settings;

    private float _desiredDutchTilt;

    private float _desiredNormalizedFOV;

    private bool _wallRideImpulseActive;
    private float _wallRideImpulseTimer;

    private bool _wormImpulseActive;
    private float _wormImpulseMagnitude;
    private float _wormImpulseTimer;

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

        _wormImpulseTimer -= Time.deltaTime;
        if (_wormImpulseActive && _wormImpulseTimer <= 0)
        {
            _wormImpulseTimer = _wormImpulse.ImpulseDefinition.ImpulseDuration;
            float strength = _wormImpulseCurve.Evaluate(_wormImpulseMagnitude);
            _wormImpulse.GenerateImpulse(strength);
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

    public void SetWormImpulse(float strength)
    {
        _wormImpulseMagnitude = strength;
        _wormImpulseActive = strength > 0;
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