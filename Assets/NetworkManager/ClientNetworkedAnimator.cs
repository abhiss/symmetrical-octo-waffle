using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkedAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative() => false;
}
