using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Master controller handling game flow, user wagering, outcome verification, and UI updates.
/// </summary>
public class SlotGameManager : MonoBehaviour
{
    [Header("Reel References")]
    [SerializeField] private List<ReelController> reels = new List<ReelController>();
    [SerializeField] private List<SymbolData> availableSymbols = new List<SymbolData>();

    [Header("Timing Parameters")]
    [Tooltip("Base duration before the first reel begins deceleration.")]
    [SerializeField] private float baseSpinDuration = 1.6f;

    [Tooltip("Staggered delay between consecutive reel stops.")]
    [SerializeField] private float reelStopInterval = 0.35f;

    [Header("Economy Settings")]
    [SerializeField] private int initialCredits = 1000;
    [SerializeField] private int currentBet = 10;

    [Header("User Interface")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI betText;
    [SerializeField] private Button spinButton;
    [SerializeField] private GameObject winPopup;
    [SerializeField] private TextMeshProUGUI winAmountPopupText;

    private int _currentCredits;
    private int _stoppedReelsCounter;
    private SymbolData[] _activeSpinOutcomes;
    private bool _isSpinInProgress;
    private Coroutine _popupCoroutine;

    private readonly Color _winTextColor = new Color(1f, 0.85f, 0.2f);
    private readonly Color _lossTextColor = Color.white;

    private void Awake()
    {
        _currentCredits = initialCredits;
        _activeSpinOutcomes = new SymbolData[reels.Count];
    }

    private void Start()
    {
        UpdateEconomyUI();

        for (int i = 0; i < reels.Count; i++)
        {
            if (reels[i] != null)
            {
                reels[i].Initialize(availableSymbols);
            }
        }

        if (winPopup != null)
        {
            winPopup.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (spinButton != null)
        {
            spinButton.onClick.AddListener(OnSpinButtonPressed);
        }
    }

    private void OnDisable()
    {
        if (spinButton != null)
        {
            spinButton.onClick.RemoveListener(OnSpinButtonPressed);
        }
    }

    /// <summary>
    /// Validates bankroll and initiates spin execution.
    /// </summary>
    public void OnSpinButtonPressed()
    {
        if (_isSpinInProgress || _currentCredits < currentBet) return;

        _currentCredits -= currentBet;
        _isSpinInProgress = true;

        if (spinButton != null) spinButton.interactable = false;
        if (winPopup != null) winPopup.SetActive(false);

        UpdateEconomyUI();
        StartCoroutine(RunSpinSequence());
    }

    /// <summary>
    /// Executes staggered spinning across all registered reels.
    /// </summary>
    private IEnumerator RunSpinSequence()
    {
        _stoppedReelsCounter = 0;

        // Resolve outcome via weighted RNG before reels begin slowing down
        for (int i = 0; i < reels.Count; i++)
        {
            _activeSpinOutcomes[i] = RNGManager.GetRandomSymbol(availableSymbols);
        }

        // Trigger staggered stop sequences
        for (int i = 0; i < reels.Count; i++)
        {
            float reelDuration = baseSpinDuration + (i * reelStopInterval);
            reels[i].StartSpinning(_activeSpinOutcomes[i], reelDuration, OnIndividualReelHalted);
        }

        yield return new WaitUntil(() => _stoppedReelsCounter >= reels.Count);
        EvaluateWinningConditions();
    }

    private void OnIndividualReelHalted()
    {
        _stoppedReelsCounter++;
    }

    /// <summary>
    /// Compares payline symbols across all reels to determine payout.
    /// </summary>
    private void EvaluateWinningConditions()
    {
        bool isJackpot = true;
        SymbolData leadSymbol = _activeSpinOutcomes[0];

        for (int i = 1; i < _activeSpinOutcomes.Length; i++)
        {
            if (_activeSpinOutcomes[i] == null || leadSymbol == null ||
                _activeSpinOutcomes[i].SymbolId != leadSymbol.SymbolId)
            {
                isJackpot = false;
                break;
            }
        }

        if (isJackpot && leadSymbol != null)
        {
            int winnings = leadSymbol.PayoutMultiplier * currentBet;
            _currentCredits += winnings;
            ShowStatusPopup($"JACKPOT!\n+{winnings}", _winTextColor);
        }
        else
        {
            ShowStatusPopup("Better Luck\nNext Time!", _lossTextColor);
        }

        _isSpinInProgress = false;
        if (spinButton != null)
        {
            spinButton.interactable = _currentCredits >= currentBet;
        }

        UpdateEconomyUI();
    }

    private void ShowStatusPopup(string message, Color displayColor)
    {
        if (winPopup == null) return;

        if (_popupCoroutine != null)
        {
            StopCoroutine(_popupCoroutine);
        }

        winPopup.SetActive(true);
        if (winAmountPopupText != null)
        {
            winAmountPopupText.text = message;
            winAmountPopupText.color = displayColor;
        }

        _popupCoroutine = StartCoroutine(DismissPopupAfterDelay(2.0f));
    }

    private IEnumerator DismissPopupAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (winPopup != null)
        {
            winPopup.SetActive(false);
        }
    }

    private void UpdateEconomyUI()
    {
        if (creditsText != null) creditsText.text = $"CREDITS: {_currentCredits}";
        if (betText != null) betText.text = $"BET: {currentBet}";
    }
}