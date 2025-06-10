using System.Collections.Generic;
using UnityEngine;

public class DamageNumberPool : MonoBehaviour {
    [SerializeField] private DamageNumber damageNumberPrefab;
    [SerializeField] private int poolSize = 50;

    private Queue<DamageNumber> pool = new Queue<DamageNumber>();

    public static DamageNumberPool Instance;

    private void Awake() {
        Instance = this;

        for (int i = 0; i < poolSize; i++) {
            DamageNumber dn = Instantiate(damageNumberPrefab, transform);
            dn.gameObject.SetActive(false);
            pool.Enqueue(dn);
        }
    }

    public DamageNumber Get(Vector3 position) {
        DamageNumber dn = pool.Count > 0 ? pool.Dequeue() : Instantiate(damageNumberPrefab, transform);
        dn.transform.position = position;
        dn.gameObject.SetActive(true);
        return dn;
    }

    public void ReturnToPool(DamageNumber dn) {
        dn.gameObject.SetActive(false);
        pool.Enqueue(dn);
    }
}
