using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class OwnerAnimator : NetworkAnimator
{
    // ?为什么要重写该方法
    protected override bool OnIsServerAuthoritative() {
        return false;
    }
}
