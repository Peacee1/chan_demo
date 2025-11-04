﻿using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class Bot : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform board, board1, playerboard;
    [SerializeField] private int cardsPerRow = 4;   // số lá mỗi hàng
    [SerializeField] private CardManager cardManager;

    private bool isPlaying = false;
    [SerializeField] private TextMeshProUGUI botNameText; // gắn UI text trong Inspector

    void Start()
    {
        string botName = BotNameGenerator.GenerateBotName();
        Debug.Log("Tên bot: " + botName);

        if (botNameText != null)
            botNameText.text = botName;
    }

    private void Update()
    {
        if (gameManager.turnState == GameManager.TurnState.botTurn && !isPlaying)
        {
            isPlaying = true;
            StartCoroutine(BotPlayCard());
        }
    }
    void DisplayCardsFanUpdate()
    {
        CardData[] sortedCards; ;
        sortedCards = GetComponentsInChildren<CardData>();
        float totalAngle = 190f;
        int n = sortedCards.Length;
        if (n <= 1) return;

        float step = totalAngle / (n - 1);
        float initialAngle = 95f;

        for (int i = 0; i < n; i++)
        {
            var card = sortedCards[i];
            GameObject cardGO = card.gameObject;

            // Gắn parent, giữ nguyên local position cũ (chỉ update rotation)
            cardGO.transform.SetParent(gameObject.transform, false);
            // Góc xoè
            float angle = initialAngle - (step * i);
            float clamped = Mathf.Repeat(angle + 180f, 360f) - 180f;

            // Tween xoay
            cardGO.transform.DOLocalRotate(
                new Vector3(0, 0, clamped),
                0.3f,
                RotateMode.Fast
            ).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                card.transform.GetChild(0).transform.localPosition = new Vector3(0, -1.2f, 0);
            }); ;
        }
    }
    private IEnumerator BocAnimation(Transform hand)
    {
        cardManager.transform.GetChild(0).transform.DOMove(hand.position, 0.5f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(0.5f);
        cardManager.transform.GetChild(0).transform.parent = hand.transform;
        DisplayCardsFanUpdate();
    }
    private IEnumerator BotPlayCard()
    {
        // Đợi 3 giây
        yield return new WaitForSeconds(2f);

        // 👉 Check ăn trước: chỉ xét đúng lá cuối cùng user vừa đánh
        CardData playerCard = null;
        if (playerboard.childCount > 0)
        {
            playerCard = playerboard.GetChild(playerboard.childCount - 1).GetComponent<CardData>();
        }

        if (playerCard != null && playerCard.canBeEaten) // chỉ ăn lá mới nhất và chưa bị khóa
        {
            CardData[] botCards = gameObject.GetComponentsInChildren<CardData>();
            CardData matchingCard = botCards.FirstOrDefault(c =>
                c.cardType == playerCard.cardType && c.cardSuit == playerCard.cardSuit);

            if (matchingCard == null)
            {
                matchingCard = botCards.FirstOrDefault(c =>
                    c.cardSuit == playerCard.cardSuit && c.cardType != playerCard.cardType);
            }

            if (matchingCard != null)
            {
                Debug.Log($"Bot ăn quân {playerCard.cardType}-{playerCard.cardSuit}");

                yield return StartCoroutine(AnAnimation(board1.transform, matchingCard, playerCard));

                // 🔒 Khóa lá vừa ăn để không bị ăn lại
                playerCard.canBeEaten = false;
                matchingCard.canBeEaten = false;

                isPlaying = false;
                yield break; // 👉 Thoát luôn, không đánh nữa
            }
            else
            {
                Debug.Log("Bot không có quân phù hợp để ăn!");
                // ❌ Nếu không ăn thì khóa vĩnh viễn
                playerCard.canBeEaten = false;
                playerCard.UpdateSprite();
            }
        }

        // 👉 Nếu không ăn được thì đánh cây đầu tiên
        if (transform.childCount > 0)
        {
            CardData cardData = transform.GetChild(0).GetComponent<CardData>();
            Danh(cardData);
        }
    }
    // gọi từ chỗ cũ: Danh(card) -> sẽ start coroutine bên dưới
    public void Danh(CardData card)
    {
        if (card == null) return;
        StartCoroutine(DanhCoroutine(card));
        cardManager.ResetTurnFlags();
    }
    private IEnumerator DanhCoroutine(CardData selected)
    {
        if (selected == null) yield break;

        GameObject cardGO = selected.gameObject;
        selected.transform.GetChild(0).localPosition = Vector3.zero;
        selected.isOpen = true;
        selected.UpdateSprite();

        int maxCardsOnBoard = 4;
        float spacingX = 0.3f;
        float spacingZ = -0.02f;
        float moveDur = 0.25f;

        // snapshot lá đang có
        List<Transform> existing = new List<Transform>();
        for (int i = 0; i < board.childCount; i++)
            existing.Add(board.GetChild(i));

        bool willRemoveFirst = existing.Count >= maxCardsOnBoard;

        if (willRemoveFirst)
        {
            // xoá ngay lá đầu tiên, không animate
            Transform first = existing[0];
            if (first != null)
                Destroy(first.gameObject);

            // dịch các lá còn lại sang trái
            for (int i = 1; i < existing.Count; i++)
            {
                Transform t = existing[i];
                if (t == null) continue;

                int newIndex = i - 1;
                Vector3 targetLocal = new Vector3(newIndex * spacingX, 0f, newIndex * spacingZ);

                var cdOld = t.GetComponent<CardData>();
                if (cdOld != null)
                {
                    cdOld.canBeEaten = false;
                    cdOld.canChoose = false;
                    cdOld.UpdateSprite();
                }

                // tween sang vị trí mới
                t.DOLocalMove(targetLocal, moveDur).SetEase(Ease.OutQuad);
                t.SetSiblingIndex(newIndex);
            }

            yield return new WaitForSeconds(moveDur);
        }

        // vị trí lá mới
        int afterCount = board.childCount;
        int targetIndex = Mathf.Clamp(afterCount, 0, maxCardsOnBoard - 1);
        Vector3 targetWorldPos = board.position + new Vector3(targetIndex * spacingX, 0f, targetIndex * spacingZ);

        // tách lá từ tay
        cardGO.transform.SetParent(null);

        // tween lá mới
        cardGO.transform.DOMove(targetWorldPos, moveDur).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            cardGO.transform.SetParent(board);
            cardGO.transform.localPosition = new Vector3(targetIndex * spacingX, 0f, targetIndex * spacingZ);
            cardGO.transform.localRotation = Quaternion.identity;

            // sorting
            var sr = cardGO.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = targetIndex;

            // khoá toàn bộ
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

            // chỉ mở cho lá mới
            var cdLast = cardGO.GetComponent<CardData>();
            if (cdLast != null)
            {
                cdLast.canBeEaten = true;
                cdLast.canChoose = false;
                cdLast.UpdateSprite();
            }

            DisplayCardsFanUpdate();
            gameManager.NextTurn();
            gameManager.OnBotPlayedCard();
            isPlaying = false;
        });

        yield break;
        
    }
    public void An()
    {
        // Lấy lá cuối cùng user vừa đánh
        if (playerboard.childCount == 0) return;

        CardData playerCard = playerboard.GetChild(playerboard.childCount - 1).GetComponent<CardData>();
        if (playerCard == null) return;

        // Nếu lá này đã bị khóa thì bỏ qua
        if (!playerCard.canBeEaten)
        {
            Debug.Log("Bot: Lá cuối cùng đã bị khóa, không thể ăn.");
            return;
        }

        // Lấy bài trên tay bot
        CardData[] botCards = gameObject.GetComponentsInChildren<CardData>();
        CardData matchingCard = null;

        // Ưu tiên chắn (trùng type + suit)
        matchingCard = botCards.FirstOrDefault(c =>
        c.cardType == playerCard.cardType && c.cardSuit == playerCard.cardSuit);

        // Nếu không có chắn thì thử cạ (trùng suit, khác type)
        if (matchingCard == null)
        {
            matchingCard = botCards.FirstOrDefault(c =>
            c.cardSuit == playerCard.cardSuit && c.cardType != playerCard.cardType);
        }

        // Nếu tìm thấy lá phù hợp thì ăn
        if (matchingCard != null)
        {
            Debug.Log($"Bot ăn quân {playerCard.cardType}-{playerCard.cardSuit}");

            matchingCard.isOpen = true;
            matchingCard.transform.GetChild(0).localPosition = Vector3.zero;

            // Thực hiện animation ăn
            StartCoroutine(AnAnimation(board1.transform, matchingCard, playerCard));

            // Khóa lá vừa ăn để không bị ăn lại
            playerCard.canBeEaten = false;
            matchingCard.canBeEaten = false;
            playerCard.canChoose = false;
            matchingCard.canChoose = false;
        }
        else
        {
            Debug.Log("Bot không có quân phù hợp để ăn!");

            // ❌ Quan trọng: nếu không ăn thì khóa luôn lá cuối cùng vĩnh viễn
            playerCard.canBeEaten = false;
            playerCard.UpdateSprite();
        }
    }
    public void SortCards()
    {
        CardData[] cardDatas = gameObject.GetComponentsInChildren<CardData>();
        List<CardData> cards = new List<CardData>();
        foreach (CardData cardData in cardDatas)
        {
            cards.Add(cardData);
        }
        var group = GroupCards(cards);
        var sorted = new List<CardData>();
        sorted.AddRange(group.Chan.SelectMany(p => new[] { p.Item1, p.Item2 }));
        sorted.AddRange(group.Ca.SelectMany(p => new[] { p.Item1, p.Item2 }));
        sorted.AddRange(group.Que);
        DisplayCardsFan(sorted, gameObject.transform);
    }
    private IEnumerator AnAnimation(Transform target, CardData card1, CardData card2)
    {
        card1.transform.rotation = Quaternion.Euler(0, 0, 0);
        card1.transform.GetChild(0).transform.localPosition = Vector3.zero;
        card1.isOpen = true;
        card1.UpdateSprite();
        card2.transform.rotation = Quaternion.Euler(0, 0, 0);
        card2.transform.GetChild(0).transform.localPosition = Vector3.zero;
        card2.isOpen = true;
        card2.UpdateSprite();
        float duration = 0.5f;
        Vector3 targetPoint = target.position + new Vector3((target.childCount/2)*(-0.33f),0,0);
        // Tween di chuyển 2 lá về target
        card1.transform.DOMove(targetPoint, duration).SetEase(Ease.InOutQuad);
        card2.transform.DOMove(targetPoint + new Vector3(0,0.2f,-0.1f), duration).SetEase(Ease.InOutQuad);

        // chờ 0.5s cho tween chạy xong
        yield return new WaitForSeconds(duration);
        card1.transform.GetChild(1).gameObject.SetActive(true);
        StartCoroutine(BocAnimation(transform));
        // Sau khi đến nơi → gắn parent để gom về target
        card1.transform.SetParent(target);
        card2.transform.SetParent(target);
    }
    void DisplayCardsFan(List<CardData> sortedCards, Transform handAnchor)
    {
        float totalAngle = 190f;
        int n = sortedCards.Count;
        float step = totalAngle / (n - 1);
        float initialAngle = 95f;

        for (int i = 0; i < n; i++)
        {
            var card = sortedCards[i];
            GameObject cardGO = card.gameObject;
            cardGO.SetActive(true);
            cardGO.transform.SetParent(handAnchor, false);

            cardGO.transform.localPosition = Vector3.zero;
            cardGO.transform.localRotation = Quaternion.identity;
            float angle = initialAngle - (step * i);
            float clamped = Mathf.Repeat(angle + 180f, 360f) - 180f;

            float delay = i * 0.02f;
            cardGO.transform.DOLocalRotate(
                new Vector3(0, 0, clamped),
                0.5f,
                RotateMode.FastBeyond360
            ).SetEase(Ease.OutBack)
            .SetDelay(delay).OnComplete(() =>
            {
                card.transform.GetChild(0).transform.DOLocalMove(new Vector2(0, -1.2f), 0.1f);
            });
        }
        gameManager.NextTurn();
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
}