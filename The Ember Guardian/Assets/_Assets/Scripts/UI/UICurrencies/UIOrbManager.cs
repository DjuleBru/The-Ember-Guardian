using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOrbManager : MonoBehaviour
{

    public static UIOrbManager Instance;

    [SerializeField] private Transform blueOrbsSpawnPosition;
    [SerializeField] private Transform smallBlueOrbsContainer;
    [SerializeField] private Transform bigBlueOrbsContainer;
    [SerializeField] private Transform blueOrbUIPrefab;
    [SerializeField] private Transform smallBlueOrbUIPrefab;

    [SerializeField] private int smallOrbValue = 5;

    private float smallOrbSmoothTime = 1f;
    private float bigOrbSmoothTime = 1f;

    SmallOrb_UI[] smallOrbsInCauldron;
    List<SmallOrb_UI> smallOrbsFormingBigOrb = new List<SmallOrb_UI>();
    private bool formingBigOrb;

    private float timeBetweenSmallOrbsPickup = .2f;
    private float smallOrbsPickupTimer;
    int smallOrbIndex;
    int smallOrbsNecessaryToFormBigOrb = 5;

    private void Awake() {
        Instance = this;
    }

    private void Update() {

        if(Input.GetKeyDown(KeyCode.O)) {
            TryFormBigOrb();
        } 
        
        if(Input.GetKeyDown(KeyCode.S)) {
            CancelOrbFormation();
        }

        if(formingBigOrb) {

            if(smallOrbIndex < 0) {
                CancelOrbFormation();
            }

            smallOrbsPickupTimer -= Time.deltaTime;

            if(smallOrbsPickupTimer <= 0) {

                smallOrbsInCauldron = smallBlueOrbsContainer.GetComponentsInChildren<SmallOrb_UI>();
                smallOrbsInCauldron[smallOrbIndex].SetFormingBigOrb(true);
                smallOrbsInCauldron[smallOrbIndex].SetDestination(blueOrbsSpawnPosition.position, smallOrbSmoothTime);
                smallOrbsFormingBigOrb.Add(smallOrbsInCauldron[smallOrbIndex]);

                if(smallOrbsFormingBigOrb.Count == smallOrbsNecessaryToFormBigOrb) {
                    // Enough small orbs extracted : FORM BIG ORB

                    FormBigOrb();

                } else {
                    // Next small orb
                    smallOrbsPickupTimer = timeBetweenSmallOrbsPickup;
                    smallOrbIndex--;
                }
            }
        }

    }

    private void CancelOrbFormation() {
        formingBigOrb = false;

        foreach (SmallOrb_UI smallOrb in smallOrbsFormingBigOrb) {
            // Not enough small orbs extracted : CANCEL
            smallOrb.SetFormingBigOrb(false);
        }
        smallOrbsFormingBigOrb.Clear();
    }

    private void FormBigOrb() {
        formingBigOrb = false;

        foreach (SmallOrb_UI smallOrb in smallOrbsFormingBigOrb) {
            Destroy(smallOrb.gameObject);
        }

        BigOrb_UI bigOrb = Instantiate(blueOrbUIPrefab, blueOrbsSpawnPosition.transform.position, Quaternion.identity, bigBlueOrbsContainer).GetComponent<BigOrb_UI>();
        bigOrb.SetMovingBigOrb(true);
        bigOrb.SetDestination(Vector3.zero, bigOrbSmoothTime);

        smallOrbsFormingBigOrb.Clear();
    }

    public void AddBlueOrb() {
        Instantiate(blueOrbUIPrefab, blueOrbsSpawnPosition.position, Quaternion.identity, bigBlueOrbsContainer);
    }

    public void TryFormBigOrb() {
        formingBigOrb = true;
        smallOrbsPickupTimer = 0;
        smallOrbsInCauldron = smallBlueOrbsContainer.GetComponentsInChildren<SmallOrb_UI>();
        smallOrbIndex = smallOrbsInCauldron.Length -1;
    }

    public void CrackleBigOrb(Vector3 originPosition) {

        for (int i = 0; i < smallOrbValue; i++) {

            Vector3 spawnPosition = new Vector3(originPosition.x + Random.Range(-3f, 3f), originPosition.y + Random.Range(-3f, 3f), 0);

            Rigidbody2D smallOrbRigidBody = Instantiate(smallBlueOrbUIPrefab, spawnPosition, Quaternion.identity, smallBlueOrbsContainer).GetComponent<Rigidbody2D>();

            Vector2 forceDirectionNormalized = (smallOrbRigidBody.transform.position - originPosition).normalized;
            float force = 75f;
            smallOrbRigidBody.AddForce(forceDirectionNormalized * force, ForceMode2D.Impulse);
        }
    }

}
