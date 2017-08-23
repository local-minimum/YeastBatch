using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMigrationArrow : MonoBehaviour {

    Transform tileTransf;
    SpriteRenderer sRend;

    private void Start()
    {
        tileTransf = transform.parent;
        sRend = GetComponent<SpriteRenderer>();
        Hide();
    }

    public void Hide()
    {
        //TODO: Trigger hidden anim
        sRend.color = new Color(0, 0, 0, 0);
    }
    public void ReShow()
    {
        sRend.color = new Color(1, 1, 1, 1);
    }

    public void ShowMigration(Transform target)
    {
        transform.position = (tileTransf.position + target.position) / 2f;
        transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.Cross(Vector3.back, target.position - tileTransf.position));
        sRend.color = new Color(1, 1, 1, 1);
    }
}
