using Alteruna;
using UnityEngine;

namespace AlterunaCars
{
	public class InitLayers : MonoBehaviour
	{
		public LayerMask IgnoredRbLayers;

		private void Awake()
		{
			RigidbodySynchronizable.IgnoredLayers = IgnoredRbLayers;
		}
	}
}