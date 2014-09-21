using System;

namespace SharpPerceptual {
    public class CameraNotFoundException : Exception {
        public CameraNotFoundException(string message)
            : base(message) {
        }
    }
}