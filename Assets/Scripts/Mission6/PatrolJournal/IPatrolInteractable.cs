using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPatrolInteractable
{
    string GetInteractionText();
    void Interact();
}
