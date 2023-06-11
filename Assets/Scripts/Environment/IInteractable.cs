using UnityEngine;

namespace Interactables
{
    public static class Interactable
    {
        public static void MessageTargets(GameObject[] targets)
        {
            foreach (var target in targets)
            {
                IInteractable targetInterface = target.GetComponent<IInteractable>();
                if (targetInterface == null)
                {
                    Debug.LogError(target.name + " is missing IInterface!");
                    return;
                }

                targetInterface.Interact();
            }
        }
    }

    public interface IInteractable
    {
        void Interact();
    }
}