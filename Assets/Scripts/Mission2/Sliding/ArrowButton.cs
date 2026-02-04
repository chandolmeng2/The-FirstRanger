using UnityEngine;
using UnityEngine.UI;

public class ArrowButton : MonoBehaviour
{
    private Vector2Int direction;
    private RobotController robot;

    public void Initialize(RobotController robot, Vector2Int dir)
    {
        this.robot = robot;
        this.direction = dir;

        GetComponent<Button>().onClick.AddListener(() =>
        {
            robot.MoveInDirection(direction);
        });
    }
}
