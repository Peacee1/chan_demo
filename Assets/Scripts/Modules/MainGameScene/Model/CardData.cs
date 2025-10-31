using DG.Tweening;
using UnityEngine;
public class CardData : MonoBehaviour
{
    [SerializeField] private CardManager cardManager;
    //cardType = 1(Văn) 2(Vạn) 3(Sach) 0(ChiChi)
    //cardSuit = 1(Nhi) 2(Tam) 3(Tu) 4(Ngu) 5(Luc) 6(That) 7(Bat) 8(Cuu) 0(ChiChi)
    [Header("Card Stats")]
    public int cardType;
    public int cardSuit;
    public bool isOpen;
    public bool canChoose;
    public bool canBeEaten = false;

    [Header("Ref")]
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D stackCollider;
    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        stackCollider = GetComponentInChildren<BoxCollider2D>();
        cardManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<CardManager>();
    }
    public int GetCardValue()   
    {
        int typeValue = (int)cardType;
        int suitValue = (int)cardSuit;
        return typeValue * 10 + suitValue;
    }
    public void UpdateSprite()
    {
        // Chỉ enable collider khi lá mở và có thể chọn (tức là lá trên tay)
        stackCollider.enabled = isOpen && canChoose;

        if(!isOpen) 
        {
            spriteRenderer.sprite = cardManager.cardSprite[cardManager.cardSprite.Length-1];
        }
        else
        {
            if(cardType == 0 && cardSuit == 0) 
            {
                spriteRenderer.sprite = cardManager.cardSprite[0];
            }
            else
            {
                spriteRenderer.sprite = cardManager.cardSprite[(cardType-1)*8 + cardSuit];
            }
        }
    }
    public void OnCardSelected()
    {
        if (isOpen && canChoose)
        {
            transform.GetChild(0).DOLocalMoveY(1.4f, 0.2f);
        }
    }
    public void OnCardUnselected()
    {
        transform.GetChild(0).DOLocalMoveY(1.2f, 0.2f);
    }
}
