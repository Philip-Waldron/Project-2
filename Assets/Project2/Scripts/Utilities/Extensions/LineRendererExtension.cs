using UnityEngine;
using UnityEngine.Rendering;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Utilities
{
    public static class LineRendererExtension
    {
	    public enum Orientation
	    {
		    Forward, 
		    Right, 
		    Up
	    }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="material"></param>
        /// <param name="startWidth"></param>
        /// <param name="endWidth"></param>
        /// <param name="startEnabled"></param>
        /// <param name="worldSpace"></param>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static LineRenderer Line(this GameObject parent, Material material, float startWidth, float endWidth, bool startEnabled, bool worldSpace, int positions = 2)
        {
            LineRenderer lr = parent.AddComponent<LineRenderer>();
            lr.material = material;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.startWidth = startWidth;
            lr.endWidth = endWidth;
            lr.numCapVertices = 32;
            lr.numCornerVertices = 32;
            lr.useWorldSpace = worldSpace;
            lr.enabled = startEnabled;
            lr.positionCount = positions;
            return lr;
        }

        /// <summary>
        /// Returns a LineRenderer, attached to the GameObject passed in, and configured with the relevant information
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="material"></param>
        /// <param name="width"></param>
        /// <param name="startEnabled"></param>
        /// <param name="worldSpace"></param>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static LineRenderer Line(this GameObject parent, Material material, float width, bool startEnabled = true, bool worldSpace = true, int positions = 2)
        {
	        LineRenderer line = parent.AddComponent<LineRenderer>();
	        line.material = material;
	        line.shadowCastingMode = ShadowCastingMode.Off;
	        line.receiveShadows = false;
	        line.Width(width);
	        line.numCapVertices = 32;
	        line.numCornerVertices = 32;
	        line.useWorldSpace = worldSpace;
	        line.enabled = startEnabled;
	        line.positionCount = positions;
	        return line;
        }
        /// <summary>
        /// Creates a line
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="material"></param>
        /// <param name="alignment"></param>
        /// <returns></returns>
        public static LineRenderer Line(this GameObject parent, Material material, LineAlignment alignment = LineAlignment.View)
        {
	        LineRenderer line = parent.TryGetComponent(out LineRenderer extant) ? extant : parent.AddComponent<LineRenderer>();
	        line.material = material;
	        line.shadowCastingMode = ShadowCastingMode.On;
	        line.receiveShadows = false;
	        line.numCapVertices = 8;
	        line.numCornerVertices = 8;
	        line.useWorldSpace = false;
	        line.enabled = true;
	        line.positionCount = 0;
	        line.alignment = alignment;
	        return line;
        }

        /// <summary>
        /// Creates a line
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="material"></param>
        /// <param name="color"></param>
        /// <param name="width"></param>
        /// <param name="alignment"></param>
        /// <returns></returns>
        public static LineRenderer Line(this GameObject parent, Material material, float width, Color color, LineAlignment alignment = LineAlignment.View)
        {
	        LineRenderer line = parent.TryGetComponent(out LineRenderer extant) ? extant : parent.AddComponent<LineRenderer>();
	        line.material = material;
	        line.material.color = color;
	        line.shadowCastingMode = ShadowCastingMode.On;
	        line.startWidth = width;
	        line.endWidth = width;
	        line.receiveShadows = false;
	        line.numCapVertices = 8;
	        line.numCornerVertices = 8;
	        line.useWorldSpace = true;
	        line.enabled = true;
	        line.positionCount = 0;
	        line.alignment = alignment;
	        return line;
        }
        /// <summary>
		/// Draws a bezier line renderer
		/// </summary>
		/// <param name="lr"></param>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="segments"></param>
		public static void BezierLine(this LineRenderer lr, Vector3 p0, Vector3 p1, Vector3 p2, int segments = 40)
		{
			lr.positionCount = segments;
			lr.SetPosition(0, p0);
			lr.SetPosition(segments - 1, p2);

			for (int i = 1; i < segments; i++)
			{
				Vector3 point = GetPoint(p0, p1, p2, i / (float) segments);
				lr.SetPosition(i, point);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="flight"></param>
		/// <param name="step"></param>
		/// <returns></returns>
		private static float GetTimeStep(float flight, float step)
		{
			return Mathf.Lerp(0, flight, step);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="lr"></param>
		/// <param name="focus"></param>
		/// <param name="midpoint"></param>
		/// <param name="controller"></param>
		/// <param name="target"></param>
		/// <param name="quality"></param>
		public static void DrawCurvedLine(this LineRenderer lr, GameObject focus, GameObject midpoint, Transform controller, GameObject target, int quality)
		{
			midpoint.transform.localPosition = new Vector3(0, 0, controller.MidpointDistance(target.transform));
            
			lr.Width(.001f, focus != null ? .01f : 0f);
            
			lr.BezierLine(controller.position,
				midpoint.transform.position, 
				target.transform.position,
				quality);
		}
		/// <summary>
		/// Draws a rectangle as defined by providing two opposite corners
		/// </summary>
		/// <param name="lineRenderer"></param>
		/// <param name="cornerPointA"></param>
		/// <param name="cornerPointB"></param>
		public static void DrawRectangle(this LineRenderer lineRenderer, Vector3 cornerPointA, Vector3 cornerPointB)
		{
			lineRenderer.SetPosition(0, cornerPointA);
			lineRenderer.SetPosition(1, new Vector3(cornerPointA.x, cornerPointA.y, cornerPointB.z));
			lineRenderer.SetPosition(2, cornerPointB);
			lineRenderer.SetPosition(3, new Vector3(cornerPointB.x, cornerPointB.y, cornerPointA.z));
			lineRenderer.SetPosition(4, cornerPointA);
		}
		/// <summary>
		/// Draws a rectangle as defined by providing two points on the corners, and one on an edge
		/// </summary>
		/// <param name="lineRenderer"></param>
		/// <param name="cornerPointA"></param>
		/// <param name="cornerPointB"></param>
		/// <param name="edgePoint"></param>
		public static void DrawRectangle(this LineRenderer lineRenderer, Vector3 cornerPointA, Vector3 cornerPointB, Vector3 edgePoint)
		{
			lineRenderer.SetPosition(0, cornerPointA);
			lineRenderer.SetPosition(1, cornerPointB);
			lineRenderer.SetPosition(2, new Vector3(cornerPointB.x, edgePoint.y, edgePoint.z));
			lineRenderer.SetPosition(3, new Vector3(cornerPointA.x, edgePoint.y, edgePoint.z));
			lineRenderer.SetPosition(4, cornerPointA);
		}
		/// <summary>
		/// Draw a rectangular line from four points
		/// </summary>
		/// <param name="lineRenderer"></param>
		/// <param name="cornerPointA"></param>
		/// <param name="cornerPointB"></param>
		/// <param name="cornerPointC"></param>
		/// <param name="cornerPointD"></param>
		public static void DrawRectangle(this LineRenderer lineRenderer, Vector3 cornerPointA, Vector3 cornerPointB, Vector3 cornerPointC, Vector3 cornerPointD)
		{
			lineRenderer.SetPosition(0, cornerPointA);
			lineRenderer.SetPosition(1, cornerPointB);
			lineRenderer.SetPosition(2, cornerPointC);
			lineRenderer.SetPosition(3, cornerPointD);
			lineRenderer.SetPosition(4, cornerPointA);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		private static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
		{
			return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
		}
		/// <summary>
		/// Draws a line renderer between two transforms
		/// </summary>
		/// <param name="lr"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public static void DrawLine(this LineRenderer lr, Transform start, Transform end)
		{
			lr.positionCount = 2;
			lr.SetPosition(0, start.position);
			lr.SetPosition(1, end.position);
		}
		/// <summary>
		/// Draws a line renderer between two transforms
		/// </summary>
		/// <param name="lr"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public static void DrawLine(this LineRenderer lr, Transform start, Transform end, bool state)
		{
			lr.DrawLine(start, end);
			lr.enabled = state;
		}
		/// <summary>
		/// Draws a line renderer between two points
		/// </summary>
		/// <param name="lr"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public static void DrawLine(this LineRenderer lr, Vector3 start, Vector3 end)
		{
			lr.positionCount = 2;
			lr.SetPosition(0, start);
			lr.SetPosition(1, end);
		}
		/// <summary>
		/// Draws a line renderer between two points
		/// </summary>
		/// <param name="lr"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="offset"></param>
		public static void DrawOffsetLine(this LineRenderer lr, Vector3 start, Vector3 end, float offset = .25f)
		{
			lr.positionCount = 2;
			Vector3 adjustedEnd = Vector3.Lerp(start, end, offset);
			lr.SetPosition(0, start);
			lr.SetPosition(1, adjustedEnd);
		}
		/// <summary>
		/// Draws a line renderer in an arc
		/// </summary>
		/// <param name="line"></param>
		/// <param name="radius"></param>
		/// <param name="startAngle"></param>
		/// <param name="endAngle"></param>
		/// <param name="orientation"></param>
		/// <param name="quality"></param>
		public static void DrawArc(this LineRenderer line, float radius, float startAngle, float endAngle, Orientation orientation, int quality)
		{
			line.positionCount = quality;
			line.useWorldSpace = false;

			float angle = startAngle;
			float arcLength = endAngle - startAngle;

			for (int i = 0; i < quality; i++)
			{
				float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
				float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
				switch (orientation)
				{
					case Orientation.Forward:
						line.SetPosition(i, new Vector3(x, y, 0));
						break;
					case Orientation.Right:
						line.SetPosition(i, new Vector3(x, 0, y));
						break;
					case Orientation.Up:
						line.SetPosition(i, new Vector3(0, x, y));
						break;
					default:
						return;
				}
				angle += arcLength / quality;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		/// <param name="radius"></param>
		/// <param name="orientation"></param>
		/// <param name="quality"></param>
		public static void DrawCircle(this LineRenderer line, float radius, Orientation orientation, int quality)
		{
			line.positionCount = quality;
			line.useWorldSpace = false;
			line.loop = true;
			
			float angle = 0f;
			const float arcLength = 360f;
			
			for (int i = 0; i < quality; i++)
			{
				float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
				float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
				switch (orientation)
				{
					case Orientation.Forward:
						line.SetPosition(i, new Vector3(x, y, 0));
						break;
					case Orientation.Right:
						line.SetPosition(i, new Vector3(x, 0, y));
						break;
					case Orientation.Up:
						line.SetPosition(i, new Vector3(0, x, y));
						break;
					default:
						return;
				}

				angle += arcLength / quality;
			}
		}
    }
}
