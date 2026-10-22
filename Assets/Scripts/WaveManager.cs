using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Spawns dwarf waves in sequence: [1, 2, 4]. Waits until every dwarf in the
/// current wave is dead before spawning the next wave. When all waves are
/// cleared, it fires OnAllWavesCleared (used to spawn the health potion /
/// unlock the door to the dragon room).
/// Attach to an empty "WaveManager" GameObject in Room 1.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject dwarfPrefab;
    public Transform[] spawnPoints;      // Assign 4+ spawn points in the room
    public int[] waveSizes = { 1, 2, 4 }; // Wave 1 = 1 dwarf, Wave 2 = 2, Wave 3 = 4

    [Header("Timing")]
    public float delayBeforeFirstWave = 1f;
    public float delayBetweenWaves = 2f;

    public static event Action OnAllWavesCleared;
    public static event Action<int, int> OnWaveStarted; // (waveIndex, waveCount)

    private int currentWaveIndex = -1;
    private int aliveInCurrentWave = 0;
    private bool allWavesFinished = false;

    void OnEnable()
    {
        Dwarf.OnDwarfDied += HandleDwarfDied;
    }

    void OnDisable()
    {
        Dwarf.OnDwarfDied -= HandleDwarfDied;
    }

    void Start()
    {
        Invoke(nameof(StartNextWave), delayBeforeFirstWave);
    }

    void StartNextWave()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= waveSizes.Length)
        {
            allWavesFinished = true;
            OnAllWavesCleared?.Invoke();
            return;
        }

        int count = waveSizes[currentWaveIndex];
        aliveInCurrentWave = count;
        OnWaveStarted?.Invoke(currentWaveIndex + 1, waveSizes.Length);

        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
            Instantiate(dwarfPrefab, spawnPoint.position, Quaternion.identity);
        }
    }

    void HandleDwarfDied(Dwarf dwarf)
    {
        if (allWavesFinished) return;

        aliveInCurrentWave--;
        if (aliveInCurrentWave <= 0)
        {
            Invoke(nameof(StartNextWave), delayBetweenWaves);
        }
    }
}
