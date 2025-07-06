/*using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Gestisce la transizione tra Idle e Walk leggendo la velocità del NavMeshAgent.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class ClientAnimatorController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Legge la velocità del NavMeshAgent
        float speed = agent.velocity.magnitude;

        // Aggiorna il parametro Speed nell'Animator
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }
}
*/