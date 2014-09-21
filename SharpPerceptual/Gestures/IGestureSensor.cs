using System;

namespace SharpPerceptual.Gestures {
    public interface IGestureSensor {
        event Action GestureSwipeLeft;
        event Action GestureSwipeRight;
        event Action GestureSwipeUp;
        event Action GestureSwipeDown;
        event Action GestureHandWave;
        event Action GestureHandCircle;
    }
}