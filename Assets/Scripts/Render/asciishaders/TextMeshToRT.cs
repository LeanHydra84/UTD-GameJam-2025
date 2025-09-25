using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TextMeshToRT : MonoBehaviour
{

	[SerializeField] TextMeshProUGUI text;
	[SerializeField] private RenderTexture rt;
	
	RenderTexture RenderTMP(TextMeshProUGUI textMesh)
	{
		Vector2 size = new Vector2(100, 100);
		// RenderTexture rt = //RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
		// 	new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
		
		GL.PushMatrix();
		GL.LoadIdentity();

		Matrix4x4 proj = Matrix4x4.Ortho(-size.x / 2f, size.x / 2f, -size.x / 2f, size.y / 2f, -10f, 100f);
		GL.LoadProjectionMatrix(proj);
		
		textMesh.materialForRendering.SetPass(0);
		
		RenderTexture currentRT = RenderTexture.active;
		Graphics.SetRenderTarget(rt);
		
		GL.Clear(false, true, new Color(0, 0, 0));
		Graphics.DrawMeshNow(textMesh.mesh, Matrix4x4.identity);
		
		// restore
		GL.PopMatrix();
		RenderTexture.active = currentRT;

		return rt;
	}

	IEnumerator Hold()
	{
		yield return new WaitForEndOfFrame();
		var rt = RenderTMP(text);
		GetComponent<Renderer>().material.SetTexture("_MainTex", rt);
	}

	void Start()
	{
		StartCoroutine(Hold());
	}
	
}
