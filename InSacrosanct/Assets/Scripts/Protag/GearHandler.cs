using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GearHandler : MonoBehaviour
{
    private enum GearState
    {
        Default,
        Cycling
    }

    [Header("Dependencies")]

    [SerializeField]
    private Transform _gearParent;

    [FormerlySerializedAs("CycleInterval")]
    [Header("Config")]

    [SerializeField]
    private float _fastestCycleInterval;

    [SerializeField]
    private float _slowestCycleInterval;

    [SerializeField]
    private float _windDownDuration;

    [SerializeField]
    private AnimationCurve _cycleIntervalFalloff;

    public bool IsUsing => _currentGear != null && _currentGear.IsUsing();

    private readonly List<GearBase> _gears = new();

    private int _cycleIndex;

    private GearBase _currentGear;

    private Protag _protag;
    private InputProvider _inputProvider;

    private GearState _gearState = GearState.Default;

    // Cycling
    private float _cyclingTimer;

    private float _intervalTimer;
    private float _intervalDuration;

    public void Initialize(Protag protag, InputProvider inputProvider, List<GearBase> gearPrefabs)
    {
        _protag = protag;
        _inputProvider = inputProvider;

        foreach (GearBase gearPrefab in gearPrefabs)
        {
            GearBase gear = Instantiate(gearPrefab, _gearParent);
            gear.Initialize(inputProvider, protag);
            _gears.Add(gear);
            gear.Show(false);
        }

        if (_gears.Count > 0)
        {
            EquipGear(_gears[0]);
        }
    }

    public void Tick()
    {
        if (_gearState == GearState.Default)
        {
            if (_currentGear != null)
            {
                _currentGear.Tick();
            }

            if (Input.GetMouseButtonDown(1) && _gears.Count > 1 && !IsUsing)
            {
                _currentGear.OnUnequip();
                // _gearState = GearState.Cycling;
                _cyclingTimer = 0;
                _intervalTimer = 0;
                _intervalDuration = _fastestCycleInterval;
                if (_currentGear)
                {
                    _cycleIndex = (_gears.IndexOf(_currentGear) + 1) % _gears.Count;
                }
                else
                {
                    _cycleIndex = 0;
                }

                EquipGear(_gears[_cycleIndex]);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) && _gears.Count > 0)
            {
                EquipGear(_gears[0]);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) && _gears.Count > 1)
            {
                EquipGear(_gears[1]);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) && _gears.Count > 2)
            {
                EquipGear(_gears[2]);
            }
        }
        else
        {
            _cyclingTimer += Time.deltaTime;
            _intervalTimer += Time.deltaTime;

            // Cycle
            if (_intervalTimer > _intervalDuration)
            {
                _intervalTimer -= _intervalDuration;

                _intervalDuration = Mathf.Lerp(_fastestCycleInterval, _slowestCycleInterval,
                    _cycleIntervalFalloff.Evaluate(_cyclingTimer / _windDownDuration));

                _cycleIndex = (_cycleIndex + 1) % _gears.Count;
                SetSingleVisible(_cycleIndex);
            }

            if (Input.GetMouseButtonDown(1))
            {
                _gearState = GearState.Default;
                EquipGear(_gears[_cycleIndex]);
            }
        }
    }

    private void SetSingleVisible(int index)
    {
        for (var i = 0; i < _gears.Count; i++)
        {
            _gears[i].Show(index == i);
        }
    }

    private void EquipGear(GearBase gear)
    {
        if (_currentGear)
        {
            _currentGear.OnUnequip();
        }

        SetSingleVisible(_gears.IndexOf(gear));
        _currentGear = gear;
        _currentGear.OnEquip();
    }

    public void UnequipGear()
    {
        if (_currentGear)
        {
            _currentGear.OnUnequip();
            _currentGear = null;
        }
    }
}