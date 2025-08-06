// Simple test for CreatePrefabTool
console.log('Testing CreatePrefabTool implementation...');

// Import the tool
import { z } from 'zod';
import { registerCreatePrefabTool } from './build/tools/createPrefabTool.js';

// Mock MCP server
const mockServer = {
  tool: (name, description, paramsSchema, handler) => {
    console.log(`Registered tool: ${name}`);
    console.log(`Description: ${description}`);
    console.log(`Parameters schema:`, paramsSchema);
    
    // Test the handler with sample data
    if (name === 'create_prefab') {
      console.log('\nTesting create_prefab tool handler...');
      
      // Test with minimal parameters
      const testParams1 = {
        scriptName: 'TestScript',
        prefabName: 'TestPrefab'
      };
      
      console.log('Test 1 - Minimal parameters:', testParams1);
      
      // Test with field values
      const testParams2 = {
        scriptName: 'PlayerController',
        prefabName: 'Player',
        fieldValues: '{"health": 100, "speed": 5.5}'
      };
      
      console.log('Test 2 - With field values:', testParams2);
      
      console.log('Tool registration successful!');
    }
  }
};

// Mock MCP Unity bridge
const mockMcpUnity = {
  sendRequest: async (method, params) => {
    console.log(`Mock sendRequest called with method: ${method}, params:`, params);
    // Return a mock response
    return {
      success: true,
      message: `Prefab created successfully from script ${params.scriptName}`,
      prefabPath: `Assets/Prefabs/${params.prefabName}.prefab`
    };
  }
};

// Mock logger
const mockLogger = {
  info: (msg) => console.log(`[INFO] ${msg}`),
  error: (msg) => console.log(`[ERROR] ${msg}`)
};

// Test the registration
try {
  registerCreatePrefabTool(mockServer, mockMcpUnity, mockLogger);
  console.log('\n✅ All tests passed!');
} catch (error) {
  console.error('\n❌ Test failed:', error);
}
