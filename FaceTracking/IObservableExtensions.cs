namespace Kinect.Reactive.FaceTracking
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reactive.Linq;
	using Microsoft.Kinect;
	using Microsoft.Kinect.Toolkit.FaceTracking;

	public static class IObservableExtensions
	{
		public static IObservable<FaceTrackFrame> SelectFaceTrackFrame(this IObservable<AllFramesReadyEventArgs> observable, FaceTracker faceTracker)
		{
			if (observable == null) throw new ArgumentNullException("observable");
			if (faceTracker == null) throw new ArgumentNullException("faceTracker");

			return observable.SelectFormatStreams()
							 .Select(_ => faceTracker.Track(_.Item1, _.Item2, _.Item3, _.Item4, _.Item5.First()))
							 .Where(_ => _.TrackSuccessful);
		}

		public static IObservable<IEnumerable<Tuple<Int32, SkeletonTrackingState, FaceTrackFrame>>> SelectFaceTrackFrame(this IObservable<Tuple<ColorImageFormat, byte[], DepthImageFormat, short[], Skeleton[]>> observable, FaceTracker faceTracker)
		{
			if (observable == null) throw new ArgumentNullException("observable");
			if (faceTracker == null) throw new ArgumentNullException("faceTracker");

			return observable.Select(_ => _.Item5.Where(__ => __.TrackingState == SkeletonTrackingState.Tracked)
												 .ForEach(__ => Tuple.Create(__.TrackingId, __.TrackingState, faceTracker.Track(_.Item1, _.Item2, _.Item3, _.Item4, __)))
												 .Where(__ => __.Item3.TrackSuccessful));
		}

		public static IObservable<IEnumerable<Tuple<Int32, SkeletonTrackingState, JointCollection, EnumIndexableCollection<FeaturePoint, PointF>>>> SelectPersonPoints(this IObservable<Tuple<ColorImageFormat, byte[], DepthImageFormat, short[], Skeleton[]>> observable, FaceTracker faceTracker)
		{
			if (observable == null) throw new ArgumentNullException("observable");
			if (faceTracker == null) throw new ArgumentNullException("faceTracker");

			return observable.Select(_ => _.Item5.ForEach<Skeleton, Tuple<Int32, SkeletonTrackingState, JointCollection, EnumIndexableCollection<FeaturePoint, PointF>>>(__ => 
			{
				if(__.TrackingState == SkeletonTrackingState.PositionOnly)
					return Tuple.Create<Int32, SkeletonTrackingState, JointCollection, EnumIndexableCollection<FeaturePoint, PointF>>(__.TrackingId, __.TrackingState, __.Joints, null);

				var faceTrackFrame = faceTracker.Track(_.Item1, _.Item2, _.Item3, _.Item4, __);
				
				if(!faceTrackFrame.TrackSuccessful)
					return Tuple.Create<Int32, SkeletonTrackingState, JointCollection, EnumIndexableCollection<FeaturePoint, PointF>>(__.TrackingId, __.TrackingState, __.Joints, null);

				return Tuple.Create(__.TrackingId, __.TrackingState, __.Joints, faceTrackFrame.GetProjected3DShape());
			}));
		}

		public static IObservable<IEnumerable<Tuple<Int32, SkeletonTrackingState, Tuple<FeaturePoint, PointF>>>> Select(this IObservable<IEnumerable<Tuple<Int32, SkeletonTrackingState, FaceTrackFrame>>> source, FeaturePoint featurePoint)
		{
			if (source == null) throw new ArgumentNullException("source");

			return source.Select(_ => _.ForEach(__ => Tuple.Create(__.Item1, __.Item2, Tuple.Create(featurePoint, __.Item3.GetProjected3DShape()[featurePoint]))));
		}

		public static IObservable<IEnumerable<Tuple<Int32, SkeletonTrackingState, Dictionary<FeaturePoint, PointF>>>> Select(this IObservable<IEnumerable<Tuple<Int32, SkeletonTrackingState, FaceTrackFrame>>> source, IEnumerable<FeaturePoint> featurePoints)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (featurePoints == null) throw new ArgumentNullException("featurePoints");

			return source.Select(_ => _.ForEach(__ =>
			{
				var shapes = __.Item3.GetProjected3DShape();
				var list = new Dictionary<FeaturePoint, PointF>();
				featurePoints.ExecuteForEach(___ => list.Add(___, shapes[___]));
				return Tuple.Create(__.Item1, __.Item2, list);
			}));
		}
	}
}