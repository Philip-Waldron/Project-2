using UnityEngine;

namespace Project2.Scripts.Interfaces
{
    public static class InteractiveObjectInterfaces
    {
        public interface ITakeDamage
        {
            void TakeDamage(float damage);
            void ExceedDamage();
        }
        public interface ICanGrab
        {
            void Grab(Color color);
            void Release();
        }
        public interface ICanAttach
        {
            void Attach(Color color);
            void Detach();
        }
    }
}
