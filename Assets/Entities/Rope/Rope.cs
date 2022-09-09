using BML.Scripts.Utils;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] private LayerMask _ceilingMask;
    [SerializeField] private int _maxDistance = 100;
    [SerializeField] private Collider _collider;

    public void Deploy () {
        RaycastHit hit;

        if(Physics.Raycast(transform.position, Vector3.up, out hit, _maxDistance, _ceilingMask)) {
            float distToHitPoint = Vector3.Distance(hit.point, transform.position);
            transform.localScale += new Vector3(0, distToHitPoint, 0);
            _collider.isTrigger = false;
        }
    }
}
