using Alteruna;
using UnityEngine;

namespace AlterunaCars
{
	[RequireComponent(typeof(InputSynchronizable), typeof(AudioSource))]
	public class CarController : CommunicationBridge
	{
		private const float ENGINE_SMOOTHING = 1f;
		private const float WHEEL_TORQUE_SMOOTHING = 1f;
		private const float STEERING_SMOOTHING = 0.5f;

		[SerializeField] [HideInInspector] private InputSynchronizable _inputManager;

		[SerializeField] private WheelController[] wheels;
		[SerializeField] private AudioSource driftSource;
		[SerializeField] private AudioSource engineSource;
		[SerializeField] private Transform centerOfMass;
		public float _steering;

		private SyncedKey _handbrake;
		private bool _isMe = true;

		private Rigidbody _rb;
		private bool _stopped;
		private float _targetDriftPitch;
		private float _targetDriftVolume;
		private float _targetEngineVolume;
		private SyncedAxis _targetSteering;
		private SyncedAxis _targetTorque;

		private float _torque;

		private new void Reset()
		{
			base.Reset();
			_inputManager = GetComponent<InputSynchronizable>();
		}

		private void Start()
		{
			if (_inputManager == null) _inputManager = GetComponent<InputSynchronizable>();

			_rb = GetComponent<Rigidbody>();
			if (centerOfMass != null) _rb.centerOfMass = centerOfMass.localPosition;

			// Setup inputs.
			_handbrake = new SyncedKey(_inputManager, KeyCode.Space);
			_targetSteering = new SyncedAxis(_inputManager, "Horizontal");
			_targetTorque = new SyncedAxis(_inputManager, "Vertical");

			// Set owner for wheels.
			foreach (var wheel in wheels) wheel.CarController = this;
		}

		private void FixedUpdate()
		{
			DeltaSmoothing(ref _steering, _targetSteering.Value, STEERING_SMOOTHING);

#region Drift

			if (driftSource.isPlaying)
			{
				if (_targetDriftVolume < 0.01f)
					driftSource.Stop();
				else
					UpdateDrift();

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

			DeltaSmoothing(ref _torque, _targetTorque, WHEEL_TORQUE_SMOOTHING);

			var speed = Vector3.Dot(transform.forward, _rb.velocity);
			var speedAbs = Mathf.Abs(speed);

			var engineTarget = EnginePower(speed, _targetTorque);
			var torqueToWheel = _torque;

			// quick brake fix
			if (torqueToWheel > 0 && _targetTorque < 0)
			{
				torqueToWheel = _targetTorque;
			}
			// stop reverse when moving forward
			else if (torqueToWheel < 0 && _targetTorque > 0)
			{
				_torque = 0;
				torqueToWheel = 0;
			}


			static float EnginePower(float speed, float torque)
			{
				if (torque < 0)
				{
					if (speed > 0.01f) return 0;
				}
				else if (torque > 0)
				{
					if (speed < -0.01f) return 0;
				}

				return Mathf.Abs(torque);
			}

			DeltaSmoothing(ref _targetEngineVolume, engineTarget, ENGINE_SMOOTHING);

			if (_targetEngineVolume < 0.005f && speedAbs < 0.1f)
			{
				// stop engine and fade out sound
				if (engineSource.isPlaying)
				{
					if (engineSource.volume < 0.01f)
					{
						engineSource.Stop();
						_stopped = true;
					}
				}
				else
				{
					engineSource.volume = Mathf.Lerp(engineSource.volume, 0, Time.fixedDeltaTime * 10);
				}
			}
			else
			{
				_targetEngineVolume = Mathf.Max(_targetEngineVolume, 0.005f);

				if (!engineSource.isPlaying)
				{
					engineSource.Play();
					_stopped = false;
				}

				engineSource.volume = 0.8f + _targetEngineVolume * 0.05f;
				engineSource.pitch = _targetEngineVolume * 0.6f + Mathf.Lerp(0f, 0.5f, speed / 200) + 0.1f;
			}

#endregion

#region Apply to wheels

			if (TrackController.IsStarted)
				foreach (var wheel in wheels)
					wheel.UpdateWheel(_steering, torqueToWheel, _handbrake);
			else
				foreach (var wheel in wheels)
					wheel.UpdateWheel(_steering, 0, true);

#endregion

			if (!_isMe) return;

			RacingUI.Instance.SetSpeed(speed);
		}

		public override void Possessed(bool isMe, User user)
		{
			_isMe = isMe;
			if (RacingUI.Instance != null) RacingUI.Instance.gameObject.SetActive(true);
		}

		public void SetDrift(float drift)
		{
			_targetDriftVolume = Mathf.Max(_targetDriftVolume, drift);
			_targetDriftPitch = Mathf.Max(_targetDriftPitch, drift);
		}

		private void DeltaSmoothing(ref float value, float target, float smoothing) =>
			value = Mathf.LerpUnclamped(value, target, Mathf.Min(Time.fixedDeltaTime / smoothing, 1f));
	}
}