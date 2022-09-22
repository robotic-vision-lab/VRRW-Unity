using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostdriveController : MonoBehaviour
{
    public GameObject RealRobot;
    public DemoController RealRobotDrive;
    public GameObject GhostRobot;
    public Material GhostMaterial;
    public List<float> ArticulationTargets;
    public List<float> ArticulationPositions;
    public List<float> ArticulationVelocities;

    public ArticulationBody BaseArticulation;
    public List<ArticulationBody> JointArticulations;
    public bool IsGhostDrive = false;

    void Start()
    {
        RealRobotDrive.Vanish();
        string baseChain = "base_link/base_link_inertia";
        BaseArticulation = GhostRobot.transform.Find(baseChain).GetComponent<ArticulationBody>();
        BaseArticulation.GetDriveTargets(ArticulationTargets);
        BaseArticulation.GetJointPositions(ArticulationPositions);
        BaseArticulation.GetJointVelocities(ArticulationVelocities);
        JointArticulations = new List<ArticulationBody>() {
            GhostRobot.transform.Find(baseChain + "/shoulder_link").GetComponent<ArticulationBody>(),
            GhostRobot.transform.Find(baseChain + "/shoulder_link/upper_arm_link").GetComponent<ArticulationBody>(),
            GhostRobot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link").GetComponent<ArticulationBody>(),
            GhostRobot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link").GetComponent<ArticulationBody>(),
            GhostRobot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link").GetComponent<ArticulationBody>(),
            GhostRobot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link").GetComponent<ArticulationBody>(),
            GhostRobot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/left_knuckle_link/left_bar_link/left_distal_phalanx_link").GetComponent<ArticulationBody>(),
            GhostRobot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/left_proximal_phalanx_link").GetComponent<ArticulationBody>(),
            GhostRobot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/left_knuckle_link").GetComponent<ArticulationBody>(),
            GhostRobot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/right_knuckle_link/right_bar_link/right_distal_phalanx_link").GetComponent<ArticulationBody>(),
            GhostRobot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/right_proximal_phalanx_link").GetComponent<ArticulationBody>(),
            GhostRobot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/right_knuckle_link").GetComponent<ArticulationBody>(),
        };
        foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>()) r.material = GhostMaterial;
        Vanish();
    }

    void Update()
    {
        BaseArticulation.GetDriveTargets(ArticulationTargets);
        BaseArticulation.GetJointPositions(ArticulationPositions);
        BaseArticulation.GetJointVelocities(ArticulationVelocities);
    }

    public void Vanish()
    {
        MakeRobotRigid();
        foreach(Collider c in gameObject.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach(Renderer r in gameObject.GetComponentsInChildren<Renderer>()) r.enabled = false;
        RealRobotDrive.Appear();
    }

    public void Appear()
    {
        RealRobotDrive.MakeRobotRigid();
        RealRobotDrive.Vanish();
        BaseArticulation.SetDriveTargets(RealRobotDrive.ArticulationPositions);
        BaseArticulation.SetJointPositions(RealRobotDrive.ArticulationPositions);
        foreach(Collider c in gameObject.GetComponentsInChildren<Collider>()) c.enabled = true;
        foreach(Renderer r in gameObject.GetComponentsInChildren<Renderer>()) r.enabled = true;
    }

    public void ToggleGhostDrive()
    {
        if (IsGhostDrive) Appear();
        else Vanish();
        IsGhostDrive = !IsGhostDrive;
    }

    public void MakeRobotRigid()
    {
        for (int i = 0; i < 6; i++)
        {
            ArticulationDrive drive = JointArticulations[i].xDrive;
            drive.stiffness = 100000f;
            drive.damping = 80000f;
            drive.forceLimit = float.PositiveInfinity;
            BaseArticulation.SetDriveTargets(ArticulationPositions);
            JointArticulations[i].xDrive = drive;
        }
    }

    public void MakeRobotCompliant()
    {
        for (int i = 0; i < 6; i++)
        {
            ArticulationDrive drive = JointArticulations[i].xDrive;
            drive.stiffness = 0f;
            drive.damping = 30f;
            JointArticulations[i].xDrive = drive;
        }
    }

    public void SnapArmToConfiguration(List<float> joints)
    {
        List<float> current = new List<float>();
        BaseArticulation.GetJointPositions(current);
        for (int i = 0; i < joints.Count; i++)
        {
            current[i] = joints[i];
        }
        BaseArticulation.SetDriveTargets(current);
        BaseArticulation.SetJointPositions(current);
    }
}
