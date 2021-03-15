using Project2.Scripts.XR_Player.Common.XR_Input;
using UnityEngine;

namespace Project2.Scripts.XR_Player.Common.XR_Manipulation.XR_Environment_Manipulation
{
    [CreateAssetMenu(fileName = "XREnvironmentalManipulationType", menuName = "XR Prototyping/Environment Manipulation", order = 0)]
    public class XREnvironmentalManipulationType : ScriptableObject
    {
        /// <summary>
        /// 
        /// </summary>
        public enum ObjectOrientationBehaviour
        {
            DirectManipulation,
            LockedVertical
        }
        /// <summary>
        /// 
        /// </summary>
        public enum ObjectScalingBehaviour
        {
            NoScaling,
            UnclampedScaling,
            ClampedScaling
        }
        /// <summary>
        /// 
        /// </summary>
        public enum BimanualManipulationBehaviour
        {
            AllowBimanual,
            UnimanualOnly
        }
        /// <summary>
        /// 
        /// </summary>
        public enum ManipulationTrigger
        {
            Unimanual,
            Bimanual,
            BimanualOnly
        }
        [Header("Settings")]
        public ObjectOrientationBehaviour objectOrientationBehaviour = ObjectOrientationBehaviour.DirectManipulation;
        public ManipulationTrigger manipulationTrigger = ManipulationTrigger.Unimanual;
        public BimanualManipulationBehaviour bimanualManipulationBehaviour = BimanualManipulationBehaviour.AllowBimanual;
        public XRInputController.Hand environmentalManipulationHand = XRInputController.Hand.NonDominant;
        public ObjectScalingBehaviour objectScalingBehaviour = ObjectScalingBehaviour.UnclampedScaling;
        [Range(0f, 1f)] public float minimumScale = .5f;
        [Range(1f, 5f)] public float maximumScale = 1.5f;
    }
}