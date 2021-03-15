using UnityEngine;

namespace XR_Prototyping.Scripts.Utilities.Interfaces
{
    public static class UndoRedoInterface
    {
        public interface IUndoRedo
        {
            /// <summary>
            /// 
            /// </summary>
            void Undo();
            /// <summary>
            /// 
            /// </summary>
            void Redo();
        }
    }
}
