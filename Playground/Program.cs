using System;
using SharpPerceptual;
using SharpPerceptual.Gestures;

namespace Playground {
    class Program {

        private Camera _cam;

        static void Main(string[] args) {
            new Program();
            Console.ReadLine();
        }

        public Program() {
            _cam = new Camera();
            _cam.Start();

            var gesture = new CustomGesture(_cam.LeftHand);
            gesture.AddMovement(Movement.Forward(10, 500));
            gesture.GestureDetected += () => {
                Console.WriteLine("Forward 10cm");
            };
        }
    }
}
