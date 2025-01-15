using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HypnosisDropdownUI : MonoBehaviour
{
    private Canvas canvasDropdown;
    public GameObject trigger;
    // Start is called before the first frame update
    void Start()
    {
        canvasDropdown = transform.GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        canvasDropdown.overrideSorting = trigger.activeSelf;
    }
}
