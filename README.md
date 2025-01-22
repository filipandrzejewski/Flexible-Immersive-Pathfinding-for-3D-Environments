# Flexible Pathfinding System for 3D Environments
## User Setup Guide 

### Introduction
This document provides a comprehensive guide on how to use the Flexible Pathfinding System. The pathfinding tool consists of two separate systems: **Link Generation** and **Pathfinding Logic**.

- **Link Generation** automatically enhances the navigation mesh by creating a detailed network of links connecting all accessible points in the level.
- **Pathfinding Logic** customizes pathfinding based on individual character parameters, allowing agents to choose appropriate paths based on their physical attributes.

It is recommended to use these systems together for optimal results. However, they can also be used independently if needed.

---

### Link Generation

#### Setup

1. **Add a Controller Object**:
   - Create an empty `GameObject` in the Unity scene to serve as the controller.
   - Attach the `NavMeshLinksGenerator` and `NavLinkManager` components to this object.

2. **Auto-Assign Components**:
   - In the Inspector window, click the **Auto Assign Components** button under `NavLinkManager`.
   - Ensure the following:
     - `NavMeshLinksGenerator` and `NavLinkManager` are properly added.
     - Two prefabs exist in the project:
       - `LinkPrefab(Standard)` for edge-to-edge connections.
       - `LinkPrefab(Wide)` for dropdown edge-to-floor connections.
     - A `NavMeshSurface` component exists in the scene.

3. **Manual Adjustments (Optional)**:
   - Customize the `Manager Object References and Prefabs` section in `NavLinkManager` to use custom prefabs or a different `NavMeshSurface`.

4. **Bake the Mesh**:
   - Disable automatic link generation in `NavMeshSurface` settings.
   - Bake the navigation mesh for the level.

5. **Generate Links**:
   - Use the **Create Links** button in the `NavLinkManager` editor view to generate links automatically.

6. **Manual Adjustments (Optional)**:
   - Adjust links as needed. Use the **Update Links** button to reflect changes.
   - Add new links manually and press **Update Links** to include them.

---

#### Modifying Link Generation Settings

- **Link Raycast Settings**:
  - `enableJumpArcRayCasts`: Enables box casts to check collisions on jump paths.
  - `maxjumpArcHeight`: Sets the jump arc's height.

- **Edge Detection Settings**:
  - `minEdgeLength`: Filters out short edges.
  - `maxGroupSize` and `minGroupSize`: Configure edge grouping.

- **Basic Link Generation Settings**:
  - `maxEdgeLinkDistance`: Maximum distance for edge-to-edge links.
  - `shortLinkDistance`: Allows permissive connections for close edges.
  - `maxDropDownLinkDistance`: Maximum distance for dropdown links.

- **Advanced Link Generation Settings**:
  - **Dropdown Link Settings**:
    - `dropDownSteepnessModifier`: Controls steepness of dropdown detection.
    - `dropDownLinkAngles`: Sets dropdown search angles.
  - **Edge-to-Edge Angle Restrictions**:
    - `standardAngleRestrictionForward` and `standardAngleRestrictionUpward`: Define standard angle limits.
    - `permissiveAngleRestrictionForward` and `permissiveAngleRestrictionUpward`: Define angle limits for shorter links.

---

### Pathfinding Logic

#### Setup

1. **Verify Existing Components**:
   - Ensure the manager object has `NavLinkManager` and `NavLinkGenerator`.
   - Use the **Update Links** button to index all links.

2. **Create a Navigation Agent**:
   - Add an agent to the scene and set its `AgentType` to match the navigation mesh.
   - Customize speed and other Unity Navigation parameters.

3. **Attach the PlayerController Script**:
   - Add `PlayerController` to the agent.
   - Assign the main camera for raycasting and configure the mouse button for movement.

4. **Set Up Physical Stats**:
   - Attach `PhysicalStatsLogic` to the agent and configure:
     - `maxJumpHeight`
     - `maxJumpDistance`
     - `maxDropDistance`

5. **Test the System**:
   - Start the game and observe the agent navigating the scene based on physical limitations.

---

### Customization

- **Modifying Path Request Behavior**:
  - Use the `NavLinkManager` public method:
    ```csharp
    RequestPath(PhysicalStatsLogic character, Vector3 destination, Action<bool> onPathCalculated = null)
    ```
    - `character`: The agent's `PhysicalStatsLogic` component.
    - `destination`: Target position.
    - `onPathCalculated`: Optional callback.

- **Optimizing Performance**:
  - Avoid frequent path requests. Limit requests per second to prevent overloading the system.
