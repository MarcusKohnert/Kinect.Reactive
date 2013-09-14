namespace Kinect.Reactive.FaceTracking
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reactive.Linq;
	using Kinect.Reactive;
	using Microsoft.Kinect;
	using Microsoft.Kinect.Toolkit.FaceTracking;

	public static class IObservableExtensions
	{
		/// <summary>
		/// Selects the successfully tracked FaceTrackFrames of the first tracked skeleton from the AllFramesReadyEventArgs observable.
		/// </summary>
		/// <param name="source">The source observable</param>
		/// <param name="faceTracker">The FaceTracker that is used to track the faces.</param>
		/// <returns>A sequence of FaceTrackFrame elements</returns>
		public static IObservable<FaceTrackFrame> SelectFaceTrackFrame(this IObservable<AllFramesReadyEventArgs> source, FaceTracker faceTracker)
		{
			if (source == null) throw new ArgumentNullException("observable");
			if (faceTracker == null) throw new ArgumentNullException("faceTracker");

			return source.SelectFormatStreams()
						 .Select(_ => faceTracker.Track(_.Item1, _.Item2, _.Item3, _.Item4, _.Item5.First()))
						 .Where(_ => _.TrackSuccessful);
		}

		/// <summary>
		/// Selects the successfully tracked FaceTrackFrames of all tracked skeletons from the AllFramesReadyEventArgs observable.
		/// </summary>
		/// <param name="source">The source observable.</param>
		/// <param name="faceTracker">The FaceTracker that is used to track the faces.</param>
		/// <returns>A sequence of a collection of FaceTrackFrames and their identifier in a tuple.</returns>
		public static IObservable<IEnumerable<Tuple<Int32, SkeletonTrackingState, FaceTrackFrame>>> SelectFaceTrackFrame(this IObservable<Tuple<ColorImageFormat, byte[], DepthImageFormat, short[], Skeleton[]>> source, FaceTracker faceTracker)
		{
			if (source == null) throw new ArgumentNullException("observable");
			if (faceTracker == null) throw new ArgumentNullException("faceTracker");

			return source.Select(_ => _.Item5.Where(__ => __.TrackingState == SkeletonTrackingState.Tracked)
											 .ForEach(__ => Tuple.Create(__.TrackingId, __.TrackingState, faceTracker.Track(_.Item1, _.Item2, _.Item3, _.Item4, __)))
											 .Where(__ => __.Item3.TrackSuccessful));
		}

		/// <summary>
		/// Selects the FeaturePoints of all tracked skeletons from the source observable.
		/// </summary>
		/// <param name="observable">The source observable.</param>
		/// <param name="faceTracker">The FaceTracker that is used to track the faces.</param>
		/// <returns>A sequence of a collection of FeaturePoints and their identifiers in a tuple.</returns>
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

		/// <summary>
		/// Selects the specified FeaturePoint from the source observable.
		/// </summary>
		/// <param name="source">THe source observable.</param>
		/// <param name="featurePoint">The FeaturePoint to select.</param>
		/// <returns>A sequence of a collection of the specified FeaturePoint in a tuple.</returns>
		public static IObservable<IEnumerable<Tuple<Int32, SkeletonTrackingState, Tuple<FeaturePoint, PointF>>>> Select(this IObservable<IEnumerable<Tuple<Int32, SkeletonTrackingState, FaceTrackFrame>>> source, FeaturePoint featurePoint)
		{
			if (source == null) throw new ArgumentNullException("source");

			return source.Select(_ => _.ForEach(__ => Tuple.Create(__.Item1, __.Item2, Tuple.Create(featurePoint, __.Item3.GetProjected3DShape()[featurePoint]))));
		}

		/// <summary>
		/// Selects all specified FeaturePoints from the source observable.
		/// </summary>
		/// <param name="source">The source observable.</param>
		/// <param name="featurePoints">The feature points to select.</param>
		/// <returns>A sequence of a collection of the specified FeaturePoints in a tuple.</returns>
		public static IObservable<IEnumerable<Tuple<Int32, SkeletonTrackingState, Dictionary<FeaturePoint, PointF>>>> Select(this IObservable<IEnumerable<Tuple<Int32, SkeletonTrackingState, FaceTrackFrame>>> source, IEnumerable<FeaturePoint> featurePoints)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (featurePoints == null) throw new ArgumentNullException("featurePoints");

			return source.Select(_ => _.ForEach(__ =>
			{
				var shapes = __.Item3.GetProjected3DShape();
				var list = new Dictionary<FeaturePoint, PointF>();
				featurePoints.ForEach(___ => list.Add(___, shapes[___]));
				return Tuple.Create(__.Item1, __.Item2, list);
			}));
		}
	}
}