using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button blueButton;
    [SerializeField] private Button redButton;
    [SerializeField] private Button yellowButton;
    [SerializeField] private RectTransform equippedArrow;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction blueAction;
    private InputAction redAction;
    private InputAction yellowAction;
    private InputAction continueAction;

    public enum Berry
    {
        Blue = 0,
        Red = 1,
        Yellow = 2,
    }

    public static BerryItem equipped;

    public static System.Action<BerryItem> OnEquippedChanged;

    public static Dictionary<Berry, BerryItem> Berries = new();

    private static Berry[] berriesAvailable;


    private void Start()
    {
        GameManager.Instance.OnLevelLoaded += LevelLoaded;

        blueButton.onClick.AddListener(BlueButtonPressed);
        redButton.onClick.AddListener(RedButtonPressed);
        yellowButton.onClick.AddListener(YellowButtonPressed);

        Berries.Add(Berry.Blue,
            new BerryItem(
                Berry.Blue,
                "Blue",
                Resources.Load<Sprite>("Sprites/Berries/Blue"),
                blueButton.GetComponent<RectTransform>().position + new Vector3(120.0f, 0.0f, 0.0f)
        ));
        Berries.Add(Berry.Red,
            new BerryItem(
                Berry.Red,
                "Red",
                Resources.Load<Sprite>("Sprites/Berries/Red"),
                redButton.GetComponent<RectTransform>().position + new Vector3(120.0f, 0.0f, 0.0f)
        ));
        Berries.Add(Berry.Yellow,
            new BerryItem(
                Berry.Yellow,
                "Yellow",
                Resources.Load<Sprite>("Sprites/Berries/Yellow"),
                yellowButton.GetComponent<RectTransform>().position + new Vector3(120.0f, 0.0f, 0.0f)
        ));

        equipped = GetBerry(Berry.Blue);

        inputActions.FindActionMap("Player").Enable();

        blueAction = inputActions.FindAction("Berry_Blue");
        redAction = inputActions.FindAction("Berry_Red");
        yellowAction = inputActions.FindAction("Berry_Yellow");
        continueAction = inputActions.FindAction("Continue");
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnLevelLoaded -= LevelLoaded;
    }

    private void Update()
    {
        if (blueAction.WasPressedThisFrame())
            BlueButtonPressed();
        if (redAction.WasPressedThisFrame())
            RedButtonPressed();
        if (yellowAction.WasPressedThisFrame())
            YellowButtonPressed();
        if (continueAction.WasPressedThisFrame())
            GameManager.Instance.ui.HideTut();
    }

    public static BerryItem GetBerry(Berry type)
    {
        return Berries[type];
    }

    void LevelLoaded(LevelData level)
    {
        berriesAvailable = level.berriesAvailable;

        bool blue = berriesAvailable.Contains(Berry.Blue);
        bool red = berriesAvailable.Contains(Berry.Red);
        bool yellow = berriesAvailable.Contains(Berry.Yellow);

        equipped = GetBerry(Berry.Blue);
        UpdateEquipped();

        UpdateButtons(blue, red, yellow);
    }

    void UpdateButtons(bool blue, bool red, bool yellow)
    {   
        blueButton.gameObject.SetActive(blue);
        redButton.gameObject.SetActive(red);
        yellowButton.gameObject.SetActive(yellow);
    }

    public void BlueButtonPressed()
    {
        if (berriesAvailable.Contains(Berry.Blue))
        {
            equipped = GetBerry(Berry.Blue);
            UpdateEquipped();
        }

    }
    public void RedButtonPressed()
    {
        if (berriesAvailable.Contains(Berry.Red))
        {
            equipped = GetBerry(Berry.Red);
            UpdateEquipped();
        }
    }
    public void YellowButtonPressed()
    {
        if (berriesAvailable.Contains(Berry.Yellow))
        {
            equipped = GetBerry(Berry.Yellow);
            UpdateEquipped();
        }
    }

    public void UpdateEquipped()
    {
        equippedArrow.position = equipped.arrowPos;
        OnEquippedChanged?.Invoke(equipped);
    }
}

public class BerryItem
{
    public ItemManager.Berry type;
    public string name { get; private set; }
    public Sprite sprite { get; private set; }
    public Vector3 arrowPos;

    public BerryItem(ItemManager.Berry _type, string _name, Sprite _sprite, Vector3 _arrowPos)
    {
        type = _type;
        name = _name;
        sprite = _sprite;
        arrowPos = _arrowPos;
    }
}