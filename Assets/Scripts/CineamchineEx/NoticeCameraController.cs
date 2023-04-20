using CFUtilPoolLib;
using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.CineamchineEx
{
    class NoticeCameraController : CinemachineController, INoticeCamera
    {
        public void SetParam(object[] value)
        {
            //todo
        }
        protected override CinemachineVirtualCameraBase VritualCamera()
        {
            return null;
        }
    }
}
