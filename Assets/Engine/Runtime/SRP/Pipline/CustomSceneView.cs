using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.SRP
{
    public static class CustomSceneView
    {
        #if UNITY_EDITOR
        private static Material material;
        static bool AcceptedDrawMode(SceneView.CameraMode cameraMode)
        {
            return true;
            if (
                cameraMode.drawMode == DrawCameraMode.Textured ||
                cameraMode.drawMode == DrawCameraMode.TexturedWire ||
                cameraMode.drawMode == DrawCameraMode.Wireframe ||
                //cameraMode.drawMode == DrawCameraMode.ShadowCascades ||
                //cameraMode.drawMode == DrawCameraMode.RenderPaths ||
                cameraMode.drawMode == DrawCameraMode.AlphaChannel ||
                cameraMode.drawMode == DrawCameraMode.Overdraw ||
                cameraMode.drawMode == DrawCameraMode.Mipmaps ||
                cameraMode.drawMode == DrawCameraMode.UserDefined // ||
                //cameraMode.drawMode == DrawCameraMode.SpriteMask ||
                //cameraMode.drawMode == DrawCameraMode.DeferredDiffuse ||
                //cameraMode.drawMode == DrawCameraMode.DeferredSpecular ||
                //cameraMode.drawMode == DrawCameraMode.DeferredSmoothness ||
                //cameraMode.drawMode == DrawCameraMode.DeferredNormal ||
                //cameraMode.drawMode == DrawCameraMode.ValidateAlbedo ||
                //cameraMode.drawMode == DrawCameraMode.ValidateMetalSpecular ||
                //cameraMode.drawMode == DrawCameraMode.ShadowMasks
                //cameraMode.drawMode == DrawCameraMode.LightOverlap
                )
                return true;

            return false;
        }

        public static void SetupDrawMode()
        {

            //Setup draw mode
            SceneView.ClearUserDefinedCameraModes();
                SceneView.AddCameraMode(
                "ShadedWireframe",
                "Custom");
                
            ArrayList sceneViewArray = SceneView.sceneViews;
            foreach (SceneView sceneView in sceneViewArray)
            {
                sceneView.onValidateCameraMode -= AcceptedDrawMode; // Clean up
                sceneView.onValidateCameraMode += AcceptedDrawMode;
            }
        }
        
        public static bool GetDebugDrawMode()
        {
            ArrayList sceneViewsArray = SceneView.sceneViews;
            foreach (SceneView sceneView in sceneViewsArray)
            {
                if(sceneView.cameraMode.name == "ShadedWireframe")
                {
                    // RenderContext.shadedWireframeMode = true;
                    return true;
                }
            }

            // RenderContext.shadedWireframeMode = false;
            return false;
        }
        #endif
    }

}
