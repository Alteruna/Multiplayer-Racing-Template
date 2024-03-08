using UnityEngine;

namespace AlterunaCars
{
	public class CameraFollow : MonoBehaviour
	{
		public static CameraFollow Instance;

		public Transform Target;

		public float CameraSpeed = 1;
		public float Distance = 10;
		public float Height = 3.5f;

		private void Awake()
		{
			Instance = this;
		}

		private void Update()
		{
			if (Target)
			{
				var targetPos = Target.position;
				var pos = transform.position;
				var distanceMultiplayer = Mathf.Min(Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(targetPos.x, targetPos.z)) - Distance, 1);
				pos = Vector3.Lerp(pos, new Vector3(targetPos.x, targetPos.y + Height, targetPos.z), Time.deltaTime * CameraSpeed * distanceMultiplayer);
				transform.position = pos;

				transform.LookAt(Target);
			}
		}
	}
}