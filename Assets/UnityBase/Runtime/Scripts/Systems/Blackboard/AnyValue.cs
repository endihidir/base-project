using System;
using UnityEngine;

[Serializable]
public struct AnyValue
{
    public enum ValueType {Int, Float, Bool, String, Vector3}
    
    public ValueType type;
    
    public bool boolValue;
    
    public int intValue;
    
    public float floatValue;
    
    public string stringValue;

    public Vector3 vector3Value;

    public static implicit operator bool(AnyValue value) => value.ConvertValue<bool>();
    public static implicit operator int(AnyValue value) => value.ConvertValue<int>();
    public static implicit operator float(AnyValue value) => value.ConvertValue<float>();
    public static implicit operator string(AnyValue value) => value.ConvertValue<string>();
    public static implicit operator Vector3(AnyValue value) => value.ConvertValue<Vector3>();

    private T ConvertValue<T>() => type switch
    {
        ValueType.Bool => AsBool<T>(boolValue),
        ValueType.Int => AsInt<T>(intValue),
        ValueType.Float => AsFloat<T>(floatValue),
        ValueType.String => (T)(object)stringValue,
        ValueType.Vector3 => (T)(object)vector3Value,
        _ => throw new NotSupportedException($"Not Supported Value type: {typeof(T)}")
    };

    private T AsBool<T>(bool value) => typeof(T) == typeof(bool) && value is T correctType ? correctType : default;
    private T AsInt<T>(int value) => typeof(T) == typeof(int) && value is T correctType ? correctType : default;
    private T AsFloat<T>(float value) => typeof(T) == typeof(float) && value is T correctType ? correctType : default;
}