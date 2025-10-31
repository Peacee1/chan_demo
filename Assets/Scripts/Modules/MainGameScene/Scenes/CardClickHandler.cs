using UnityEngine;
using UnityEngine.EventSystems;

public class CardClickHandler : MonoBehaviour, IPointerClickHandler
{
    private CardData cardData;
    [SerializeField] private CardManager cardManager;


    private void Awake()
    {
        cardData = GetComponent<CardData>();
        cardManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<CardManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Clicked on card: {cardData.cardType} - {cardData.cardSuit}");
        cardManager.selectedCards = gameObject.GetComponent<CardData>();
        cardManager.UnselectedCard();
        gameObject.GetComponent<CardData>().OnCardSelected();
    }
}
