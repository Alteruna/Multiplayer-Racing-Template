using UnityEngine;

public class TrailRotation : MonoBehaviour
{
    private readonly Quaternion _rot;
    public TrailRotation()
    {
        _rot = Quaternion.Euler(new Vector3(90,0,0));
    }
    void LateUpdate()
    {
        transform.rotation = _rot;
    }
}
