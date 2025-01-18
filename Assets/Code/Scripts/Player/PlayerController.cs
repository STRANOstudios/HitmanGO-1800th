using Agents;
using Interactables;
using PathSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] private Node m_startNode;

        [SerializeField, Range(0, 360)] private float maxAllowedAngle = 45f;

        [Title("Debug")]
        [SerializeField] private bool _debug = false;
        [ShowIfGroup("_debug")]
        [ShowInInspector, ReadOnly] private List<Angle> angles = new();
        [ShowIfGroup("_debug")]
        [ShowInInspector, ReadOnly] private PlayerVisibilityState m_visibilityState = PlayerVisibilityState.Visible;
        [ShowIfGroup("_debug")]
        [ShowInInspector, ReadOnly] private Node currentNode;

        [SerializeField] private bool _debugLog = false;
        [SerializeField] private bool _drawGizmos = true;

        private bool OnDistractor = false;

        public static event Action OnPlayerMove;
        public static event Action OnPlayerEndMove;
        public static event Action OnPlayerEndTurn;
        public static event Action OnPlayerDistractionReady;

        private void Awake()
        {
            ServiceLocator.Instance.Player = this;

            currentNode = m_startNode;

            currentNode.Storages.Add(this.gameObject);

            if (_debugLog) Debug.Log("Current Node: " + currentNode.name);

            transform.position = currentNode.transform.position;
        }

        private void Start()
        {
            CalculateAngle();
        }

        private void OnEnable()
        {
            PlayerHandle.OnPlayerSwipe += CheckAngle;
        }

        private void OnDisable()
        {
            PlayerHandle.OnPlayerSwipe -= CheckAngle;
        }

        private void CheckAngle(Vector3 point)
        {
            Node node = null;

            // calcola il coefficiente angolare
            Vector3 origin = transform.position;
            float slope = Utils.CalculateSlope(origin, point);

            if (_debugLog) Debug.Log($"{origin} | {point} | Slope: {slope}°");

            foreach (Angle angle in angles)
            {
                if (Utils.IsAngleInRange(slope, angle.angleMin, angle.angleMax))
                {
                    node = angle.node;
                    break;
                }
            }

            if (node != null)
            {
                Debug.DrawLine(node.transform.position, node.transform.position + Vector3.up, Color.magenta, 1f);
                PlayerTurn(node);
            }
        }

        private void PlayerTurn(Node node)
        {
            StartCoroutine(Movement(node));
        }

        private IEnumerator Movement(Node node)
        {
            OnPlayerMove?.Invoke();

            while (Vector3.Distance(transform.position, node.transform.position) > 0.1f)
            {
                yield return null;

                transform.position = Vector3.MoveTowards(transform.position, node.transform.position, 1f);
            }

            OnPlayerEndMove?.Invoke();

            yield return new WaitForSeconds(0.5f);

            //PlayerEndTurn();

            StoreNode(node);
        }

        private void CalculateAngle()
        {
            angles.Clear();

            foreach (Node node in currentNode.neighbours)
            {
                float slope = Utils.CalculateSlope(transform.position, node.transform.position);

                float maxAngle = Utils.NormalizeAngle(slope + (maxAllowedAngle / 2));
                float minAngle = Utils.NormalizeAngle(slope - (maxAllowedAngle / 2));

                angles.Add(new(node, maxAngle, minAngle));
            }
        }

        private void PlayerEndTurn()
        {
            CalculateAngle();

            if(!OnDistractor)
                OnPlayerEndTurn?.Invoke();
            else OnDistractor = false;
        }

        public void StoreNode(Node node)
        {
            Distractor tmp = null;

            m_visibilityState = PlayerVisibilityState.Visible;

            for (int i = 0; i < node.Storages.Count; i++)
            {
                if (node.Storages[i].CompareTag("Enemy"))
                {
                    node.Storages[i].GetComponent<Agent>().Death();
                    continue;
                }
                else if (node.Storages[i].CompareTag("Distractor"))
                {
                    OnPlayerDistractionReady?.Invoke();
                    OnDistractor = true;
                    tmp = node.Storages[i].GetComponent<Distractor>();
                }

                if (node.Storages[i].CompareTag("HiddenPlace"))
                {
                    m_visibilityState = PlayerVisibilityState.Hidden;
                }
            }

            node.Storages.Add(gameObject);
            currentNode.Storages.Remove(gameObject);

            currentNode = node;

            if (OnDistractor)
                tmp.Spawn();

            PlayerEndTurn();
        }

        public Node CurrentNode => currentNode;
        public bool IsVisible => m_visibilityState == PlayerVisibilityState.Visible;

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;

            foreach (Angle angle in angles)
            {
                // Posizione del nodo
                Vector3 nodePosition = angle.node.transform.position;

                // Calcoliamo il vettore direzionale dal nodo all'origine
                Vector3 directionToNode = (nodePosition - transform.position).normalized;

                // Angoli da convertire in radiante per il calcolo dei vettori
                float minAngleRad = Mathf.Deg2Rad * angle.angleMin;
                float maxAngleRad = Mathf.Deg2Rad * angle.angleMax;

                // Calcoliamo i vettori radiali
                Vector3 radialVectorMin = new Vector3(Mathf.Cos(minAngleRad), 0f, Mathf.Sin(minAngleRad));
                Vector3 radialVectorMax = new Vector3(Mathf.Cos(maxAngleRad), 0f, Mathf.Sin(maxAngleRad));

                // Disegniamo la retta del nodo
                Vector3 perpendicularDirection = Vector3.Cross(directionToNode, Vector3.up).normalized;

                // Disegniamo i vettori radiali
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + radialVectorMin * 10f);  // Vettore radiale minimo
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + radialVectorMax * 10f);  // Vettore radiale massimo

                // Calcolare l'intersezione
                Vector3 intersection1 = Utils.CalculateRayIntersection(new Ray(nodePosition, perpendicularDirection), transform.position, radialVectorMin);
                Vector3 intersection2 = Utils.CalculateRayIntersection(new Ray(nodePosition, perpendicularDirection), transform.position, radialVectorMax);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(intersection1, intersection2);

                // Disegnare le intersezioni
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(intersection1, 0.1f);  // Intersezione 1
                Gizmos.DrawSphere(intersection2, 0.1f);  // Intersezione 2
            }
        }

        [Serializable]
        private struct Angle
        {
            public Node node;
            public float angleMax;
            public float angleMin;

            public Angle(Node node, float angleMax, float angleMin)
            {
                this.node = node;
                this.angleMax = angleMax;
                this.angleMin = angleMin;
            }
        }
    }

    public enum PlayerVisibilityState
    {
        Visible,
        Hidden,
        Stealth,
        Inactive
    }
}
