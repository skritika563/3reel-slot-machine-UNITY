using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the movement, animation phases, and positioning for an individual slot reel.
/// </summary>
public class ReelController : MonoBehaviour
{
    [Header("Layout Settings")]
    [Tooltip("Vertical spacing between adjacent symbol slots in pixels.")]
    [SerializeField] private float symbolHeight = 90f;

    [Tooltip("UI Image slots ordered from bottom to top (Index 1 is payline center).")]
    [SerializeField] private List<Image> symbolSlots = new List<Image>();

    [Header("Animation Tuning")]
    [SerializeField] private float spinSpeed = 1600f;
    [SerializeField] private float bounceOffset = 14f;

    private List<SymbolData> _symbolPool;
    private SymbolData _targetPaylineSymbol;
    private Vector2 _defaultLocalPosition;

    // Vertical offsets: [0]=Bottom, [1]=Center/Payline, [2]=Top, [3]=Buffer
    private float[] _targetSlotYOffsets;

    private void Awake()
    {
        _defaultLocalPosition = transform.localPosition;
        _targetSlotYOffsets = new float[]
        {
            -symbolHeight,
            0f,
            symbolHeight,
            symbolHeight * 2f
        };
    }

    /// <summary>
    /// Binds symbol assets and aligns all slots to starting grid coordinates.
    /// </summary>
    public void Initialize(List<SymbolData> symbols)
    {
        _symbolPool = symbols;

        // Auto-detect UI Images if unassigned in the inspector
        if (symbolSlots == null || symbolSlots.Count == 0)
        {
            symbolSlots = new List<Image>(GetComponentsInChildren<Image>());
        }

        for (int i = 0; i < symbolSlots.Count; i++)
        {
            if (symbolSlots[i] != null)
            {
                SetRandomSymbol(symbolSlots[i]);
            }
        }

        ResetSlotPositions();
    }

    /// <summary>
    /// Initiates the spin sequence targeting a predetermined payline outcome.
    /// </summary>
    public void StartSpinning(SymbolData targetSymbol, float duration, Action onSpinCompleted)
    {
        _targetPaylineSymbol = targetSymbol;
        StartCoroutine(SpinSequence(duration, onSpinCompleted));
    }

    private IEnumerator SpinSequence(float duration, Action onSpinCompleted)
    {
        if (symbolSlots.Count == 0)
        {
            onSpinCompleted?.Invoke();
            yield break;
        }

        // 1. Acceleration Phase
        float currentSpeed = 0f;
        while (currentSpeed < spinSpeed)
        {
            currentSpeed += Time.deltaTime * spinSpeed * 4f;
            ScrollSlots(currentSpeed * Time.deltaTime);
            yield return null;
        }

        // 2. Constant Velocity Spin Phase
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            ScrollSlots(spinSpeed * Time.deltaTime);
            yield return null;
        }

        // 3. Smooth Deceleration & Grid Alignment
        yield return StartCoroutine(DecelerateAndSnap());

        // 4. Elastic Recoil Game-Feel
        yield return StartCoroutine(BounceRoutine());

        onSpinCompleted?.Invoke();
    }

    /// <summary>
    /// Translates symbol rects downwards and cycles off-screen symbols back to the top.
    /// </summary>
    private void ScrollSlots(float deltaY)
    {
        float bottomBoundary = -symbolHeight * 1.5f;
        float wrapTopAnchor = symbolHeight * 2.5f;

        for (int i = 0; i < symbolSlots.Count; i++)
        {
            if (symbolSlots[i] == null) continue;

            RectTransform rt = symbolSlots[i].rectTransform;
            rt.anchoredPosition -= new Vector2(0f, deltaY);

            // Re-wrap off-screen items to maintain continuous loop
            if (rt.anchoredPosition.y < bottomBoundary)
            {
                float excess = bottomBoundary - rt.anchoredPosition.y;
                rt.anchoredPosition = new Vector2(0f, wrapTopAnchor - excess);
                SetRandomSymbol(symbolSlots[i]);
            }
        }
    }

    /// <summary>
    /// Smoothly glides slots to exact snap points and injects the target symbol into the payline.
    /// </summary>
    private IEnumerator DecelerateAndSnap()
    {
        float elapsed = 0f;
        const float snapDuration = 0.22f;

        // Cache positions at moment of stop
        Vector2[] initialPositions = new Vector2[symbolSlots.Count];
        for (int i = 0; i < symbolSlots.Count; i++)
        {
            if (symbolSlots[i] != null)
            {
                initialPositions[i] = symbolSlots[i].rectTransform.anchoredPosition;
            }
        }

        // Apply pre-determined outcome to middle payline slot (Index 1)
        if (symbolSlots.Count > 1 && symbolSlots[1] != null && _targetPaylineSymbol != null)
        {
            symbolSlots[1].sprite = _targetPaylineSymbol.SymbolSprite;
        }

        // Ensure visible upper and lower slots hold valid sprites
        if (symbolSlots.Count > 0 && symbolSlots[0].sprite == null) SetRandomSymbol(symbolSlots[0]);
        if (symbolSlots.Count > 2 && symbolSlots[2].sprite == null) SetRandomSymbol(symbolSlots[2]);

        while (elapsed < snapDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / snapDuration);

            for (int i = 0; i < symbolSlots.Count && i < _targetSlotYOffsets.Length; i++)
            {
                if (symbolSlots[i] != null)
                {
                    float interpolatedY = Mathf.Lerp(initialPositions[i].y, _targetSlotYOffsets[i], progress);
                    symbolSlots[i].rectTransform.anchoredPosition = new Vector2(0f, interpolatedY);
                }
            }
            yield return null;
        }

        ResetSlotPositions();
    }

    /// <summary>
    /// Provides kinetic physical feedback when the reel halts.
    /// </summary>
    private IEnumerator BounceRoutine()
    {
        Vector2 downPosition = _defaultLocalPosition - new Vector2(0f, bounceOffset);

        // Downward overshoot
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 12f;
            transform.localPosition = Vector2.Lerp(_defaultLocalPosition, downPosition, t);
            yield return null;
        }

        // Recovery to default resting position
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 12f;
            transform.localPosition = Vector2.Lerp(downPosition, _defaultLocalPosition, t);
            yield return null;
        }

        transform.localPosition = _defaultLocalPosition;
    }

    private void ResetSlotPositions()
    {
        for (int i = 0; i < symbolSlots.Count && i < _targetSlotYOffsets.Length; i++)
        {
            if (symbolSlots[i] != null)
            {
                symbolSlots[i].rectTransform.anchoredPosition = new Vector2(0f, _targetSlotYOffsets[i]);
            }
        }
    }

    private void SetRandomSymbol(Image targetImage)
    {
        if (_symbolPool == null || _symbolPool.Count == 0 || targetImage == null) return;

        SymbolData randomSymbol = RNGManager.GetRandomSymbol(_symbolPool);
        if (randomSymbol != null)
        {
            targetImage.sprite = randomSymbol.SymbolSprite;
        }
    }
}