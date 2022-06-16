using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class Controller : MonoBehaviour
{
    [SerializeField] private ActionBasedController controller; //Game Object of controller
    [SerializeField] private InputActionManager inputActionManager; //Input database of controller
    private InputAction Primary_Button; //Primary Button memory location
    [SerializeField] private Transform t_controller; //Controller's transform

    private Vector3 position; //Position of controller
    private Vector3 rotation; //rotation of controller

    [Range(0.5f, 1f)]
    public float range = 0.5f; //Adjustable Grab range of controller
    [Space]
    private Transform grabbed_shape; //3D or 4D shape that is grabbed
    private Transform selected_shape; //4D shape that is selected

    private Vector3 starting_position_offset; //Difference between initial grab position and center of shape
    private Vector3 starting_position; //Initial grab position of controller
    private Vector3 starting_rotation; //Initial rotation of controller
    private bool isgrabbing;
    private bool isselecting;
    private int last_primary_button = 0;
    private Shape3D shape3d_script; //Script of currently grabbed or selected shape
    private Shape4D shape4d_script; //Script of currently grabbed or selected shape

    // Start is called before the first frame update
    void Start()
    {
        //Adjusts primary button based on which controller this script is attached to.
        t_controller = controller.gameObject.transform;
        if (controller.gameObject.name == "RightHand Controller")
        {
            Primary_Button = inputActionManager.actionAssets[0].FindActionMap("XRI RightHand").FindAction("Primary Button");
        }
        else if (controller.gameObject.name == "LeftHand Controller")
        {
            Primary_Button = inputActionManager.actionAssets[0].FindActionMap("XRI LeftHand").FindAction("Primary Button");
        }
        isgrabbing = false;
        isselecting = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Updates position and rotation of controller
        position = t_controller.position;
        rotation = t_controller.rotation.eulerAngles;

        //Orients capsule to controllers position and rotation
        this.gameObject.transform.position = position;
        this.gameObject.transform.rotation = t_controller.rotation;

        //Stores if the controller is grabbing or selecting
        float activate = controller.activateAction.action.ReadValue<float>();
        float select = controller.selectAction.action.ReadValue<float>();

        //Logic for grabbing input -> Initial grab, grabbing, release
        if (activate > 0f && !isgrabbing && !isselecting)
        {
            isgrabbing = true;
            Grab();
        }
        else if (activate > 0f && isgrabbing)
        {
            Grabbing();
        }
        else if (isgrabbing && activate == 0f)
        {
            isgrabbing = false;
            Release();
        }
        //Logic for selecting input -> Initial selection, selecting, release
        if (select > 0f && !isselecting && !isgrabbing)
        {
            isselecting = true;
            Select();
        }
        else if (select > 0f && isselecting)
        {
            Selecting();
        }
        else if (isselecting && select == 0f)
        {
            isselecting = false;
            Unselect();
        }
        //Logic for primary buttons of either controller
        if (Primary_Button.ReadValue<float>() == 1 && last_primary_button == 0)
        {
            last_primary_button = 1;
            if (controller.gameObject.name == "RightHand Controller")
            {
                Change_Mode();
            }
        }
        else if ((Primary_Button.ReadValue<float>() == 0 && last_primary_button == 1))
        {
            //Reset_Scene() is only called when button is released so infinite resets do not occur
            last_primary_button = 0;
            if (controller.gameObject.name == "LeftHand Controller")
            {
                Reset_Scene();
            }
        }
    }

    //Called the first frame the trigger button is pushed
    private void Grab()
    {
        var rend = this.gameObject.GetComponent<Renderer>();
        //If this controller currently has no grabbed shape
        if (grabbed_shape == null)
        {
            //Iterate through the 3D shapes
            Transform shapes3d = GameObject.Find("3D Shapes").transform;
            for (int i = 0; i < shapes3d.childCount; i++)
            {
                //If the indexed shape is active in the scene
                if (shapes3d.GetChild(i).gameObject.activeSelf)
                {
                    Shape3D script3d = shapes3d.GetChild(i).GetComponent<Shape3D>();
                    Vector3 center = script3d.center;
                    //If the controller is close enough to the shape
                    if ((position - center).magnitude <= (range * script3d.obj_scale * script3d.grab_range))
                    {
                        //Grab the shape
                        grabbed_shape = script3d.gameObject.transform;
                        shape3d_script = script3d;
                        //Change the color of the capsule to blue
                        rend.material.SetColor("_BaseColor", new Color(0f, 0f, 1f, 1f));
                        //Store the current position offset from the shapes center,
                        // and the controllers rotation
                        starting_position_offset = center - position;
                        starting_rotation = rotation;
                        //Stop interating through the 3D shapes
                        break;
                    }
                }
                //If not, color the capsule red 
                rend.material.SetColor("_BaseColor", new Color(1f, 0f, 0f, 1f));
            }
        }
        //If this controller currently has no grabbed shape
        if (grabbed_shape == null)
        {
            //Iterate through the 4D shapes
            Transform shapes4d = GameObject.Find("4D Shapes").transform;
            for (int i = 0; i < shapes4d.childCount; i++)
            {
                //If the indexed shape is active in the scene
                if (shapes4d.GetChild(i).gameObject.activeSelf)
                {
                    Shape4D script4d = shapes4d.GetChild(i).GetComponent<Shape4D>();
                    Vector4 center = script4d.center;
                    Transform shadow_points = shapes4d.GetChild(i).GetChild(0).GetChild(0);
                    //Calculate the bounding box of the shadow of the shape
                    Vector3[] boundingbox = script4d.boundingbox();
                    Vector3 min = boundingbox[0];
                    Vector3 max = boundingbox[1];
                    //If the controller is in the bounding box of the shape
                    if ((position.x >= min.x && position.y >= min.y && position.z >= min.z)
                        && (position.x <= max.x && position.y <= max.y && position.z <= max.z))
                    {
                        //Grab the shape
                        grabbed_shape = script4d.gameObject.transform;
                        shape4d_script = script4d;
                        //Store the current position offset from the shapes center,
                        // and the controllers rotation
                        Vector3 temp_center = center;
                        starting_position_offset = temp_center - position;
                        starting_rotation = rotation;
                        //Change the color of the capsule to blue
                        rend.material.SetColor("_BaseColor", new Color(0f, 0f, 1f, 1f));
                        //Stop iterating through 4D shapes
                        break;
                    }
                }
                //If not, color the capsule red
                rend.material.SetColor("_BaseColor", new Color(1f, 0f, 0f, 1f));
            }
        }
    }

    //Called every frame that the trigger button is being pressed
    private void Grabbing()
    {
        //If the grabbed shape is 3D
        if (shape3d_script != null)
        {
            //Rotate the offset of the initial positional offset by change in rotation of the shape
            Vector3[] pos_rotation = Matrix.rotate3d((rotation.x - starting_rotation.x) / 60f,
                                                     (rotation.y - starting_rotation.y) / 60f,
                                                     (rotation.z - starting_rotation.z) / 60f);
            Vector3 position_offset = Matrix.rotate_point(starting_position_offset, pos_rotation);
            //Change the center of the shape to the controller's position + the initial positional offset
            shape3d_script.Translate(position + position_offset);
            //Rotate the shape by the difference between the controllers rotation and the initial contorller rotation
            shape3d_script.Rotate((rotation.x - starting_rotation.x) / 60f, 
                                  (rotation.y - starting_rotation.y) / 60f, 
                                  (rotation.z - starting_rotation.z) / 60f);
        }
        //If the brabbed shape is 4D
        else if (shape4d_script != null)
        {
            //Rotate the offset of the initial positional offset by change in rotation of the shape
            Vector3[] pos_rotation = Matrix.rotate3d((rotation.x - starting_rotation.x) / 60f,
                                                     (rotation.y - starting_rotation.y) / 60f,
                                                     (rotation.z - starting_rotation.z) / 60f);
            Vector3 position_offset = Matrix.rotate_point(starting_position_offset, pos_rotation);
            //Change the center of the shape to the controller's position + the initial positional offset
            Vector3 temp_total = position + position_offset;
            shape4d_script.Translate(new Vector4(temp_total.x, temp_total.y, temp_total.z, shape4d_script.center.w));
            //Rotate the shape in 3 of the 6 fourth-dimensional rotations,
            //(which resemble our 3 three-dimensional rotations)
            //by the difference between the controllers rotation and the initial contorller rotation
            shape4d_script.Rotate(0f, 0f,
                                  (rotation.x - starting_rotation.x) / 60f,
                                  0f,
                                  -(rotation.y - starting_rotation.y) / 60f,
                                  (rotation.z - starting_rotation.z) / 60f);
        }
    }

    //Called when the trigger button is released
    private void Release()
    {
        //Set the color of the capsule to white
        var rend = this.gameObject.GetComponent<Renderer>();
        rend.material.SetColor("_BaseColor", new Color(1f, 1f, 1f, 1f));

        //Update the storage of the shape's vertices
        if (shape3d_script != null)
        { shape3d_script.Vertex_Store(); }
        if (shape4d_script != null)
        { shape4d_script.Vertex_Store(); }

        //Let go of shapes
        grabbed_shape = null;
        shape3d_script = null;
        shape4d_script = null;
    }

    //Called the first frame the grab button is pushed
    private void Select()
    {
        var rend = this.gameObject.GetComponent<Renderer>();
        //If this controller currently has no selected shape
        if (selected_shape == null)
        {
            //Iterate through the 4D shapes
            Transform shapes4d = GameObject.Find("4D Shapes").transform;
            for (int i = 0; i < shapes4d.childCount; i++)
            {
                //If the indexed shape is active in the scene
                if (shapes4d.GetChild(i).gameObject.activeSelf)
                {
                    Shape4D script4d = shapes4d.GetChild(i).GetComponent<Shape4D>();
                    Vector4 center = script4d.center;
                    Transform shadow_points = shapes4d.GetChild(i).GetChild(0).GetChild(0);
                    //Calculate the bounding box of the shadow of the shape
                    Vector3[] boundingbox = script4d.boundingbox();
                    Vector3 min = boundingbox[0];
                    Vector3 max = boundingbox[1];
                    //If the controller is in the bounding box of the shape
                    if ((position.x >= min.x && position.y >= min.y && position.z >= min.z)
                        && (position.x <= max.x && position.y <= max.y && position.z <= max.z))
                    {
                        //Select the shape
                        selected_shape = script4d.gameObject.transform;
                        shape4d_script = script4d;
                        //Store the initial position of the controller
                        starting_position = position;
                        //Change the color of the capsule to green
                        rend.material.SetColor("_BaseColor", new Color(0f, 1f, 0f, 1f));
                        //Stop iterating through 4D shapes
                        break;
                    }
                }
                //If not, change the color of the capsule to Red
                rend.material.SetColor("_BaseColor", new Color(1f, 0f, 0f, 1f));
            }
        }
    }

    //Called every frame that the grab button is being pressed
    private void Selecting()
    {
        //If there is a 4D shape selected
        if (shape4d_script != null)
        {
            //Rotate the shape in 3 of the 6 fourth-dimensional rotations,
            //which do not resemble any of our three-dimensional rotations
            //by the difference between the controllers intial position and its current position
            Vector3 position_diff = (position - starting_position) * shape4d_script.obj_scale * 4f;
            shape4d_script.Rotate(position_diff.z, position_diff.y, 0f, position_diff.x, 0f, 0f);
        }
    }

    //Called when the grab button is released 
    private void Unselect()
    {
        //Change the color of the capsule to white
        var rend = this.gameObject.GetComponent<Renderer>();
        rend.material.SetColor("_BaseColor", new Color(1f, 1f, 1f, 1f));

        //Update the storage of the shape's vertices
        if (shape4d_script != null)
        { shape4d_script.Vertex_Store(); }

        //Let go of shapes
        selected_shape = null;
        shape4d_script = null;
    }

    //Called when the right controller's primary button is first pressed
    private void Change_Mode()
    {
        //Let go of all shapes
        grabbed_shape = null;
        selected_shape = null;
        shape3d_script = null;
        shape4d_script = null;

        //Determine if 3D objects must be deactivate or activated
        Transform shapes3d = GameObject.Find("3D Shapes").transform;
        bool active3ds = shapes3d.GetChild(0).gameObject.activeSelf;

        //Swap the activation mode of 3D shapes and 4D shapes
        for (int i = 0; i < shapes3d.childCount; i++)
        {
            shapes3d.GetChild(i).gameObject.SetActive(!active3ds);
        }

        Transform shapes4d = GameObject.Find("4D Shapes").transform;
        for (int i = 0; i < shapes4d.childCount; i++)
        {
            shapes4d.GetChild(i).gameObject.SetActive(active3ds);
        }
    }

    //Called when the left controller's primary button is released
    private void Reset_Scene()
    {
        //Reset the software
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }
}