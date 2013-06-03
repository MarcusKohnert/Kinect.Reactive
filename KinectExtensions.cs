namespace Kinect.Reactive
{
	using System;
	using System.Reactive.Linq;
	using Microsoft.Kinect;
	using Microsoft.Kinect.Toolkit.Interaction;

	public static class KinectExtensions
	{
		public static IObservable<AllFramesReadyEventArgs> GetAllFramesReadyObservable(this KinectSensor kinectSensor)
		{
			if (kinectSensor == null) throw new ArgumentNullException("kinectSensor");

			return Observable.FromEventPattern<AllFramesReadyEventArgs>(h => kinectSensor.AllFramesReady += h, h => kinectSensor.AllFramesReady -= h)
							 .Select(e => e.EventArgs);
		}

		public static IObservable<ColorImageFrameReadyEventArgs> GetColorFrameReadyObservable(this KinectSensor kinectSensor)
		{
			if (kinectSensor == null) throw new ArgumentNullException("kinectSensor");

			return Observable.FromEventPattern<ColorImageFrameReadyEventArgs>(h => kinectSensor.ColorFrameReady += h, h => kinectSensor.ColorFrameReady -= h)
							 .Select(e => e.EventArgs);
		}

		public static IObservable<DepthImageFrameReadyEventArgs> GetDepthFrameReadyObservable(this KinectSensor kinectSensor)
		{
			if (kinectSensor == null) throw new ArgumentNullException("kinectSensor");

			return Observable.FromEventPattern<DepthImageFrameReadyEventArgs>(h => kinectSensor.DepthFrameReady += h, h => kinectSensor.DepthFrameReady -= h)
							 .Select(e => e.EventArgs);
		}

		public static IObservable<SkeletonFrameReadyEventArgs> GetSkeletonFrameReadyObservable(this KinectSensor kinectSensor)
		{
			if (kinectSensor == null) throw new ArgumentNullException("kinectSensor");

			return Observable.FromEventPattern<SkeletonFrameReadyEventArgs>(h => kinectSensor.SkeletonFrameReady += h, h => kinectSensor.SkeletonFrameReady -= h)
							 .Select(e => e.EventArgs);
		}

		public static IObservable<SoundSourceAngleChangedEventArgs> GetSoundSourceAngleObservable(this KinectAudioSource kinectAudioSource)
		{
			if (kinectAudioSource == null) throw new ArgumentNullException("kinectAudioSource");

			return Observable.FromEventPattern<SoundSourceAngleChangedEventArgs>(h => kinectAudioSource.SoundSourceAngleChanged += h, h => kinectAudioSource.SoundSourceAngleChanged -= h)
							 .Select(e => e.EventArgs);
		}

		public static IObservable<BeamAngleChangedEventArgs> GetBeamAngleObservable(this KinectAudioSource kinectAudioSource)
		{
			if (kinectAudioSource == null) throw new ArgumentNullException("kinectAudioSource");

			return Observable.FromEventPattern<BeamAngleChangedEventArgs>(h => kinectAudioSource.BeamAngleChanged += h, h => kinectAudioSource.BeamAngleChanged -= h)
							 .Select(e => e.EventArgs);
		}

		public static IObservable<InteractionFrameReadyEventArgs> GetInteractionFrameReadyObservable(this InteractionStream interactionStream)
		{
			if (interactionStream == null) throw new ArgumentNullException("interactionStream");

			return Observable.FromEventPattern<InteractionFrameReadyEventArgs>(h => interactionStream.InteractionFrameReady += h,
																			   h => interactionStream.InteractionFrameReady -= h)
							 .Select(e => e.EventArgs);
		}
	}
}