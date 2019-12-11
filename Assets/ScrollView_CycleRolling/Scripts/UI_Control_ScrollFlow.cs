using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UI_Control_ScrollFlow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public static UI_Control_ScrollFlow instance;
    public RectTransform Rect;
    public List<UI_Control_ScrollFlow_Item> Items;
    [Header("是否为垂直拖动，否默认为水平拖动")]
    public bool Vertical = false;
    public float Width = 500;
    public float MaxScale;
   
    /// <summary>
    /// 开始坐标值，间隔坐标值，小于vmian 达到最左，大于vmax达到最右
    /// </summary>
    private float StartValue = 0.5f;
    public float VMin = 0.1f, VMax = 0.9f;


    [Header("单边是多少,总数是乘以2+1,例m_count=3 总数 = 7")]
    [SerializeField]
    private int m_nCount;
    public int nCount
    {
        get
        {
            return m_nCount * 2 + 1;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 坐标曲线
    /// </summary>
    public AnimationCurve PositionCurve;
    /// <summary>
    /// 大小曲线
    /// </summary>
    public AnimationCurve ScaleCurve;
    /// <summary>
    /// 透明曲线
    /// </summary>
    public AnimationCurve ApaCurve;

    private float v = 0;
    private List<UI_Control_ScrollFlow_Item> GotoFirstItems = new List<UI_Control_ScrollFlow_Item>(), GotoLaserItems = new List<UI_Control_ScrollFlow_Item>();
    /// <summary>
    /// 计算值
    /// </summary>
    private Vector2 start_point, add_vect;
    public event CallBack<UI_Control_ScrollFlow_Item> MoveEnd;
    private float centerX;
    private float height;
    private float spacingx;
    private UI_Control_ScrollFlow_Item firstitem;
    public void Refresh()
    {
        Debug.Log("===================Refresh=================");
        for (int i = 0; i < Rect.childCount; i++)
        {
            Transform tran = Rect.GetChild(i);
            if (ItemsOld.Contains(tran)) { continue; }
            if (!tran.gameObject.activeInHierarchy) { continue; }

            UI_Control_ScrollFlow_Item item = tran.GetComponent<UI_Control_ScrollFlow_Item>();
            if (item != null)
            {
                Items.Add(item);
                item.Init(this);
                item.Drag(StartValue + (Items.Count - 1) * (1.0f / nCount));
                if (item.v - 0.5 < 0.05f)
                {
                    Current = item;
                    firstitem = Current;
                    centerX = Current.transform.localPosition.x;
                    height = Current.GetComponent<RectTransform>().sizeDelta.y;

                    foreach (UI_Control_ScrollFlow_Item t in Items)
                    {
                        t.GetComponent<Image>().sprite = 
                            t.GetComponent<UI_Control_ScrollFlow_Item>().Norimg;
                    }

                    Current.GetComponent<Image>().sprite =
                        Current.GetComponent<UI_Control_ScrollFlow_Item>().Preimg;
                    //item.GetComponent<uicontrolScrollViewItem>().Selected();
                }
                if (Items.Count == 2)
                {
                    spacingx =Mathf.Abs(Items[0].transform.localPosition.x - Items[1].transform.localPosition.x);
                    //Debug.LogError(spacingx);
                }
            }
            
            //Debug.Log(Items.Count.ToString() + "___" + item.name);
        }

        while (Items.Count < nCount)
        {
            m_nCount--;
        }

        VMax = 0.85f + (Items.Count - nCount) * (1.0f / nCount);
        VMin = 0.15f;


        Debug.Log("ddddd");
        if (MoveEnd != null)
        {
            MoveEnd(Current);
        }
        Check(1);
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].Refresh();
        }
        ItemsOld.Clear();
    }

    public void ResetI()
    {
        if (MoveEnd != null)
        {
            MoveEnd(firstitem);
        }
        float k = firstitem.v;
        k = 0.5f - k;
        AnimToEnd(k);
        add_vect = Vector3.zero;
        //foreach (UI_Control_ScrollFlow_Item t in Items)
        //{
        //    t.GetComponent<Image>().sprite = t.GetComponent<UI_Control_ScrollFlow_Item>().Preimg;
        //}
        //firstitem.GetComponent<LSW_ScrollViewItem>().Selected();
    }

    public List<Transform> ItemsOld;
    public void Clear()
    {
        ItemsOld = new List<Transform>();
        for (int i = 0; i < Items.Count; i++)
        {
            ItemsOld.Add(Items[i].transform);
            Destroy(Items[i].gameObject);
        }
        Items.Clear();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        start_point = eventData.position;
        add_vect = Vector3.zero;
        _anim = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        add_vect = eventData.position - start_point;
        if (Vertical)
        {
            v = -eventData.delta.y * 1.00f / Width;
        }
        else
        {
            v = eventData.delta.x * 1.00f / Width;
        }
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].Drag(v);
        }

        Check(v);
    }


    public void Check(float _v)
    {
        if (Items.Count < nCount) { return; }
        if (_v < 0)
        {//向左运动
            Debug.Log("向左运动");
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].v < (VMin - (1.0f / (float)nCount) / 2))
                {
                    GotoLaserItems.Add(Items[i]);
                }
            }
            if (GotoLaserItems.Count > 0)
            {
                for (int i = 0; i < GotoLaserItems.Count; i++)
                {
                    GotoLaserItems[i].v = Items[Items.Count - 1].v + (1.0f / (float)nCount);
                    Items.Remove(GotoLaserItems[i]);
                    Items.Add(GotoLaserItems[i]);
                }
                GotoLaserItems.Clear();
            }
        }
        else if (_v > 0)
        {//向右运动，需要把右边的放到前面来
            Debug.Log("向右运动");
            for (int i = Items.Count - 1; i > 0; i--)
            {
                if (Items[i].v > VMax)
                {
                    GotoFirstItems.Add(Items[i]);
                }
            }
            if (GotoFirstItems.Count > 0)
            {
                for (int i = 0; i < GotoFirstItems.Count; i++)
                {
                    GotoFirstItems[i].v = Items[0].v - (1.0f / nCount);
                    Items.Remove(GotoFirstItems[i]);
                    Items.Insert(0, GotoFirstItems[i]);
                }
                GotoFirstItems.Clear();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
        //add_vect = Vector3.zero;
        //AnimToEnd(k);
        add_vect = Vector3.zero;
        foreach (UI_Control_ScrollFlow_Item t in Items)
        {
            if (Mathf.Abs(t.transform.localPosition.x - centerX) < (spacingx / 2))
            {
                float k = t.v;
                k = 0.5f - k;
                AnimToEnd(k);
                //t.GetComponent<LSW_ScrollViewItem>().Selected();
            }
            else
            {
                //t.GetComponent<LSW_ScrollViewItem>().UnSelected();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (add_vect.sqrMagnitude <= 1)
        {
            UI_Control_ScrollFlow_Item script = eventData.pointerPressRaycast.gameObject.GetComponent<UI_Control_ScrollFlow_Item>();
            if (script != null)
            {
                float k = script.v;
                k = 0.5f - k;
                AnimToEnd(k);
                //script.GetComponent<LSW_ScrollViewItem>().Selected();
                Debug.Log(script.gameObject.transform.Find("Text").GetComponent<Text>().text);

                foreach (UI_Control_ScrollFlow_Item temp in Items)
                {
                    temp.gameObject.GetComponent<Image>().sprite = 
                        temp.GetComponent<UI_Control_ScrollFlow_Item>().Norimg;
                }
                script.GetComponent<Image>().sprite =
                    script.GetComponent<UI_Control_ScrollFlow_Item>().Preimg;
            }
        }
    }


    public float GetApa(float v)
    {
        return ApaCurve.Evaluate(v);
    }
    public float GetPosition(float v)
    {
        return PositionCurve.Evaluate(v) * Width;
    }
    public float GetScale(float v)
    {
        return ScaleCurve.Evaluate(v) * MaxScale;
    }


    private List<UI_Control_ScrollFlow_Item> SortValues = new List<UI_Control_ScrollFlow_Item>();
    public int index = 0;
    public void LateUpdate()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].v >= 0.1f && Items[i].v <= 0.9f)
            {
                index = 0;
                for (int j = 0; j < SortValues.Count; j++)
                {
                    if (Items[i].sv >= SortValues[j].sv)
                    {
                        index = j + 1;
                    }
                }

                SortValues.Insert(index, Items[i]);
            }
        }

        for (int k = 0; k < SortValues.Count; k++)
        {
            SortValues[k].rect.SetSiblingIndex(k);
        }
        SortValues.Clear();
    }

    public void ToLaster(UI_Control_ScrollFlow_Item item)
    {
        item.v = Items[Items.Count - 1].v + (1.0f / nCount);
        Items.Remove(item);
        Items.Add(item);
    }

    /// <summary>
    /// 是否开启动画
    /// </summary>
    public bool _anim = false;
    private float AddV = 0, Vk = 0, CurrentV = 0, Vtotal = 0, VT = 0;
    private float _v1 = 0, _v2 = 0;
    /// <summary>
    /// 动画速度
    /// </summary>
    public float _anim_speed = 1f;
    // private float start_time = 0, running_time = 0;

    public UI_Control_ScrollFlow_Item Current;



    public void AnimToEnd(float k)
    {
        AddV = k;
        if (AddV > 0) { Vk = 1; }
        else if (AddV < 0) { Vk = -1; }
        else
        {
            if (MoveEnd != null) { MoveEnd(Current); }
            return;
        }
        Vtotal = 0;
        _anim = true;
        
    }

    void Update()
    {
        if (_anim)
        {
            CurrentV = Time.deltaTime * _anim_speed * Vk;
            VT = Vtotal + CurrentV;
            if (Vk > 0 && VT >= AddV) { _anim = false; CurrentV = AddV - Vtotal; }
            if (Vk < 0 && VT <= AddV) { _anim = false; CurrentV = AddV - Vtotal; }
            //==============
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Drag(CurrentV);
                if (Items[i].v - 0.5 < 0.05f)
                {
                    Current = Items[i];
                    foreach (UI_Control_ScrollFlow_Item t in Items)
                    {
                        t.GetComponent<Image>().sprite =
                            t.GetComponent<UI_Control_ScrollFlow_Item>().Norimg;
                    }

                    Current.GetComponent<Image>().sprite =
                        Current.GetComponent<UI_Control_ScrollFlow_Item>().Preimg;
                }
            }
            Check(CurrentV);
            Vtotal = VT;


            if (!_anim)
            {
                if (MoveEnd != null) { MoveEnd(Current); }
            }
        }
    }


    public void ToNext()
    {
        if (Items.Count < nCount)
        {
            if (Items[0].v - VMin < 0.05)
            {
                //AnimToEnd(VMin - Items[0].v);
                return;
            }
        }

        float k = 0.5f - (1.0f / nCount) - Current.v;
        AnimToEnd(k);
    }
    public void ToBefore()
    {
        if (Items.Count < nCount)
        {
            if (Items[Items.Count - 1].v - VMax > -0.05f)
            {
                return;
            }
        }

        float k = 0.5f + (1.0f / nCount) - Current.v;
        AnimToEnd(k);
    }

}