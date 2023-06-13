using UnityEngine;

public class Obstructable : MonoBehaviour
{
    public bool isObstructing = false;
    [SerializeField] private float dissolve = -1.0f;
    [SerializeField] private float fadeRate = 5.0f;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock = new MaterialPropertyBlock();
    private float fadeValue = 0.75f;
    private float visibleValue = -0.5f;
    private void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        float targetNum = visibleValue;
        if (isObstructing)
        {
            gameObject.layer = LayerMask.NameToLayer("Faded Obstruction");
            targetNum = fadeValue;
        }

        dissolve = Mathf.MoveTowards(dissolve, targetNum, Time.deltaTime * fadeRate);
        // _propertyBlock.SetFloat("_Dissolve", dissolve);
        _renderer.SetPropertyBlock(_propertyBlock);

        if (!isObstructing && dissolve == visibleValue)
        {
            gameObject.layer = LayerMask.NameToLayer("Wall");
            Destroy(this);
        }
    }
}
