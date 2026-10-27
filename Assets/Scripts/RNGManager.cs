using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles statistical randomness and decoupled symbol outcome calculations.
/// </summary>
public static class RNGManager
{
    /// <summary>
    /// Selects a SymbolData from a pool using weighted cumulative distribution.
    /// Higher dropWeight increases selection frequency.
    /// </summary>
    /// <param name="symbolPool">Available symbols configured for the machine.</param>
    /// <returns>Selected SymbolData outcome.</returns>
    public static SymbolData GetRandomSymbol(List<SymbolData> symbolPool)
    {
        if (symbolPool == null || symbolPool.Count == 0)
        {
            Debug.LogWarning("[RNGManager] Provided symbol pool is null or empty.");
            return null;
        }

        // Calculate total weight accumulator
        int totalWeight = 0;
        for (int i = 0; i < symbolPool.Count; i++)
        {
            totalWeight += symbolPool[i].DropWeight;
        }

        int randomRoll = Random.Range(0, totalWeight);
        int accumulatedWeight = 0;

        for (int i = 0; i < symbolPool.Count; i++)
        {
            accumulatedWeight += symbolPool[i].DropWeight;
            if (randomRoll < accumulatedWeight)
            {
                return symbolPool[i];
            }
        }

        // Fallback default
        return symbolPool[0];
    }
}