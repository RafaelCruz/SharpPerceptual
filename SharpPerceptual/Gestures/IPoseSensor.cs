using System;

namespace SharpPerceptual.Gestures {
    public interface IPoseSensor {
        event Action PosePeaceBegin;
        event Action PosePeaceEnd;
        
        event Action PoseThumbsUpBegin;
        event Action PoseThumbsUpEnd;
        
        event Action PoseThumbsDownBegin;
        event Action PoseThumbsDownEnd;

        event Action BigFiveBegin;
        event Action BigFiveEnd;
    }
}