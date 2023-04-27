using Alteruna;
using UnityEngine;

namespace AlterunaCars
{
	[RequireComponent(typeof(InputSynchronizable),typeof(AudioSource))]
	public class CarController : MonoBehaviour
	{
		[SerializeField, HideInInspector] InputSynchronizable _inputManager;

		[SerializeField] private WheelController[] wheels;

		[SerializeField] private AudioSource driftSource;

		[SerializeField] private AudioSource engineSource;

		private SyncedKey _handbrake;
		private SyncedAxis _steering;
		private SyncedAxis _targetTorque;

		private float _torque;
		private float _targetDriftPitch;
		private float _targetDriftVolume;
		private float _targetEngineVolume;
		private bool _stopped;

		private void Start()
		{
			if (_inputManager == null)
			{
				_inputManager = GetComponent<InputSynchronizable>();
			}

			// Setup inputs.
			_handbrake = new SyncedKey(_inputManager, KeyCode.Space);
			_steering = new SyncedAxis(_inputManager, "Horizontal");
			_targetTorque = new SyncedAxis(_inputManager, "Vertical");

			// Set owner for wheels.
			foreach (WheelController wheel in wheels)
			{
				wheel.CarController = this;
			}
		}

		private void FixedUpdate()
		{
#region Drift

			if (driftSource.isPlaying)
			{
				if (_targetDriftVolume < 0.01f)
				{
					driftSource.Stop();
				}
				else
				{
					UpdateDrift();
				}

				ResetDrift();
			}
			else if (_targetDriftVolume > 0.01f)
			{
				if (!_stopped)
				{
					UpdateDrift();
					driftSource.Play();
					ResetDrift();
				}
			}


			void ResetDrift()
			{
				_targetDriftVolume = 0;
				_targetDriftPitch = 0;
			}

			void UpdateDrift()
			{
				driftSource.pitch = Mathf.Lerp(driftSource.pitch, _targetDriftPitch, Time.fixedDeltaTime * 10);
				driftSource.volume = Mathf.Lerp(driftSource.volume, _targetDriftVolume, Time.fixedDeltaTime * 10);
			}

#endregion

#region Engine

			if (_torque < _targetTorque)
			{
				_torque = _torque * (1 - Time.fixedDeltaTime * 10) + _targetTorque * Time.fixedDeltaTime * 10;
			}
			else
			{
				_torque = _torque * (1 - Time.fixedDeltaTime) + _targetTorque * Time.fixedDeltaTime;
			}

			_targetEngineVolume = _targetEngineVolume * (1 - Time.fixedDeltaTime * 10) + Mathf.Abs(_torque) * Time.fixedDeltaTime * 10;

			if (_targetEngineVolume < 0.005f)
			{
				if (engineSource.isPlaying)
				{
					engineSource.Stop();
					_stopped = true;
				}
			}
			else
			{
				if (!engineSource.isPlaying)
				{
					engineSource.Play();
					_stopped = false;
				}

				engineSource.volume = 0.8f + _targetEngineVolume * 0.05f;
				engineSource.pitch = _targetEngineVolume + 0.1f;
			}

#endregion

#region Apply to wheels

			foreach (WheelController wheel in wheels)
			{
				wheel.UpdateWheel(_steering, _torque, _handbrake);
			}

#endregion
		}

		public void SetDrift(float drift)
		{
			_targetDriftVolume = Mathf.Max(_targetDriftVolume, drift);
			_targetDriftPitch = Mathf.Max(_targetDriftPitch, drift);
		}

		private void Reset()
		{
			_inputManager = GetComponent<InputSynchronizable>();
		}
	}
}