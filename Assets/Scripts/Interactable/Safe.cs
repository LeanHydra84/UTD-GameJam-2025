using System.Collections.Generic;
using UnityEngine;

public class Safe : MonoBehaviour
{
	private string correct = "468";
	private string during = "";

	private List<SafeButton> buttons;
	
	public void Register(SafeButton but)
	{
		buttons ??= new List<SafeButton>();
		buttons.Add(but);
	}

	public void ResetAll()
	{
		foreach (var v in buttons)
		{
			v.Reset();
		}
	}
	
	public void Push(int value)
	{
		during += value.ToString();
		if (during.Length == correct.Length)
		{
			if (during == correct)
			{
				Destroy(gameObject);
				return;
			}
			
			during = "";
			ResetAll();
		}
	}
	
}
