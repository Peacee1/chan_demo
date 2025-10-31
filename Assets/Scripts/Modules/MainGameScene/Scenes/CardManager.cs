using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;

public class CardManager : MonoBehaviour
{
    [Header("Player Hand")]
    [SerializeField] private Transform handAnchor;
    [SerializeField] private Transform botHandAnchor;
    [Header("Board Position")]
    [SerializeField] private Transform board, board1;
    [SerializeField] private Transform botBoard;
    [Header("Card Manager")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameManager gameManager;
    [SerializeField] public Sprite[] cardSprite;
    [SerializeField] public CardData selectedCards;
    [SerializeField] private UManager uManager;
    [SerializeField] private BaoManager batBao;
    public bool hasDrawnThisTurn = false;
    public bool hasEatenThisTurn = false;
    public bool mustPlayDrawnCard = false;  // bốc xong -> phải đánh đúng lá đó
    public bool hasPlayedThisTurn = false;  // đã đánh trong lượt
    public bool actionTakenThisTurn = false; // đã ăn hoặc bốc trong lượt

    private List<(int type, int suit)> deck = new List<(int, int)>();
    private bool canAn = true;
    private int opponentCardsEaten = 0;
    public CardData lastDrawnCard = null;

    private void Awake()
    {
        cardSprite = Resources.LoadAll<Sprite>("Cards");
    }

    void Start()
    {
        if (cardPrefab == null || handAnchor == null)
        {
            Debug.LogError("Chưa gán cardPrefab hoặc handAnchor!");
            return;
        }
        SortCards();
    }

    // Tạo bộ bài (mỗi lá max 4 lần)

    public void SortCards()
    {
        CardData[] cardDatas = handAnchor.GetComponentsInChildren<CardData>();
        List<CardData> cards = new List<CardData>();
        foreach (CardData cardData in cardDatas)
        {
            cardData.isOpen = true;
            cardData.canChoose = true;
            cardData.UpdateSprite();
            cards.Add(cardData);
        }
        var group = GroupCards(cards);
        var sorted = new List<CardData>();
        sorted.AddRange(group.Que);
        sorted.AddRange(group.Ca.SelectMany(p => new[] { p.Item1, p.Item2 }));
        sorted.AddRange(group.Chan.SelectMany(p => new[] { p.Item1, p.Item2 }));

        // Đảm bảo thứ tự các lá bài trong handAnchor đúng với sorted
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].transform.SetSiblingIndex(i);
        }

        DisplayCardsFan(sorted, handAnchor);
    }
    void DisplayCardsFan(List<CardData> sortedCards, Transform handAnchor)
    {
        int n = sortedCards.Count;
        if (n == 0) return;

        // Giới hạn min-max góc xòe
        float minAngle = 30f;      // khi chỉ còn ít lá
        float maxAngleFull = 190f; // khi đủ lá
        int fullHand = 19;         // số lá tối đa ban đầu

        // Tính góc tối đa theo số lá còn lại
        float t = Mathf.Clamp01((float)(n - 1) / (fullHand - 1));
        float usedAngle = Mathf.Lerp(minAngle, maxAngleFull, t);

        float step = (n > 1) ? usedAngle / (n - 1) : 0f;
        float startAngle = -usedAngle / 2f;   // căn giữa

        for (int i = 0; i < n; i++)
        {
            var card = sortedCards[i];
            GameObject cardGO = card.gameObject;
            cardGO.SetActive(true);
            cardGO.transform.SetParent(handAnchor, false);

            cardGO.transform.localPosition = Vector3.zero;
            cardGO.transform.localRotation = Quaternion.identity;

            float angle = startAngle + (i * step);

            float delay = i * 0.02f;
            cardGO.transform.DOLocalRotate(
                new Vector3(0, 0, angle),
                0.5f,
                RotateMode.FastBeyond360
            ).SetEase(Ease.OutBack)
            .SetDelay(delay).OnComplete(() =>
            {
                card.transform.GetChild(0).transform.DOLocalMove(new Vector2(0, 1.2f), 0.1f);
            });
        }
    }
    private CardGroupResult GroupCards(List<CardData> cards)
    {
        var result = new CardGroupResult();
        var remaining = new List<CardData>(cards);

        // Tìm chắn
        var grouped = remaining
            .GroupBy(c => new { c.cardType, c.cardSuit })
            .Where(g => g.Count() >= 2)
            .ToList();

        foreach (var g in grouped)
        {
            var list = g.Take(2).ToList();
            result.Chan.Add((list[0], list[1]));
            remaining.Remove(list[0]);
            remaining.Remove(list[1]);
        }

        // Tìm cạ
        remaining = remaining.OrderBy(c => c.cardType).ThenBy(c => c.cardSuit).ToList();
        var used = new HashSet<CardData>();

        for (int i = 0; i < remaining.Count; i++)
        {
            var c1 = remaining[i];
            if (used.Contains(c1)) continue;

            for (int j = i + 1; j < remaining.Count; j++)
            {
                var c2 = remaining[j];
                if (used.Contains(c2)) continue;

                if (c1.cardSuit == c2.cardSuit &&
                    Mathf.Abs((int)c1.cardType - (int)c2.cardType) == 1)
                {
                    result.Ca.Add((c1, c2));
                    used.Add(c1);
                    used.Add(c2);
                    break;
                }
            }
        }

        // Còn lại là què
        foreach (var c in remaining)
        {
            if (!used.Contains(c))
                result.Que.Add(c);
        }

        // 🧹 Sắp xếp các nhóm từ thấp đến cao
        result.Chan = result.Chan
            .OrderBy(pair => (int)pair.Item1.cardType)
            .ThenBy(pair => (int)pair.Item1.cardSuit)
            .ToList();

        result.Ca = result.Ca
            .OrderBy(pair => Mathf.Min((int)pair.Item1.cardType, (int)pair.Item2.cardType))
            .ThenBy(pair => (int)pair.Item1.cardSuit)
            .ToList();

        result.Que = result.Que
            .OrderBy(c => (int)c.cardType)
            .ThenBy(c => (int)c.cardSuit)
            .ToList();

        return result;
    }
    public void UnselectedCard()
    {
        CardData[] cardDatas = handAnchor.GetComponentsInChildren<CardData>();
        foreach (CardData cardData in cardDatas)
        {
            cardData.OnCardUnselected();
        }
    }
    // ====== ĐÁNH ======
    public void Danh()
    {
        if (selectedCards == null) return;

        // ❌ Chặn: chỉ được đánh nếu đã Ăn
        if (!hasEatenThisTurn)
        {
            PopMessageManager.Instance.Show("Chỉ được đánh sau khi ăn!");
            return;
        }
        if (!canAn)
        {
            PopMessageManager.Instance.Show("Chỉ được đánh sau khi ăn!");
            return;
        }

        // ✅ Nếu đánh hợp lệ
        hasPlayedThisTurn = true;
        hasEatenThisTurn = false;  // reset sau khi đánh
        actionTakenThisTurn = false;
        hasDrawnThisTurn = false;

        StartCoroutine(DanhCoroutine(selectedCards));
    }

    private IEnumerator DanhCoroutine(CardData selected)
    {
        GameObject cardGO = selected.gameObject;

        int maxCardsOnBoard = 4;
        float spacingX = 0.3f;
        float spacingZ = -0.02f;
        float moveDur = 0.25f;

        // Snapshot các lá hiện có
        List<Transform> existing = new List<Transform>();
        for (int i = 0; i < board.childCount; i++)
            existing.Add(board.GetChild(i));

        bool willRemoveFirst = existing.Count >= maxCardsOnBoard;

        if (willRemoveFirst)
        {
            Transform first = existing[0];
            if (first != null)
                Destroy(first.gameObject);

            for (int i = 1; i < existing.Count; i++)
            {
                Transform t = existing[i];
                int newIndex = i - 1;
                Vector3 targetWorld = board.position + new Vector3(newIndex * spacingX, 0f, newIndex * spacingZ);

                var cdOld = t.GetComponent<CardData>();
                if (cdOld != null)
                {
                    cdOld.canBeEaten = false;
                    cdOld.canChoose = false;
                    cdOld.UpdateSprite();
                }

                t.DOMove(targetWorld, moveDur).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    if (t != null && t.parent != board)
                        t.SetParent(board);
                    t.localPosition = new Vector3(newIndex * spacingX, 0f, newIndex * spacingZ);
                    t.localRotation = Quaternion.identity;   // ✅ luôn để thẳng
                    t.SetSiblingIndex(newIndex);
                });
            }
            yield return new WaitForSeconds(moveDur + 0.01f);
            if (first != null) Destroy(first.gameObject);
        }

        int afterCount = board.childCount;
        int targetIndex = Mathf.Clamp(afterCount, 0, maxCardsOnBoard - 1);
        Vector3 targetWorldPos = board.position + new Vector3(targetIndex * spacingX, 0f, targetIndex * spacingZ);

        // Tách lá từ tay
        cardGO.transform.SetParent(null);
        selected.transform.GetChild(0).localPosition = Vector3.zero;

        // Animate lá mới bay xuống
        cardGO.transform.DOMove(targetWorldPos, moveDur).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            cardGO.transform.SetParent(board);
            cardGO.transform.localPosition = new Vector3(targetIndex * spacingX, 0f, targetIndex * spacingZ);
            cardGO.transform.localRotation = Quaternion.identity;  // ✅ để thẳng đứng

            var sr = cardGO.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = targetIndex;

            var cd = cardGO.GetComponent<CardData>();
            if (cd != null)
            {
                cd.canBeEaten = true;
                cd.canChoose = false;
                cd.UpdateSprite();
            }

            gameManager.NextTurn();
            DisplayCardsFanUpdate();
        });

        // Khóa toàn bộ lá trên sân trước
        for (int i = 0; i < board.childCount; i++)
        {
            var t = board.GetChild(i).GetComponent<CardData>();
            if (t != null)
            {
                t.canBeEaten = false;
                t.canChoose = false;
                t.UpdateSprite();
            }
        }

        // ✅ Chỉ mở lá cuối cùng
        if (board.childCount > 0)
        {
            var cdLast = board.GetChild(board.childCount - 1).GetComponent<CardData>();
            if (cdLast != null)
            {
                cdLast.canBeEaten = true;
                cdLast.canChoose = false;
                cdLast.UpdateSprite();
            }
        }

        canAn = true;
        hasDrawnThisTurn = false;
        selectedCards = null;
        yield break;
    }
    // ====== ĂN ======
    public void An()
    {
        if (!canAn)
        {
            PopMessageManager.Instance.Show("Không được ăn lúc này !");
            return;
        }

        if (hasEatenThisTurn)
        {
            PopMessageManager.Instance.Show("Đã ăn trong lượt này rồi !");
            return;
        }
        CardData target = null;
        // Nếu vừa bốc -> chỉ ăn lá bốc
        if (hasDrawnThisTurn && lastDrawnCard != null)
        {
            target = lastDrawnCard;
        }
        else
        {
            // Nếu chưa bốc -> ăn lá bot vừa đánh (lá cuối botBoard)
            if (botBoard.childCount > 0)
            {
                target = botBoard.GetChild(botBoard.childCount - 1).GetComponent<CardData>();
            }
        }

        if (target == null || !target.canBeEaten)
        {
            PopMessageManager.Instance.Show("Không có lá nào để ăn!");
            return;
        }

        CardData[] playerCards = handAnchor.GetComponentsInChildren<CardData>();

        CardData matchingCard = playerCards
            .FirstOrDefault(c => c.cardType == target.cardType && c.cardSuit == target.cardSuit);

        if (matchingCard == null)
        {
            matchingCard = playerCards
                .FirstOrDefault(c => c.cardSuit == target.cardSuit && c.cardType != target.cardType);
        }

        if (matchingCard != null)
        {
            matchingCard.isOpen = true;
            matchingCard.canChoose = false;
            matchingCard.transform.GetChild(0).localPosition = Vector3.zero;

            StartCoroutine(AnAnimation(board1.transform, matchingCard, target));

            opponentCardsEaten++;
            
            if (opponentCardsEaten >= 3 && uManager != null)
                uManager.EnableUButton(true);
            if (opponentCardsEaten >= 7 && batBao != null)
                batBao.ShowPanel();

            // Sau khi ăn xong -> đánh tiếp
            hasEatenThisTurn = true;
            actionTakenThisTurn = true;
            hasDrawnThisTurn = false;
            lastDrawnCard = null;
            mustPlayDrawnCard = false;
        }
        else
        {
            Debug.Log("Không có quân phù hợp để ăn!");
        }
    }

    // ====== BỐC ======
    public void Boc()
    {
        if (hasEatenThisTurn)
        {
            PopMessageManager.Instance.Show("Đã ăn không được bốc!");
            return;
        }

        if (hasDrawnThisTurn)
        {
            PopMessageManager.Instance.Show("Đã bốc trong lượt!");
            return;
        }

        if (transform.childCount == 0)
        {
            PopMessageManager.Instance.Show("Hết bài để bốc!");
            return;
        }
        hasDrawnThisTurn = true;
        actionTakenThisTurn = true;
        mustPlayDrawnCard = true;    // chỉ xử lý lá vừa bốc
        hasPlayedThisTurn = false;

        // Lấy lá đầu tiên từ bộ bài
        Transform cardT = transform.GetChild(0);
        CardData card = cardT.GetComponent<CardData>();

        card.isOpen = true;
        card.canChoose = false;
        card.UpdateSprite();

        cardT.SetParent(null);

        // Vị trí trên board
        int afterCount = board.childCount;
        int targetIndex = Mathf.Clamp(afterCount, 0, 3); // max 4 lá
        Vector3 targetWorldPos = board.position + new Vector3(targetIndex * 0.3f, 0f, targetIndex * -0.02f);

        // đặt thẳng (không nghiêng)
        cardT.rotation = Quaternion.identity;

        // Animate bay ra board
        cardT.DOMove(targetWorldPos, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            cardT.SetParent(board);
            cardT.localPosition = new Vector3(targetIndex * 0.3f, 0f, targetIndex * -0.02f);

            // Chỉ lá bốc mới có thể bị ăn
            card.canBeEaten = true;
            card.UpdateSprite();

            // lưu lại lá vừa bốc để chỉ được ăn nó
            lastDrawnCard = card;

            // disable ăn cho các lá cũ trên board
            for (int i = 0; i < board.childCount - 1; i++)
            {
                var old = board.GetChild(i).GetComponent<CardData>();
                if (old != null)
                {
                    old.canBeEaten = false;
                    old.UpdateSprite();
                }
            }

            canAn = true;
            selectedCards = null;
            DisplayCardsFanUpdate();
        });
    }


    // ====== BỎ LƯỢT ======
    public void BoLuot()
    {
        if (!hasDrawnThisTurn || hasEatenThisTurn)
        {
            PopMessageManager.Instance.Show("Bỏ lượt sau khi bốc mà không ăn!");
            return;
        }
        Debug.Log("Bạn bỏ lượt, tới bot!");

        // Reset lại
        hasDrawnThisTurn = false;
        lastDrawnCard = null;
        mustPlayDrawnCard = false;
        actionTakenThisTurn = true;

        gameManager.NextTurn();
    }
    private IEnumerator AnAnimation(Transform target, CardData card1, CardData card2)
    {
        // Reset hiển thị
        card1.transform.rotation = Quaternion.identity;
        card1.isOpen = true;
        card1.UpdateSprite();

        card2.transform.rotation = Quaternion.identity;
        card2.isOpen = true;
        card2.UpdateSprite();

        float duration = 0.5f;

        // Chỉ số chồng mới (mỗi bộ ăn được coi là 1 stack)
        int stackIndex = target.childCount / 2; // vì mỗi bộ có 2 lá
        Vector3 stackPoint = target.position + new Vector3(stackIndex * 0.4f, 0, 0);

        // Tween di chuyển 2 lá về cùng chỗ
        card1.transform.DOMove(stackPoint, duration).SetEase(Ease.InOutQuad);
        card2.transform.DOMove(stackPoint, duration).SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(duration);

        // Đổi parent sang board
        card1.transform.SetParent(target);
        card2.transform.SetParent(target);

        // Gom chồng: cùng X, lệch Y một chút
        Vector3 baseLocal = new Vector3(stackIndex * 0.33f, 0.5f, 0);
        card1.transform.localPosition = baseLocal;
        card2.transform.localPosition = baseLocal + new Vector3(0, 0.25f, -0.1f);

        card1.transform.GetChild(1).gameObject.SetActive(true);

        // Clear selected
        selectedCards = null;

        // Cập nhật lại tay bài player
        DisplayCardsFanUpdate();
    }
    void DisplayCardsFanUpdate()
    {
        CardData[] sortedCards = handAnchor.GetComponentsInChildren<CardData>();
        int n = sortedCards.Length;
        if (n == 0) return;

        // Giới hạn min-max góc xòe
        float minAngle = 30f;      // khi chỉ còn ít lá
        float maxAngleFull = 190f; // khi đủ lá
        int fullHand = 19;         // số lá tối đa ban đầu

        // Tính góc tối đa theo số lá còn lại
        float t = Mathf.Clamp01((float)(n - 1) / (fullHand - 1));
        float usedAngle = Mathf.Lerp(minAngle, maxAngleFull, t);

        float step = (n > 1) ? usedAngle / (n - 1) : 0f;
        float startAngle = -usedAngle / 2f;   // căn giữa

        for (int i = 0; i < n; i++)
        {
            var card = sortedCards[i];
            GameObject cardGO = card.gameObject;

            // Gắn parent, giữ nguyên local position cũ
            cardGO.transform.SetParent(handAnchor, false);

            // Góc xoè
            float angle = startAngle + (i * step);

            // Tween xoay
            cardGO.transform.DOLocalRotate(
                new Vector3(0, 0, angle),
                0.3f,
                RotateMode.Fast
            ).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                card.transform.GetChild(0).transform.localPosition = new Vector3(0, 1.2f, 0);
            });
        }
    }
    public void ResetTurnFlags()
    {
        hasEatenThisTurn = false;
        mustPlayDrawnCard = false;
        actionTakenThisTurn = false;
        hasDrawnThisTurn = false;
        hasPlayedThisTurn = false;
        lastDrawnCard = null;
        selectedCards = null;

        Debug.Log("CardManager flags reset!");
    }
}
public class CardGroupResult
{
    public List<(CardData, CardData)> Chan = new();
    public List<(CardData, CardData)> Ca = new();
    public List<CardData> Que = new();
}
