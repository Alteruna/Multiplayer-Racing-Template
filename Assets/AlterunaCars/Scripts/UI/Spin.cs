using UnityEngine;

namespace AlterunaCars
{
	public class Spin : MonoBehaviour
	{
		public float speed = 16;

		private void Update()
		{
			transform.Rotate(Vector3.up, Time.deltaTime * speed);
		}
	}
}