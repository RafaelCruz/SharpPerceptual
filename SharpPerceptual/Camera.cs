using SharpPerceptual.Gestures;

namespace SharpPerceptual {
    public class Camera {
        public static int ResolutionWidth = 320;
        public static int ResolutionHeight = 240;
        private Pipeline _pipeline;

        public Hand LeftHand {
            get { return _pipeline.LeftHand; }
        }

        public Hand RightHand {
            get { return _pipeline.RightHand; }
        }

        public Face Face {
            get { return _pipeline.Face; }
        }

        public IGestureSensor Gestures {
            get { return _pipeline.Gestures; }
        }

        public IPoseSensor Poses {
            get { return _pipeline.Poses; }
        }

        public Camera() {
            _pipeline = new Pipeline();
        }

        public void Start() {
            _pipeline.Start();
        }
    }
}