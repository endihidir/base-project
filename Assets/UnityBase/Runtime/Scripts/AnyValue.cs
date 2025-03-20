using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public struct AnyValue
{
    public enum ValueType {Int, Float, Bool, String, Vector3}

    [AllowNesting, HideInInspector]
    public ValueType type;
    
    [AllowNesting, ShowIf(nameof(IsBool))]
    public bool boolValue;
    
    [AllowNesting, ShowIf(nameof(IsInt))]
    public int intValue;
    
    [AllowNesting, ShowIf(nameof(IsFloat))]
    public float floatValue;
    
    [AllowNesting, ShowIf(nameof(IsString))]
    public string stringValue;
    
    [AllowNesting, ShowIf(nameof(IsVector3))]
    public Vector3 vector3Value;

    public bool IsBool => type == ValueType.Bool;
    public bool IsInt => type == ValueType.Int;
    public bool IsFloat => type == ValueType.Float;
    public bool IsString => type == ValueType.String;
    public bool IsVector3 => type == ValueType.Vector3;

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
        ValueType.String => AsString<T>(stringValue),
        ValueType.Vector3 => AsVector3<T>(vector3Value),
        _ => throw new NotSupportedException($"Not Supported Value type: {typeof(T)}")
    };

    private T AsBool<T>(bool value) => typeof(T) == typeof(bool) && value is T correctType ? correctType : default;
    private T AsInt<T>(int value) => typeof(T) == typeof(int) && value is T correctType ? correctType : default;
    private T AsFloat<T>(float value) => typeof(T) == typeof(float) && value is T correctType ? correctType : default;
    private T AsString<T>(string value) => typeof(T) == typeof(string) && value is T correctType ? correctType : default;
    private T AsVector3<T>(Vector3 value) => typeof(T) == typeof(Vector3) && value is T correctType ? correctType : default;
}