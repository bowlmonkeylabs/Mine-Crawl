using BML.ScriptableObjectCore.Scripts.SceneReferences;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using BML.Scripts.Utils;
using BML.Scripts.Player;
using Shapes;

namespace BML.Scripts
{
    public class OreCritMarker : MonoBehaviour
{
    [SerializeField] private Collider _oreCollider;
    [SerializeField] private Health _health;
    [SerializeField] private GameObject _critMarker;
    [SerializeField] [MinMaxSlider(0, 90)] private Vector2 _critMarkerMinMaxAngle = new Vector2(15f, 30f);
    [SerializeField] private float _critMarkerSurfaceOffset = .05f;
    [SerializeField] private TransformSceneReference _playerCamRef;

    private Vector3 lastHitPos;
    private Vector3 centerToPlayer;

    public void MoveCritMarker(HitInfo hitInfo) {
        this.MoveCritMarker();
    }

    public void MoveCritMarker()
    {
        if (_health.IsDead)
        {
            _critMarker.SetActive(false);
            return;
        }
        
        if (!_critMarker.activeSelf) _critMarker.SetActive(true);

        centerToPlayer = _playerCamRef.Value.position - _oreCollider.bounds.center;

        float randXRot = MathUtils.GetRandomInRangeReflected(_critMarkerMinMaxAngle.x, _critMarkerMinMaxAngle.y);
        float randYRot = MathUtils.GetRandomInRangeReflected(_critMarkerMinMaxAngle.x, _critMarkerMinMaxAngle.y);

        Quaternion randomRotation = Quaternion.Euler(randXRot, randYRot, 0f);

        Vector3 newCritPoint = _oreCollider.bounds.center;
        newCritPoint += randomRotation * centerToPlayer;
        newCritPoint = _oreCollider.ClosestPoint(newCritPoint);
        newCritPoint += centerToPlayer.normalized * _critMarkerSurfaceOffset;
        _critMarker.transform.position = newCritPoint;
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

        //Draw cones to represent the min and max angle ranges for random rotation
        float length = centerToPlayer.magnitude;
        float minRadius = length * Mathf.Tan(_critMarkerMinMaxAngle.x * Mathf.Deg2Rad);
        float maxRadius = length * Mathf.Tan(_critMarkerMinMaxAngle.y * Mathf.Deg2Rad);
        Draw.Cone(lastHitPos, -centerToPlayer.normalized, minRadius, 
            centerToPlayer.magnitude, false, new Color(1f, 0f, 0f, a));
        Draw.Cone(lastHitPos + centerToPlayer * .05f, -centerToPlayer.normalized, maxRadius, 
            centerToPlayer.magnitude, false, new Color(0f, 1f, 0f, a));
        
    }
}

}
