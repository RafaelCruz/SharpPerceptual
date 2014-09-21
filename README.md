SharpPerceptual
===============
An easier way to use Intel RealSense SDK. Custom poses, gestures and much more.

```
static void Main(string[] args) {
	var cam = new Camera();
	cam.Start();
	cam.RightHand.Closed += () => Console.WriteLine("Closed");
	cam.RightHand.Moved += m => Console.WriteLine("-> x:{0} y:{1}", m.X, m.Y);
	Console.ReadLine();
}
````


