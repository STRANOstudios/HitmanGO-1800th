using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ShiftManager : MonoBehaviour
{
    [Title("Settings")]
    [SerializeField, Unit(Units.Second, Units.Second), MinValue(0f)] private float _shiftEnemyDuration = 1f;

    [Title("Debug")]
    [SerializeField] private bool _debug = false;
    [SerializeField, ShowIf("_debug")] private bool _autoShift = false;
    [ShowInInspector, HideLabel, ShowIf("_debug"), ProgressBar(0, "_shiftEnemyDuration", DrawValueLabel = true, CustomValueStringGetter = "$GetProgressBarLabel")]
    private double Animate
    {
        get
        {
            Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
            return IsPlayerTurn ? 0 : Time.time - timerStart;
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
        OnPlayerTurn?.Invoke();
        IsPlayerTurn = true;

        if (_autoShift)
        {
            BeginEnemyTurn();
        }
    }

    private IEnumerator EnemyTurnTimer()
    {
        yield return new WaitForSeconds(_shiftEnemyDuration);
        BeginPlayerTurn();
    }

    private string GetProgressBarLabel()
    {
        if (IsPlayerTurn)
        {
            return "Player Turn";
        }
        else
        {
            double elapsed = Time.time - timerStart;
            return $"Enemy Turn ({elapsed:F1}/{_shiftEnemyDuration}s)";
        }
    }
}
