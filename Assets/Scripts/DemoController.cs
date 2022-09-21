using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public class DemoController : MonoBehaviour
{
    public GameObject Robot;
    public ROSConnection ROSBackend;
    public ArticulationBody BaseArticulation;

    public List<float> ArticulationPositions;
    public List<float> ArticulationTargets;

    public List<string> JointNames;
    public List<ArticulationBody> JointArticulations;
    Dictionary<string, ArticulationBody> RobotJoints;
    Dictionary<string, List<float>> PreconfiguredPositions;

    private void Start()
    {
        DefineRobotComponents();
    }

    private void Update()
    {
        BaseArticulation.GetDriveTargets(ArticulationTargets);
        BaseArticulation.GetJointPositions(ArticulationPositions);
    }

    void MakeRobotRigid()
    {
        for (int i = 0; i < 6; i++)
        {
            ArticulationDrive drive = JointArticulations[i].xDrive;
            drive.stiffness = 5000f;
            drive.damping = 8000f;
            JointArticulations[i].xDrive = drive;
        }
    }

    void MakeRobotCompliant()
    {
        for (int i = 0; i < JointArticulations.Count; i++)
        {
            ArticulationDrive drive = JointArticulations[i].xDrive;
            drive.stiffness = 0f;
            drive.damping = 30f;
            JointArticulations[i].xDrive = drive;
        }
    }

    void SnapJointToTargetAngle(int index, float target)
    {
        List<float> current = new List<float>();
        BaseArticulation.GetJointPositions(current);
        current[index] = target;
        BaseArticulation.SetDriveTargets(current);
        BaseArticulation.SetJointPositions(current);
    }

    void SnapArmToPosition(string position)
    {
        if (!PreconfiguredPositions.ContainsKey(position))
        {
            Debug.Log($"{position} is not known in preset library...");
            return;
        }
        BaseArticulation.SetDriveTargets(PreconfiguredPositions[position]);
        BaseArticulation.SetJointPositions(PreconfiguredPositions[position]);
    }

    private void DefineRobotComponents()
    {
        Robot = this.gameObject;

        string baseChain = "base_link/base_link_inertia";
        BaseArticulation = Robot.transform.Find(baseChain).GetComponent<ArticulationBody>();

        JointNames = new List<string>() {
            "shoulder_pan_joint",
            "shoulder_lift_joint",
            "elbow_joint",
            "wrist_1_joint",
            "wrist_2_joint",
            "wrist_3_joint",
            "left_inner_knuckle_joint",
            "left_inner_finger_joint",
            "finger_joint",
            "right_inner_knuckle_joint",
            "right_inner_finger_joint",
            "right_finger_joint",
        };

        JointArticulations = new List<ArticulationBody>() {
            Robot.transform.Find(baseChain + "/shoulder_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/left_knuckle_link/left_bar_link/left_distal_phalanx_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/left_proximal_phalanx_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/left_knuckle_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/right_knuckle_link/right_bar_link/right_distal_phalanx_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/right_proximal_phalanx_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/right_knuckle_link").GetComponent<ArticulationBody>(),
        };

        RobotJoints = new Dictionary<string, ArticulationBody>();
        for (int i = 0; i < JointNames.Count; i++)
        {
            RobotJoints.Add(JointNames[i], JointArticulations[i]);
        }

        PreconfiguredPositions = new Dictionary<string, List<float>>() {
            {"zero", new List<float>(new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0})},
            {"home", new List<float>(new float[] {0, -Mathf.PI / 2.0f, 0, -Mathf.PI / 2.0f, 0, 0, 0, 0, 0, 0, 0, 0})}
        };

        MakeRobotRigid();
        SnapArmToPosition("home");
    }
}