using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingFrictionForceVisualizer : MonoBehaviour
{
    public GameObject agent1;
    public GameObject agent2;

    [Range(0, 10)]
    public float speed1;
    [Range(0, 10)]
    public float speed2;
    
    void Update()
    {
        Debug.DrawLine(agent1.transform.position, agent1.transform.position + Vector3.up * 3, Color.green);
        Debug.DrawLine(agent1.transform.position, agent1.transform.position + agent1.transform.forward * speed1, Color.cyan);
        Debug.DrawLine(agent2.transform.position, agent2.transform.position + agent2.transform.forward * speed2, Color.cyan);

        var n = (agent2.transform.position - agent1.transform.position).normalized;
        var tangent = Vector3.Cross(Vector3.up, n);
        Debug.DrawLine(agent1.transform.position, agent1.transform.position + tangent * 2, Color.yellow);

        var magnitude = Vector3.Dot(agent1.transform.forward * speed1 - agent2.transform.forward * speed2, tangent);
        Debug.DrawLine(agent1.transform.position, agent1.transform.position + tangent * magnitude * 2, Color.red);
        
        Debug.DrawLine(agent1.transform.position + agent1.transform.forward * speed1, agent2.transform.position + agent2.transform.forward * speed2, Color.magenta);

        #region Visualization

#if UNITY_EDITOR
        if (Application.isFocused)
        {
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        }
#endif

        #endregion
    }
}
