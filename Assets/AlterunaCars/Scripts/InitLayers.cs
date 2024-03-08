using Alteruna;
using UnityEngine;

namespace AlterunaCars
{
	public class InitLayers : MonoBehaviour
	{
		public LayerMask IgnoredRbLayers;

		private void Awake()
		{
			RigidbodySynchronizableCommon.IgnoredLayers = IgnoredRbLayers;
		}
	}
}