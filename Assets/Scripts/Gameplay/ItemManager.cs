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
        Blue,
        Red,
        Yellow,
    }

    public Berry equipped = Berry.Blue;

    public System.Action<Berry> OnEquippedChanged;


    private void Start()
    {
        GameManager.Instance.AnyLevelLoaded += LevelLoaded;

        blueButton.onClick.AddListener(BlueButtonPressed);
        redButton.onClick.AddListener(RedButtonPressed);
        yellowButton.onClick.AddListener(YellowButtonPressed);
    }

    private void OnDestroy()
    {
        GameManager.Instance.AnyLevelLoaded -= LevelLoaded;
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
        equipped = Berry.Blue;
        OnEquippedChanged?.Invoke(equipped);
    }
    public void RedButtonPressed()
    {
        equipped = Berry.Red;
        OnEquippedChanged?.Invoke(equipped);
    }
    public void YellowButtonPressed()
    {
        equipped = Berry.Yellow;
        OnEquippedChanged?.Invoke(equipped);
    }
}
