using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITileSelector : MonoBehaviour {

    static UITileSelector _instance;

	void Start () {
        _instance = this;
        sRend = GetComponent<SpriteRenderer>();
        sRend.enabled = false;
	}
	
    public static void ShowFor(Transform t)
    {
        _instance.showingFor = t;
        _instance.pingTime = Time.timeSinceLevelLoad;
        _instance.transform.position = t.transform.position + _instance.offset;
        if (!_instance.sRend.enabled)
        {
            _instance.sRend.enabled = true;
        }
    }

    Transform showingFor;
    float pingTime = -2f;

    SpriteRenderer sRend;

    [SerializeField]
    float beforeHide = 0.3f;

    [SerializeField]
    Vector3 offset;

	void Update () {
		if (showingFor)
        {
            if (Time.timeSinceLevelLoad > pingTime + beforeHide)
            {
                sRend.enabled = false;
            }
        }
	}

    void OnDestroy() {
        _instance = null;
    }

}
