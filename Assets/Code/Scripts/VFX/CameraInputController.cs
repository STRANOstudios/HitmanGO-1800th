using Cinemachine;
using Player;
using UnityEngine;

namespace VFX.Player
{
    [RequireComponent(typeof(CinemachineInputProvider))]
    public class CameraInputController : MonoBehaviour
    {
        private CinemachineInputProvider inputProvider;

        private bool isEnabled = false;

        private void Start()
        {
            inputProvider = GetComponent<CinemachineInputProvider>();
        }

        private void OnEnable()
        {
            PlayerHandle.OnPlayerStateChange += Toggle;
        }

        private void OnDisable()
        {
            PlayerHandle.OnPlayerStateChange -= Toggle;
        }

        private void Update()
        {
            if (isEnabled)
            {
                inputProvider.enabled = false;
                return;
            }

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                inputProvider.enabled = true;
            }
            else
            {
                inputProvider.enabled = false;
            }
        }

        private void Toggle(PlayerState state)
        {
            isEnabled = state == PlayerState.ACTIVE;
        }
    }
}
