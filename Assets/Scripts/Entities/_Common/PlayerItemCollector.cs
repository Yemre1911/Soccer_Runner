/*using UnityEngine;

public class PlayerItemCollector : MonoBehaviour, IItemReceiver
{
    public void ReceiveItem(ItemDefinitionSO itemDefinition)
    {
        if (itemDefinition == null)
        {
            Debug.LogWarning("[ItemCollector] Null itemDefinition received.", this);
            return;
        }

        switch (itemDefinition.useType)
        {
            case ItemUseType.Booster:
                HandleBooster(itemDefinition);
                break;

            case ItemUseType.Ability:
                HandleAbility(itemDefinition);
                break;

            case ItemUseType.PassiveWorldItem:
                HandlePassiveWorldItem(itemDefinition);
                break;

            default:
                Debug.LogWarning($"[ItemCollector] Unsupported item use type: {itemDefinition.useType}", this);
                break;
        }
    }

    private void HandleBooster(ItemDefinitionSO itemDefinition)
    {
        Debug.Log(
            $"[ItemCollector] Booster received -> {itemDefinition.displayName} | Activation: {itemDefinition.activationMode} | Duration: {itemDefinition.duration}",
            this
        );
    }

    private void HandleAbility(ItemDefinitionSO itemDefinition)
    {
        Debug.Log(
            $"[ItemCollector] Ability received -> {itemDefinition.displayName} | Activation: {itemDefinition.activationMode} | DurationBased: {itemDefinition.isDurationBased}",
            this
        );
    }

    private void HandlePassiveWorldItem(ItemDefinitionSO itemDefinition)
    {
        Debug.Log(
            $"[ItemCollector] Passive item received -> {itemDefinition.displayName}",
            this
        );
    }
}*/

using UnityEngine;

public class PlayerItemCollector : MonoBehaviour, IItemReceiver
{
    [Header("Refs")]
    [SerializeField] private BoosterController boosterController;

    private void Awake()
    {
        if (!boosterController)
            boosterController = GetComponent<BoosterController>();
    }

    public void ReceiveItem(ItemDefinitionSO itemDefinition)
    {
        if (itemDefinition == null)
        {
            Debug.LogWarning("[ItemCollector] Null itemDefinition received.", this);
            return;
        }

        switch (itemDefinition.useType)
        {
            case ItemUseType.Booster:
                HandleBooster(itemDefinition);
                break;

            case ItemUseType.Ability:
                HandleAbility(itemDefinition);
                break;

            case ItemUseType.PassiveWorldItem:
                HandlePassiveWorldItem(itemDefinition);
                break;

            default:
                Debug.LogWarning($"[ItemCollector] Unsupported item use type: {itemDefinition.useType}", this);
                break;
        }
    }

    private void HandleBooster(ItemDefinitionSO itemDefinition)
    {
        Debug.Log(
            $"[ItemCollector] Booster received -> {GetLabel(itemDefinition)} | Activation: {itemDefinition.activationMode} | Duration: {itemDefinition.duration}",
            this
        );

        if (boosterController == null)
        {
            Debug.LogWarning("[ItemCollector] BoosterController missing.", this);
            return;
        }

        boosterController.ActivateBooster(itemDefinition);
    }

    private void HandleAbility(ItemDefinitionSO itemDefinition)
    {
        Debug.Log(
            $"[ItemCollector] Ability received -> {GetLabel(itemDefinition)} | Activation: {itemDefinition.activationMode} | DurationBased: {itemDefinition.isDurationBased}",
            this
        );

        // Sonraki adımda AbilityInventory'ye bağlayacağız.
    }

    private void HandlePassiveWorldItem(ItemDefinitionSO itemDefinition)
    {
        Debug.Log($"[ItemCollector] Passive item received -> {GetLabel(itemDefinition)}", this);
    }

    private static string GetLabel(ItemDefinitionSO itemDefinition)
    {
        if (itemDefinition == null) return "NULL";
        return string.IsNullOrWhiteSpace(itemDefinition.displayName)
            ? itemDefinition.name
            : itemDefinition.displayName;
    }
}