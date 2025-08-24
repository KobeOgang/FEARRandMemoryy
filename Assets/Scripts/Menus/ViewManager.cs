using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    private static ViewManager instance;

    [SerializeField]
    private View[] views; 

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public static void ShowView(View viewToShow)
    {
        foreach (var view in instance.views)
        {
            view.Hide(); 
        }
        viewToShow.Show(); 
    }
}
