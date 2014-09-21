using System.Diagnostics;

namespace SharpPerceptual {
    public static class CameraFinder {
        public static void Find() {
            PXCMSession session;
            PXCMSession.CreateInstance(out session);
            Debug.WriteLine("SDK Version {0}.{1}", session.version.major, session.version.minor);
            
            // session is a PXCMSession instance
            PXCMSession.ImplDesc desc1 = new PXCMSession.ImplDesc();
            desc1.group = PXCMSession.ImplGroup.IMPL_GROUP_SENSOR;
            desc1.subgroup = PXCMSession.ImplSubgroup.IMPL_SUBGROUP_VIDEO_CAPTURE;

            for (uint m = 0; ; m++) {
                PXCMSession.ImplDesc desc2;
                if (session.QueryImpl(ref desc1, m, out desc2) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                Debug.WriteLine("Module[{0}]: {1}", m, desc2.friendlyName.get());

                PXCMCapture capture;
                session.CreateImpl<PXCMCapture>(ref desc2, PXCMCapture.CUID, out capture);

                for (uint d = 0; ; d++) {
                    PXCMCapture.DeviceInfo dinfo;
                    if (capture.QueryDevice(d, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                    Debug.WriteLine("    Device[{0}]: {1}", d, dinfo.name.get());
                }
                capture.Dispose();
            }
            session.Dispose();
        }
    }
}