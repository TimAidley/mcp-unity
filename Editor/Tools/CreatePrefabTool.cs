using System;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using McpUnity.Unity;
using McpUnity.Utils;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for creating prefabs with optional MonoBehaviour scripts
    /// </summary>
    public class CreatePrefabTool : McpToolBase
    {
        public CreatePrefabTool()
        {
            Name = "create_prefab";
            Description = "Creates a prefab with optional MonoBehaviour script and serialized field values";
        }
        
        /// <summary>
        /// Execute the CreatePrefab tool with the provided parameters
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        public override JObject Execute(JObject parameters)
        {
            // Extract parameters
            string scriptName = parameters["scriptName"]?.ToObject<string>();
            string prefabName = parameters["prefabName"]?.ToObject<string>();
            JObject fieldValues = parameters["fieldValues"]?.ToObject<JObject>();
            
            // Validate required parameters
            if (string.IsNullOrEmpty(prefabName))
            {
                return McpUnitySocketHandler.CreateErrorResponse(
                    "Required parameter 'prefabName' not provided", 
                    "validation_error"
                );
            }
            
            try
            {
                // Create a temporary GameObject
                GameObject tempObject = new GameObject(prefabName);
                Component component = null;
                
                // Add script component if scriptName is provided
                if (!string.IsNullOrEmpty(scriptName))
                {
                    // Find the script type
                    Type scriptType = Type.GetType($"{scriptName}, Assembly-CSharp");
                    if (scriptType == null)
                    {
                        // Try with just the class name
                        scriptType = Type.GetType(scriptName);
                    }
                    
                    if (scriptType == null)
                    {
                        // Try to find the type using AppDomain
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            scriptType = assembly.GetType(scriptName);
                            if (scriptType != null)
                                break;
                        }
                    }
                    
                    if (scriptType == null)
                    {
                        return McpUnitySocketHandler.CreateErrorResponse(
                            $"Script type '{scriptName}' not found. Ensure the script is compiled.", 
                            "not_found_error"
                        );
                    }
                    
                    // Check if the type is a MonoBehaviour
                    if (!typeof(MonoBehaviour).IsAssignableFrom(scriptType))
                    {
                        return McpUnitySocketHandler.CreateErrorResponse(
                            $"Type '{scriptName}' is not a MonoBehaviour", 
                            "invalid_type_error"
                        );
                    }
                    
                    // Add the script component
                    component = tempObject.AddComponent(scriptType);
                }
                
                // Apply field values if provided and component exists
                if (fieldValues != null && fieldValues.Count > 0 && component != null)
                {
                    Undo.RecordObject(component, "Set field values");
                    
                    foreach (var property in fieldValues.Properties())
                    {
                        try
                        {
                            // Get the field/property info
                            var fieldInfo = component.GetType().GetField(property.Name, 
                                System.Reflection.BindingFlags.Public | 
                                System.Reflection.BindingFlags.NonPublic | 
                                System.Reflection.BindingFlags.Instance);
                                
                            if (fieldInfo != null)
                            {
                                // Set field value
                                object value = property.Value.ToObject(fieldInfo.FieldType);
                                fieldInfo.SetValue(component, value);
                            }
                            else
                            {
                                // Try property
                                var propInfo = component.GetType().GetProperty(property.Name, 
                                    System.Reflection.BindingFlags.Public | 
                                    System.Reflection.BindingFlags.NonPublic | 
                                    System.Reflection.BindingFlags.Instance);
                                    
                                if (propInfo != null && propInfo.CanWrite)
                                {
                                    object value = property.Value.ToObject(propInfo.PropertyType);
                                    propInfo.SetValue(component, value);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            McpLogger.LogWarning($"Failed to set field '{property.Name}': {ex.Message}");
                        }
                    }
                }
                else if (fieldValues != null && fieldValues.Count > 0 && component == null)
                {
                    McpLogger.LogWarning("Field values provided but no script component to apply them to");
                }
                
                // Ensure Prefabs directory exists
                string prefabsPath = "Assets/Prefabs";
                if (!AssetDatabase.IsValidFolder(prefabsPath))
                {
                    string guid = AssetDatabase.CreateFolder("Assets", "Prefabs");
                    prefabsPath = AssetDatabase.GUIDToAssetPath(guid);
                }
                
                // Create prefab path
                string prefabPath = $"{prefabsPath}/{prefabName}.prefab";
                
                // Check if prefab already exists
                if (AssetDatabase.AssetPathToGUID(prefabPath) != "")
                {
                    // For safety, we'll create a unique name
                    int counter = 1;
                    string basePrefabPath = prefabPath;
                    while (AssetDatabase.AssetPathToGUID(prefabPath) != "")
                    {
                        prefabPath = $"{prefabsPath}/{prefabName}_{counter}.prefab";
                        counter++;
                    }
                }
                
                // Create the prefab
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tempObject, prefabPath);
                
                // Clean up temporary object
                UnityEngine.Object.DestroyImmediate(tempObject);
                
                // Refresh the asset database
                AssetDatabase.Refresh();
                
                // Log the action
                McpLogger.LogInfo($"Created prefab '{prefab.name}' at path '{prefabPath}' from script '{scriptName}'");
                
                // Create the response
                return new JObject
                {
                    ["success"] = true,
                    ["type"] = "text",
                    ["message"] = $"Successfully created prefab '{prefab.name}' at path '{prefabPath}'",
                    ["prefabPath"] = prefabPath
                };
            }
            catch (Exception ex)
            {
                return McpUnitySocketHandler.CreateErrorResponse(
                    $"Error creating prefab: {ex.Message}", 
                    "creation_error"
                );
            }
        }
    }
}
