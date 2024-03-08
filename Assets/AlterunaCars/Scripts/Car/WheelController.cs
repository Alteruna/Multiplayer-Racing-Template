using UnityEngine;

namespace AlterunaCars
{
	public class WheelController : MonoBehaviour
	{
		/// <summary>
		/// Height of the trail from the ground.
		/// </summary>
		private const float TRAIL_HEIGHT = 0.05f;

		public WheelCollider WheelCollider;

		[SerializeField] private Transform wheelTransform;

		[SerializeField] private float maxSteerAngle = 30f;

		[SerializeField] private float maxTorque = 30f;

		[SerializeField] private float brakePower = 20f;

		/// <summary>
		/// Electric slip prevention.
		/// prevent wheel from spinning while accelerating.
		/// </summary>
		[SerializeField] private bool esp = true;

		[SerializeField] private bool handbrake;

		[HideInInspector] public bool Steering, Drive;

		[HideInInspector] public CarController CarController;

		[SerializeField] private TrailRenderer Trail;
		private float _oldExtreme;
		private bool _oldHandbrake;


		private float _torqueModifier = 1f;

		private void Start()
		{
			Steering = maxSteerAngle != 0;
			if (maxTorque == 0)
			{
				esp = false;
				Drive = false;
			}
			else
			{
				Drive = true;
			}
		}

		private void FixedUpdate()
		{
			WheelCollider.GetWorldPose(out var position, out var rotation);
			wheelTransform.position = position;
			wheelTransform.rotation = rotation;
			Trail.transform.position = position - WheelCollider.transform.up * (WheelCollider.radius - TRAIL_HEIGHT);
		}

		public void UpdateWheel(float inSteering, float inTorque, bool inHandbrake = false)
		{
			if (Steering) WheelCollider.steerAngle = inSteering * maxSteerAngle;

			if (WheelCollider.GetGroundHit(out var hit))
			{
				var forwardSlip = Mathf.Abs(hit.forwardSlip);
				var slip = forwardSlip + Mathf.Abs(hit.sidewaysSlip);
				if (slip > 0.5f)
				{
					Trail.emitting = true;
					CarController.SetDrift(slip);
				}
				else
				{
					Trail.emitting = false;
				}

				if (esp) _torqueModifier = _torqueModifier * 0.9f + (1 - Mathf.Min(forwardSlip * 2, 0)) * 0.1f;
			}
			else
			{
				Trail.emitting = false;
				_torqueModifier = _torqueModifier * 0.9f + 0.1f;
			}

			inHandbrake &= handbrake;
			if (inHandbrake)
			{
				inTorque = _torqueModifier = 0;
				if (!_oldHandbrake)
				{
					WheelCollider.brakeTorque = 10;
					_oldHandbrake = true;

					var fSide = WheelCollider.sidewaysFriction;
					fSide.stiffness /= 2;
					WheelCollider.sidewaysFriction = fSide;
				}
			}
			else if (_oldHandbrake)
			{
				WheelCollider.brakeTorque = 0;
				_oldHandbrake = false;

				var fSide = WheelCollider.sidewaysFriction;
				fSide.stiffness *= 2;
				WheelCollider.sidewaysFriction = fSide;
			}

			if (Drive)
			{
				if (!inHandbrake && inTorque < 0 && WheelCollider.rpm > 0.01f)
				{
					WheelCollider.motorTorque = 0;
					WheelCollider.brakeTorque = brakePower;
				}
				else
				{
					WheelCollider.motorTorque = inTorque * maxTorque * _torqueModifier;
					if (!inHandbrake) WheelCollider.brakeTorque = 0;
				}
			}
			else if (!inHandbrake)
			{
				if (inTorque < 0 && WheelCollider.rpm > 1)
				{
					WheelCollider.motorTorque = 0;
					WheelCollider.brakeTorque = brakePower;
				}
				else
				{
					WheelCollider.brakeTorque = 0;
				}
			}
		}
	}
}