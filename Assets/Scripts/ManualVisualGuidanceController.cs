using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualVisualGuidanceController : MonoBehaviour
{
    public GameObject RealRobot;
    public ManualController RealRobotDrive;
    public GameObject GhostRobot;
    public Material GhostMaterial;
    public List<float> ArticulationTargets;
    public List<float> ArticulationPositions;
    public List<float> ArticulationVelocities;
    public ArticulationBody BaseArticulation;
    public List<ArticulationBody> JointArticulations;

    public bool IsInvisible = false;

    // Start is called before the first frame update
    void Start()
    {
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
        foreach(Collider c in gameObject.GetComponentsInChildren<Collider>()) c.enabled = false;
        MakeRobotRigid();
        Vanish();
    }

    // Update is called once per frame
    void Update()
    {
        BaseArticulation.GetDriveTargets(ArticulationTargets);
        BaseArticulation.GetJointPositions(ArticulationPositions);
        BaseArticulation.GetJointVelocities(ArticulationVelocities);
    }

    public void Vanish()
    {
        IsInvisible = true;
        foreach(Renderer r in gameObject.GetComponentsInChildren<Renderer>()) r.enabled = false;
    }

    public void Appear()
    {
        IsInvisible = false;
        foreach(Renderer r in gameObject.GetComponentsInChildren<Renderer>()) r.enabled = true;
    }

    public void MakeRobotRigid()
    {
        for (int i = 0; i < JointArticulations.Count; i++)
        {
            ArticulationDrive drive = JointArticulations[i].xDrive;
            drive.stiffness = 100000f;
            drive.damping = 80000f;
            drive.forceLimit = float.PositiveInfinity;
            BaseArticulation.SetDriveTargets(ArticulationPositions);
            JointArticulations[i].xDrive = drive;
        }
    }

    public void SetShoulderPanTarget(float t)
    {
        if (IsInvisible) Appear();
        SnapJointToTargetAngle(0, t);
    }

    public void SetShoulderLiftTarget(float t)
    {
        if (IsInvisible) Appear();
        SnapJointToTargetAngle(1, t);
    }

    public void SetElbowTarget(float t)
    {
        if (IsInvisible) Appear();
        SnapJointToTargetAngle(2, t);
    }

    public void SetWrist1Target(float t)
    {
        if (IsInvisible) Appear();
        SnapJointToTargetAngle(3, t);
    }

    public void SetWrist2Target(float t)
    {
        if (IsInvisible) Appear();
        SnapJointToTargetAngle(4, t);
    }

    public void SetWrist3Target(float t)
    {
        if (IsInvisible) Appear();
        SnapJointToTargetAngle(5, t);
    }

    private void SnapJointToTargetAngle(int index, float target)
    {
        List<float> current = new List<float>();
        BaseArticulation.GetJointPositions(current);
        current[index] = target * Mathf.Deg2Rad;
        BaseArticulation.SetDriveTargets(current);
        BaseArticulation.SetJointPositions(current);
    }

    public IEnumerator ExecutePreplannedTrajectory(float timeToComplete = 1f, int steps = 50)
    {
        WaitForSecondsRealtime lag = new WaitForSecondsRealtime(timeToComplete / steps);

        List<float> current = new List<float>();
        List<float> expected = new List<float>();

        RealRobotDrive.BaseArticulation.GetJointPositions(current);
        BaseArticulation.GetJointPositions(expected);

        float delta0 = (expected[0] - current[0]) / steps;
        float delta1 = (expected[1] - current[1]) / steps;
        float delta2 = (expected[2] - current[2]) / steps;
        float delta3 = (expected[3] - current[3]) / steps;
        float delta4 = (expected[4] - current[4]) / steps;
        float delta5 = (expected[5] - current[5]) / steps;

        for (int i = 0; i < steps; i++)
        {
            current[0] += delta0;
            current[1] += delta1;
            current[2] += delta2;
            current[3] += delta3;
            current[4] += delta4;
            current[5] += delta5;
            RealRobotDrive.BaseArticulation.SetDriveTargets(current);
            RealRobotDrive.BaseArticulation.SetJointPositions(current);
            yield return lag;
        }
        Vanish();
    }

    public void ExecButtonHook()
    {
        StartCoroutine(ExecutePreplannedTrajectory());
    }
}
