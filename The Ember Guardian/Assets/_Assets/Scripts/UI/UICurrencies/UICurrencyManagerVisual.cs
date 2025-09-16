using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class UICurrencyManagerVisual : MonoBehaviour
{
    [SerializeField] private bool debugAlwaysShow;
    [SerializeField] private UICurrencyManager uICurrencyManager;

    [SerializeField] private RectTransform ammoSpawnTransform;
    [SerializeField] private RectTransform gemSpawnTransform;
    [SerializeField] private Sprite level2OrbContainerSprite;
    [SerializeField] private Sprite level3OrbContainerSprite;
    [SerializeField] private Sprite level2AmmoPocketSprite;
    [SerializeField] private Sprite level3AmmoPocketSprite;
    [SerializeField] private Sprite level2GemPocketSprite;
    [SerializeField] private Sprite level3GemPocketSprite;
    [SerializeField] private Sprite level2OrbContainerSprite_Front;
    [SerializeField] private Sprite level3OrbContainerSprite_Front;
    [SerializeField] private Sprite level2AmmoPocketSprite_Front;
    [SerializeField] private Sprite level3AmmoPocketSprite_Front;
    [SerializeField] private Sprite level2GemPocketSprite_Front;
    [SerializeField] private Sprite level3GemPocketSprite_Front;
    [SerializeField] private Image orbContainerImage;
    [SerializeField] private Image ammoPocketImage;
    [SerializeField] private Image gemPocketImage;
    [SerializeField] private Image orbContainerImage_Front;
    [SerializeField] private Image ammoPocketImage_Front;
    [SerializeField] private Image gemPocketImage_Front;
    [SerializeField] private Image orbContainerShadowImage;
    [SerializeField] private Image ammoPocketShadowImage;
    [SerializeField] private Image gemPocketShadowImage;
    [SerializeField] private GameObject level1OrbContainerCollider;
    [SerializeField] private GameObject level2OrbContainerCollider;
    [SerializeField] private GameObject level3OrbContainerCollider;
    [SerializeField] private GameObject level1AmmoPocketCollider;
    [SerializeField] private GameObject level2AmmoPocketCollider;
    [SerializeField] private GameObject level3AmmoPocketCollider;
    [SerializeField] private GameObject level1GemPocketCollider;
    [SerializeField] private GameObject level2GemPocketCollider;
    [SerializeField] private GameObject level3GemPocketCollider;
    [SerializeField] private GameObject ammoCollidersParent;
    [SerializeField] private GameObject gemCollidersParent;
    [SerializeField] private Vector2 level2OrbContainerAmmoPocketPosition;
    [SerializeField] private Vector2 level2OrbContainerGemPocketPosition;
    [SerializeField] private Vector2 level2AmmoCollidersParentPosition;
    [SerializeField] private Vector2 level2GemCollidersParentPosition;
    [SerializeField] private Vector2 level3OrbContainerAmmoPocketPosition;
    [SerializeField] private Vector2 level3OrbContainerGemPocketPosition;
    [SerializeField] private Vector2 level3AmmoCollidersParentPosition;
    [SerializeField] private Vector2 level3GemCollidersParentPosition;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup currencyCanvasGroup;
    [SerializeField] private Animator backpackFrontAnimator;
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private List<CurrencyUI_AlmostFullColliders> almostFullColliders;
    [SerializeField] private Light2D emberLight;

    public float backpackDisplayTime = 2f;   // Durée d'affichage de la barre
    public float backpackDisplayTimer;   // Durée d'affichage de la barre
    public float frontDisplayTime = 1f;  // Durée du fade-out
    public float fadeOutDuration = .2f;  // Durée du fade-out
    public float fadeInDuration = .2f;  // Durée du fade-in


    private bool backpackAlmostFull;
    private float backpackBarDiplayTimer = 0f;
    private bool isShowingBackpackFront = true;
    private bool isFadingOut = true;
    private bool isFadingIn = false;
    private bool tabMenuOpen = false;
    private bool isPlayerInventory;
    private bool hubMerchantShopOpen;

    private Coroutine chestCoroutine;
    private void Start() {
        uICurrencyManager.OnCurrencyDropped += UICurrencyManager_OnCurrencyDropped;
        uICurrencyManager.OnCurrencyTryPay += UICurrencyManager_OnCurrencyTryPay;
        uICurrencyManager.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        uICurrencyManager.OnCurrencyFailedToDrop += UICurrencyManager_OnCurrencyFailedToDrop;

        PlayerStats.Instance.OnBackpackDimensionsChanged += PlayerStats_OnBackpackDimensionsChanged;
        PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
        PlayerShoot.Instance.OnPlayerTryReloadAmmoBelt_NoAmmoInBag += PlayerShoot_OnPlayerTryReloadAmmoBelt_NoAmmoInBag;
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;

        isPlayerInventory = (uICurrencyManager == UICurrencyManager.PlayerInventoryUI);

        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Tutorial) {
            PlayerTabMenuUI.Instance.OnPlayerTabClosed += PlayerTabMenuUI_OnPlayerTabClosed;
            PlayerTabMenuUI.Instance.OnPlayerTabOpened += PLayerTabMenuUI_OnPlayerTabOpened;
        }

        if(uICurrencyManager == UICurrencyManager.HubInventoryUI) {
            HubChest.Instance.OnChestOpened += HubChest_OnChestOpened;
            HubChest.Instance.OnChestClosed += HubChest_OnChestClosed;
            HubMerchant.OnPlayerOpenedAnyHubMerchantShop += HubMerchant_OnPlayerOpenedAnyHubMerchantShop;
            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        }

        RefreshBackpackVisuals();
    }

    private void PlayerStats_OnBackpackDimensionsChanged(object sender, System.EventArgs e) {
        RefreshBackpackVisuals();
    }

    private void RefreshBackpackVisuals() {
        if (!isPlayerInventory) return;

        level1OrbContainerCollider.SetActive(false);
        level2OrbContainerCollider.SetActive(false);
        level3OrbContainerCollider.SetActive(false);
        level1AmmoPocketCollider.SetActive(false);
        level2AmmoPocketCollider.SetActive(false);
        level3AmmoPocketCollider.SetActive(false);
        level1GemPocketCollider.SetActive(false);
        level2GemPocketCollider.SetActive(false);
        level3GemPocketCollider.SetActive(false);

        if (PlayerStats.Instance.GetBackpackOrbSizePercentBuff_Meta() == 0f) {
            level1OrbContainerCollider.SetActive(true);
        }
        if (PlayerStats.Instance.GetBackpackOrbSizePercentBuff_Meta() == 15f) {
            level2OrbContainerCollider.SetActive(true);
            orbContainerImage.sprite = level2OrbContainerSprite;
            orbContainerImage_Front.sprite = level2OrbContainerSprite_Front;
            orbContainerShadowImage.sprite = level2OrbContainerSprite;

            RectTransform ammoPocketRT = ammoPocketImage.GetComponent<RectTransform>();
            ammoPocketRT.anchoredPosition = level2OrbContainerAmmoPocketPosition;
            RectTransform gemPocketRT = gemPocketImage.GetComponent<RectTransform>();
            gemPocketRT.anchoredPosition = level2OrbContainerGemPocketPosition;

            RectTransform gemColliderParentRT = gemCollidersParent.GetComponent<RectTransform>();
            gemColliderParentRT.anchoredPosition = level2GemCollidersParentPosition;
            RectTransform ammoColliderParentRT = ammoCollidersParent.GetComponent<RectTransform>();
            ammoColliderParentRT.anchoredPosition = level2AmmoCollidersParentPosition;

        }
        if (PlayerStats.Instance.GetBackpackOrbSizePercentBuff_Meta() == 35f) {
            level3OrbContainerCollider.SetActive(true);
            orbContainerImage.sprite = level3OrbContainerSprite;
            orbContainerImage_Front.sprite = level3OrbContainerSprite_Front;
            orbContainerShadowImage.sprite = level3OrbContainerSprite;

            RectTransform ammoPocketRT = ammoPocketImage.GetComponent<RectTransform>();
            ammoPocketRT.anchoredPosition = level3OrbContainerAmmoPocketPosition;
            RectTransform gemPocketRT = gemPocketImage.GetComponent<RectTransform>();
            gemPocketRT.anchoredPosition = level3OrbContainerGemPocketPosition;

            RectTransform gemColliderParentRT = gemCollidersParent.GetComponent<RectTransform>();
            gemColliderParentRT.anchoredPosition = level3GemCollidersParentPosition;
            RectTransform ammoColliderParentRT = ammoCollidersParent.GetComponent<RectTransform>();
            ammoColliderParentRT.anchoredPosition = level3AmmoCollidersParentPosition;


            Vector3 ammoSpawnPosition = ammoSpawnTransform.anchoredPosition;
            ammoSpawnPosition.x -= 5f;
            ammoSpawnTransform.anchoredPosition = ammoSpawnPosition;

            Vector3 gemSpawnPosition = gemSpawnTransform.anchoredPosition;
            gemSpawnPosition.x += 5f;
            gemSpawnTransform.anchoredPosition = gemSpawnPosition;
        }

        if (PlayerStats.Instance.GetBackpackAmmoSizePercentBuff_Meta() == 0f) {
            level1AmmoPocketCollider.SetActive(true);
        }
        if (PlayerStats.Instance.GetBackpackAmmoSizePercentBuff_Meta() == 15f) {
            level2AmmoPocketCollider.SetActive(true);
            ammoPocketImage.sprite = level2AmmoPocketSprite;
            ammoPocketImage_Front.sprite = level2AmmoPocketSprite_Front;
            ammoPocketShadowImage.sprite = level2AmmoPocketSprite;
        }
        if (PlayerStats.Instance.GetBackpackAmmoSizePercentBuff_Meta() == 35f) {
            level3AmmoPocketCollider.SetActive(true);
            ammoPocketImage.sprite = level3AmmoPocketSprite;
            ammoPocketImage_Front.sprite = level3AmmoPocketSprite_Front;
            ammoPocketShadowImage.sprite = level3AmmoPocketSprite;
        }

        if (PlayerStats.Instance.GetBackpackGemSizePercentBuff_Meta() == 0f) {
            level1GemPocketCollider.SetActive(true);
        }
        if (PlayerStats.Instance.GetBackpackGemSizePercentBuff_Meta() == 15f) {
            level2GemPocketCollider.SetActive(true);
            gemPocketImage.sprite = level2GemPocketSprite;
            gemPocketImage_Front.sprite = level2GemPocketSprite_Front;
            gemPocketShadowImage.sprite = level2GemPocketSprite;
        }
        if (PlayerStats.Instance.GetBackpackGemSizePercentBuff_Meta() == 35f) {
            level3GemPocketCollider.SetActive(true);
            gemPocketImage.sprite = level3GemPocketSprite;
            gemPocketImage_Front.sprite = level3GemPocketSprite_Front;
            gemPocketShadowImage.sprite = level3GemPocketSprite;
        }
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {
        if (!hubMerchantShopOpen) return;

        ShowIntenvoryFront();
        hubMerchantShopOpen = false;
    }

    private void HubMerchant_OnPlayerOpenedAnyHubMerchantShop(object sender, System.EventArgs e) {
        HideInventoryFront();
        hubMerchantShopOpen = true;
    }

    private void HubChest_OnChestClosed(object sender, System.EventArgs e) {
        debugAlwaysShow = false;
        if (chestAnimator != null) {
            StopCoroutine(chestCoroutine);
            chestAnimator.SetTrigger("Close");
            chestAnimator.ResetTrigger("Open");
        }
    }

    private void HubChest_OnChestOpened(object sender, System.EventArgs e) {
        ShowBackpack(2f);
        debugAlwaysShow = true;
        if (chestAnimator != null) {
            chestCoroutine = StartCoroutine(OpenChestUIAfterDelay(0f));
        }
    }

    private IEnumerator OpenChestUIAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        chestAnimator.SetTrigger("Open");
        chestAnimator.ResetTrigger("Close");
    }

    private void PLayerTabMenuUI_OnPlayerTabOpened(object sender, System.EventArgs e) {
        //if (uICurrencyManager == UICurrencyManager.PlayerInventoryUI && SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) return;
        HideInventoryFront();
    }

    private void PlayerTabMenuUI_OnPlayerTabClosed(object sender, System.EventArgs e) {
        //if (uICurrencyManager == UICurrencyManager.PlayerInventoryUI && SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) return;
        ShowIntenvoryFront();
    }

    private void HideInventoryFront() {
        tabMenuOpen = true;

        if (canvasGroup.alpha == 0) {
            isFadingIn = true;
        }

        isShowingBackpackFront = true;

        if (!isPlayerInventory) {
            chestAnimator.SetTrigger("HideFront");
            chestAnimator.ResetTrigger("ShowFront");
            return;
        };

        backpackFrontAnimator.SetTrigger("HideFront");
        backpackFrontAnimator.ResetTrigger("ShowFront");
        currencyCanvasGroup.alpha = 1;
    }
    private void ShowIntenvoryFront() {
        tabMenuOpen = false;
        backpackDisplayTimer = frontDisplayTime; // Initialise le timer pour le fade
        isFadingIn = false;

        if (!isPlayerInventory) {
            chestAnimator.SetTrigger("ShowFront");
            chestAnimator.ResetTrigger("HideFront");
            return;
        };

        backpackFrontAnimator.SetTrigger("ShowFront");
        backpackFrontAnimator.ResetTrigger("HideFront");
    }

    private void Update() {
        if (debugAlwaysShow) return;

        if (isFadingIn) {
            HandleFadeIn();
            return;
        }

        if (tabMenuOpen) return;

        HandleFadeOut();
    }

    private void UICurrencyManager_OnCurrencyFailedToDrop(object sender, System.EventArgs e) {
        ShowBackpack(2f);
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        ShowBackpack(2f);

        if (e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember) {
            emberLight.enabled = true;
        }
    }

    private void UICurrencyManager_OnCurrencyTryPay(object sender, UICurrencyManager.OnCurrencyTryPayEventArgs e) {
        ShowBackpack(2f);

        if (e.currencyType == PlayerCurrencies.CurrencyType.ember) {
            emberLight.enabled = false;
        }
    }

    private void UICurrencyManager_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        ShowBackpack(2f);

        if (e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.ember) {
            emberLight.enabled = false;
        }
    }

    private void PlayerShoot_OnPlayerTryReloadAmmoBelt_NoAmmoInBag(object sender, System.EventArgs e) {
        ShowBackpack(2f);
    }

    private void ShowBackpack(float displayTime = 1f) {
        canvasGroup.alpha = 1;
        backpackDisplayTimer = displayTime;
        isFadingOut = false;
        isShowingBackpackFront = false;
        CheckBagIsAlmostFull();

        if (!isPlayerInventory) return;

        currencyCanvasGroup.alpha = 1;
        backpackFrontAnimator.ResetTrigger("ShowFront");
        backpackFrontAnimator.SetTrigger("HideFront");
    }

    private void HandleFadeIn() {
        // Réduit le timer pour le fade-in
        backpackDisplayTimer -= Time.deltaTime;

        float alpha = Mathf.Clamp01(1 - (backpackDisplayTimer / fadeInDuration));
        canvasGroup.alpha = alpha;

        // Quand le fade-in est terminé
        if (backpackDisplayTimer <= 0f) {
            canvasGroup.alpha = 1f;
            isFadingIn = false;
            backpackDisplayTimer = backpackDisplayTime; // Initialise le timer pour maintenir la barre visible
        }
    }

    private void HandleFadeOut() {

        // Si le timer est en cours et que le fade-out n'a pas commencé
        if (backpackDisplayTimer > 0f && !isFadingOut) {
            backpackDisplayTimer -= Time.deltaTime;

            // Démarre le fade-out lorsque le timer atteint 0
            if (backpackDisplayTimer <= 0f && !isShowingBackpackFront) {
                isShowingBackpackFront = true;
                backpackDisplayTimer = frontDisplayTime; // Initialise le timer pour le fade

                if (isPlayerInventory) {
                    backpackFrontAnimator.ResetTrigger("HideFront");
                    backpackFrontAnimator.SetTrigger("ShowFront");
                };
            }

            if (backpackDisplayTimer <= 0f && isShowingBackpackFront) {
                isFadingOut = true;
                backpackDisplayTimer = fadeOutDuration; // Initialise le timer pour le fade

                if(isPlayerInventory) {
                    currencyCanvasGroup.alpha = 0;
                }
            }

        }

        // Gestion du fade-out
        if (isFadingOut) {
            float alpha = Mathf.Clamp01(backpackDisplayTimer / fadeOutDuration);
            canvasGroup.alpha = alpha;

            // Réduit le timer pour le fade-out
            backpackDisplayTimer -= Time.deltaTime;

            // Quand le fade-out est terminé
            if (backpackDisplayTimer <= 0f) {
                canvasGroup.alpha = 0;
                isFadingOut = false;
            }
        }
    }

    private void CheckBagIsAlmostFull() {
        bool isAlmostFull = false;

        foreach(CurrencyUI_AlmostFullColliders almostFullCollider in almostFullColliders) {
            if(almostFullCollider.GetBagIsAlmostFull()) {
                isAlmostFull = true;
            }
        }
        backpackAlmostFull = isAlmostFull;
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        if (backpackAlmostFull) {
            //ShowBackpack(2f);
        }
    }

    private void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        if (backpackAlmostFull) {
            //ShowBackpack(2f);
        }
    }

    private void OnDestroy() {
        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Tutorial) {
            PlayerTabMenuUI.Instance.OnPlayerTabClosed -= PlayerTabMenuUI_OnPlayerTabClosed;
            PlayerTabMenuUI.Instance.OnPlayerTabOpened -= PLayerTabMenuUI_OnPlayerTabOpened;
        }


        if (uICurrencyManager == UICurrencyManager.HubInventoryUI) {
            HubChest.Instance.OnChestOpened -= HubChest_OnChestOpened;
            HubChest.Instance.OnChestClosed -= HubChest_OnChestClosed;
            HubMerchant.OnPlayerOpenedAnyHubMerchantShop -= HubMerchant_OnPlayerOpenedAnyHubMerchantShop;
            HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        }
    }

}
