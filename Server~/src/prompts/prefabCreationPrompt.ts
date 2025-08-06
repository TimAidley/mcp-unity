import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import * as z from "zod";

/**
 * Registers the prefab creation prompt with the MCP server.
 * This prompt defines the proper workflow for creating prefabs in Unity, optionally with MonoBehaviour scripts.
 * 
 * @param server The McpServer instance to register the prompt with.
 */
export function registerPrefabCreationPrompt(server: McpServer) {
  server.prompt(
    'prefab_creation_strategy',
    'Defines the proper workflow for creating prefabs in Unity, optionally with MonoBehaviour scripts',
    {
      scriptName: z.string().optional().describe("The name of the MonoBehaviour script to create a prefab from (without .cs extension). Optional."),
      prefabName: z.string().describe("The name for the new prefab (without .prefab extension)."),
      fieldValues: z.string().optional().describe("Optional key-value pairs to set serialized field values on the component, as a JSON string."),
    },
    async ({ scriptName, prefabName, fieldValues }) => ({
      messages: [
        {
          role: 'user', 
          content: {
            type: 'text',
            text: `You are an expert AI assistant integrated with Unity via MCP.

When creating prefabs in Unity, you have access to the following tools:
- Tool "create_prefab" to create a prefab with optional MonoBehaviour script and serialized field values.

Workflow:
1. Determine an appropriate name for the new prefab.
2. Optionally, identify the MonoBehaviour script name that should be used for the prefab.
3. Optionally, identify any serialized field values that should be set on the component.
4. Use the "create_prefab" tool with the prefab name, optional script name, and optional field values.
5. Confirm success and report the path of the created prefab.

Guidance:
- The script must already be compiled in the Unity project if provided.
- The prefab will be saved in the Assets/Prefabs/ directory.
- If a prefab with the same name already exists, a unique name will be generated.
- Field values should match the serialized fields in the MonoBehaviour script if provided.
- Field values should be provided as a JSON string.
- Always validate inputs and request clarification if needed.`
          }
        },
        {
          role: 'user',
          content: {
            type: 'text',
            text: `Create a prefab named "${prefabName}"${scriptName ? ` from the script "${scriptName}"` : ''}${fieldValues ? ` with field values: ${fieldValues}` : ''}.`
          }
        }
      ]
    })
  );
}
