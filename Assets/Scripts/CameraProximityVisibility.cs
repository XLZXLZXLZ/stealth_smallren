using UnityEngine;

public class CameraProximityVisibility : MonoBehaviour
{
    [SerializeField] private float m_VisibilityDistance = 1f;
    private Transform m_CameraTransform;
    private MeshRenderer m_MeshRenderer;

    private void Start()
    {
        m_MeshRenderer = this.GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (!m_MeshRenderer || !TankCharacterController.Instance)
        {
            return;
        }
        if (!m_CameraTransform)
        {
            m_CameraTransform = TankCharacterController.Instance.gameObject.GetComponentInChildren<Camera>().transform;
        }

        m_MeshRenderer.enabled = Vector3.Distance(this.transform.position, m_CameraTransform.position) > m_VisibilityDistance;
    }
}
