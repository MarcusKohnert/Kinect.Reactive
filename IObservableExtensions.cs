namespace Kinect.Reactive
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reactive;
	using System.Reactive.Linq;
	using Microsoft.Kinect;
	using Microsoft.Kinect.Toolkit.FaceTracking;
	using Microsoft.Kinect.Toolkit.Interaction;

	public static class IObservableExtensions
	{
		/// <summary>
		/// Gets all Streams in a Tuple. If one frame is null OnNext() will not be called.
		/// </summary>
		/// <param name="source">The source observable.</param>
		/// <returns>A Tuple that includes the three frames.</returns>
		public static IObservable<Tuple<byte[], short[], Skeleton[]>> SelectStreams(this IObservable<AllFramesReadyEventArgs> source)
		{
			if (source == null) throw new ArgumentNullException("source");

			return source.Select(_ =>
				{
					using (var colorImage = _.OpenColorImageFrame())
					using (var depthImage = _.OpenDepthImageFrame())
					using (var skeletonFrame = _.OpenSkeletonFrame())
					{
						if (colorImage == null || depthImage == null || skeletonFrame == null) return null;

						var colorData = new byte[colorImage.PixelDataLength];
						var depthData = new short[depthImage.PixelDataLength];
						var skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];

						colorImage.CopyPixelDataTo(colorData);
						depthImage.CopyPixelDataTo(depthData);
						skeletonFrame.CopySkeletonDataTo(skeletonData);

						return Tuple.Create(colorData, depthData, skeletonData);
					}
				}).Where(_ => _ != null);
		}

		public static IObservable<Tuple<byte[], DepthImagePixel[], Skeleton[], Tuple<T1, T2>>> SelectStreams<T1, T2>(this IObservable<AllFramesReadyEventArgs> source, Func<DepthImageFrame, SkeletonFrame, Tuple<T1, T2>> selector)
		{
			if (source == null) throw new ArgumentNullException("source");

			return source.Select(_ =>
			{
				using (var colorImage = _.OpenColorImageFrame())
				using (var depthImage = _.OpenDepthImageFrame())
				using (var skeletonFrame = _.OpenSkeletonFrame())
				{
					if (colorImage == null || depthImage == null || skeletonFrame == null) return null;

					var colorData = new byte[colorImage.PixelDataLength];
					var skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];

					colorImage.CopyPixelDataTo(colorData);
					skeletonFrame.CopySkeletonDataTo(skeletonData);

					return Tuple.Create(colorData, depthImage.GetRawPixelData(), skeletonData, selector(depthImage, skeletonFrame));
				}
			}).Where(_ => _ != null);
		}

		public static IObservable<Tuple<ColorImageFormat, byte[], DepthImageFormat, short[], Skeleton[]>> SelectFormatStreams(this IObservable<AllFramesReadyEventArgs> source)
		{
			if (source == null) throw new ArgumentNullException("source");

			return source.Select(_ =>
			{
				using (var colorImage = _.OpenColorImageFrame())
				using (var depthImage = _.OpenDepthImageFrame())
				using (var skeletonFrame = _.OpenSkeletonFrame())
				{
					if (colorImage == null || depthImage == null || skeletonFrame == null) return null;

					var colorData = new byte[colorImage.PixelDataLength];
					var depthData = new short[depthImage.PixelDataLength];
					var skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];

					colorImage.CopyPixelDataTo(colorData);
					depthImage.CopyPixelDataTo(depthData);
					skeletonFrame.CopySkeletonDataTo(skeletonData);

					return Tuple.Create(colorImage.Format, colorData, depthImage.Format, depthData, skeletonData);
				}
			}).Where(_ => _ != null);
		}

		public static IObservable<Joint> Select(this IObservable<AllFramesReadyEventArgs> observable, JointType jointType)
		{
			if (observable == null) throw new ArgumentNullException("observable");

			return observable.Select(_ =>
			{
				using (var frame = _.OpenSkeletonFrame())
				{
					if (frame == null) return default(Joint);

					var skeletons = new Skeleton[frame.SkeletonArrayLength];
					frame.CopySkeletonDataTo(skeletons);

					var skeleton = skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
					if (skeleton == null) return default(Joint);

					return skeleton.Joints[jointType];
				}
			}).Where(_ => _ != default(Joint));
		}

		public static IObservable<Joint> Select(this IObservable<SkeletonFrameReadyEventArgs> observable, JointType jointType)
		{
			if (observable == null) throw new ArgumentNullException("observable");

			return observable.SelectStruct(_ => _.Joints[jointType]);
		}

		public static IObservable<Skeleton[]> SelectSkeletons(this IObservable<SkeletonFrameReadyEventArgs> source)
		{
			if (source == null) throw new ArgumentNullException("source");

			return source.Select<SkeletonFrameReadyEventArgs, Skeleton[]>(_ =>
			{
				using (var frame = _.OpenSkeletonFrame())
				{
					if (frame == null) return new Skeleton[0];

					var skeletons = new Skeleton[frame.SkeletonArrayLength];
					frame.CopySkeletonDataTo(skeletons);

					return skeletons;
				}
			});
		}

		public static IObservable<UserInfo[]> SelectUserInfo(this IObservable<InteractionFrameReadyEventArgs> source)
		{
			if(source == null) throw new ArgumentNullException("source");

			return source.Select(_ =>
			{
				using (var frame = _.OpenInteractionFrame())
				{
					if (frame == null) return null;

					var copy = new UserInfo[InteractionFrame.UserInfoArrayLength];
					frame.CopyInteractionDataTo(copy);

					return copy;
				}
			})
			.Where(_ => _ != null);
		}

		public static IObservable<TResult> SelectStruct<TResult>(this IObservable<SkeletonFrameReadyEventArgs> observable, Func<Skeleton, TResult> func) where TResult : struct
		{
			if (observable == null) throw new ArgumentNullException("observable");

			return observable.Select<SkeletonFrameReadyEventArgs, TResult>(_ =>
			{
				using (var frame = _.OpenSkeletonFrame())
				{
					if (frame == null) return default(TResult);

					var skeletons = new Skeleton[frame.SkeletonArrayLength];
					frame.CopySkeletonDataTo(skeletons);

					var skeleton = skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
					if (skeleton == null) return default(TResult);

					return func(skeleton);
				}
			}).Where(_ => !_.Equals(default(TResult)));
		}

		public static IObservable<TResult> Select<TResult>(this IObservable<SkeletonFrameReadyEventArgs> observable, Func<Skeleton, TResult> func) where TResult : class
		{
			if (observable == null) throw new ArgumentNullException("observable");

			return observable.Select<SkeletonFrameReadyEventArgs, TResult>(_ =>
			{
				using (var frame = _.OpenSkeletonFrame())
				{
					if (frame == null) return default(TResult);

					var skeletons = new Skeleton[frame.SkeletonArrayLength];
					frame.CopySkeletonDataTo(skeletons);

					var skeleton = skeletons.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
					if (skeleton == null) return default(TResult);

					return func(skeleton);
				}
			}).Where(_ => _ != default(TResult));
		}

		public static IObservable<TOut> CombineLatestWithExpiry<TLeft, TRight, TOut>(this IObservable<TLeft> left,
																					 IObservable<TRight> right,
																					 Func<TLeft, TRight, TOut> selector,
																					 TimeSpan expiry)
		{
			if (left == null) throw new ArgumentNullException("left");
			if (right == null) throw new ArgumentNullException("right");

			return left.Timestamp().CombineLatest(right.Timestamp(), (_left, _right) => Tuple.Create(_left, _right))
								   .Where(_ => (_.Item1.Timestamp - _.Item2.Timestamp).Duration() < expiry)
								   .Select(_ => selector(_.Item1.Value, _.Item2.Value));
		}
	}
}