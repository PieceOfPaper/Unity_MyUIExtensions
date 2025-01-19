using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class BehaviourEventListener : MonoBehaviour
{
    public bool executeOnEditor = false;
    
    public UnityEvent onEnable = new UnityEvent();
    public UnityEvent onDisable = new UnityEvent();
    public UnityEvent start = new UnityEvent();
    public UnityEvent update = new UnityEvent();
    public UnityEvent lateUpdate = new UnityEvent();


    private void OnEnable()
    {
        if (executeOnEditor == true || Application.isPlaying == true)
            onEnable.Invoke();
    }
    
    private void OnDisable()
    {
        if (executeOnEditor == true || Application.isPlaying == true)
            onDisable.Invoke();
    }
    
    private void Start()
    {
        if (executeOnEditor == true || Application.isPlaying == true)
            start.Invoke();
    }
    private void Update()
    {
        if (executeOnEditor == true || Application.isPlaying == true)
            update.Invoke();
    }
    private void LateUpdate()
    {
        if (executeOnEditor == true || Application.isPlaying == true)
            lateUpdate.Invoke();
    }
}
