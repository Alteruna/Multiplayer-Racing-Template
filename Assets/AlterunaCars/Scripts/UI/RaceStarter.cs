using System;
using System.Collections;
using Alteruna;
using UnityEngine;
using UnityEngine.UI;

namespace AlterunaCars
{
	public class RaceStarter : Synchronizable
	{
		public TrackController TrackController;

		public int Countdown = 3;
		public AudioClip[] CountdownSounds = Array.Empty<AudioClip>();

		public Text Text;
		public Button Button;
		private AudioSource _audioSource;

		private bool _isHost;

		private void Start()
		{
			Button.onClick.AddListener(StartRace);

			Button.gameObject.SetActive(_isHost = Multiplayer.GetUser().IsHost);

			if (CountdownSounds.Length > 0 && _audioSource == null)
			{
				_audioSource = GetComponent<AudioSource>();
				if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
			}

			if (TrackController.IsStarted) gameObject.SetActive(false);
		}

		private void StartRace()
		{
			Multiplayer.Sync(this);
			StartCoroutine(StartCountDown());
		}

		public override void AssembleData(Writer writer, byte LOD = 100)
		{
			writer.Write((ushort)Countdown);
		}

		public override void DisassembleData(Reader reader, byte LOD = 100)
		{
			Countdown = reader.ReadUshort();
			StartCoroutine(StartCountDown());
		}

		private IEnumerator StartCountDown()
		{
			Text.gameObject.SetActive(true);
			Button.gameObject.SetActive(false);

			if (Countdown < CountdownSounds.Length) _audioSource.PlayOneShot(CountdownSounds[Countdown]);

			while (Countdown > 0)
			{
				Text.text = Countdown.ToString();
				yield return new WaitForSeconds(1);
				Countdown--;
				if (Countdown < CountdownSounds.Length) _audioSource.PlayOneShot(CountdownSounds[Countdown]);
			}

			Text.text = "Go!";


			if (_isHost) TrackController.StartRace();

			yield return new WaitForSeconds(1);
			Text.gameObject.SetActive(false);
		}
	}
}