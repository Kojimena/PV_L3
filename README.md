# PV_L3
Lab 3: Navigation &amp; Animation

---

## Requirements

- **3 distinct enemies**: `EnemyAI1`, `EnemyAI2`, `EnemyAI3`.
- **Navigation**: all use `NavMeshAgent`, waypoints, and dynamic destinations.
- **Behaviors and animations:**
  - Enemy A: taunt → patrol → chase → attack.
  - Enemy B: “angry” when it sees the player → after a while switches to “rescue the princess.”
  - Enemy C: patrol/chase/attack with cooldown and SFX on hit jump in navlink animation.
  - Princess: patrols → blows a kiss when rescued by the player or scream when is rescued by boldor.
- **Animations and Characters**: Five different characters were used, one for each enemy, one for the player, and one for the princess.

## Navigation (NavMesh)
- NavMesh **baked** in the scene (Navigation → Bake).
- All enemies use **waypoints** for patrol and `SetDestination()` to chase or move to points.

---

## Assets
- Animations and/or characters: **Mixamo**.
- Character controllers: **StarterAssets**.
