using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FenzyRide3D.Scripts.Input;

namespace FenzyRide3D.Scripts.CarControlling
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(GearBox))]
    [RequireComponent(typeof(IAccelerate))]
    [RequireComponent(typeof(ISteering))]
    [RequireComponent(typeof(IBreaking))]
    [RequireComponent(typeof(IWheelsVisualUpdate))]
    public class CarControlling : MonoBehaviour
    {
        [Header("References:")]
        [SerializeField] public WheelCollider[] _wheelColliders;

        [Space(10)]

        [SerializeField] public Transform[] _wheelTransforms;

        [Space(10)]

        [SerializeField] private GameObject _centerOfMass;

        [Space(10)]

        private GearBox _gearBox;
        private IAccelerate _accelerator;
        private ISteering _steeringSystem;
        private IBreaking _breakingSystem;
        private IWheelsVisualUpdate _wheelsVisualUpdater;

        [Header("Stats:")]
        [SerializeField] private float _maxSteeringAngle;
        [SerializeField] public float _motorTorque;
        [SerializeField] private float _brakesTorque;

        [Header("Info:")]
        [SerializeField] private float _currentVerticalInput;
        [SerializeField] private float _currentHorizontalInput;

        [SerializeField] private float _currentSteeringAngle;
        [SerializeField] private float _currentMotorTorqueValue;

        private void Awake()
        {
            CheckRequiredComponent<GearBox>(_gearBox);
            CheckRequiredComponent<IAccelerate>(_accelerator);
            CheckRequiredComponent<ISteering>(_steeringSystem);
            CheckRequiredComponent<IBreaking>(_breakingSystem);
            CheckRequiredComponent<IWheelsVisualUpdate>(_wheelsVisualUpdater);

            this.gameObject.GetComponent<AbstractWheelsVIsualUpdate>().SetWheels(ref this._wheelColliders,
                                                                                ref this._wheelTransforms);
        }

        private void CheckRequiredComponent<T>(T component)
        {
            if (component == null)
            {
                component = this.gameObject.GetComponent<T>();

                if (component == null)
                {
                    Debug.LogError("CarControlling script can't work without " + typeof(T).ToString());
                }
            }
        }

        private void FixedUpdate()
        {
            GetVerticalInput();
            GetHorizontalInput();
            Brake();
            Accelerate();
            Steering();
            UpdateVisuals();


            // * Calculate motor torque 
            _currentMotorTorqueValue = (_wheelColliders[0].motorTorque
                                    + _wheelColliders[1].motorTorque
                                    + _wheelColliders[2].motorTorque
                                    + _wheelColliders[3].motorTorque) / 4f;


            // * Set center of mass 
            this.gameObject.GetComponent<Rigidbody>().centerOfMass = _centerOfMass.transform.localPosition;
        }

        private void GetVerticalInput()
        {
            if (!VirtualInputManager.Instance.MoveFront && !VirtualInputManager.Instance.MoveBack)
            {
                _currentVerticalInput = 0f;
                return;
            }

            if (VirtualInputManager.Instance.MoveFront)
            {
                if (_currentVerticalInput >= 1f)
                    return;
                if (_currentVerticalInput < 0f)
                    _currentVerticalInput = 0f;

                _currentVerticalInput += 0.1f;

            }

            if (VirtualInputManager.Instance.MoveBack)
            {
                if (_currentVerticalInput <= -1f)
                    return;
                if (_currentVerticalInput > 0f)
                    _currentVerticalInput = 0f;

                _currentVerticalInput -= 0.1f;
            }
        }

        private void GetHorizontalInput()
        {
            if (!VirtualInputManager.Instance.MoveLeft && !VirtualInputManager.Instance.MoveRight)
            {
                _currentHorizontalInput = 0f;
                return;
            }

            if (VirtualInputManager.Instance.MoveRight)
            {
                if (_currentHorizontalInput >= 1f)
                    return;
                if (_currentHorizontalInput < 0f)
                    _currentHorizontalInput = 0f;

                _currentHorizontalInput += 0.1f;

            }

            if (VirtualInputManager.Instance.MoveLeft)
            {
                if (_currentHorizontalInput <= -1f)
                    return;
                if (_currentHorizontalInput > 0f)
                    _currentHorizontalInput = 0f;

                _currentHorizontalInput -= 0.1f;
            }
        }

        private void Accelerate()
        {
            this.gameObject.GetComponent<IAccelerate>().Accelerate(_motorTorque, _currentVerticalInput);
        }

        private void Steering()
        {
            this.gameObject.GetComponent<ISteering>().Steering(maxSteerAngle: _maxSteeringAngle, currentHorizontalInput: _currentHorizontalInput);
        }

        private void Brake()
        {
            if (VirtualInputManager.Instance.Brake)
                this.gameObject.GetComponent<IBreaking>().Brake(brakeTorque: _brakesTorque);
            else
                this.gameObject.GetComponent<IBreaking>().Brake(brakeTorque: 0f);
        }

        private void UpdateVisuals()
        {
            this.gameObject.GetComponent<IWheelsVisualUpdate>().UpdateWheelsPositionRotation();
        }
    }
}