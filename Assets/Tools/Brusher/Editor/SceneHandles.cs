using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace PainterEditor
{
	public static class SceneHandles
	{
		private static Stack<Color> handleColorStack = new Stack<Color>();
		private static Stack<Matrix4x4> handlesMatrix = new Stack<Matrix4x4>();

		public static void PushHandleColor()
		{
			handleColorStack.Push(Handles.color);
		}

		public static void PopHandleColor()
		{
			Handles.color = handleColorStack.Pop();
		}

		public static void PushMatrix()
		{
			handlesMatrix.Push(Handles.matrix);
		}

		public static void PopMatrix()
		{
			Handles.matrix = handlesMatrix.Pop();
		}

		public static void DrawBrush(	Vector3 point,
										Vector3 normal,
										BrushSettings brushSettings,
										Matrix4x4 matrix)
		{
			PushHandleColor();

			Vector3 p = matrix.MultiplyPoint3x4(point);
			Vector3 n = matrix.MultiplyVector(normal).normalized;
			
			/// radius
			Handles.color = brushSettings.outterColor;
			Handles.DrawWireDisc(p, n, brushSettings.radius);

			/// falloff
			Handles.color = brushSettings.innerColor;
			Handles.DrawWireDisc(p, n, brushSettings.radius * brushSettings.falloff);

			Handles.color = new Color(	Mathf.Abs(n.x),
										Mathf.Abs(n.y),
										Mathf.Abs(n.z),
										1f);

			Handles.DrawLine(p, p + n.normalized * HandleUtility.GetHandleSize(p));

			PopHandleColor();
		}

		public static void DrawScatterBrush(Vector3 point, Vector3 normal, BrushSettings settings, Matrix4x4 localToWorldMatrix)
		{
			Vector3 p = localToWorldMatrix.MultiplyPoint3x4(point);
			Vector3 n = localToWorldMatrix.MultiplyVector(normal).normalized;
			
			float r = settings.radius;
			Vector3 a = Vector3.zero;
			Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);

			for(int i = 0; i < 10; i++)
			{
				a.x = Mathf.Cos(Random.Range(0f, 360f));
				a.y = Mathf.Sin(Random.Range(0f, 360f));
				a = a.normalized * Random.Range(0f, r);

				Vector3 v = localToWorldMatrix.MultiplyPoint3x4(point + rotation * a);

				Handles.DrawLine(v, v  + (n * .5f));
				Handles.CubeHandleCap(i + 2302, v, Quaternion.identity, .01f, Event.current.type);
			}

			/// radius
			Handles.DrawWireDisc(p, n, settings.radius);
		}
	}
}
