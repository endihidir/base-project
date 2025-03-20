using System.IO;
using Newtonsoft.Json.Linq;
//using Unity.RemoteConfig.Editor;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class UgsEnvironmentUtilityEditor
{
    private static string FolderPath = "Assets/__Funflare/Scripts/UgsEnvironment";
    
    private static string ScriptName = "UgsEnvironmentProvider";
    
    private static string FullPath = Path.Combine(FolderPath, $"{ScriptName}.cs");
    
    static UgsEnvironmentUtilityEditor() => CreateFiles();

    private static void CreateFiles()
    {
        /*if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }
        
        var files = Directory.GetFiles(FolderPath, "*.cs");
        foreach (var file in files)
        {
            if (!file.EndsWith($"{ScriptName}.cs"))
            {
                File.Delete(file);
            }
        }*/

       // RemoteConfigWebApiClient.fetchEnvironmentsFinished += OnFetchEnvironmentFinished;
        
       // RemoteConfigWebApiClient.FetchEnvironments("67a9966f-84a9-456f-a459-5c85663a766e", Debug.LogError);
    }

    private static void OnFetchEnvironmentFinished(JArray environment)
    {
        string scriptTemplate = null;
        
        string enumName = "EnvironmentType";
        
        string enumValues = "";
        
        string idArrayName = "EnvironmentIds";
        string idArrayValues = "";

        for (var i = 0; i < environment.Count; i++)
        {
            if (environment[i] is JObject envObject)
            {
                string environmentId = envObject["id"]?.ToString();
                string environmentName = envObject["name"]?.ToString();
                
                enumValues += $"    {environmentName.Replace(" ", "_").ToUpperFirstLetter()} = {i},\n";
                
                idArrayValues += $"        \"{environmentId}\",\n";
            }
        }
        
        scriptTemplate = 
$@"using System;

public static class UgsEnvironmentProvider
{{
    private static readonly string[] {idArrayName} = 
    {{
{idArrayValues.TrimEnd('\n', ',')}
    }};

    public static string GetEnvironmentId({enumName} environment)
    {{
        int index = (int)environment;
        
        return EnvironmentIds[index];
    }}

    public static {enumName} GetEnvironmentEnum(string environmentId)
    {{
        int index = Array.IndexOf(EnvironmentIds, environmentId);

        return ({enumName})index;
    }}
}}

public enum {enumName}
{{
{enumValues.TrimEnd('\n', ',')}
}}";
        
        
        if (File.Exists(FullPath))
        {
            string existingContent = File.ReadAllText(FullPath);

            if (existingContent != scriptTemplate)
            {
                File.Delete(FullPath);
                File.WriteAllText(FullPath, scriptTemplate);
                AssetDatabase.Refresh();
            }
        }
        else
        {
            File.WriteAllText(FullPath, scriptTemplate);
            AssetDatabase.Refresh();
        }

        //RemoteConfigWebApiClient.fetchEnvironmentsFinished -= OnFetchEnvironmentFinished;
    }
    
    private static string ToUpperFirstLetter(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
    
        var firstLetter = char.ToUpperInvariant(str[0]);
        var restOfString = str.Substring(1);

        return firstLetter + restOfString;
    }
}
