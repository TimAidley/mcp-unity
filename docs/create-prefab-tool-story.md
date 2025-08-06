# CreatePrefabTool – Brownfield Addition

## User Story

As a **Unity game developer**,
I want an **AI-driven CreatePrefabTool that automatically generates a prefab from a newly created C# `MonoBehaviour` script and updates its serialized fields**,
So that **I can set up gameplay entities (e.g., player, enemy, spawner) rapidly without repetitive manual steps**.

## Story Context

**Existing System Integration:**
- Integrates with: *Unity MCP* (C# Editor tools & Node.js server)
- Technology: *Unity 2022.3+, TypeScript/Node 18+*
- Follows pattern: *Existing MCP tool architecture (C# `McpToolBase` & TS wrapper)*
- Touch points:
  - `Editor/Tools/` → new `CreatePrefabTool.cs`
  - `Server~/src/tools/` → new `createPrefabTool.ts`
  - `Server~/src/index.ts` tool registration
  - Asset path: `Assets/Prefabs/<name>.prefab`

## Acceptance Criteria

### Functional Requirements
1. Given a fully-compiled C# script name and desired prefab name, invoking the MCP tool **creates** a prefab under `Assets/Prefabs/` with the script component attached.
2. Tool accepts an optional JSON object of *serialized field values* and applies them to the prefab instance.
3. Tool returns the prefab asset path on success.

### Integration Requirements
4. Existing MCP tools continue to operate unchanged.
5. New tool conforms to standard MCP JSON request/response schema.
6. Prefab creation works both via CLI (`stdio` transport) and future Web transport.

### Quality Requirements
7. No console errors/warnings after prefab generation.
8. Prefab asset passes Unity import/refresh without issues.
9. Documentation for the tool is updated (`README` & prompt).

## Technical Notes
- **Integration Approach:** Use `PrefabUtility.SaveAsPrefabAsset` after creating a temporary `GameObject` with the script component.
- **Existing Pattern Reference:** Mirror structure of `AddAssetToSceneTool.cs` & its TS wrapper.
- **Key Constraints:** Runs in **Unity Editor** context only; requires compiled script (assembly reload complete).

## Definition of Done
- [x] Functional requirements met (1-3)
- [x] Integration requirements verified (4-6)
- [x] No console errors; Unity refresh clean (7-8)
- [x] Documentation updated (9)
- [ ] PR reviewed & merged

## Minimal Risk Assessment
| Item | Description |
|------|-------------|
| **Primary Risk** | Script not yet compiled when tool runs → prefab lacks component |
| **Mitigation** | Detect missing type & retry after assembly reload or return informative error |
| **Rollback** | Delete generated prefab asset |

## Compatibility Verification Checklist
- [x] No breaking changes to existing MCP APIs.
- [x] Prefab path is additive; does not overwrite existing assets without confirmation.
- [x] Performance impact negligible (< 100 ms per prefab).

## Validation Checklist
- [x] Story can be completed within one development session (~4 hrs).
- [x] Integration approach is straightforward, mirrors existing patterns.
- [x] Acceptance criteria are testable via manual invocation.
- [x] Rollback is simple (delete asset).

## Success Criteria
1. ✅ Prefab is created automatically with the correct component and serialized values.
2. ✅ Developer workflow is reduced to one MCP call.
3. ✅ No regressions in existing MCP functionality.
4. ✅ Clear docs allow any dev to use the tool immediately.

## Important Notes
- If additional complexity (multi-prefab generation, asset dependencies) arises, escalate to **brownfield-create-epic**.
- Preserve existing folder structure; do not hard-code absolute paths.
- Prioritize developer ergonomics and clear error messages.
