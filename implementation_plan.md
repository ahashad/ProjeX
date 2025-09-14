# Implementation Plan: Team Planning & Actual Assignments

## Purpose

This implementation plan provides structured steps for an AI agent (or developer) to build the **team planning** and **actual assignment** features in the ProjeX system. It translates business logic into concrete development tasks, aligned with existing patterns (Domain → Application → Web).

---

## 1. Data Model Updates

### RolesCatalog

* Ensure `RolesCatalog` is authoritative source of all valid project roles.
* Add validation: prevent duplicate role definitions.

### PlannedTeamSlot

* Fields: `Id`, `ProjectId`, `RoleId (from RolesCatalog)`, `AllowedAllocation%`, `PlannedCost`.
* Constraint: `AllowedAllocation% ≤ 100`.

### ActualAssignment

* Fields: `Id`, `SlotId`, `AssigneeType (Employee/Vendor)`, `AssigneeId`, `StartDate`, `EndDate`, `Allocation%`, `Status`.
* Constraints:

  * Dates within project start/end.
  * Σ allocations per slot ≤ AllowedAllocation%.
  * Σ allocations per employee per period ≤ 100%.

---

## 2. Application Layer Services

### PlannedTeamSlotService

* Create/update slots based on RolesCatalog.
* Validate uniqueness of role in project (no duplicates).
* Compute total planned allocations and planned cost.

### AssignmentService

* Create/update assignments.
* Check conflicts:

  * Employee overallocation >100%.
  * Slot allocation > AllowedAllocation%.
  * Overlapping date ranges.
* Generate warnings (role mismatch, cost variance).

### UtilizationService

* Calculate planned vs actual utilization.
* Expose employee-level and project-level reports.

---

## 3. Workflow

### Project Planning

1. Manager selects roles from RolesCatalog.
2. System creates PlannedTeamSlots with allocation % and cost.
3. Finance approves planned budget.

### Project Execution

1. Manager assigns employees/vendors into slots.
2. Validations run automatically.
3. Approved assignments become active.
4. Utilization updated in real-time.

### Monitoring

* Dashboards show:

  * Team allocations.
  * Employee utilization.
  * Project cost vs budget.

---

## 4. UI Components (Web Layer)

* **Project Planning View**

  * Grid of PlannedTeamSlots by role.
  * Inline allocation% and cost editing.
* **Assignment View**

  * Calendar/Gantt of ActualAssignments.
  * Conflict warnings shown inline.
* **Employee Page**

  * Utilization chart (timeline of % allocations).
  * List of active projects.

---

## 5. Integrations

* **Third-Party Invoicing/ERP**

  * Sync actual assignment cost data → Finance system.
  * Ensure GL references for payroll/vendor billing.

---

## 6. Validation Rules Checklist

* No duplicate roles in project plan.
* No assignment outside project start/end.
* No slot allocation > AllowedAllocation%.
* No employee allocation > 100% across projects.
* Overlapping assignments flagged.
* Finance approval required for budget changes.

---

## 7. Reporting & Dashboards

* Project-level:

  * Planned vs actual utilization.
  * Planned vs actual cost.
* Employee-level:

  * Current utilization%.
  * Overlapping/conflicting assignments.
* Portfolio-level:

  * Company-wide utilization heatmap.
  * Available capacity.

---

## 8. Development Tasks (Priority Order)

1. Extend Domain Models (RolesCatalog, PlannedTeamSlot, ActualAssignment).
2. Implement validations in services.
3. Build PlannedTeamSlot management UI.
4. Build ActualAssignment UI with conflict detection.
5. Implement UtilizationService & reporting.
6. Integrate with finance API for costs.
7. Build dashboards (employee, project, portfolio).
8. Write automated tests for validations & workflows.

---
