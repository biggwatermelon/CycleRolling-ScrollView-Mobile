using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UI_Control_ScrollFlow_Item : MonoBehaviour
{
	private UI_Control_ScrollFlow parent;
	[HideInInspector]
	public RectTransform rect;
	public Image img;
    public Sprite Norimg;
    public Sprite Preimg;

    public List<Image> imgList;
	public List<Text> txtList;


	public float v=0;
	private Vector3 p, s;
	/// <summary>
	/// 缩放值
	/// </summary>
	public float sv;
	// public float index = 0,index_value;
	private Color color;

	public void Init(UI_Control_ScrollFlow _parent)
	{
		rect = GetComponent<RectTransform>();
		parent = _parent;
		if (img!=null)
		{
			color = img.color;
		}

	}

	public void Drag(float value)
	{
		v += value;
		Refresh();
	}
	public void Refresh()
	{
		p = rect.localPosition;
		p.x = parent.GetPosition(v);
		rect.localPosition = p;

		color.a = parent.GetApa(v);
		if (img != null)
		{
			img.color = color;
		}

		for (int i = 0; i < imgList.Count; i++)
		{
			imgList[i].color = new Color(imgList[i].color.r, imgList[i].color.g, imgList[i].color.b, color.a); ;
		}
		for (int i = 0; i < txtList.Count; i++)
		{
			txtList[i].color = new Color(txtList[i].color.r, txtList[i].color.g, txtList[i].color.b, color.a);
		}
		sv = parent.GetScale(v);
		s.x = sv;
		s.y = sv;
		s.z = 1;
		rect.localScale = s;
	}
}