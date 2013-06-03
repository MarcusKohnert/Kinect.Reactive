namespace Kinect.Reactive
{
	using System;
	using Microsoft.Kinect;
	using Microsoft.Kinect.Toolkit.Interaction;

	public static class InteractionStreamExtensions
	{
		public static InteractionStream SubscribeInteractionStream(this InteractionStream interactionStream, KinectSensor kinectSensor)
		{
			if (interactionStream == null) throw new ArgumentNullException("interactionStream");
			if (kinectSensor == null) throw new ArgumentNullException("kinectSensor");

			kinectSensor.GetAllFramesReadyObservable()
						.SelectStreams((_, __) => Tuple.Create(_.Timestamp, __.Timestamp))
						.Subscribe(_ =>
						{
							interactionStream.ProcessSkeleton(_.Item3, kinectSensor.AccelerometerGetCurrentReading(), _.Item4.Item1);
							interactionStream.ProcessDepth(_.Item2, _.Item4.Item2);
						});

			// TODO: Reference to IDisposable must be handled

			return interactionStream;
		}
	}
}