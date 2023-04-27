using Alteruna;
using UnityEngine;

namespace AlterunaCars
{
	[RequireComponent(typeof(RigidbodySynchronizable))]
	public class CarReset : MonoBehaviour
	{
		public KeyCode ResetKey = KeyCode.R;

		[SerializeField, HideInInspector] private RigidbodySynchronizable _rigidbody;

		private void Start()
		{
			if (_rigidbody == null)
			{
				_rigidbody = GetComponent<RigidbodySynchronizable>();
			}

			// reset center of mass
			_rigidbody.Rigidbody.centerOfMass = Vector3.zero;
		}

		public void Possess(User user)
		{
			bool isMe = user == _rigidbody.Multiplayer.Me;
			enabled = isMe;
			if (isMe)
			{
				CameraFollow.Instance.Target = transform;
				_rigidbody.WakeUp();
			}
		}

		public void Unpossess(User user)
		{
			enabled = false;
		}

		private void Update()
		{
			if (Input.GetKeyDown(ResetKey))
			{
				// get transform
				var t = transform;

				// reset rotation and move up a bit
				t.localPosition += new Vector3(0, 0.025f, 0);
				t.eulerAngles = new Vector3(0, t.eulerAngles.y, 0);

				// rest velocity
				_rigidbody.velocity = Vector3.zero;
				_rigidbody.angularVelocity = Vector3.zero;
			}
		}

		private void Reset()
		{
			_rigidbody = GetComponent<RigidbodySynchronizable>();
		}
	}
}