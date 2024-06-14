using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GlobalManager : MonoBehaviour
{
    public Slider TimeScaleSlider;
    public TMP_Text TimeScaleText;
    public static float TimeScale;

    public Slider CriterionForEncodingSlider;
    public TMP_Text CriterionForEncodingText;
    public static float CriterionForEncoding;

    public Slider DecayRateSlider;
    public TMP_Text DecayRateText;
    public static float DecayRate;

    public Slider RefreshDurationSlider;
    public TMP_Text RefreshDurationText;
    public static float RefreshDuration;

    public Slider EncodingRateSlider;
    public TMP_Text EncodingRateText;
    public static float EncodingRate;

    public Slider FreeTimeSlider;
    public TMP_Text FreeTimeText;
    public static float FreeTime;

    public Slider RecallThresholdSlider;
    public TMP_Text RecallThresholdText;
    public static float RecallThreshold;

    public Slider RecallNoiseSlider;
    public TMP_Text RecallNoiseText;
    public static float RecallNoise;

    public static float EncodingTime;
    public static bool PlayAnimation = true;

    private List<float> timeList = new List<float>();
    private List<taskType> taskList = new List<taskType>();
    
    private int currentState;
    public int setSize;

    private float timerExternal;  // 实验流程计时器
    private int taskIndexExternal = 0;
    private float timerInternal;  // 心理加工过程计时器
    public static State internalState;
    public static float internalStateTime = Mathf.Infinity;

    public static bool timerStart;
    private int alreadyConsolidatedItemNum = 0;
    private GameObject attention;
    private AttentionController attentionController;

    private void Awake()
    {
        TimeScale = 1.0f;
        CriterionForEncoding = 0.95f;
        DecayRate = 0.5f;
        RefreshDuration = 0.08f;
        FreeTime = 0.0f;
        RecallThreshold = 0.5f;
        RecallNoise = 0.1f;
    }

    private void Start()
    {
        attention = GameObject.FindGameObjectWithTag("attention");
        attentionController = attention.GetComponent<AttentionController>();
    }


    // Update is called once per frame
    void Update()
    {
        TimeScale = TimeScaleSlider.value;
        TimeScaleText.text = TimeScaleSlider.value.ToString("f2");
        Time.timeScale = TimeScale;

        CriterionForEncoding = CriterionForEncodingSlider.value;
        CriterionForEncodingText.text = CriterionForEncodingSlider.value.ToString("f2");

        DecayRate = DecayRateSlider.value;
        DecayRateText.text = DecayRateSlider.value.ToString("f2");

        RefreshDuration = RefreshDurationSlider.value;
        RefreshDurationText.text = RefreshDurationSlider.value.ToString("f2");

        EncodingRate = EncodingRateSlider.value;
        EncodingRateText.text = EncodingRateSlider.value.ToString("f2");

        FreeTime = FreeTimeSlider.value;
        FreeTimeText.text = FreeTimeSlider.value.ToString("f2");

        RecallThreshold = RecallThresholdSlider.value;
        RecallThresholdText.text = RecallThresholdSlider.value.ToString("f2");

        RecallNoise = RecallNoiseSlider.value;
        RecallNoiseText.text = RecallNoiseSlider.value.ToString("f2");

        if (timerStart)
        {
            timerExternal += Time.deltaTime;
            timerInternal += Time.deltaTime;
            HandleTimer();
        }
        
    }

    public void RunTrial()
    {        
        InitializeState();
    }

    private void InitializeState()
    {
        timerExternal = 0f;
        taskIndexExternal = -1;        
        timerInternal = 0f;
        alreadyConsolidatedItemNum = 0;
        (timeList, taskList, setSize) = ComplexSpan1();
        HandleState();
        timerStart = true;
    }

    /// <summary>
    /// handle external and internal timer
    /// </summary>
    private void HandleTimer()
    {
        if (timerExternal >= timeList[taskIndexExternal])
        {
            timerExternal = 0f;
            HandleState();
        }
        if (timerInternal >= internalStateTime)
        {
            timerInternal = 0f;
            HandleStateAfterEncoding();
        }
    }

    private void HandleState()
    {
        taskIndexExternal += 1;  // current state

        // internal state change according to new stimulus
        attentionController.refreshingIndex = 0;
        if (taskList[taskIndexExternal] == taskType.memory)
        {
            internalState = State.MemoryEncoding;
            GetEncodingTime(timeList[taskIndexExternal]);
            internalStateTime = EncodingTime;

            attentionController.ChangeFoA(alreadyConsolidatedItemNum, internalStateTime);
            alreadyConsolidatedItemNum += 1;            
        }
        if (taskList[taskIndexExternal] == taskType.processing)
        {
            internalState = State.Processing;
            internalStateTime = timeList[taskIndexExternal] - FreeTime;
            Debug.Log(internalStateTime);
            attentionController.ChangeFoA(setSize, internalStateTime);  // FoA is on the processing task
        }
        if (taskList[taskIndexExternal] == taskType.recall)
        {
            internalState = State.Retrieval;
            internalStateTime = 2f;
            attentionController.ChangeFoA(setSize, internalStateTime);  // 暂时如此
            timerStart = false;
        }        
    }


    /// <summary>
    /// change state after encoding
    /// </summary>
    private void HandleStateAfterEncoding()
    {
        float refreshingTotalTime;
        if (taskList[taskIndexExternal] == taskType.memory)
        {
            // if there is remained time, refresh memory items
            CumulativeRefreshing(alreadyConsolidatedItemNum);
        }
        if (taskList[taskIndexExternal] == taskType.processing)
        {
            CumulativeRefreshing(alreadyConsolidatedItemNum);
        }
        if (taskList[taskIndexExternal] == taskType.recall)
        {

        }
    }

    private void CumulativeRefreshing(int itemNumTotal)
    {
        internalState = State.Refreshing;
        internalStateTime = Mathf.Min(RefreshDuration, timeList[taskIndexExternal] - timerExternal);
        attentionController.ChangeFoA(attentionController.refreshingIndex, internalStateTime);
        attentionController.refreshingIndex += 1;  // next item being refreshed
        if (attentionController.refreshingIndex >= itemNumTotal)
        {
            attentionController.refreshingIndex = 0;
        }
    }

    public static (List<float>, List<taskType>, int) ComplexSpan1()
    {
        // time parameter
        List<float> timeList = new List<float>();
        List<taskType> taskList = new List<taskType>();
        int setSize = 4;
        int processingEpisode = 1;
        float memPreTime = 1.0f;
        float isi = 1.8f;
        (timeList, taskList) = CSGeneration1(setSize, processingEpisode, memPreTime, isi);        

        return (timeList, taskList, setSize);

        // scene reset
    }

    private void GetEncodingTime(float memPreTime)
    {
        EncodingTime = Mathf.Min(memPreTime, -(Mathf.Log(1 - CriterionForEncoding) / EncodingRate));
    }

    /// <summary>
    /// complex span paradigm with fixed free time
    /// </summary>
    /// <param name="setSize"></param>
    /// <param name="processingEpisode"></param>
    /// <param name="memPreTime"></param>
    /// <param name="freeTime"></param>
    /// <param name="isi"></param>
    private static (List<float>, List<taskType>) CSGeneration1(int setSize, int processingEpisode, float memPreTime, float isi)
    {
        List<float> timeList = new List<float>();
        List<taskType> taskList = new List<taskType>();
        for (int i = 0; i < setSize; i = i + 1)
        {
            timeList.Add(memPreTime);
            taskList.Add(taskType.memory);

            for (int j = 0; j < processingEpisode; j = j + 1)
            {
                timeList.Add(isi);
                taskList.Add(taskType.processing);
            }
        }
        timeList.Add(1.0f);
        taskList.Add(taskType.recall);
        return (timeList, taskList);
    }

}


public enum State
{
    MemoryEncoding,
    Processing,
    Refreshing,
    Retrieval
}

public enum taskType
{
    memory,
    processing,
    recall
}