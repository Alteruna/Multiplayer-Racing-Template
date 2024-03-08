using Alteruna;
using UnityEngine;
using UnityEngine.Events;

namespace AlterunaCars
{
	[RequireComponent(typeof(Spawner))]
	public class CarSelector : CommunicationBridge
	{
		public Transform[] Cars;

		public UnityEvent OnSpawn;

		public Transform SpawnPoint;

		private int _currentCar;

		[ContextMenu("Show car")]
		public void ShowCar()
		{
			for (var i = 0; i < Cars.Length; i++) Cars[i].gameObject.SetActive(_currentCar == i);
		}

		[ContextMenu("Next car")]
		public void NextCar()
		{
			Cars[_currentCar].gameObject.SetActive(false);
			_currentCar++;
			_currentCar %= Cars.Length;
			Cars[_currentCar].gameObject.SetActive(true);
		}

		[ContextMenu("Previous car")]
		public void PreviousCar()
		{
			Cars[_currentCar].gameObject.SetActive(false);
			_currentCar--;
			if (_currentCar < 0) _currentCar = Cars.Length - 1;
			Cars[_currentCar].gameObject.SetActive(true);
		}

		[ContextMenu("Select car")]
		public void SelectCar()
		{
			var spawnPoint = SpawnPoint.GetChild(Multiplayer.GetUser().Index % SpawnPoint.childCount);
			var spawner = GetComponent<Spawner>();
			spawner.Spawn(_currentCar, spawnPoint.position, spawnPoint.rotation);
			OnSpawn.Invoke();
		}
	}
}