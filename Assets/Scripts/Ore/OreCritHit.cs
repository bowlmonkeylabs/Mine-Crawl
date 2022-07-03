using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using BML.Scripts.Utils;
using BML.Scripts.Player;
using Shapes;

public class OreCritHit : MonoBehaviour
{
    [SerializeField] private int _critDamageMultiplier = 2;
    [SerializeField] private Collider _oreCollider;
    [SerializeField] private GameObject _critMarker;
    [SerializeField] private float _critHitRadius = .5f;
    [SerializeField] [MinMaxSlider(0, 90)] private Vector2 _critMarkerMinMaxAngle = new Vector2(15f, 30f);
    [SerializeField] private float _critMarkerSurfaceOffset = .05f;
    [SerializeField] private UnityEvent<int> _onCritHit;
    [SerializeField] private UnityEvent<int> _onHit;

    private Vector3 lastHitPos;
    private Vector3 centerToHit;

    public void CritMarkerHit(PickaxeHitInfo pickaxeHitInfo)
    {
        float distToCrit = Vector3.Distance(pickaxeHitInfo.HitPositon, _critMarker.transform.position);
        MoveCritMarker(pickaxeHitInfo.HitPositon);
        if (distToCrit <= _critHitRadius && _critMarker.activeSelf)
        {
            _onCritHit.Invoke(pickaxeHitInfo.Damage * _critDamageMultiplier);
        } else {
            _onHit.Invoke(pickaxeHitInfo.Damage);
        }
    }

    private void MoveCritMarker(Vector3 hitPosition)
    {
        if (!_critMarker.activeSelf) _critMarker.SetActive(true);

        centerToHit = hitPosition - _oreCollider.bounds.center;

        float randXRot = MathUtils.GetRandomInRangeReflected(_critMarkerMinMaxAngle.x, _critMarkerMinMaxAngle.y);
        float randYRot = MathUtils.GetRandomInRangeReflected(_critMarkerMinMaxAngle.x, _critMarkerMinMaxAngle.y);

        Quaternion randomRotation = Quaternion.Euler(randXRot, randYRot, 0f);

        Vector3 newCritPoint = _oreCollider.bounds.center;
        newCritPoint += randomRotation * centerToHit;
        newCritPoint += centerToHit.normalized * _critMarkerSurfaceOffset;
        _critMarker.transform.position = _oreCollider.ClosestPointOnBounds(newCritPoint);
    }
    
    private void OnDrawGizmos()
    {
        if (!_critMarker.activeSelf) return;
        
        Draw.LineGeometry = LineGeometry.Volumetric3D;
        Draw.LineThicknessSpace = ThicknessSpace.Meters;
        Draw.LineThickness = .025f;
        float a = .25f;
        
        //Draw line from collider center to hit point
        Draw.Line(_oreCollider.bounds.center, lastHitPos, new Color(1f, 1f, 0f, a));
        
        //Draw hit radius for crit
        Draw.Sphere(_critMarker.transform.position, _critHitRadius, new Color(1f, 0f, 1f, a));

        //Draw cones to represent the min and max angle ranges for random rotation
        float length = centerToHit.magnitude;
        float minRadius = length * Mathf.Tan(_critMarkerMinMaxAngle.x * Mathf.Deg2Rad);
        float maxRadius = length * Mathf.Tan(_critMarkerMinMaxAngle.y * Mathf.Deg2Rad);
        Draw.Cone(lastHitPos, -centerToHit.normalized, minRadius, 
            centerToHit.magnitude, false, new Color(1f, 0f, 0f, a));
        Draw.Cone(lastHitPos + centerToHit * .05f, -centerToHit.normalized, maxRadius, 
            centerToHit.magnitude, false, new Color(0f, 1f, 0f, a));
        
    }
}
