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

    public static event Action ShiftEnemy;
    public static event Action ShiftPlayer;

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
        ShiftEnemy?.Invoke();
        StartCoroutine(EnemyTurnTimer());
    }

    private void BeginPlayerTurn()
    {
        ShiftPlayer?.Invoke();
        IsPlayerTurn = true;
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
