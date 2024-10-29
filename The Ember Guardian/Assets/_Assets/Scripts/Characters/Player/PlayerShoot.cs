using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public static PlayerShoot Instance;

    public event EventHandler OnPlayerShotProjectile;
    public event EventHandler OnPlayerShootStopped;

    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform projectilePrefab;
    [SerializeField] private float projectileInitialForce;

    [SerializeField] private GunSO gunSO;
    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerShootCanceled += GameInput_OnPlayerShootCanceled;
        GameInput.Instance.OnPlayerShootStarted += GameInput_OnPlayerShootStarted;
    }

    private void Shoot() {
        PlayerAim.Instance.AddRecoil(gunSO.gunRecoil, gunSO.gunRecoilDamping);

        Vector2 gunKnockbackForce = new Vector2(PlayerMovement.Instance.GetLastMoveDir() * gunSO.gunKnockback * -1 , 0);
        Player.Instance.AddKnockBack(gunKnockbackForce);

        Transform projectileInstantiated = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        float shootAngle = PlayerAim.Instance.GetAimAngle();

        Vector3 shootDir = new Vector3(Mathf.Cos(shootAngle * Mathf.Deg2Rad), Mathf.Sin(shootAngle * Mathf.Deg2Rad), 0);
        Vector2 projectileInitialForceVector = shootDir * projectileInitialForce;

        projectileInstantiated.GetComponent<Rigidbody2D>().AddForce(projectileInitialForceVector, ForceMode2D.Impulse);
    }

    private void GameInput_OnPlayerShootStarted(object sender, System.EventArgs e) {
        Shoot();
        OnPlayerShotProjectile?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnPlayerShootCanceled(object sender, System.EventArgs e) {
    }
}
