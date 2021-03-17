using UnityEngine;

public class SpinForce : MonoBehaviour
{
    public Rigidbody Rigidbody;
    public Vector3 Torque;

    // Start is called before the first frame update
    void Start()
    {
        if (Rigidbody == null)
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        Rigidbody.AddTorque(Torque, ForceMode.Impulse);
    }
}
