using System;
using Alteruna;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlterunaCars
{
	public class TrackController : Synchronizable
	{
		public static bool IsStarted = true;
		public static float StartTime;

		public byte LapCount = 3;

		[Range(1, 64)] public int SegmentResolution = 8;

		public bool UseLineRenderer;

		[NonSerialized] private Point[] _points = Array.Empty<Point>();

		[NonSerialized] public float TrackLength;

		public static TrackController Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
			IsStarted = false;
			RecalculateTrack();
		}

		private void Start()
		{
			Multiplayer.OnRoomLeft.AddListener(OnRoomLeft);
		}

		private new void OnDestroy()
		{
			base.OnDestroy();
			Multiplayer.OnRoomLeft.RemoveListener(OnRoomLeft);
		}

		public void StartRace()
		{
			IsStarted = true;
			StartTime = Time.time;
			Multiplayer.Sync(this);
		}

		[ContextMenu("RecalculateTrack")]
		public void RecalculateTrack()
		{
			var checkpointsLength = transform.childCount;
			var pointsLength = checkpointsLength * SegmentResolution;
			_points = new Point[pointsLength];

			// Create all points
			for (var i = 0; i < checkpointsLength; i++)
			{
				var i2 = (i + 1) % checkpointsLength;

				var c1 = transform.GetChild(i);
				var c2 = transform.GetChild(i2);

				var p0 = c1.position;
				var p1 = p0 + c1.forward * c1.localScale.y;
				var p3 = c2.position;
				var p2 = p3 - c2.forward * c2.localScale.y;

				var point = i * SegmentResolution;
				var t = 1f / SegmentResolution;
				for (var j = 0; j < SegmentResolution; j++) _points[point + j] = new Point(BezierPathCalculation(p0, p1, p2, p3, t * j), 0);
			}

			// Calculate distance
			TrackLength = 0;
			var l = pointsLength - 1;
			for (var i = 0; i < l; i++) TrackLength += _points[i].Distance = Vector3.Distance(_points[i].Position, _points[i + 1].Position);
			TrackLength += _points[l].Distance = Vector3.Distance(_points[l].Position, _points[0].Position);

			// Set distance percentage
			float pointDistance = 0;
			for (var i = 0; i < pointsLength; i++)
			{
				pointDistance += _points[i].Distance;
				_points[i].Distance = pointDistance / TrackLength;
			}

			// Set line renderer
			if (UseLineRenderer && Application.isPlaying)
			{
				var lineRenderer = GetComponent<LineRenderer>();
				if (lineRenderer == null) lineRenderer = gameObject.AddComponent<LineRenderer>();

				lineRenderer.loop = true;
				lineRenderer.positionCount = pointsLength;
				for (var i = 0; i < pointsLength; i++) lineRenderer.SetPosition(i, _points[i].Position);
			}
		}

		private Vector3 BezierPathCalculation(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			var tt = t * t;
			var ttt = t * tt;
			var u = 1.0f - t;
			var uu = u * u;
			var uuu = u * uu;

			var B = new Vector3();
			B = uuu * p0;
			B += 3.0f * uu * t * p1;
			B += 3.0f * u * tt * p2;
			B += ttt * p3;

			return B;
		}

		// Get % of track progress from position
		public float GetTrackProgressFromPosition(Vector3 position, bool interpolate = false)
		{
			var closestDistance = float.MaxValue;
			var secondClosestDistance = float.MaxValue;
			var closestIndex = 0;
			var secondClosestIndex = 0;

			var l = _points.Length;
			for (var i = 0; i < l; i++)
			{
				var distance = Vector3.Distance(_points[i].Position, position);
				if (distance < closestDistance)
				{
					secondClosestIndex = closestIndex;

					closestDistance = distance;
					closestIndex = i;
				}
				else if (distance < secondClosestDistance && distance > closestDistance)
				{
					secondClosestDistance = distance;
					secondClosestIndex = i;
				}
			}

			if (interpolate) return InterpolatePos(_points[closestIndex], _points[secondClosestIndex], position);
			return _points[closestIndex].Distance;
		}

		public Point GetClosestPoint(Vector3 position) => GetClosestPoints(position).closest;

		public (Point closest, Point secondClosest) GetClosestPoints(Vector3 position)
		{
			var closestDistance = float.MaxValue;
			var closestIndex = 0;

			var checkpointsLength = transform.childCount;

			for (var i = 0; i < checkpointsLength; i++)
			{
				var distance = Vector3.Distance(transform.GetChild(i).position, position);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestIndex = i;
				}
			}

			var c2Index = (closestIndex + 1) % checkpointsLength;
			var c3Index = closestIndex - 1;
			if (c3Index < 0) c3Index = checkpointsLength - 1;

			closestDistance = float.MaxValue;
			var secondClosestDistance = float.MaxValue;
			var secondClosestIndex = 0;
			var idle = 0;

			{
				var i = c3Index * SegmentResolution;
				var l = c2Index * SegmentResolution;


				if (l < i)
				{
					for (var i2 = 0; i2 < l; i2++)
					{
						CheckClosest(i2);
						if (idle > 4) break;
					}

					idle = 0;
					for (; i < _points.Length; i++)
					{
						CheckClosest(i);
						if (idle > 4) break;
					}
				}
				else
				{
					for (; i < l; i++)
					{
						CheckClosest(i);
						if (idle > 4) break;
					}
				}
			}

			return (_points[closestIndex], _points[secondClosestIndex]);

			void CheckClosest(int i)
			{
				var distance = Vector3.Distance(_points[i].Position, position);
				if (distance < closestDistance)
				{
					secondClosestIndex = closestIndex;
					closestDistance = distance;
					closestIndex = i;
					idle = 0;
				}
				else if (distance < secondClosestDistance && distance > closestDistance)
				{
					secondClosestDistance = distance;
					secondClosestIndex = i;
				}
				else
				{
					idle++;
				}
			}
		}

		// Get % of track progress from position
		public float GetTrackProgressFromPositionFast(Vector3 position, bool interpolate = false)
		{
			var p = GetClosestPoints(position);
			return interpolate ? InterpolatePos(p.closest, p.secondClosest, position) : p.closest.Distance;
		}

		// Interpolates the distance between two points
		private static float InterpolatePos(Point pointA, Point pointB, Vector3 pointC)
		{
			var vectorAb = pointB.Position - pointA.Position;
			var vectorAc = pointC - pointA.Position;

			var projectionLength = Vector3.Dot(vectorAc, vectorAb) / Vector3.Dot(vectorAb, vectorAb);
			var projectedVector = vectorAb * projectionLength;

			var projectedPoint = pointA.Position + projectedVector;
			var distanceAtoProjected = Vector3.Distance(pointA.Position, projectedPoint);
			var distanceAb = Vector3.Distance(pointA.Position, pointB.Position);

			var percentage = distanceAtoProjected / distanceAb;

			return Mathf.Lerp(pointA.Distance, pointB.Distance, percentage);
		}

		private void OnRoomLeft(Multiplayer arg0)
		{
			IsStarted = false;
		}

		public void ReachedFinishLine(float lapTime)
		{
			var userId = Multiplayer.GetUser().Index;
			var time = Time.time - StartTime;

			// Invoke method for others
			InvokeRemoteMethod(0, userId, time, lapTime);
			// Force a sync.
			Multiplayer.Sync(this);
			// Invoke method for local player
			PlayerReachedFinishLine(userId, time, lapTime);
		}

		[SynchronizableMethod]
		private void PlayerReachedFinishLine(ushort userId, float time, float lapTime)
		{
			RacingUI.Instance.PlayerReachedFinishLine(Multiplayer.GetUser(userId), time, lapTime);
		}

		public override void AssembleData(Writer writer, byte LOD = 100)
		{
			writer.Write(IsStarted);
		}

		public override void DisassembleData(Reader reader, byte LOD = 100)
		{
			var isStarted = reader.ReadBool();
			if (isStarted && !IsStarted) StartTime = Time.time;
			IsStarted = isStarted;
		}

		public struct Point
		{
			public Vector3 Position;
			public float Distance;

			public Point(Vector3 position, float distance)
			{
				Position = position;
				Distance = distance;
			}
		}

#if UNITY_EDITOR
		private const float ARROW_SIZE = 0.1f;
		private void OnDrawGizmos()
		{
			if (transform.childCount < 2) return;

			var checkpointsLength = transform.childCount;
			if (_points.Length != checkpointsLength * SegmentResolution) RecalculateTrack();

			Gizmos.color = Color.white;
			var l = _points.Length - 1;
			for (var i = 0; i < l; i++) Gizmos.DrawLine(_points[i].Position, _points[i + 1].Position);
			//Gizmos.DrawSphere(_points[i].Position, 0.4f);
			Gizmos.DrawLine(_points[l].Position, _points[0].Position);

			if (Selection.activeTransform == null || (Selection.activeTransform != transform && Selection.activeTransform.parent != transform)) return;

			Gizmos.color = Color.blue;

			//Gizmos.color = Color.red;
			for (var i = 0; i < checkpointsLength; i++)
			{
				var c1 = transform.GetChild(i);
				var p = c1.position;
				var smooth = c1.localScale.y;

				Gizmos.DrawSphere(p, 1f);

				var forward = c1.forward * smooth;
				var tip = p + forward;
				Gizmos.DrawLine(p, tip);
				forward *= ARROW_SIZE;
				var right = c1.right * smooth * ARROW_SIZE;
				Gizmos.DrawRay(tip, right - forward);
				Gizmos.DrawRay(tip, -right - forward);
			}
		}
#endif
	}
}