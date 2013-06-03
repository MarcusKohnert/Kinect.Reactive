Kinect.Reactive
===============

This project contains extension methods to ease the use of the Kinect SDK with the help of Rx.

KinectSensor kinect = ...
var mapper = new CoordinateMapper(kinect);
kinect.GetSkeletonFrameReadyObservable()
  		.Select(JointType.HandRight)
			.Select(_ => _.Position)
			.Subscribe(_ =>
			 {
			    var point = _.MapToColor(mapper);
					Console.WriteLine(String.Format("x: {0} y: {1}", point.X, point.Y));
			 });
