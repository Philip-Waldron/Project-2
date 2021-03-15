using System;
using System.Collections.Generic;
using System.Linq;
using Project2.Scripts.XR_Player.Common.XR_Input;
using TMPro;
using UnityEngine;
using XR_Prototyping.Scripts.Common;
using XR_Prototyping.Scripts.Common.XR_Input;
using XR_Prototyping.Scripts.Utilities.Generic;

namespace XR_Prototyping.Scripts.Utilities.XR_Debug 
{
	public class XRDebug : XRInputAbstraction
	{
		public TMP_FontAsset debugFont;
		public Material debugMaterial;
		public const float XRDebugLineWidth = .0005f;
		public bool Enabled { get; private set; }
		private GameObject debugParent;

		private void Awake()
		{
			debugParent = Set.Object(gameObject, "[XR Debug Parent]", Vector3.zero);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		private void SetXRDebugState(bool state)
		{
			Enabled = state;
			debugParent.SetActive(Enabled);
		}
		/// <summary>
		/// 
		/// </summary>
		public void ToggleXRDebugState()
		{
			SetXRDebugState(!Enabled);
		}

		// public Dictionary<string, BaseXRDebug> debug = new Dictionary<string, BaseXRDebug>();
		public Dictionary<string, XRDebugLine> debugLines = new Dictionary<string, XRDebugLine>();
		public Dictionary<string, XRDebugRay> debugRays = new Dictionary<string, XRDebugRay>();
		public Dictionary<string, XRDebugSphere> debugSpheres = new Dictionary<string, XRDebugSphere>();
		public Dictionary<string, XRDebugCone> debugCones = new Dictionary<string, XRDebugCone>();
		public Dictionary<string, XRDebugLog> debugLogs = new Dictionary<string, XRDebugLog>();

		/// <summary>
		/// Draw a debug line between two points in 3D space, will render on top of everything else in the scene
		/// </summary>
		/// <param name="index"></param>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="color"></param>
		public void DrawLine(string index, Vector3 from, Vector3 to, Color color)
		{
			// LINQ expression for finding a match in the dictionary of debug calls
			bool MatchedIndex(KeyValuePair<string, XRDebugLine> debugLine) => index == debugLine.Value.debugIndex;
			
			// Find the first matched pair, checks the index of the instance calling the debug method
			// If none exists, then it will create that pair and initialise it
			foreach (KeyValuePair<string, XRDebugLine> debugLine in debugLines.Where(MatchedIndex))
			{
				debugLine.Value.DrawLine(from, to, color);
				return;
			}
			
			// Initialises a new XRDebug line and adds it to the dictionary of debug calls
			XRDebugLine newLine = Set.Object(debugParent, $"[XR Debug Line] {index}", Vector3.zero).AddComponent<XRDebugLine>();
			newLine.debugIndex = index;
			debugLines.Add(index, newLine);
		}
		/// <summary>
		/// Draws a ray from the point defined, in the direction defined, will render on top of everything else in the scene
		/// </summary>
		/// <param name="index"></param>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="distance"></param>
		/// <param name="color"></param>
		public void DrawRay(string index, Vector3 position, Vector3 direction, float distance, Color color)
		{
			// LINQ expression for finding a match in the dictionary of debug calls
			bool MatchedIndex(KeyValuePair<string, XRDebugRay> debugRay) => index == debugRay.Value.debugIndex;
			
			// Find the first matched pair, checks the index of the instance calling the debug method
			// If none exists, then it will create that pair and initialise it
			foreach (KeyValuePair<string, XRDebugRay> debugRay in debugRays.Where(MatchedIndex))
			{
				debugRay.Value.DrawRay(position, direction, distance, color);
				return;
			}
			
			// Initialises a new XRDebug Ray and adds it to the dictionary of debug calls
			XRDebugRay newRay = Set.Object(debugParent, $"[XR Debug Ray] {index}", Vector3.zero).AddComponent<XRDebugRay>();
			newRay.debugIndex = index;
			debugRays.Add(index, newRay);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="position"></param>
		/// <param name="radius"></param>
		/// <param name="color"></param>
		/// <param name="quality"></param>
		public void DrawSphere(string index, Vector3 position, float radius, Color color, int quality = 64)
		{
			// LINQ expression for finding a match in the dictionary of debug calls
			bool MatchedIndex(KeyValuePair<string, XRDebugSphere> debugSphere) => index == debugSphere.Value.debugIndex;
			
			// Find the first matched pair, checks the index of the instance calling the debug method
			// If none exists, then it will create that pair and initialise it
			foreach (KeyValuePair<string, XRDebugSphere> debugSphere in debugSpheres.Where(MatchedIndex))
			{
				debugSphere.Value.DrawSphere(position, radius, color, quality);
				return;
			}
			
			// Initialises a new XRDebug Sphere and adds it to the dictionary of debug calls
			XRDebugSphere newSphere = Set.Object(debugParent, $"[XR Debug Sphere] {index}", position).AddComponent<XRDebugSphere>();
			newSphere.debugIndex = index;
			debugSpheres.Add(index, newSphere);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="origin"></param>
		/// <param name="angle"></param>
		/// <param name="height"></param>
		/// <param name="color"></param>
		/// <param name="quality"></param>
		public void DrawCone(string index,Transform origin, float angle, float height, Color color, int quality = 64)
		{
			// LINQ expression for finding a match in the dictionary of debug calls
			bool MatchedIndex(KeyValuePair<string, XRDebugCone> debugCone) => index == debugCone.Value.debugIndex;
			
			// Find the first matched pair, checks the index of the instance calling the debug method
			// If none exists, then it will create that pair and initialise it
			foreach (KeyValuePair<string, XRDebugCone> debugCone in debugCones.Where(MatchedIndex))
			{
				debugCone.Value.DrawCone(origin, angle, height, color, quality);
				return;
			}
			
			// Initialises a new XRDebug Sphere and adds it to the dictionary of debug calls
			XRDebugCone newCone = Set.Object(debugParent, $"[XR Debug Cone] {index}", origin.position).AddComponent<XRDebugCone>();
			newCone.debugIndex = index;
			debugCones.Add(index, newCone);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="text"></param>
		/// <param name="position"></param>
		/// <param name="size"></param>
		public void Log(string index, string text, Vector3 position, float size = .1f)
		{
			// LINQ expression for finding a match in the dictionary of debug calls
			bool MatchedIndex(KeyValuePair<string, XRDebugLog> debugLog) => index == debugLog.Value.debugIndex;
			
			// Find the first matched pair, checks the index of the instance calling the debug method
			// If none exists, then it will create that pair and initialise it
			foreach (KeyValuePair<string, XRDebugLog> debugLog in debugLogs.Where(MatchedIndex))
			{
				debugLog.Value.Log(text, position, XRInputController.Transform(XRInputController.Check.Head), size);
				return;
			}
			
			// Initialises a new XRDebug Sphere and adds it to the dictionary of debug calls
			XRDebugLog newLog = Set.Object(debugParent, $"[XR Debug Log] {index}", position).AddComponent<XRDebugLog>();
			newLog.debugIndex = index;
			debugLogs.Add(index, newLog);
		}
	}
}