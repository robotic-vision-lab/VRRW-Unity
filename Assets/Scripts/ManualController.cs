using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public class ManualController : MonoBehaviour
{
    public GameObject Robot;
    public ROSConnection ROSBackend;
    public ArticulationBody BaseArticulation;

    public List<float> ArticulationTargets;
    public List<float> ArticulationPositions;
    public List<float> ArticulationVelocities;

    public List<string> JointNames;
    public List<ArticulationBody> JointArticulations;
    Dictionary<string, ArticulationBody> RobotJoints;

    public List<ArticulationBody> GripperJoints;
    bool IsGripperClose = false;
    [Range(0.1f, 0.8f)] public float GripperRange = 0.8f;

    // public GameObject GhostRobot;
    // bool IsGhostDrive;

    Queue<IEnumerator> GripperControlQueue = new Queue<IEnumerator>();
    Queue<IEnumerator> ArmPlannedMotionQueue = new Queue<IEnumerator>();

    // Start is called before the first frame update
    void Start()
    {
        DefineRobotComponents();
        StartCoroutine(GripperQueueManagerCoroutine());
    }

    // Update is called once per frame
    void Update()
    {

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

        GripperJoints = JointArticulations.GetRange(6, 6);

        RobotJoints = new Dictionary<string, ArticulationBody>();
        for (int i = 0; i < JointNames.Count; i++)
        {
            RobotJoints.Add(JointNames[i], JointArticulations[i]);
        }

        // float ninety = Mathf.PI / 2.0f;

        // PreconfiguredPositions = new Dictionary<string, List<float>>() {
        //     {"zero", new List<float>(new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0})},
        //     {"home", new List<float>(new float[] {0, -ninety, 0, -ninety, 0, 0, 0, 0, 0, 0, 0, 0})},
        //     {"square_right" , new List<float>(new float[] {0, -ninety, -ninety, -ninety, ninety, 0, 0, 0, 0, 0, 0, 0})},
        //     {"square_left"  , new List<float>(new float[] {-ninety * 2, -ninety, -ninety, -ninety, ninety, 0, 0, 0, 0, 0, 0, 0})},
        //     {"square_center", new List<float>(new float[] {-ninety, -ninety, -ninety, -ninety, ninety, 0, 0, 0, 0, 0, 0, 0})}
        // };

        BaseArticulation.GetDriveTargets(ArticulationTargets);
        BaseArticulation.GetJointPositions(ArticulationPositions);

        MakeRobotRigid();
    }

    public void CloseGripper() { GripperControlQueue.Enqueue(CloseGripperCoroutine()); }

    public void OpenGripper() { GripperControlQueue.Enqueue(OpenGripperCoroutine()); }

    public void ToggleGripper()
    {
        if (IsGripperClose)
        {
            OpenGripper();
            IsGripperClose = false;
        }
        else
        {
            CloseGripper();
            IsGripperClose = true;
        }
    }

    private IEnumerator GripperQueueManagerCoroutine()
    {
        while (true)
        {
            if (GripperControlQueue.Count > 0)
            {
                yield return StartCoroutine(GripperControlQueue.Dequeue());
            }
            yield return null;
        }
    }

    private IEnumerator CloseGripperCoroutine(float timeToComplete = 1f, int steps = 25)
    {
        WaitForSecondsRealtime lag = new WaitForSecondsRealtime(timeToComplete / steps);
        float deltaAngle = (GripperRange / steps) * Mathf.Rad2Deg;
        for (int i = 0; i < steps; i++)
        {
            for (int j = 0; j < GripperJoints.Count; j++)
            {
                ArticulationDrive drive = GripperJoints[j].xDrive;
                if (j == 0 || j == 3)
                {
                    drive.target -= deltaAngle;
                }
                else
                {
                    drive.target += deltaAngle;

                }
                GripperJoints[j].xDrive = drive;
            }
            yield return lag;
        }
    }

    private IEnumerator OpenGripperCoroutine(float timeToComplete = 1f, int steps = 25)
    {
        WaitForSecondsRealtime lag = new WaitForSecondsRealtime(timeToComplete / steps);
        float deltaAngle = (GripperRange / steps) * Mathf.Rad2Deg;
        for (int i = 0; i < steps; i++)
        {
            foreach (ArticulationBody joint in GripperJoints)
            {
                ArticulationDrive drive = joint.xDrive;
                if (drive.target < 0)
                {
                    drive.target += deltaAngle;
                }
                else
                {
                    drive.target -= deltaAngle;
                }
                joint.xDrive = drive;
            }
            yield return lag;
        }
    }

    public void SnapJointToTargetAngle(int index, float target)
    {
        List<float> current = new List<float>();
        BaseArticulation.GetJointPositions(current);
        current[index] = target;
        BaseArticulation.SetDriveTargets(current);
        BaseArticulation.SetJointPositions(current);
    }
}
