
using System.Collections.Generic;

[System.Serializable]
public class MeasurementModel 
{
    public float weight;
    public float bodyFat;
    public float chest;
    public float shoulder;
    public float hips;
    public float waist;
    public float leftThigh;
    public float rightThigh;
    public float leftBicep;
    public float rightBicep;
    public float leftForearm;
    public float rightForearm;
    public float leftCalf;
    public float rightCalf;
    public WeightUnit weightUnit; // New field to store the weight unit
}
[System.Serializable]
public class MeasurementHistory
{
    public List<MeasurementHistoryItem> measurmentHistory = new List<MeasurementHistoryItem>();
}
[System.Serializable]
public class MeasurementHistoryItem
{
    public string name;
    public string dateTime;
    public float value;
    public WeightUnit weightUnit; // Add this field
}