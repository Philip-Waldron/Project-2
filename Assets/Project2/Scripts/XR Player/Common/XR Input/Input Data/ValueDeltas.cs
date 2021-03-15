using System;

namespace Project2.Scripts.XR_Player.Common.XR_Input.Input_Data
{
    [Serializable] public class ValueDeltas
        {
            [Serializable] public class ValueDelta
            {
                public float previousValue, currentValue, delta;
                /// <summary>
                /// 
                /// </summary>
                /// <param name="current"></param>
                /// <param name="logValue"></param>
                public void SetDelta(float current, bool logValue)
                {
                    currentValue = current;

                    if (!logValue)
                    {
                        previousValue = currentValue;
                        delta = 0f;
                    }
                    else
                    {
                        delta = currentValue - previousValue;
                        delta = delta > .75f ? 0f : delta; 
                        previousValue = currentValue;
                    }
                }
                /// <summary>
                /// 
                /// </summary>
                /// <returns></returns>
                public float GetDelta()
                {
                    return delta;
                }
            }
            private ValueDelta nonDominantValueDelta = new ValueDelta(), dominantValueDelta = new ValueDelta();
            /// <summary>
            /// 
            /// </summary>
            /// <param name="check"></param>
            /// <returns></returns>
            public float GetDelta(XRInputController.Check check)
            {
                switch (check)
                {
                    case XRInputController.Check.Left:
                        return nonDominantValueDelta.GetDelta();
                    case XRInputController.Check.Right:
                        return dominantValueDelta.GetDelta();
                    case XRInputController.Check.Head:
                        return 0f;
                    default:
                        return 0f;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="dominant"></param>
            /// <param name="nonDominant"></param>
            public void SetDelta(float dominant, float nonDominant)
            {
                dominantValueDelta.SetDelta(dominant, true);
                nonDominantValueDelta.SetDelta(nonDominant, true);
            }
        }
}