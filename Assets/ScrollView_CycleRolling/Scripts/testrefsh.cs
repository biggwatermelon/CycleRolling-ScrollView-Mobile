using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class testrefsh : MonoBehaviour {
    public UI_Control_ScrollFlow _ScrollFlow;
	void Start () {
        _ScrollFlow.Refresh();
	    _ScrollFlow.MoveEnd += MoveOverEvent;
    }
	void OnEnable ()
	{
		
	}
	private void MoveOverEvent(UI_Control_ScrollFlow_Item t)
	{
		//上面参数框里MoveOverEvent(UI_Control_ScrollFlow_Item t)
		//t表示滑动后或者直接点选的对象 ，假如大家在做的时候 每个Item上有
		//代码绑定 可以通过t.GetComponent<>()来查找 并操作。
		Debug.Log("你要做的事情");
        foreach (UI_Control_ScrollFlow_Item temp in UI_Control_ScrollFlow.instance.Items)
        {
            temp.gameObject.GetComponent<Image>().sprite = temp.GetComponent<UI_Control_ScrollFlow_Item>().Norimg;
        }
        t.GetComponent<Image>().sprite = t.GetComponent<UI_Control_ScrollFlow_Item>().Preimg;
    }
	void OnDisable()
	{
		_ScrollFlow.MoveEnd -= MoveOverEvent;
	}
	void Update () {
	
	}

	/// <summary>
	/// 下一个
	/// </summary>
	public void Next()
	{
		_ScrollFlow.ToNext ();
	}
	/// <summary>
	/// 上一个
	/// </summary>
	public void Before()
	{
		_ScrollFlow.ToBefore ();
	}
}
