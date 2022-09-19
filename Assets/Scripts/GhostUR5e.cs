using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostUR5e : MonoBehaviour
{
    public Material GhostMaterial;
    GameObject Robot;
    List<ArticulationBody> Joints;

    // Start is called before the first frame update
    void Start()
    {
        string searchPath = "base_link/base_link_inertia/shoulder_link";
        Robot = this.gameObject;

        foreach (Collider c in Robot.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        foreach (Renderer r in Robot.GetComponentsInChildren<Renderer>())
        {
            var mats = new Material[r.materials.Length];
            for (int i = 0; i < r.materials.Length; i++)
            {
                mats[i] = GhostMaterial;
            }
            r.materials = mats;
        }

        Joints = new List<ArticulationBody>() {
            Robot.transform.Find(searchPath).GetComponent<ArticulationBody>(),
            Robot.transform.Find(searchPath + "/upper_arm_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(searchPath + "/upper_arm_link/forearm_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(searchPath + "/upper_arm_link/forearm_link/wrist_1_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(searchPath + "/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(searchPath + "/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link").GetComponent<ArticulationBody>()
        };

        for (int i = 0; i < Joints.Count; i++)
        {
            ArticulationBody joint = Joints[i];
            ArticulationDrive drive = joint.xDrive;
            drive.stiffness = 100000f;
            drive.damping = 20000f;
            drive.forceLimit = 100000f;
            switch (i)
            {
                case 0:
                    drive.target = -25.41f;
                    break;
                case 1:
                    drive.target = -132.35f;
                    break;
                case 2:
                    drive.target = -53.80f;
                    break;
                case 3:
                    drive.target = 280.62f;
                    break;
                case 4:
                    drive.target = 80.95f;
                    break;
                case 5:
                    drive.target = 47.29f;
                    break;
                default:
                    break;
            }
            joint.xDrive = drive;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
