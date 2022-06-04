using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum oml_move_method_enum
{
	teleport,
	normal_speed,
	parabola,
	curve
}

[RequireComponent(typeof(NavMeshAgent))]
public class AgentLinkMover : MonoBehaviour
{
	public oml_move_method_enum method = oml_move_method_enum.parabola;
	public AnimationCurve curve = new AnimationCurve();

	IEnumerator Start()
	{
		NavMeshAgent agent = GetComponent<NavMeshAgent>();
		agent.autoTraverseOffMeshLink = false;
		while (true)
		{
			if (agent.isOnOffMeshLink)
			{
				if (method == oml_move_method_enum.normal_speed)
					yield return StartCoroutine(NormalSpeed(agent));
				else if (method == oml_move_method_enum.parabola)
					yield return StartCoroutine(Parabola(agent, 2.0f, 0.5f));
				else if (method == oml_move_method_enum.curve)
					yield return StartCoroutine(Curve(agent, 0.5f));
				if (agent.isOnNavMesh) {
					agent.CompleteOffMeshLink();
				}
			}
			yield return null;
		}
	}

	IEnumerator NormalSpeed(NavMeshAgent agent)
	{
		OffMeshLinkData data = agent.currentOffMeshLinkData;
		Vector3 end_pos = data.endPos + Vector3.up * agent.baseOffset;
		while (agent.transform.position != end_pos)
		{
			agent.transform.position = Vector3.MoveTowards(agent.transform.position, end_pos, agent.speed * Time.deltaTime);
			yield return null;
		}
	}

	IEnumerator Parabola(NavMeshAgent agent, float height, float duration)
	{
		OffMeshLinkData data = agent.currentOffMeshLinkData;
		Vector3 start_pos = agent.transform.position;
		Vector3 end_pos = data.endPos + Vector3.up * agent.baseOffset;
		float normalized_time = 0.0f;
		while (normalized_time < 1.0f)
		{
			float yOffset = height * 4.0f * (normalized_time - normalized_time * normalized_time);
			agent.transform.position = Vector3.Lerp(start_pos, end_pos, normalized_time) + yOffset * Vector3.up;
			normalized_time += Time.deltaTime / duration;
			yield return null;
		}
	}

	IEnumerator Curve(NavMeshAgent agent, float duration)
	{
		OffMeshLinkData data = agent.currentOffMeshLinkData;
		Vector3 start_pos = agent.transform.position;
		Vector3 end_pos = data.endPos + Vector3.up * agent.baseOffset;
		float normalized_time = 0.0f;
		while (normalized_time < 1.0f)
		{
			float yOffset = curve.Evaluate(normalized_time);
			agent.transform.position = Vector3.Lerp(start_pos, end_pos, normalized_time) + yOffset * Vector3.up;
			normalized_time += Time.deltaTime / duration;
			yield return null;
		}
	}
}