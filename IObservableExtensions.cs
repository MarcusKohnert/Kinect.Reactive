namespace Kinect.Reactive
{
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit.Interaction;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    public static class IObservableExtensions
    {
        /// <summary>
        /// Returns a sequence with continuous GrippedState HandEventType until GripRelease.
        /// </summary>
        /// <param name="source">The source observable.</param>
        /// <returns>The observable.</returns>
        public static IObservable<UserInfo[]> ContinousGrippedState(this IObservable<UserInfo[]> source)
        {
            if (source == null) throw new ArgumentNullException("source");

            var memory = new Dictionary<Tuple<int, InteractionHandType>, object>();
            var propInfo = typeof(InteractionHandPointer).GetProperty("HandEventType");
            var handEventTypeSetter = new Action<InteractionHandPointer>(o => propInfo.SetValue(o, InteractionHandEventType.Grip));

            return source.Select(_ =>
            {
                _.ForEach(u => u.HandPointers.ForEach(h =>
                {
                    if (h.HandEventType == InteractionHandEventType.Grip)
                    {
                        memory.Add(Tuple.Create(u.SkeletonTrackingId, h.HandType), null);
                    }
                    else if (h.HandEventType == InteractionHandEventType.GripRelease)
                    {
                        memory.Remove(Tuple.Create(u.SkeletonTrackingId, h.HandType));
                    }
                    else if (memory.ContainsKey(Tuple.Create(u.SkeletonTrackingId, h.HandType)))
                    {
                        handEventTypeSetter(h);
                    }
                }));

                return _;
            });
        }

        /// <summary>
        /// Gets all Streams in a tuple. If one frame is null OnNext() will not be called.
        /// </summary>
        /// <param name="source">The source observable.</param>
        /// <returns>An observable of tuple that includes the three frames.</returns>
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

        /// <summary>
        /// Gets all Streams in a tuple and applies the selector function to the depthImageFrame and skeletonFrame.
        /// </summary>
        /// <typeparam name="T1">The type of the tuples first parameter.</typeparam>
        /// <typeparam name="T2">The type of the tuples second parameter.</typeparam>
        /// <param name="source">The source observable.</param>
        /// <param name="selector">The selector function to apply to the depthImageFrame and skeletonFrame.</param>
        /// <returns>An observable of tuple that contains the three frames and the result of the selector function.</returns>
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

        /// <summary>
        /// Gets all Streams in a tuple including the colorImageFormat and depthImageFormat.
        /// </summary>
        /// <param name="source">The source observable.</param>
        /// <returns>An observable of tuple that contains the three frames including the formats.</returns>
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

        /// <summary>
        /// Selects the specified jointType from the first tracked skeleton.
        /// </summary>
        /// <param name="source">The source observable.</param>
        /// <param name="jointType">The jointType to select.</param>
        /// <returns>An observable of joints.</returns>
        public static IObservable<Joint> Select(this IObservable<AllFramesReadyEventArgs> source, JointType jointType)
        {
            if (source == null) throw new ArgumentNullException("observable");

            return source.Select(_ =>
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

        /// <summary>
        /// Selects the specified joinType from the first tracked skeleton.
        /// </summary>
        /// <param name="source">The source observable.</param>
        /// <param name="jointType">The jointType to select.</param>
        /// <returns>An observable of joints.</returns>
        public static IObservable<Joint> Select(this IObservable<SkeletonFrameReadyEventArgs> source, JointType jointType)
        {
            if (source == null) throw new ArgumentNullException("observable");

            return source.SelectStruct(_ => _.Joints[jointType]);
        }

        /// <summary>
        /// Selects the skeletons from the skeleton stream.
        /// </summary>
        /// <param name="source">The source observable.</param>
        /// <returns>An observable sequence of skeletons.</returns>
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

        /// <summary>
        /// Selects the userInfos from the interaction stream.
        /// </summary>
        /// <param name="source">The source observable.</param>
        /// <returns>An observable sequence of userInfos.</returns>
        public static IObservable<UserInfo[]> SelectUserInfo(this IObservable<InteractionFrameReadyEventArgs> source)
        {
            if (source == null) throw new ArgumentNullException("source");

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

        /// <summary>
        /// Applies a function to the first tracked skeleton and returns an observable sequence of the specified value type.
        /// </summary>
        /// <typeparam name="TResult">The value type of the result observable.</typeparam>
        /// <param name="source">The source observable.</param>
        /// <param name="func">The function to apply to the first tracked skeleton.</param>
        /// <returns>An observable sequence of the specified type.</returns>
        public static IObservable<TResult> SelectStruct<TResult>(this IObservable<SkeletonFrameReadyEventArgs> source, Func<Skeleton, TResult> func) where TResult : struct
        {
            if (source == null) throw new ArgumentNullException("observable");

            return source.Select<SkeletonFrameReadyEventArgs, TResult>(_ =>
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

        /// <summary>
        /// Applies a function to the first tracked skeleton and returns an observable sequence of the specified reference type.
        /// </summary>
        /// <typeparam name="TResult">The reference type of the result observable.</typeparam>
        /// <param name="source">The source observable.</param>
        /// <param name="func">The function to apply to the first tracked skeleton.</param>
        /// <returns>An observable sequence of the specified type.</returns>
        public static IObservable<TResult> Select<TResult>(this IObservable<SkeletonFrameReadyEventArgs> source, Func<Skeleton, TResult> func) where TResult : class
        {
            if (source == null) throw new ArgumentNullException("observable");

            return source.Select<SkeletonFrameReadyEventArgs, TResult>(_ =>
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

        /// <summary>
        /// Merges two observable sequences into one observable sequence by using the
        /// selector function whenever both observable sequences produce an element within the expiry timespan.
        /// </summary>
        /// <typeparam name="TLeft">The type of the elements in the first source sequence.</typeparam>
        /// <typeparam name="TRight">The type of the elements in the second source sequence.</typeparam>
        /// <typeparam name="TOut">The type of the elements in the result sequence, returned by the selector function.</typeparam>
        /// <param name="left">The first source sequence.</param>
        /// <param name="right">The second source sequence.</param>
        /// <param name="selector">The function to apply to the result elements.</param>
        /// <param name="expiry">The timespan in which elements of the sequences expire.</param>
        /// <returns>An observable sequence.</returns>
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