using UnityEngine;

public class BattlePassButtons : MonoBehaviour
{
    public CinematicPan cinematicPan;
    public CanvasManager canvasManager;

    public void OnBackButtonClick()
    {
        // Set the CinematicPan index back to the main menu (index 0)
        cinematicPan.MoveToPoint(0);

        // Set the main menu canvas active and the settings canvas inactive
        canvasManager.ActivateCanvas(0);
    }
}
