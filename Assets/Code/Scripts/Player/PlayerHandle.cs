using Interactables;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Player
{
    public class PlayerHandle : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] private LayerMask playerMask;
        [SerializeField] private LayerMask distractorMask;

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
            PlayerController.OnPlayerDistractionReady += OnDistractionReady;
        }

        private void OnDisable()
        {
            ShiftManager.OnPlayerTurn -= Toggle;
            PlayerController.OnPlayerEndTurn -= Toggle;
            PlayerController.OnPlayerDistractionReady -= OnDistractionReady;
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

                Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

                if (playerState == PlayerState.ITEM_READY)
                {
                    Debug.Log("Distractor");

                    if (Physics.Raycast(ray, out RaycastHit hit2, raycastDistance, distractorMask))
                    {
                        Debug.Log("hit");

                        if (hit2.collider.CompareTag("DistractorIndicator"))
                        {
                            DistractorCollider collider = hit2.collider.GetComponent<DistractorCollider>();
                            collider.distractor.SetTarget = collider.node;
                            playerState = PlayerState.IDLE;
                            isEnable = false;
                        }
                    }

                    return;
                }

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
                    if (_debugLog) Debug.Log("End Position (World): " + hit.point);
                    Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.green, 1f);

                    // Usalo per altre azioni
                    OnPlayerSwipe?.Invoke(hit.point);
                }
                else
                {
                    // Se non colpisce nulla, puoi decidere cosa fare (es: utilizzare un punto predefinito)
                    Debug.Log("Raycast non ha colpito nulla.");
                }

                if (playerState != PlayerState.ITEM_READY)
                    PlayerChangeStatus(PlayerState.IDLE);
            }

        }

        private void PlayerChangeStatus(PlayerState state)
        {
            if (state == playerState) return;

            OnPlayerStateChange?.Invoke(state);
            playerState = state;
        }

        private void OnDistractionReady()
        {
            PlayerChangeStatus(PlayerState.ITEM_READY);
        }
    }

    public enum PlayerState
    {
        IDLE,
        ACTIVE,
        ITEM_READY
    }
}
