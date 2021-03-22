using Project2.Scripts.XR_Player.Common.XR_Interaction;
using UnityEngine;

namespace Project2.Scripts.XR_Player.Common.XRPlayer
{
    public class XRPlayerVisual : MonoBehaviour
    {
        public XRInteractionInformation XRInteractionInformation { private get; set; }
        
        /*
         * ,
                finderMidpoint, 
                anchorVisual, 
                magneticLasso
                magnetMidpoint, 
                
                            public LineRenderer 
                magnetVisual, 
                anchorFinder;
         */
        
        //private static readonly int State = Shader.PropertyToID("_State");
        //private static readonly int Colour = Shader.PropertyToID("_Colour");
        // anchorVisual.ScaleFactor(XRInteractionInformation.ScaleFactor);
        // todo move these all to a networked visual object
        //magneticLasso = Set.Object(castOrigin.gameObject, $"[Lasso] {handedness}", position: LassoPosition).transform;
        //GameObject visual = Instantiate(playerController.castOrigin, castOrigin, worldPositionStays: true);
        //magnetMidpoint = Set.Object(castOrigin.gameObject, $"[Movement Midpoint] {handedness}", position: Vector3.zero).transform;
        //finderMidpoint = Set.Object(castOrigin.gameObject, $"[Finder Midpoint] {handedness}", position: Vector3.zero).transform;
        //magnetVisual = magnetAnchor.gameObject.Line(magnetMaterial, magnetWidth, startEnabled: false);
        //anchorFinder = finderAnchor.gameObject.Line(finderMaterial, finderWidth);
        //anchorVisual = Instantiate(playerController.finderAnchorVisual).transform;
        //anchorVisual.parent = potentialAnchor;
        //anchorVisual.ResetLocalTransform();
    }
}