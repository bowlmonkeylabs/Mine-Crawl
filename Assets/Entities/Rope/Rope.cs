using BML.Scripts.Utils;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] private LayerMask _terrainMask;
    [SerializeField] private int _maxDistance = 100;
    [SerializeField] private Collider _collider;

    public void Deploy () {
        RaycastHit hit;

        if(Physics.Raycast(transform.position, Vector3.up, out hit, _maxDistance, _terrainMask)) {
            float distToHitPoint = Vector3.Distance(hit.point, transform.position);
            transform.position = transform.position + (Vector3.up * (distToHitPoint / 2));
            transform.localScale += new Vector3(0, distToHitPoint, 0);
            _collider.isTrigger = false;
        }
    }
}
