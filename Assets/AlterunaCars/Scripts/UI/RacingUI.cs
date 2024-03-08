using Alteruna;
using UnityEngine;
using UnityEngine.UI;

namespace AlterunaCars
{
	public class RacingUI : MonoBehaviour
	{
		public TrackController TrackController;


		[SerializeField] private Text LapPercentText;

		[SerializeField] private Text LapCountText;

		[SerializeField] private Text TimerText;

		[SerializeField] private Text BestTimeText;

		[SerializeField] private Transform SpeedomenterNeedle;

		[SerializeField] private float SpeedometerMaxAngle = 180f;

		[SerializeField] private float SpeedometerMaxValue = 200f;

		[SerializeField] private GameObject LapTimeContainer;

		[SerializeField] private GameObject LapTimePrefab;

		private float _bestLapTime;
		private bool _halfway;
		private int _lap = 1;

		private float _lapStart;

		private float _speed, _speedOld;

		public static RacingUI Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
			gameObject.SetActive(false);
		}

		private void FixedUpdate()
		{
			if (!TrackController.IsStarted) return;

			_speedOld = Mathf.Lerp(_speedOld, _speed, Time.deltaTime * 3);
			var newValue = Mathf.LerpUnclamped(0, SpeedometerMaxAngle, _speedOld / SpeedometerMaxValue);
			newValue = Mathf.Abs(newValue);
			SpeedomenterNeedle.eulerAngles = new Vector3(0, 0, -newValue);

			if (TrackController && CameraFollow.Instance.Target != null)
			{
				var v = TrackController.GetTrackProgressFromPositionFast(CameraFollow.Instance.Target.position);
				LapPercentText.text = v.ToString("P0");

				_halfway = _halfway || (v > .3f && v < .7f);
				var lapTime = Time.time - _lapStart;
				if (v < .05f)
				{
					if (_halfway)
					{
						_halfway = false;
						Lapped(lapTime);
					}
					else if (_lapStart == 0)
					{
						_lapStart = Time.time;
					}
				}

				TimerText.text = ToMmSsMm(_bestLapTime == 0 ? Time.time - TrackController.StartTime : lapTime);
			}
		}

		public void SetSpeed(float speed)
		{
			var kmh = speed * 3.6f;
			_speed = kmh;
		}

		[ContextMenu("Lap")]
		private void Lapped() => Lapped(Time.time - _lapStart);

		private void Lapped(float lapTime)
		{
			_lapStart = Time.time;
			if (_bestLapTime == 0 || lapTime < _bestLapTime)
			{
				_bestLapTime = lapTime;
				BestTimeText.text = SetDateInString(BestTimeText.text, ToMmSsMm(lapTime));
			}

			if (_lap == TrackController.Instance.LapCount)
			{
				TrackController.Instance.ReachedFinishLine(_bestLapTime);
				LapTimeContainer.gameObject.SetActive(true);
			}

			_lap++;
			LapCountText.text = SetDateInString(LapCountText.text, _lap.ToString());
		}

		private static string ToMmSsMm(float time)
		{
			var minutes = Mathf.FloorToInt(time / 60f);
			var seconds = Mathf.FloorToInt(time % 60f);
			var milliseconds = Mathf.FloorToInt(time % 1f * 1000f);
			return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
		}

		private static string SetDateInString(string s, string v) => s.Substring(0, s.LastIndexOf(' ') + 1) + v;

		public void PlayerReachedFinishLine(User user, float time, float lapTime)
		{
			var lapTimeItem = Instantiate(LapTimePrefab, LapTimeContainer.transform);

			var texts = lapTimeItem.GetComponentsInChildren<Text>();
			texts[0].text = user.Name;
			texts[1].text = ToMmSsMm(time);
			texts[2].text = ToMmSsMm(lapTime);
			lapTimeItem.GetComponentsInChildren<RawImage>()[1].color = UniqueAvatarColor.HueFromId(Color.red, user.Index, 1f, 1f);

			lapTimeItem.SetActive(true);
		}
	}
}