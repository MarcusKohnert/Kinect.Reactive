namespace Kinect.Reactive
{
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit.Interaction;
    using System;
    using System.Reactive.Linq;

    public static class KinectExtensions
    {
        /// <summary>
        /// Converts the AllFramesReady event to an observable sequence.
        /// </summary>
        /// <param name="kinectSensor">The kinect sensor.</param>
        /// <returns>The observable sequence.</returns>
        public static IObservable<AllFramesReadyEventArgs> GetAllFramesReadyObservable(this KinectSensor kinectSensor)
        {
            if (kinectSensor == null) throw new ArgumentNullException("kinectSensor");

            return Observable.FromEventPattern<AllFramesReadyEventArgs>(h => kinectSensor.AllFramesReady += h, h => kinectSensor.AllFramesReady -= h)
                             .Select(e => e.EventArgs);
        }

        /// <summary>
        /// Converts the ColorFrameReady event to an observable sequence.
        /// </summary>
        /// <param name="kinectSensor">The kinect sensor.</param>
        /// <returns>The observable sequence.</returns>
        public static IObservable<ColorImageFrameReadyEventArgs> GetColorFrameReadyObservable(this KinectSensor kinectSensor)
        {
            if (kinectSensor == null) throw new ArgumentNullException("kinectSensor");

            return Observable.FromEventPattern<ColorImageFrameReadyEventArgs>(h => kinectSensor.ColorFrameReady += h, h => kinectSensor.ColorFrameReady -= h)
                             .Select(e => e.EventArgs);
        }

        /// <summary>
        /// Converts the DepthFrameReady event to an observable sequence.
        /// </summary>
        /// <param name="kinectSensor">The kinect sensor.</param>
        /// <returns>The observable sequence.</returns>
        public static IObservable<DepthImageFrameReadyEventArgs> GetDepthFrameReadyObservable(this KinectSensor kinectSensor)
        {
            if (kinectSensor == null) throw new ArgumentNullException("kinectSensor");

            return Observable.FromEventPattern<DepthImageFrameReadyEventArgs>(h => kinectSensor.DepthFrameReady += h, h => kinectSensor.DepthFrameReady -= h)
                             .Select(e => e.EventArgs);
        }

        /// <summary>
        /// Converts the SkeletonFrameReady event to an observable sequence.
        /// </summary>
        /// <param name="kinectSensor">The kinect sensor.</param>
        /// <returns>The observable sequence.</returns>
        public static IObservable<SkeletonFrameReadyEventArgs> GetSkeletonFrameReadyObservable(this KinectSensor kinectSensor)
        {
            if (kinectSensor == null) throw new ArgumentNullException("kinectSensor");

            return Observable.FromEventPattern<SkeletonFrameReadyEventArgs>(h => kinectSensor.SkeletonFrameReady += h, h => kinectSensor.SkeletonFrameReady -= h)
                             .Select(e => e.EventArgs);
        }

        /// <summary>
        /// Converts the SoundSourceAngleChanged event to an observable sequence.
        /// </summary>
        /// <param name="kinectAudioSource">The kinect audio source.</param>
        /// <returns>The observable sequence.</returns>
        public static IObservable<SoundSourceAngleChangedEventArgs> GetSoundSourceAngleObservable(this KinectAudioSource kinectAudioSource)
        {
            if (kinectAudioSource == null) throw new ArgumentNullException("kinectAudioSource");

            return Observable.FromEventPattern<SoundSourceAngleChangedEventArgs>(h => kinectAudioSource.SoundSourceAngleChanged += h, h => kinectAudioSource.SoundSourceAngleChanged -= h)
                             .Select(e => e.EventArgs);
        }

        /// <summary>
        /// Converts the BeamAngleChanged event to an observable sequence.
        /// </summary>
        /// <param name="kinectAudioSource">The kinect audio source.</param>
        /// <returns>The observable sequence.</returns>
        public static IObservable<BeamAngleChangedEventArgs> GetBeamAngleObservable(this KinectAudioSource kinectAudioSource)
        {
            if (kinectAudioSource == null) throw new ArgumentNullException("kinectAudioSource");

            return Observable.FromEventPattern<BeamAngleChangedEventArgs>(h => kinectAudioSource.BeamAngleChanged += h, h => kinectAudioSource.BeamAngleChanged -= h)
                             .Select(e => e.EventArgs);
        }

        /// <summary>
        /// Instantiates a new InteractionStream, feeds this InteractionStream with Skeleton- and DepthData and subscribes to the InteractionFrameReady event.
        /// </summary>
        /// <param name="kinectSensor">The Kinect sensor passed to the interaction stream instance.</param>
        /// <param name="interactionClient">The interaction client passed to the interaction stream instance.</param>
        /// <returns>An UserInfo stream that contains an action that disposes the interaction stream when the observable is disposed.</returns>
        public static IObservable<UserInfo[]> GetUserInfoObservable(this KinectSensor kinectSensor, IInteractionClient interactionClient)
        {
            if (kinectSensor == null) throw new ArgumentNullException("kinect");
            if (interactionClient == null) throw new ArgumentNullException("interactionClient");

            if (!kinectSensor.DepthStream.IsEnabled) throw new InvalidOperationException("The depth stream is not enabled, but mandatory.");
            if (!kinectSensor.SkeletonStream.IsEnabled) throw new InvalidOperationException("The skeleton stream is not enabled, but mandatory.");

            return Observable.Create<UserInfo[]>(observer =>
            {
                var stream = new InteractionStream(kinectSensor, interactionClient);
                var obs = kinectSensor.GetAllFramesReadyObservable()
                                      .SelectStreams((_, __) => Tuple.Create(_.Timestamp, __.Timestamp))
                                      .Subscribe(_ =>
                                      {
                                          stream.ProcessSkeleton(_.Item3, kinectSensor.AccelerometerGetCurrentReading(), _.Item4.Item1);
                                          stream.ProcessDepth(_.Item2, _.Item4.Item2);
                                      });

                stream.GetInteractionFrameReadyObservable()
                      .SelectUserInfo()
                      .Subscribe(_ => observer.OnNext(_));

                return new Action(() =>
                {
                    obs.Dispose();
                    stream.Dispose();
                });
            });
        }

        /// <summary>
        /// Converts the InteractionFrameReady event to an observable sequence.
        /// </summary>
        /// <param name="interactionStream">The interaction stream.</param>
        /// <returns>The observable sequence.</returns>
        public static IObservable<InteractionFrameReadyEventArgs> GetInteractionFrameReadyObservable(this InteractionStream interactionStream)
        {
            if (interactionStream == null) throw new ArgumentNullException("interactionStream");

            return Observable.FromEventPattern<InteractionFrameReadyEventArgs>(h => interactionStream.InteractionFrameReady += h,
                                                                               h => interactionStream.InteractionFrameReady -= h)
                             .Select(e => e.EventArgs);
        }
    }
}