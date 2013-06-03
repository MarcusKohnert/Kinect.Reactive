Kinect.Reactive
===============

This project contains extension methods to ease the use of the Kinect SDK with the help of Rx.

<p>
KinectSensor kinect = ... <br />
var mapper = new CoordinateMapper(kinect); <br \>
kinect.GetSkeletonFrameReadyObservable() <br />
.Select(JointType.HandRight) <br />
.Select(_ => _.Position) <br />
.Subscribe(_ => <br />
{ <br />
	var point = _.MapToColor(mapper); <br />
	Console.WriteLine(String.Format("x: {0} y: {1}", point.X, point.Y)); <br />
}); <br />
</p>
