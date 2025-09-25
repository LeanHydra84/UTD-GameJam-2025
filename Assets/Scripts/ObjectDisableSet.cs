using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ObjectDisableSet : MonoBehaviour
{
	private bool isEnabled = true;
	[SerializeField] private GameObject[] set;

	private void SetEnabledInstance(bool en)
	{
		if (en == isEnabled) return;
		
		isEnabled = en;
		foreach (GameObject go in set)
		{
			go.SetActive(en);
		}
	}

	void Start()
	{
		_instance = this; 
	}

	private static ObjectDisableSet _instance;
	public static ObjectDisableSet Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Component.FindFirstObjectByType<ObjectDisableSet>();
			}
			return _instance;
		}
	}

	public static void SetEnabled(bool en)
	{
		var currentInstance = Instance;
		if (currentInstance == null) return;
		
		currentInstance.SetEnabledInstance(en);
	}

}
