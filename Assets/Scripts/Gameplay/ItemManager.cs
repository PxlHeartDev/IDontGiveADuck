using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button blueButton;
    [SerializeField] private Button redButton;
    [SerializeField] private Button yellowButton;

    public enum Berry
    {
        Blue = 0,
        Red = 1,
        Yellow = 2,
    }

    public BerryItem equipped;

    public static System.Action<BerryItem> OnEquippedChanged;

    public Dictionary<Berry, BerryItem> Berries = new();


    private void Start()
    {
        GameManager.Instance.AnyLevelLoaded += LevelLoaded;

        blueButton.onClick.AddListener(BlueButtonPressed);
        redButton.onClick.AddListener(RedButtonPressed);
        yellowButton.onClick.AddListener(YellowButtonPressed);

        Berries.Add(Berry.Blue, new BerryItem(Berry.Blue, "Blue", Resources.Load<Sprite>("Sprites/Berries/Blue")));
        Berries.Add(Berry.Red, new BerryItem(Berry.Red, "Red", Resources.Load<Sprite>("Sprites/Berries/Red")));
        Berries.Add(Berry.Yellow, new BerryItem(Berry.Yellow, "Yellow", Resources.Load<Sprite>("Sprites/Berries/Yellow")));

        equipped = GetBerry(Berry.Blue);
    }

    private void OnDestroy()
    {
        GameManager.Instance.AnyLevelLoaded -= LevelLoaded;
    }

    public BerryItem GetBerry(Berry type)
    {
        return Berries[type];
    }

    void LevelLoaded(LevelData level)
    {
        Berry[] berries = level.berriesAvailable;

        bool blue = berries.Contains(Berry.Blue);
        bool red = berries.Contains(Berry.Red);
        bool yellow = berries.Contains(Berry.Yellow);

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
        equipped = GetBerry(Berry.Blue);
        OnEquippedChanged?.Invoke(equipped);
    }
    public void RedButtonPressed()
    {
        equipped = GetBerry(Berry.Red);
        OnEquippedChanged?.Invoke(equipped);
    }
    public void YellowButtonPressed()
    {
        equipped = GetBerry(Berry.Yellow);
        OnEquippedChanged?.Invoke(equipped);
    }
}

public class BerryItem
{
    public ItemManager.Berry type;
    public string Name { get; private set; }
    public Sprite Image { get; private set; }

    public BerryItem(ItemManager.Berry _type, string _name, Sprite _image)
    {
        type = _type;
        Name = _name;
        Image = _image;
    }
}