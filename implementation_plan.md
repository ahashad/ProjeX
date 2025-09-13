# ProjeX - Comprehensive Implementation Plan

## 1. Overview
This document outlines the detailed implementation plan for completing all remaining functionalities in the ProjeX Blazor application, based on the `business_logic_workflow.md` specification. The plan is divided into phases, each with specific tasks, priorities, and deliverables.

## 2. Core Principles & Global Policies
These principles will be implemented as cross-cutting concerns throughout the application:

- **Date Integrity**: Add validation to all date-related inputs to ensure they fall within the project's start and end dates.
- **Capacity Integrity**: Implement validation for resource allocation to prevent exceeding capacity.
- **Single Owner per Deliverable**: Enforce this rule in the deliverable assignment logic.
- **Separation of Duties**: Implement approval workflows with role-based checks.
- **Auditability**: Enhance the existing audit interceptor to capture more detailed information.
- **Localization & Calendar**: Integrate a calendar component that respects user's locale and holidays.
- **Data Completeness**: Add validation attributes to all mandatory fields in DTOs and entities.
- **Exception Management**: Implement a system for handling and logging exceptions with justifications.

## 3. Phase-by-Phase Implementation Plan

### Phase 1: Analyze Business Requirements and Create Detailed Implementation Plan (Completed)
- **Status**: ✅ Completed
- **Deliverables**: This document.

### Phase 2: Implement Core Project Planning Enhancements
- **Priority**: 1
- **Features**: Paths (Workstreams), Advanced Budgeting
- **Tasks**:
  - **2.1. Database**: 
    - Add `Path` entity with relationships to `Project` and `Deliverable`.
    - Add `Budget` entity with components (Labor, Vendor, Purchases, Overheads, Contingency).
  - **2.2. Backend**:
    - Create `PathService` and `BudgetService` with CRUD operations.
    - Implement `PathDto` and `BudgetDto` with AutoMapper profiles.
    - Add validation logic for budget and path allocation.
  - **2.3. Frontend**:
    - Create `PathComponent` for managing workstreams within a project.
    - Create `BudgetComponent` for planning and tracking project budgets.
    - Integrate these components into the `ProjectDetails` page.

### Phase 3: Develop Team Assignment and Resource Management System
- **Priority**: 2
- **Features**: Planned Team Slots, Actual Assignments
- **Tasks**:
  - **3.1. Database**:
    - The `PlannedTeamSlot` and `ActualAssignment` entities already exist, but may need enhancements.
  - **3.2. Backend**:
    - Enhance `AssignmentService` with logic for pre-checks (capacity, utilization, cost delta).
    - Implement approval workflow for assignments.
  - **3.3. Frontend**:
    - Create `AssignmentComponent` for proposing and managing assignments.
    - Add a resource management dashboard to visualize team allocation.

### Phase 4: Build Resource Utilization Tracking and Monitoring
- **Priority**: 3
- **Features**: Capacity Profiles, Utilization Records
- **Tasks**:
  - **4.1. Database**:
    - Create `CapacityProfile` entity for employees and vendor roles.
    - Create `UtilizationRecord` entity to store planned vs. actual utilization.
  - **4.2. Backend**:
    - Implement a background service to aggregate and calculate utilization data.
    - Create `UtilizationService` to provide data for reports and dashboards.
  - **4.3. Frontend**:
    - Create `UtilizationDashboard` with charts to visualize utilization curves.
    - Add alerts for over/under-utilization.

### Phase 5: Enhance Deliverables Execution with Tasks and Milestones
- **Priority**: 4
- **Features**: Tasks, Milestones, Approvals
- **Tasks**:
  - **5.1. Database**:
    - Create `Task` entity as a child of `Deliverable`.
    - Add a `IsMilestone` flag to the `Deliverable` entity.
    - Create `Approval` entity for QA and client sign-offs.
  - **5.2. Backend**:
    - Enhance `DeliverableService` with logic for task management and approvals.
    - Implement logic for auto-shifting forecasts based on dependencies.
  - **5.3. Frontend**:
    - Create `TaskComponent` for managing tasks within a deliverable.
    - Add an approval workflow to the `DeliverableDetails` page.

### Phase 6: Implement Invoicing Planning and Management System
- **Priority**: 5 & 6
- **Features**: Invoice Plans, Billing Rules, 3rd-Party Integration
- **Tasks**:
  - **6.1. Database**:
    - Create `InvoicePlan` and `BillingRule` entities.
    - Enhance `Invoice` entity with `ExternalSystemId` and other integration fields.
  - **6.2. Backend**:
    - Create `InvoicePlanService` and `InvoiceManagementService`.
    - Implement a generic 3rd-party integration service for invoicing systems.
    - Add logic for invoice review, approval, and reconciliation.
  - **6.3. Frontend**:
    - Create `InvoicePlanComponent` for defining billing schedules.
    - Create `InvoiceManagementComponent` for managing the entire invoicing lifecycle.

### Phase 7: Develop Vendor Management and Purchase Order System
- **Priority**: 7
- **Features**: Vendor Profiles, Contracts/POs, Vendor Invoices
- **Tasks**:
  - **7.1. Database**:
    - Create `Vendor`, `VendorContract`, and `VendorInvoice` entities.
  - **7.2. Backend**:
    - Create `VendorService` and `VendorContractService`.
    - Implement 3-way matching logic for vendor invoices.
  - **7.3. Frontend**:
    - Create `VendorManagementComponent` for onboarding and managing vendors.
    - Create `PurchaseOrderComponent` for creating and tracking POs.

### Phase 8: Build Client CRM and Tenders Management
- **Priority**: 8 & 9
- **Features**: Accounts, Contacts, Opportunities, Tenders
- **Tasks**:
  - **8.1. Database**:
    - Create `Account`, `Contact`, `Opportunity`, and `Tender` entities.
  - **8.2. Backend**:
    - Create `CrmService` and `TenderService`.
    - Implement logic for converting opportunities and tenders to projects.
  - **8.3. Frontend**:
    - Create `CrmDashboard` to visualize the sales pipeline.
    - Create `TenderManagementComponent` for tracking bids and proposals.

### Phase 9: Implement Finance and Governance Controls
- **Priority**: 10
- **Features**: RBAC, Approvals, Encumbrances, Change Orders
- **Tasks**:
  - **9.1. Backend**:
    - Enhance RBAC with more granular roles and permissions.
    - Implement multi-level approval workflows.
    - Add logic for budget encumbrances and change order management.
  - **9.2. Frontend**:
    - Create a centralized `ApprovalsDashboard` for managers and executives.
    - Add a `ChangeOrderComponent` to manage changes to project scope, budget, and schedule.

### Phase 10: Add Reporting and Analytics Dashboard
- **Priority**: 10
- **Features**: Executive Pack Reports
- **Tasks**:
  - **10.1. Backend**:
    - Enhance `ReportService` with new reports (Portfolio Margin, Cashflow Forecast, etc.).
  - **10.2. Frontend**:
    - Create an `ExecutiveDashboard` with high-level KPIs and reports.
    - Add more advanced charts and data visualizations.

### Phase 11: Test Complete System and Fix Issues
- **Priority**: N/A
- **Tasks**:
  - Perform end-to-end testing of all new features.
  - Fix any bugs or issues found during testing.

### Phase 12: Deliver Completed Enhanced ProjeX Solution
- **Priority**: N/A
- **Tasks**:
  - Update all documentation.
  - Package the final solution for delivery.

## 4. Timeline and Milestones
This implementation will be executed in sprints, with each sprint focusing on one or more phases. The progress will be tracked in the `todo.md` file in the repository.

## 5. Conclusion
This plan provides a clear roadmap for completing the ProjeX application. By following this plan, we will ensure that all business requirements are met and the final product is a robust, feature-rich, and high-quality project management solution.

