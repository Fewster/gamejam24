using Game.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// TODO: This works, but its a bit hacky. We should probably do a better approach for button input.
// Specifically, handling click + hold but deselect when exiting the button.

public class GameButton :
    GameBehaviour, 
    IPointerDownHandler,
    IPointerUpHandler, 
    IPointerExitHandler
{
    private bool down;
    private GameInput input;
    private float lastTapTime;

    public Button button;

    protected override void OnSetup()
    {
        input = Resolver.Resolve<GameInput>();
    }

    private void Update()
    {
        if (down)
        {
            TryTap();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        down = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (down)
        {
            down = false;
            button.enabled = false;
            button.enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (down)
        {
            down = false; 
            button.enabled = false;
            button.enabled = true;
        }
    }

    private void TryTap()
    {
        var mod = 1.0f / input.TapsPerSecond;

        var now = Time.time;
        var diff = now - lastTapTime;
        if (diff > mod)
        {
            lastTapTime = now;
            input.Tap();
        }
    }
}
