using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyController : MonoBehaviour
{
    private LobbyManager manager;
    private int myIndex;

    void Start()
    {
        manager = Object.FindAnyObjectByType<LobbyManager>();
        myIndex = GetComponent<UnityEngine.InputSystem.PlayerInput>().playerIndex;
        manager.UpdateUI(); // Refresh UI saat ada yang join
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Vector2 input = context.ReadValue<Vector2>();

        int current = (myIndex == 0) ? GameData.Instance.p0Side : GameData.Instance.p1Side;
        int target = current;

        if (input.x < -0.5f) target--;
        else if (input.x > 0.5f) target++;

        target = Mathf.Clamp(target, -1, 1);

        if (target != current) manager.RequestMove(myIndex, target);
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        // 1. Cek apakah context performed
        if (!context.performed) return;

        // 2. Cek apakah manager sudah terhubung (Fix NullReference)
        if (manager == null) manager = Object.FindAnyObjectByType<LobbyManager>();

        if (manager != null)
        {
            // 3. Tambahkan syarat: Hanya P1 yang bisa Start DAN P1 & P2 tidak boleh di tengah
            int p0Side = GameData.Instance.p0Side;
            int p1Side = GameData.Instance.p1Side;

            if (myIndex == 0 && p0Side != 0 && p1Side != 0 && p0Side != p1Side)
            {
                manager.LaunchGame();
            }
            else
            {
                Debug.Log("Pilih sisi dulu sebelum memulai game!");
            }
        }
    }
}