#pragma warning disable 649

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;

public class Prisoner : MonoBehaviour
{
    public DecalProjector m_DecalProjector;
    public Material m_LetterOverrideMaterial;
    public Material[] m_Letters;

    private NavMeshAgent m_Agent;
    private Animator m_Animator;
    private Material m_LetterMaterial;
    private bool m_Trapped;
    private PrisonerEscape m_ClosestEscape;
    private float m_LastEscapeSearch;
    private Vector3 m_LastPosition;
    private float m_CurrentVelocity;
    private bool m_Escaped;

    private void Start()
    {
        m_Agent = gameObject.GetComponent<NavMeshAgent>();
        m_Animator = gameObject.GetComponentInChildren<Animator>();
        m_Animator.SetFloat("Offset", Random.Range(0f, 1f));

        if (m_LetterOverrideMaterial != null)
        {
            m_LetterMaterial = m_LetterOverrideMaterial;
        }
        else
        {
            m_LetterMaterial = m_Letters[Random.Range(0, m_Letters.Length)];
        }

        m_DecalProjector.material = m_LetterMaterial;
        m_LastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (!m_Agent.isOnNavMesh)
        {
            return;
        }

        var distance = Vector3.Distance(m_LastPosition, transform.position);
        m_LastPosition = transform.position;

        var velocity = distance / Time.fixedDeltaTime;
        m_CurrentVelocity += (velocity - m_CurrentVelocity) * Time.fixedDeltaTime * 2f;

        m_Animator.SetFloat("Speed", m_CurrentVelocity / m_Animator.speed);

        if (Time.time - m_LastEscapeSearch > 1f)
        {
            m_LastEscapeSearch = Time.time;
            this.FindClosestEscape();

            if (m_ClosestEscape != null)
            {
                var path = new NavMeshPath();
                m_Agent.CalculatePath(m_ClosestEscape.transform.position, path);

                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    m_Agent.SetDestination(m_ClosestEscape.transform.position);
                }
                else
                {
                    m_Agent.ResetPath();
                }
            }
            else
            {
                m_Agent.ResetPath();
            }
        }

        if (!m_Escaped && m_ClosestEscape != null && Vector3.Distance(m_ClosestEscape.transform.position, transform.position) <= 1.15f)
        {
            m_Escaped = true;
            m_ClosestEscape.AddEscape();
        }
    }

    private void FindClosestEscape()
    {
        var escapes = Object.FindObjectsOfType<PrisonerEscape>();
        var closestDistance = 9999999999f;
        
        foreach (var escape in escapes)
        {
            var distance = Vector3.Distance(transform.position, escape.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                m_ClosestEscape = escape;
            }
        }
    }

    private void Update()
    {
        if (!m_Trapped)
        {

        }
    }
}

#pragma warning restore 649
