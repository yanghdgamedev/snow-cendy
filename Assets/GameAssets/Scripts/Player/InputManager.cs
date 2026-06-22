using HDG.EventDispatcher;
using UnityEngine;

/// <summary>
/// Owns the on-screen joystick and gates it by game state: shown while the game
/// is playable (StartGame), hidden otherwise (EndGame / CancelGame).
/// Input sources (e.g. InputController) read <see cref="Direction"/> through the
/// singleton instead of touching the joystick directly.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private DynamicJoystick joystick;

    /// <summary>Raw joystick direction (X = horizontal, Y = vertical); zero while hidden.</summary>
    public Vector2 Direction =>
        (joystick != null && joystick.gameObject.activeSelf) ? joystick.Direction : Vector2.zero;

    private void Awake()
    {
        Instance = this;
        SetJoystickActive(false); // off until the game actually starts
    }

    private void OnEnable()
    {
        this.RegisterListener(EventID.StartGame, OnStartGame);
        this.RegisterListener(EventID.EndGame, OnEndGame);
        this.RegisterListener(EventID.CancelGame, OnEndGame);
    }

    private void OnDisable()
    {
        this.RemoveListener(EventID.StartGame, OnStartGame);
        this.RemoveListener(EventID.EndGame, OnEndGame);
        this.RemoveListener(EventID.CancelGame, OnEndGame);
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnStartGame(object obj) => SetJoystickActive(true);
    private void OnEndGame(object obj) => SetJoystickActive(false);

    private void SetJoystickActive(bool active)
    {
        if (joystick != null)
        {
            joystick.gameObject.SetActive(active);
        }
    }
}
