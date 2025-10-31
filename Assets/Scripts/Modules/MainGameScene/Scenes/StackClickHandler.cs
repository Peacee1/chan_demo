using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class StackClickHandler : MonoBehaviour, IPointerClickHandler
{
    public int stackIndex;
    public CardDealer cardDealer;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(cardDealer != null)
        {
            cardDealer.OnStackClicked(stackIndex);
        }
    }
 
}