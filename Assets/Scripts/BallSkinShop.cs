using TMPro;
using UnityEngine;

/// <summary>
/// Optional material-based skin selector. Configure materials and prices in Inspector.
/// Attach to a UI manager and wire each shop button to BuyOrSelectSkin(index).
/// </summary>
public class BallSkinShop : MonoBehaviour
{
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Material[] skinMaterials;
    [Tooltip("Price per skin index. Index 0 is always free.")]
    [SerializeField] private int[] skinPrices;
    [Tooltip("Used when Skin Prices has no entry for a skin (prevents accidental free skins).")]
    [SerializeField, Min(0)] private int fallbackPrice = 50;
    [SerializeField] private TMP_Text statusText;

    private const string SelectedSkinKey = "ColorDashSelectedSkin";
    private const string OwnedPrefix = "ColorDashSkinOwned_";

    private void Start()
    {
        ApplySelectedSkin();
    }

    public void BuyOrSelectSkin(int index)
    {
        if (skinMaterials == null || index < 0 || index >= skinMaterials.Length)
        {
            SetStatus("Skin index is invalid.");
            return;
        }

        if (playerRenderer == null)
        {
            SetStatus("Assign the Player Renderer in Inspector.");
            return;
        }

        if (IsOwned(index))
        {
            SelectSkin(index);
            return;
        }

        int price = GetPrice(index);

        if (GameManager.Instance == null || !GameManager.Instance.SpendCoins(price))
        {
            SetStatus("Not enough coins.");
            return;
        }

        PlayerPrefs.SetInt(OwnedPrefix + index, 1);
        PlayerPrefs.Save();
        SelectSkin(index);
    }

    private int GetPrice(int index)
    {
        if (skinPrices != null && index < skinPrices.Length)
            return Mathf.Max(0, skinPrices[index]);

        Debug.LogWarning("[BallSkinShop] No price set for skin " + index +
                         ". Using fallback price " + fallbackPrice + ".");
        return fallbackPrice;
    }

    private bool IsOwned(int index)
    {
        return index == 0 || PlayerPrefs.GetInt(OwnedPrefix + index, 0) == 1;
    }

    private void SelectSkin(int index)
    {
        if (!IsOwned(index))
        {
            SetStatus("Skin is locked.");
            return;
        }

        // sharedMaterial avoids creating a new material instance on every change.
        playerRenderer.sharedMaterial = skinMaterials[index];
        PlayerPrefs.SetInt(SelectedSkinKey, index);
        PlayerPrefs.Save();
        SetStatus("Skin selected: " + skinMaterials[index].name);
    }

    private void ApplySelectedSkin()
    {
        if (playerRenderer == null || skinMaterials == null || skinMaterials.Length == 0)
            return;

        int index = Mathf.Clamp(PlayerPrefs.GetInt(SelectedSkinKey, 0), 0, skinMaterials.Length - 1);
        if (IsOwned(index))
            playerRenderer.sharedMaterial = skinMaterials[index];
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
