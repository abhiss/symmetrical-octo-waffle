using UnityEngine;

public class Obstructable : MonoBehaviour
{
    public bool isObstructing = false;
    [SerializeField] private float dissolve = -1.0f;
    [SerializeField] private float fadeRate = 5.0f;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private float fadeValue = 0.75f;
    private float visibleValue = -0.5f;
    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        float targetNum = visibleValue;
        if (isObstructing) {
            targetNum = fadeValue;
        }

        dissolve = Mathf.MoveTowards(dissolve, targetNum, Time.deltaTime * fadeRate);
        _propertyBlock.SetFloat("_Dissolve", dissolve);
        _renderer.SetPropertyBlock(_propertyBlock);

        if (!isObstructing && dissolve == visibleValue) {
            Destroy(this);
        }
    }
}
