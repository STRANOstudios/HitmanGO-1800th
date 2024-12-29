using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

public class ShiftManager : MonoBehaviour
{
    [Title("Settings")]
    [SerializeField, Unit(Units.Second, Units.Second), MinValue(0f)] private float _shiftEnemyDuration = 1f;
    [SerializeField, Unit(Units.Second, Units.Second), MinValue(0f)] private float _shiftPlayerDuration = 10f;

    [Title("Debug")]
    [SerializeField] private bool _debug = false;
    [SerializeField, ShowIf("_debug")] private bool _autoShift = false;
    [ShowInInspector, HideLabel, ShowIf("_debug"), ProgressBar(0, "_shiftEnemyDuration", DrawValueLabel = true, CustomValueStringGetter = "$GetProgressBarLabel")]
    private double Animate
    {
        get
        {
            Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
            return IsPlayerTurn ? Time.time - timerStart : 0;
        }
    }

    [ShowIf("_debug"), Button]
    private void ChangeTurn()
    {
        if (IsPlayerTurn)
        {
            BeginEnemyTurn();
        }
        else
        {
            BeginPlayerTurn();
        }
    }

    public static event Action OnEnemyTurn;
    public static event Action OnPlayerTurn;

    private bool IsPlayerTurn = true;
    private float timerStart;

    private void Start()
    {
        if (!_autoShift)
        {
            BeginPlayerTurn();
        }
        else
        {
            StartCoroutine(Timer());
        }
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1);
        BeginPlayerTurn();
    }

    private void OnEnable()
    {
        // ricevere l'evento di fine turno player dal player
    }

    private void OnDisable()
    {

    }

    private void BeginEnemyTurn()
    {
        timerStart = Time.time;
        IsPlayerTurn = false;
        OnEnemyTurn?.Invoke();
        StartCoroutine(EnemyTurnTimer());
    }

    private void BeginPlayerTurn()
    {
        timerStart = Time.time;
        IsPlayerTurn = true;
        OnPlayerTurn?.Invoke();

        if (_autoShift)
        {
            StartCoroutine(PlayerTurnTimer());
        }
    }

    private IEnumerator EnemyTurnTimer()
    {
        Debug.Log("Enemy Turn");
        yield return new WaitForSeconds(_shiftEnemyDuration);
        BeginPlayerTurn();
    }

    private IEnumerator PlayerTurnTimer()
    {
        Debug.Log("Player Turn");
        yield return new WaitForSeconds(_shiftPlayerDuration);
        BeginEnemyTurn();
    }

    private string GetProgressBarLabel()
    {

        if (!Application.isPlaying) return "";

        if (IsPlayerTurn)
        {
            double elapsed = Time.time - timerStart;
            return $"Player Turn ({elapsed:F1}/{_shiftPlayerDuration}s)";
        }
        else
        {
            double elapsed = Time.time - timerStart;
            return $"Enemy Turn ({elapsed:F1}/{_shiftEnemyDuration}s)";
        }
    }
}
