using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] hpImages;

    private int HP = 5;

    public void Damaged(int damage)
    {
        for (int i = 0; i < damage; i++)
        {
            StartCoroutine(DeleteHeart(hpImages[HP - 1]));
            HP--;
            if (HP < 1)
            {
                GameManager.instance.dispendPanel();
                GameManager.instance.player.SetActive(false);
                break;
            }
        }
    }

    IEnumerator DeleteHeart(GameObject obj)
    {
        obj.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        obj.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        obj.SetActive(false);

        // HP ‚ª 0 ‚É‚È‚Á‚½‚ç "End" ƒƒO‚ð•\Ž¦
        if (HP == 0)
        {
            //Destroy(GameManager.instance.player);
        }
    }
}
