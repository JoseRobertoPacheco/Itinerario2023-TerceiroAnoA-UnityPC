using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;

public class PlayerController : MonoBehaviour
{
    private const float _defaultFieldOfView = 60.0f;
    Camera _Camera;
    CharacterController _Controller;

    public static bool MovementEnabled = true;
    public static GameObject SoundsParent; 

    [Header("Character properties")]
    public float CharacterMovementSpeed = 10.0f;
    public float CameraSensitivity = 2.0f;
    private float CamX = 0f, CamY = 0f;

    [Space(20)]
    public Image TransitionImage;
    public TextMeshProUGUI TransitionCaptionText;

    [Header("Properties")]
    public AudioSource TransitionAudio;
    public FixedJoystick MovementJoystickHandler;
    public FixedJoystick CameraJoystickHandler;

    [Space(20)]
    public TextMeshProUGUI TimerText;

    private ObjectInteractionHandler _currentObjectController;
    private Vector3 _current_velocity = new Vector3();

    public IEnumerator Start()
    {
        _Camera = Camera.main;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.brightness = 1.0f;

        _Controller = GetComponentInParent<CharacterController>();

        TransitionCaptionText.color = new Color(0, 0, 0, 0);
        TransitionImage.color = new Color(0, 0, 0, 1);

        TransitionCaptionText.text = UserSettings.next_scene_title;

        UserSettings.CurrentPlayerController = this;
        transform.parent.transform.position = UserSettings.PlayerStartingPosition;

        void StartFadeout()
        {
            StartCoroutine(FadeoutScreen());
        }
        UserSettings.FadeoutScreen.AddListener(StartFadeout);

        yield return new WaitForSeconds(2);

        StartCoroutine(StartTransition());
    }

    public IEnumerator StartTransition()
    {
        if (Application.isMobilePlatform) TransitionAudio.Play();
        Destroy(gameObject.GetComponent<AudioListener>(), TransitionAudio.clip.length);

        void UpdateTransitionColor(Color c)
        {
            TransitionImage.color = c;
        }
        void UpdateCaptionColor(Color c)
        {
            TransitionCaptionText.color = c;
        }

        LeanTween.value(TransitionImage.gameObject, UpdateTransitionColor, new Color(0, 0, 0, 1), new Color(1, 1, 1, 1), 1f);

        yield return new WaitForSeconds(1.825f);

        TransitionCaptionText.color = new Color(0, 0, 0, 1);

        yield return new WaitForSeconds(0.5f);

        LeanTween.value(TransitionImage.gameObject, UpdateTransitionColor, new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), 1f);
        LeanTween.value(TransitionCaptionText.gameObject, UpdateCaptionColor, new Color(0, 0, 0, 1), new Color(1, 1, 1, 1), 1f);

        yield return new WaitForSeconds(6f);

        LeanTween.value(TransitionCaptionText.gameObject, UpdateCaptionColor, new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), 2f);

        yield return new WaitForSeconds(1f);

        UserSettings.TransitionEnded.Invoke();
    }

    public IEnumerator FadeoutScreen()
    {
        void UpdateTransitionColor(Color c)
        {
            TransitionImage.color = c;
        }
        LeanTween.value(TransitionImage.gameObject, UpdateTransitionColor, new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), 1f);

        yield return new WaitForSeconds(2f);
    }

    public void FixedUpdate()
    {
        if (_Controller == null || !MovementEnabled) return;

        // character movement
        if (Camera.main != null)
        {

            Vector2 MovementInput = MovementJoystickHandler.Direction;

            Transform Orientation = Camera.main.transform;
            Vector3 Direction = Orientation.forward * MovementInput.y + Orientation.right * MovementInput.x;
            Direction.y = -0.25f;

            _current_velocity = Vector3.Lerp(_current_velocity, Direction, 0.125f);
        }

        _Controller.Move(_current_velocity * CharacterMovementSpeed * Time.deltaTime);

        Vector2 CameraInput = CameraJoystickHandler.Direction * CameraSensitivity;
        CamX += CameraInput.x;
        CamY -= CameraInput.y;

        transform.eulerAngles = new Vector3(CamY, CamX, 0);
        transform.SetLocalPositionAndRotation(new Vector3(), transform.rotation);

        //transform.Rotate(0, 0.25f, 0, Space.Self);
    }

    public void Update()
    {
        TimerText.text = UserSettings.TimerText;
    }

    public IEnumerator PlayAudios(GameObject parent)
    {
        foreach (var Sound in parent.GetComponentsInChildren<AudioSource>())
        {
            Sound.Play();
            yield return new WaitForSeconds(Sound.clip.length);
        }
    }
}
