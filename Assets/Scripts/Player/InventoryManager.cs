using UnityEngine;

public class InventoryManager : MonoBehaviour
{

	public static InventoryManager Instance;
	
	// this is terrible design but whatever, we are so pressed on time...
	void Start()
	{
		Instance = this;
	}
	
	public bool HasFuzeRed;
	public bool HasFuzeBlue;
	public bool HasKey;

	public bool HasFilmReel;

}
