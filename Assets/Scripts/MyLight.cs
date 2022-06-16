using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLight : MonoBehaviour
{
    public float w = 10f; //A w value to emulate the light being a 4D points
    private float old_w; //The w of light last frame
    private Vector3 old_position; //The position of the light last frame

    // Start is called before the first frame update
    void Start()
    {
        old_position = this.gameObject.transform.position;
        old_w = w;
    }

    // Update is called once per frame
    void Update()
    {
        //Set the lights color based on its w value
        var lightrend = this.GetComponent<Renderer>();
        lightrend.material.SetColor("_BaseColor", new Color(1f - (w / 20f), 0f, w / 20f, 0.8f));

        //If the light changes position or w, iterate through all shapes
        Transform Shapes3d = GameObject.Find("3D Shapes").transform;
        Transform Shapes4d = GameObject.Find("4D Shapes").transform;
        if (this.gameObject.transform.position != old_position || w != old_w)
        {
            //Recalculate the shadow of each active 3D shape
            for (int i = 0; i < Shapes3d.childCount; i++)
            {
                if (Shapes3d.GetChild(i).gameObject.activeSelf)
                {
                    Shape3D projection = Shapes3d.GetChild(i).GetComponent<Shape3D>();
                    projection.Shadow_Update();
                }
            }
            //Recalculate the shadow of each active 4D shape
            for (int i = 0; i < Shapes4d.childCount; i++)
            {
                if (Shapes4d.GetChild(i).gameObject.activeSelf)
                {
                    Shape4D projection = Shapes4d.GetChild(i).GetComponent<Shape4D>();
                    projection.Shadow_Update();
                    projection.Update_Edges();
                }
            }
        }

        //Set the old_ variables equal to their corresponding variables
        old_position = this.gameObject.transform.position;
        old_w = w;
    }
}