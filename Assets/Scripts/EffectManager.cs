using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    [SerializeField]
    private afterImage PLAYER_SHADOW;

    [SerializeField]
    private Guro EFFECT_GURO;

    [SerializeField]
    private GameObject AttackEffect;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        EFFECT_GURO.SplitImage();
    }

    public void Play_shadow(Transform tf, Vector3 target_tf)
    {
        PLAYER_SHADOW.DrawShadow(tf, target_tf);
    }

    public void Play_guro(Vector3 worldpos)
    {
        EFFECT_GURO.PlayGuro(worldpos);
    }

    public void Play_AttackEffect(Vector3 worldPos)
    {
        AttackEffect.SetActive(false);
        AttackEffect.transform.position = worldPos;
        AttackEffect.SetActive(true);
    }
}
