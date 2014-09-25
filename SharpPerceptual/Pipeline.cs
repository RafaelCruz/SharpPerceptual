using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SharpPerceptual.Gestures;

namespace SharpPerceptual {
    public class Pipeline : UtilMPipeline {
        private Task _loopingTask;
        private const pxcmStatus NoError = pxcmStatus.PXCM_STATUS_NO_ERROR;

        public Hand LeftHand { get; private set; }
        public Hand RightHand { get; private set; }
        public Face Face { get; private set; }

        private GestureSensor _gestures;
        private PoseSensor _poses;
        public IGestureSensor Gestures {
            get { return _gestures; }
        }

        public IPoseSensor Poses {
            get { return _poses; }
        }

        public Pipeline() {
            LeftHand = new Hand(Side.Left);
            RightHand = new Hand(Side.Right);
            Face = new Face();
            _gestures = new GestureSensor();
            _poses = new PoseSensor();
        }

        public void Start() {
            EnableGesture();
            EnableFaceLandmark();
            if (!Init()) {
                throw new CameraNotFoundException("Could not initialize the camera");
            }
            _loopingTask = Task.Run(() => {
                try {
                    Loop();
                }
                catch (Exception ex) {
                    Debug.WriteLine("EXCEPTION! " + ex);
                }
            });
        }

        private void Loop() {
            while (true) {
                AcquireFrame(true);
                TrackHandAndFingers(LeftHand, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_LEFT);
                TrackHandAndFingers(RightHand, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_RIGHT);
                TrackFace();

                //Get face location
                //faceLocation = (PXCMFaceAnalysis.Detection)faceAnalysis.DynamicCast(PXCMFaceAnalysis.Detection.CUID);
                //locationStatus = faceLocation.QueryData(faceId, out faceLocationData);
                //detectionConfidence = faceLocationData.confidence.ToString();
                //parent.label1.Text = "Confidence: " + detectionConfidence;

                ////Get face landmarks (eye, mouth, nose position, etc)
                //faceLandmark = (PXCMFaceAnalysis.Landmark)faceAnalysis.DynamicCast(PXCMFaceAnalysis.Landmark.CUID);
                //faceLandmark.QueryProfile(1, out landmarkProfile);
                //faceLandmark.SetProfile(ref landmarkProfile);
                //faceLandmarkData = new PXCMFaceAnalysis.Landmark.LandmarkData[7];
                //landmarkStatus = faceLandmark.QueryLandmarkData(faceId, PXCMFaceAnalysis.Landmark.Label.LABEL_7POINTS, faceLandmarkData);

                ////Get face attributes (smile, age group, gender, eye blink, etc)
                //faceAttributes = (PXCMFaceAnalysis.Attribute)faceAnalysis.DynamicCast(PXCMFaceAnalysis.Attribute.CUID);
                //faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EMOTION, 0, out attributeProfile);
                //faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EMOTION, ref attributeProfile);
                //attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_EMOTION, faceId, out smile);

                //faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EYE_CLOSED, 0, out attributeProfile);
                //attributeProfile.threshold = 50; //Must be here!
                //faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EYE_CLOSED, ref attributeProfile);
                //attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_EYE_CLOSED, faceId, out blink);

                //faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_GENDER, 0, out attributeProfile);
                //faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_GENDER, ref attributeProfile);
                //attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_GENDER, faceId, out gender);

                //faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_AGE_GROUP, 0, out attributeProfile);
                //faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_AGE_GROUP, ref attributeProfile);
                //attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_AGE_GROUP, faceId, out age_group);


                ReleaseFrame();
                //Debug.WriteLine("L: {0}", LeftHand.GetInfo());
                //Debug.WriteLine("R: {0}", RightHand.GetInfo());
                //Debug.WriteLine("-----------------------------------------------");
            }
        }

        private void TrackHandAndFingers(Hand hand, PXCMGesture.GeoNode.Label bodyLabel) {
            var geoNode = QueryGeoNode(bodyLabel);
            TrackPosition(hand, geoNode);
            TrackOpeness(hand, geoNode);

            TrackFingers(hand.Thumb, bodyLabel | PXCMGesture.GeoNode.Label.LABEL_FINGER_THUMB);
            TrackFingers(hand.Index, bodyLabel | PXCMGesture.GeoNode.Label.LABEL_FINGER_INDEX);
            TrackFingers(hand.Middle, bodyLabel | PXCMGesture.GeoNode.Label.LABEL_FINGER_MIDDLE);
            TrackFingers(hand.Ring, bodyLabel | PXCMGesture.GeoNode.Label.LABEL_FINGER_RING);
            TrackFingers(hand.Pinky, bodyLabel | PXCMGesture.GeoNode.Label.LABEL_FINGER_PINKY);
        }

        private void TrackFace() {
            var data = new PXCMFaceAnalysis.Detection.Data();
            var face = QueryFace();
            ulong timestamp;
            int faceId;
            face.QueryFace(0, out faceId, out timestamp);
            var location = (PXCMFaceAnalysis.Detection)face.DynamicCast(PXCMFaceAnalysis.Detection.CUID);
            location.QueryData(faceId, out data);
            Face.IsVisible = data.rectangle.x > 0;
            //Debug.WriteLine("{0}|{1}|{2}|{3}",
            //    data.rectangle.x,
            //    data.rectangle.w,
            //    data.rectangle.y,
            //    data.rectangle.h
            //    );
            var ret = data.rectangle;
            //var x = ret.x + (ret.x + ret.w)/2;
            //var y = ret.y + (ret.y + ret.h)/2;
            var x = ret.x + (ret.x + ret.w) / 2;
            var y = ret.y + (ret.y + ret.h) / 2;
            Face.Position = new Point3D {
                X = (x),
                Y = (y),
            };
        }

        private void TrackPosition(Item item, PXCMGesture.GeoNode geoNode) {
            item.IsVisible = geoNode.body != PXCMGesture.GeoNode.Label.LABEL_ANY;
            if (!item.IsVisible) {
                return;
            }
            Func<double, double> meterToCentimeters = p => p * 100;
            item.Position = new Point3D {
                X = geoNode.positionImage.x,
                Y = geoNode.positionImage.y,
                Z = meterToCentimeters(geoNode.positionWorld.y)
            };
        }

        private void TrackOpeness(FlexiblePart part, PXCMGesture.GeoNode geoNode) {
            if (!part.IsVisible) {
                return;
            }
            if (geoNode.openness > 75) {
                part.IsOpen = true;
            }
            else if (geoNode.openness < 10) {
                part.IsOpen = false;
            }
        }

        private void TrackFingers(Item finger, PXCMGesture.GeoNode.Label fingerLabel) {
            var geoNode = QueryGeoNode(fingerLabel);
            TrackPosition(finger, geoNode);
        }

        private PXCMGesture.GeoNode QueryGeoNode(PXCMGesture.GeoNode.Label bodyLabel) {
            var values = new PXCMGesture.GeoNode();
            QueryGesture().QueryNodeData(0, bodyLabel, out values);
            return values;
        }

        public override void OnGesture(ref PXCMGesture.Gesture gesture) {
            base.OnGesture(ref gesture);
            Debug.WriteLine(gesture.body + " Gesture: {0} Visible: {1} Confidence: {2}", gesture.label, gesture.active, gesture.confidence);
            switch (gesture.label) {
                case PXCMGesture.Gesture.Label.LABEL_POSE_BIG5:
                    IfThisElseThat(gesture.active, _poses.OnBigFiveBegin, _poses.OnBigFiveEnd);
                    break;
                case PXCMGesture.Gesture.Label.LABEL_POSE_PEACE:
                    IfThisElseThat(gesture.active, _poses.OnPosePeaceBegin, _poses.OnPosePeaceEnd);
                    break;
                case PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_DOWN:
                    IfThisElseThat(gesture.active, _poses.OnPoseThumbsDownBegin, _poses.OnPoseThumbsDownEnd);
                    break;
                case PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_UP:
                    IfThisElseThat(gesture.active, _poses.OnPoseThumbsUpBegin, _poses.OnPoseThumbsUpEnd);
                    break;
                case PXCMGesture.Gesture.Label.LABEL_HAND_CIRCLE:
                    _gestures.OnGestureHandCircle();
                    break;
                case PXCMGesture.Gesture.Label.LABEL_HAND_WAVE:
                    _gestures.OnGestureHandWave();
                    break;
                case PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_DOWN:
                    _gestures.OnGestureSwipeDown();
                    break;
                case PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_UP:
                    _gestures.OnGestureSwipeUp();
                    break;
                case PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_RIGHT:
                    _gestures.OnGestureSwipeRight();
                    break;
                case PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_LEFT:
                    _gestures.OnGestureSwipeLeft();
                    break;
                case PXCMGesture.Gesture.Label.LABEL_SET_CUSTOMIZED:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_SET_POSE:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_SET_NAVIGATION:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_SET_HAND:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_MASK_DETAILS:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_MASK_SET:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_ANY:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnAlert(ref PXCMGesture.Alert alert) {
            base.OnAlert(ref alert);

            if (alert.body == PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_LEFT) {
                Disable(LeftHand, alert);
            }
            else if (alert.body == PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_RIGHT) {
                Disable(RightHand, alert);
            }
            Debug.WriteLine("Alert: " + alert.label + " Body: " + alert.body);
        }

        private void Disable(FlexiblePart flexiblePart, PXCMGesture.Alert alert) {
            if (alert.label == PXCMGesture.Alert.Label.LABEL_GEONODE_INACTIVE) {
                flexiblePart.IsVisible = false;
            }
        }

        private void IfThisElseThat(bool value, Action ifTrue, Action ifFalse) {
            if (value) {
                ifTrue();
            }
            else {
                ifFalse();
            }
        }
    }
}
