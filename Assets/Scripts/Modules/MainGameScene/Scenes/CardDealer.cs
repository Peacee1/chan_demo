using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardDealer : MonoBehaviour
{
    public static CardDealer Instance;

    [Header("Prefabs & Data")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private List<CardData> allCards = new();

    [Header("Animation Settings")]
    [SerializeField] private int cardsPerStack = 10;
    [SerializeField] private int handSize = 19; // số lá mỗi người phải có trên tay
    [SerializeField] private float dealInterval = 0.05f;
    [SerializeField] private float dealRadius = 1f;      // bán kính vòng tròn chia bài
    [SerializeField] private float centerSpacing = 1f;   // khoảng cách giữa 5 stack trung tâm
    [SerializeField] private float downSpace = 2f;
    [SerializeField] private CardManager cardManager;

    [Header("Stack")]
    [SerializeField] private StackClickHandler[] stackParents;   // 5 stack đã đặt sẵn trong scene

    [Header("Ref")]
    [SerializeField] private Transform playerHandAnchor, botHandAnchor;

    private List<List<GameObject>> groupedStacks = new();
    private bool canClick = false;
    [SerializeField] public GameObject BackCard;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Kiểm tra cardPrefab
        if (cardPrefab == null)
        {
            Debug.LogError("cardPrefab is not assigned in CardDealer! Please assign it in the Inspector.");
        }

        // Kiểm tra và khởi tạo playerHandAnchor
        if (playerHandAnchor == null)
        {
            playerHandAnchor = new GameObject("PlayerHandAnchor").transform;
            Debug.LogWarning("Created new PlayerHandAnchor");
        }

        // Kiểm tra và khởi tạo botHandAnchor
        if (botHandAnchor == null)
        {
            botHandAnchor = new GameObject("BotHandAnchor").transform;
            Debug.LogWarning("Created new BotHandAnchor");
        }

        // Kiểm tra và khởi tạo stackParents
        if (stackParents == null || stackParents.Length != 5)
        {
            stackParents = new StackClickHandler[5];
            for (int i = 0; i < 5; i++)
            {
                GameObject stack = new GameObject($"Stack_{i}");
                stackParents[i] = stack.AddComponent<StackClickHandler>();
                stackParents[i].stackIndex = i;
                stackParents[i].cardDealer = this;
            }
            Debug.LogWarning("Created new stackParents");
        }
        BackCard.SetActive(false);
    }
    private void Start()
    {
        StartGame();
    }
    public void StartGame()
    {
        StopAllCoroutines();
        ResetGame();
        InitDeck();
        StartCoroutine(DealCards(allCards));
    }

  private void ResetGame()
    {
        Debug.Log("Resetting game...");
        Cleanup();

        // Xóa bài cũ trong transform của Dealer
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        allCards.Clear();

        // Xóa bài trong tay player
        if (playerHandAnchor != null)
        {
            foreach (Transform child in playerHandAnchor)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("playerHandAnchor is null or missing in CardDealer!");
        }

        // Xóa bài trong tay bot
        if (botHandAnchor != null)
        {
            foreach (Transform child in botHandAnchor)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("botHandAnchor is null or missing in CardDealer!");
        }

        // Xóa stack
        if (stackParents != null)
        {
            foreach (var stack in stackParents)
            {
                if (stack != null)
                {
                    foreach (Transform child in stack.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("stackParents is null in CardDealer!");
        }

        groupedStacks.Clear();
        canClick = false;
        Debug.Log($"Game reset complete. Card count: {allCards.Count}, Player hand: {(playerHandAnchor != null ? playerHandAnchor.childCount : 0)}, Bot hand: {(botHandAnchor != null ? botHandAnchor.childCount : 0)}");
    }
    private void InitDeck()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    GameObject card = Instantiate(cardPrefab, Vector3.zero, Quaternion.Euler(0, 0, -37));
                    card.transform.parent = cardManager.transform;
                    CardData cardData = card.GetComponent<CardData>();
                    cardData.cardType = j + 1;
                    cardData.cardSuit = k + 1;
                    cardData.isOpen = false;
                    cardData.canChoose = false;
                    cardData.UpdateSprite();
                    allCards.Add(cardData);
                }
            }
            // Lá đặc biệt ChiChi
            GameObject cardChiChi = Instantiate(cardPrefab, Vector3.zero, Quaternion.Euler(0, 0, -37));
            cardChiChi.transform.parent = cardManager.transform;
            CardData cardDataChiChi = cardChiChi.GetComponent<CardData>();
            cardDataChiChi.cardType = 0;
            cardDataChiChi.cardSuit = 0;
            cardDataChiChi.canChoose = false;
            cardDataChiChi.isOpen = false;
            cardDataChiChi.UpdateSprite();
            allCards.Add(cardDataChiChi);
        }
    }

    // ===================== DEAL =====================
    IEnumerator DealCards(List<CardData> cards)
    {
        int totalStacks = 5; // Chia thành 5 bộ
        Vector3[] dealPoints = new Vector3[totalStacks];
        for (int i = 0; i < totalStacks; i++)
        {
            float angle = i * Mathf.PI * 2 / totalStacks;
            dealPoints[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * dealRadius;
        }

        groupedStacks.Clear();
        for (int i = 0; i < totalStacks; i++)
        {
            groupedStacks.Add(new List<GameObject>());
        }

        // Chia đều bài cho 5 bộ
        for (int i = 0; i < cardsPerStack; i++)
        {
            for (int j = 0; j < totalStacks; j++)
            {
                int cardIndex = j * cardsPerStack + i;

                if (cardIndex >= cards.Count)
                    yield break; // tránh lỗi nếu không đủ bài

                GameObject card = cards[cardIndex].gameObject;

                // Đặt tại center trước khi bay
                card.transform.position = Vector3.zero;

                // Animate ra vị trí stack
                card.transform.DOMove(dealPoints[j], 0.2f).SetEase(Ease.OutQuad);

                groupedStacks[j].Add(card);

                yield return new WaitForSeconds(dealInterval);
            }
        }

        yield return new WaitForSeconds(0.1f);
        StartCoroutine(MoveToCenterStacks());
    }


    IEnumerator MoveToCenterStacks()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i >= groupedStacks.Count || groupedStacks[i] == null)
            {
                Debug.LogError($"groupedStacks[{i}] is null or out of range!");
                continue;
            }

            List<GameObject> stack = groupedStacks[i];
            if (i >= stackParents.Length || stackParents[i] == null)
            {
                Debug.LogError($"stackParents[{i}] is null!");
                continue;
            }

            StackClickHandler parent = stackParents[i];
            parent.transform.position = new Vector3((i - 2) * centerSpacing, 0, 0);
            var handler = parent.GetComponent<StackClickHandler>() ?? parent.gameObject.AddComponent<StackClickHandler>();
            handler.stackIndex = i;
            handler.cardDealer = this;

            // Duyệt ngược để xóa null an toàn
            for (int j = stack.Count - 1; j >= 0; j--)
            {
                GameObject card = stack[j];
                if (card == null || card.transform == null)
                {
                    Debug.LogWarning($"Null card in stack {i}, removing it.");
                    stack.RemoveAt(j);
                    continue;
                }

                card.transform.DOMove(parent.transform.position, 0.25f).SetEase(Ease.InOutQuad);
                card.transform.SetParent(parent.transform); // Dòng 225
            }

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.3f);
        foreach (var p in stackParents)
        {
            if (p != null)
            {
                p.transform.DOMoveY(p.transform.position.y - downSpace, 0.3f).SetEase(Ease.InOutQuad);
            }
        }
        canClick = true;
    }
    // ===================== CLICK =====================
    public void OnStackClicked(int stackIndex)
    {
        if (!canClick) return;
        canClick = false;
        StartCoroutine(HandleStackClicked(stackIndex));
        BackCard.SetActive(true);
    }

    private IEnumerator HandleStackClicked(int stackIndex)
    {
        Debug.Log($"Clicked on card stack {stackIndex}");

        int randomStack = stackIndex;
        while (randomStack == stackIndex)
        {
            randomStack = Random.Range(0, stackParents.Length);
        }

        foreach (StackClickHandler stack in stackParents)
        {
            if (stack.stackIndex == stackIndex) // Player
            {
                Debug.Log("Stack người chơi");
                int child = stack.transform.childCount;

                stack.transform.DOMove(playerHandAnchor.position, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    for (int i = 0; i < child; i++)
                        stack.transform.GetChild(0).SetParent(playerHandAnchor);

                    // Bốc thêm cho đủ handSize
                    int need = handSize - child;
                    for (int i = 0; i < need && transform.childCount > 0; i++)
                    {
                        transform.GetChild(0).SetParent(playerHandAnchor);
                    }
                });
            }
            else if (stack.stackIndex == randomStack) // Bot
            {
                Debug.Log("Stack bot");
                int child = stack.transform.childCount;

                stack.transform.DOMove(botHandAnchor.position, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    for (int i = 0; i < child; i++)
                        stack.transform.GetChild(0).SetParent(botHandAnchor);

                    // Bốc thêm cho đủ handSize
                    int need = handSize - child;
                    for (int i = 0; i < need && transform.childCount > 0; i++)
                    {
                        transform.GetChild(0).SetParent(botHandAnchor);
                    }
                });
            }
            else
            {
                stack.transform.DOMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    int count = stack.transform.childCount;
                    for (int i = 0; i < count; i++)
                        stack.transform.GetChild(0).SetParent(transform);
                });
            }
        }
        yield return new WaitForSeconds(1.5f);
        cardManager.SortCards();
        botHandAnchor.GetComponent<Bot>().SortCards();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BeginFirstTurn();
        }
    }
    private void OnDestroy()
    {
        Cleanup();
    }

    public void Cleanup()
    {
        StopAllCoroutines(); // Dừng tất cả coroutine
        DOTween.KillAll(); // Dừng tất cả animation của DOTween
    }
}