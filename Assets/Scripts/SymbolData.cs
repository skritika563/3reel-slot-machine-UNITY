using UnityEngine;

/// <summary>
/// ScriptableObject defining symbol attributes, visual assets payout calculations, and RNG drop weighting.
/// </summary>
[CreateAssetMenu(fileName = "NewSymbolData", menuName = "SlotGame/Symbol Data")]
public class SymbolData : ScriptableObject
{
    [Tooltip("Unique string identifier for payout comparison.")]
    [SerializeField] private string symbolId;

    [Tooltip("Sprite rendered in the UI reel slot.")]
    [SerializeField] private Sprite symbolSprite;

    [Tooltip("Multiplier applied to the active bet upon winning.")]
    [SerializeField] private int payoutMultiplier = 5;

    [Tooltip("Relative probability weight used in the weighted RNG draw.")]
    [Range(1, 100)]
    [SerializeField] private int dropWeight = 10;

    // Public getters preserving encapsulation
    public string SymbolId => symbolId;
    public Sprite SymbolSprite => symbolSprite;
    public int PayoutMultiplier => payoutMultiplier;
    public int DropWeight => dropWeight;
}