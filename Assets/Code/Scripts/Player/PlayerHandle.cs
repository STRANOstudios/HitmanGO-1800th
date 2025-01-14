using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerHandle : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] public LayerMask playerMask;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;
        [ShowIfGroup("_debug")]
        [ShowInInspector, ReadOnly] private PlayerState playerState = PlayerState.IDLE;
        [ShowIfGroup("_debug")]
        [ShowInInspector, ReadOnly] private Vector2 startPos;
        [ShowIfGroup("_debug")]
        [ShowInInspector, ReadOnly] private Vector2 endPos;
        [ShowIfGroup("_debug")]
        [ShowInInspector, ReadOnly] private Vector3 swipeDirection;
        [ShowIfGroup("_debug")]
        [ShowInInspector, ReadOnly] private bool isEnable = false;

        [SerializeField] private bool _debugLog = false;

        private float raycastDistance = 100f;

        public static event Action<PlayerState> OnPlayerStateChange;
        public static event Action<Vector3> OnPlayerSwipe;

        private void OnEnable()
        {
            ShiftManager.OnPlayerTurn += Toggle;
            PlayerController.OnPlayerEndTurn += Toggle;
        }

        private void OnDisable()
        {
            ShiftManager.OnPlayerTurn -= Toggle;
            PlayerController.OnPlayerEndTurn -= Toggle;
        }

        private void Update()
        {
            if (!isEnable) return;

            HandlePlayerTurn();
        }

        private void Toggle()
        {
            isEnable = !isEnable;
        }

        private void HandlePlayerTurn()
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                if (_debugLog) Debug.Log("Touching");

                Vector2 inputPosition;

                if (Input.GetMouseButtonDown(0))
                    inputPosition = Input.mousePosition;
                else
                    inputPosition = Input.GetTouch(0).position;

                Ray ray = Camera.main.ScreenPointToRay(inputPosition);

                Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red, 1f);

                if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, playerMask))
                {
                    if (hit.collider != null)
                    {
                        startPos = inputPosition;
                        PlayerChangeStatus(PlayerState.ACTIVE);
                    }
                }
            }

            if ((Input.GetMouseButtonUp(0) || (Input.touchCount == 0 && playerState == PlayerState.ACTIVE)) && playerState == PlayerState.ACTIVE)
            {
                if (_debugLog) Debug.Log("Swiping");

                if (Input.GetMouseButtonUp(0))
                    endPos = Input.mousePosition;
                else if (Input.touchCount == 0)
                    endPos = Input.GetTouch(0).position;

                Vector2 swipeDirectionScreen = (endPos - startPos).normalized;

                // Crea un Ray dal punto sullo schermo (endPos)
                Ray swipeRay = Camera.main.ScreenPointToRay(endPos);

                // Variabile per memorizzare il risultato dell'intersezione del raycast

                // Esegui il Raycast per ottenere la posizione nel mondo 3D
                if (Physics.Raycast(swipeRay, out RaycastHit hit))
                {
                    // La posizione dell'intersezione con il piano 3D
                    Vector3 worldPosition = hit.point;

                    if (_debugLog) Debug.Log("End Position (World): " + worldPosition);

                    Debug.DrawRay(swipeRay.origin, swipeRay.direction * raycastDistance, Color.red, 1f);
                    Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.green, 1f);

                    // Usalo per altre azioni
                    OnPlayerSwipe?.Invoke(worldPosition);
                }
                else
                {
                    // Se non colpisce nulla, puoi decidere cosa fare (es: utilizzare un punto predefinito)
                    Debug.Log("Raycast non ha colpito nulla.");
                }

                PlayerChangeStatus(PlayerState.IDLE);
            }

        }

        private void PlayerChangeStatus(PlayerState state)
        {
            if (state == playerState) return;

            OnPlayerStateChange?.Invoke(state);
            playerState = state;
        }
    }

    public enum PlayerState
    {
        IDLE,
        ACTIVE
    }
}
