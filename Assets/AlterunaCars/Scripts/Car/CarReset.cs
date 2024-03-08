using Alteruna;
using UnityEngine;

namespace AlterunaCars
{
	[RequireComponent(typeof(RigidbodySynchronizable))]
	public class CarReset : CommunicationBridge
	{
		public KeyCode ResetKey = KeyCode.R;

		[SerializeField] [HideInInspector] private RigidbodySynchronizable _rigidbody;

		private new void Reset()
		{
			base.Reset();
			_rigidbody = GetComponent<RigidbodySynchronizable>();
		}

		private void Start()
		{
			if (_rigidbody == null) _rigidbody = GetComponent<RigidbodySynchronizable>();
		}

		private void Update()
		{
			if (Input.GetKeyDown(ResetKey))
			{
				// get transform
				var t = transform;

				// reset rotation and move up a bit
				if (TrackController.Instance)
					t.position = TrackController.Instance.GetClosestPoint(t.position).Position + new Vector3(0, 0.025f, 0);
				else
					t.position += new Vector3(0, 0.025f, 0);

				t.eulerAngles = new Vector3(0, t.eulerAngles.y, 0);

				// rest velocity
				_rigidbody.velocity = Vector3.zero;
				_rigidbody.angularVelocity = Vector3.zero;
			}
		}

		public override void Possessed(bool isMe, User user)
		{
			enabled = isMe;
			if (isMe)
			{
				CameraFollow.Instance.Target = transform;
				_rigidbody.WakeUp();
			}
		}

		public override void Unpossessed()
		{
			enabled = false;
		}
	}
}