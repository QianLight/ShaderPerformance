using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace CFEngine
{
    public class PhysicsCommand : MonoBehaviour
    {

        public void Start()
        {
            InitRayCommond();
            EngineUtility.SetRaycastCommandHelper(RegistRayCommond);
        }

        public bool RegistRayCommond(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance,
            int layerMask)
        {
            mRayCommand[0] = new RaycastCommand(origin, direction, maxDistance, layerMask);
            mJobHandle = RaycastCommand.ScheduleBatch(mRayCommand, mHitResult, 1, default(JobHandle));
            mJobHandle.Complete();
            hitInfo = mHitResult[0];
            return !ReferenceEquals(hitInfo.collider, null);
        }

        public void OnDestroy()
        {
            EngineUtility.SetRaycastCommandHelper(null);
            ReleaseRayCommond();
        }

        private NativeArray<RaycastCommand> mRayCommand;
        private NativeArray<RaycastHit> mHitResult;
        private JobHandle mJobHandle;

        private void InitRayCommond()
        {
            mHitResult = new NativeArray<RaycastHit>(1, Allocator.Persistent);
            mRayCommand = new NativeArray<RaycastCommand>(1, Allocator.Persistent);
        }

        private void ReleaseRayCommond()
        {
            if (mJobHandle.IsCompleted)
            {
                mJobHandle.Complete();
            }

            if (mHitResult.IsCreated)
                mHitResult.Dispose();

            if (mRayCommand.IsCreated)
                mRayCommand.Dispose();
        }
    }
}
