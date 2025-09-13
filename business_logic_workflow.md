# Business Logic & Workflow Specification (AI-Ready Dev Reference)

**Purpose**
Authoritative business blueprint for a professional-services/consultancy app. Written as a development reference for AI/code assistants. Focuses on **what** (business rules, workflows, validations, constraints), not implementation details. Ordered by **development priority** (build order).

**Scope**
Covers: Project Planning (team, vendors, purchases, overheads & deliverables), Team Assignments, Resource Utilization, Deliverables Execution & Monitoring, Invoicing Planning, Invoicing Management (with **3rd-party invoicing/GL integration**), Vendor Management, Client CRM, Tenders Management, and Cross-Cutting Finance & Governance controls.

---

## 0) Core Principles & Global Policies (apply to all modules)

* **Date Integrity**: No work, cost, or invoice may exist outside the project’s start/end dates.
* **Capacity Integrity**: No slot/path may exceed its allowed allocation; no person may exceed 100% utilization without an explicit approved exception.
* **Single Owner per Deliverable**: Each deliverable has one accountable owner (can be internal or vendor lead).
* **Separation of Duties**: Requester ≠ Approver for budgets, vendors, POs, and invoices.
* **Auditability**: All state changes must be recorded with who/when/what/why.
* **Localization & Calendar**: Respect business calendars (holidays/leave) and locale settings for dates/currency.
* **Data Completeness**: Mandatory fields for critical records (e.g., contract value, due dates) before state can advance.
* **Exception Management**: Soft-rule overrides require justification + approver identity; hard-rule violations are blocked.

---

## 1) Project Planning (Priority 1)

### 1.1 Objects

* **Project**: Name, Client, StartDate, EndDate, ContractValue, Currency, PaymentTerms, Status.
* **Path (Workstream)**: ProjectId, Name, Objective, AllowedAllocation% (0–100), PlannedCost, Owner.
* **Deliverable**: ProjectId/PathId, Name, Owner, Duration (days/months), Target offset (from start), AutoExpectedDate (derived), AcceptanceCriteria, Dependencies.
* **Budget**: Labor (internal), Vendor (rate-card or lump-sum), Purchases, Overheads, Contingency.

### 1.2 Workflow

1. Create Project → define dates, contract value, client.
2. Add **Paths** (workstreams) and **Deliverables** (with duration/offset).
3. Choose resourcing model per path: **Internal**, **Vendor (Rate Card)**, **Vendor (Lump Sum)**, or Mixed.
4. Build **Budget** by summing: Internal labor, Vendor fees, Purchases, Overheads, Contingency.
5. **Auto-schedule** deliverables: `ExpectedDate = ProjectStart + Duration/Offset`.
6. **Plan Approval** (PM → Finance/Executive).

### 1.3 Validations & Constraints

* Project.StartDate ≤ Project.EndDate.
* All Deliverable.ExpectedDate within project window.
* Sum(Budget Components) ≤ ContractValue unless exception approved.
* Path.AllowedAllocation% ≤ 100.
* For Lump-Sum vendor scope: at least one deliverable + due date required.
* Purchases > threshold → require Finance approval before plan approval.

### 1.4 KPIs

* Planned Margin = ContractValue − PlannedCost.
* % Deliverables with owner assigned.
* Budget Coverage (% of project scope with planned cost).

---

## 2) Team Assignments (Priority 2)

### 2.1 Objects

* **PlannedTeamSlot** (per Path/Role): AllowedAllocation% (per slot), PlannedMonthlyCost (internal), PlannedVendorCost (if vendor), Notes.
* **ActualAssignment**: SlotId OR Direct-Deliverable link, AssigneeType (Employee/Vendor), AssigneeId, StartDate, EndDate, Allocation%, Status (Planned/Active/OnHold/Completed/Cancelled), Justification (if soft-override).

### 2.2 Workflow

1. Select slot/path/deliverable → propose assignment (employee or vendor).
2. Pre-checks (capacity, utilization, role fit, cost delta).
3. Soft warnings (utilization > 100%, role mismatch, cost variance) → require justification; Hard blockers (date out of project, capacity exceeded) → must resolve.
4. Approval (Resource Manager/PM).
5. Activate.

### 2.3 Validations & Constraints

* Project.StartDate ≤ Assignment.StartDate ≤ Assignment.EndDate ≤ Project.EndDate.
* For any overlapping dates within the **same slot/path**: Σ Allocation% ≤ AllowedAllocation%.
* For each Employee day: Σ Allocation% across projects ≤ 100% (soft rule → warn + justify).
* Vendor Lump-Sum: assignments tied to deliverables; rate-card: must specify headcount × rate × period or NTE (not-to-exceed).
* Cost delta vs planned slot cost → warning + justification.

### 2.4 KPIs

* Time to approve assignment.
* % assignments with warnings (utilization/role/cost).
* Assignment churn (unassign/replace count per slot).

---

## 3) Resource Utilization (Priority 3)

### 3.1 Objects

* **Capacity Profile**: per Employee/VendorRole (hours per week or FTE%).
* **Utilization Record**: Planned (from assignments), Actual (from time reports), TimeBucket (day/week).

### 3.2 Workflow

* Aggregate planned allocations → Planned Utilization curves.
* Collect actual time → Actual Utilization curves.
* Detect over/under-utilization windows; notify for rebalancing.

### 3.3 Validations & Constraints

* Target personal utilization zone (e.g., 70–85%); >100% requires justification.
* PTO/holidays reduce capacity → trigger re-plan warnings.
* Persistent underutilization (<50%) for N periods → alert Resource/HR.

### 3.4 KPIs

* Avg/Peak utilization by person/team/role.
* % overutilized days; % underutilized days.
* Rebalancing lead time.

---

## 4) Deliverables Execution & Monitoring (Priority 4)

### 4.1 Objects

* **Task** (child of Deliverable): Owner, Start/End, Status, %Complete, RemainingDuration.
* **Milestone** (flag on deliverables).
* **Approval**: QA/Client sign-off records.

### 4.2 Workflow

1. Break deliverables into tasks; assign owners.
2. Periodic progress updates (% complete, remaining).
3. Variance analysis (schedule/cost).
4. Gate approvals (internal QA, optional Client acceptance).
5. Blockers/risks captured with mitigation owner and due date.

### 4.3 Validations & Constraints

* No deliverable “Completed” without acceptance criteria check + sign-off (if required).
* Dependent deliverables auto-shift forecast if predecessors slip.
* Task/Deliverable dates within project window.

### 4.4 KPIs

* On-time milestone rate.
* Task throughput & aging.
* Issue resolution time.
* Earned value indicators (optional).

---

## 5) Invoicing Planning (Priority 5)

### 5.1 Objects

* **Invoice Plan**: SequenceNo, Trigger (Milestone/Date/Progress%), Amount/Formula, PaymentTerms, Dependencies (Deliverables).
* **Billing Rule**: For T\&M (approved time/expense), For Fixed-Fee (milestone acceptance).

### 5.2 Workflow

1. Define billing schedule tied to deliverables/milestones or calendar dates.
2. Ensure Σ planned invoices == ContractValue (or define retention).
3. Keep Finance visibility over plan; update when scope/dates change.
4. Gate: Milestone acceptance → invoice eligible.

### 5.3 Validations & Constraints

* Cannot plan invoice outside project window.
* Cannot exceed ContractValue without approved change order.
* Trigger precondition met (e.g., deliverable accepted) before invoice generation.

### 5.4 KPIs

* Billing plan adherence.
* Forecasted vs actual billings by period.
* Days from milestone to invoice.

---

## 6) Invoicing Management + Third-Party Integration (Priority 6)

### 6.1 Objects

* **Invoice**: ExternalSystemId, ProjectId, ClientId, Lines (linked to deliverables/time/expenses), Status (Draft/Review/Submitted/Paid/Overdue/Cancelled), SentDate, DueDate, PaidDate, Attachments (backup).
* **Receipt/Payment**: ExternalReference, Amount, Date, Method, GLPostingRef.
* **AR Aging Bucket**: 0–30/31–60/61–90/90+.

### 6.2 Workflow

1. **Prepare**: Build invoice draft from plan or approved actuals (time/expense) + milestone eligibility.
2. **Review**: PM validates contents; Finance approves.
3. **Sync (API)**: Create/Update invoice in **3rd-party Invoicing/Accounting system** (e.g., AR module).
4. **Send**: Via 3rd-party system; record SentDate, DueDate.
5. **Track**: Periodic pull of status/AR data from 3rd-party (Submitted, Paid, Partially Paid, Overdue).
6. **Reconcile**: Payments recorded in 3rd-party → pulled back → update invoice status; link **GL postings** (journal references) to project & client for margin/recognition reports.
7. **Exception Handling**: Disputes/amendments create credit notes or adjustments via API and reflected back.
8. **Controls**: Amount > threshold → require CFO approval before sync/send.

### 6.3 Validations & Constraints

* Only **Approved** deliverables/time/expenses are billable.
* Σ (All Issued Invoices) ≤ ContractValue unless change order approved.
* Mandatory fields before sync: Client mapping, Tax rules, Currency, PaymentTerms.
* Duplicate prevention: same milestone cannot generate multiple invoices unless partial-billing is configured.
* Status authority: Payment status is **source-of-truth** from 3rd-party; local edits to payment state are prohibited.

### 6.4 Integration Responsibilities (Business-Level)

* **Outbound**: Create/Update Invoice, Credit Note, Attachments; include project/deliverable references for analytics in GL/BI.
* **Inbound**: Invoice status, Payment records, AR aging, GL journal refs (posting IDs), Customer balance.
* **Reconciliation**: Report mismatches (invoice exists here but not there, amounts differ, out-of-sync status).
* **Security & Compliance**: Only finance-authorized tokens/keys; log every API call; protect PII; honor currency/tax jurisdictions.

### 6.5 KPIs

* DSO (Days Sales Outstanding).
* Overdue % and aging by bucket.
* Invoice accuracy rate (no rework).
* Time from milestone acceptance → invoice sent → cash received.

---

## 7) Vendor Management (Priority 7)

### 7.1 Objects

* **Vendor**: Profile, Compliance Docs, Services, Preferred/Blocked flags, Rating.
* **Vendor Contract/PO**: Type (Rate Card/Lump Sum), Start/End, Fees, Deliverables, SLA, NTE Cap.
* **Vendor Invoice**: PO match, Deliverable match, Status.

### 7.2 Workflow

1. Onboard vendor with compliance checks & approval.
2. Issue PO/Contract for scope; block assignment without active agreement.
3. Track performance vs SLA (quality, timeliness, cost adherence).
4. Approve vendor invoices via 3-way match (PO, receipt/acceptance, invoice).
5. Score vendors and feed into future selection.

### 7.3 Validations & Constraints

* No vendor work without an **active** contract/PO.
* Lump-sum vendor invoice requires linked deliverable acceptance.
* Rate-card spend cannot exceed NTE cap without change order.

### 7.4 KPIs

* Vendor on-time/quality score.
* Variance vs contracted cost.
* % vendor invoices passed 3-way match without exception.

---

## 8) Client Management (CRM) (Priority 8)

### 8.1 Objects

* **Account**: Company, Sector, Country, Priority.
* **Contact**: Name, Role, Email/Phone, Opt-ins.
* **Opportunity**: Stage (Qualified/Proposal/Negotiation/Won/Lost), Value, CloseDate, Owner, Source.
* **Interaction**: Meeting/Call/Email logs, Next actions.

### 8.2 Workflow

1. Capture leads → qualify → convert to opportunities.
2. Track pipeline with probabilities & expected value.
3. Log all interactions; set reminders.
4. On win: convert to Project with client data carry-over; on loss: record reasons.

### 8.3 Validations & Constraints

* No duplicate accounts/contacts (de-dup rules).
* Opportunity must have owner, stage, value, expected close date before advancing.

### 8.4 KPIs

* Win rate, Cycle time, Pipeline value by stage/segment.
* Retention rate, Expansion revenue.

---

## 9) Tenders Management (Priority 9)

### 9.1 Objects

* **Tender**: Client, Title, Budget, RFP dates, Status (Preparing/Submitted/Evaluation/TechFail/FinFail/AwardInProgress/Awarded/Contracting/ContractSigned).
* **Tender Docs**: RFP, Q\&A, Proposal versions.
* **Bid Tasks**: Contributors, Deadlines.

### 9.2 Workflow

1. Register tender; assign bid manager/team; schedule tasks.
2. Centralize documents; control versions; track RFIs.
3. Bid/No-Bid gate; submit by deadline; update status on results.
4. On Awarded/ContractSigned: **convert to Project**, bring proposal scope/budget/deliverables into planning.

### 9.3 Validations & Constraints

* Submission deadline cannot be missed without executive acknowledgment.
* Convert-to-Project requires final commercial terms and dates confirmed.

### 9.4 KPIs

* Win rate; Reasons for loss (tech/price).
* Avg time from award to project kickoff.

---

## 10) Cross-Cutting Finance & Governance (Priority 10)

### 10.1 Controls

* **RBAC**: Fine-grained roles (PM, ResourceMgr, Finance, VendorMgr, Exec).
* **Approvals**: Plan, Budget, PO, Assignment (with warnings), Invoice (threshold-based), Vendor Invoice.
* **Encumbrances**: Budgets reduced by approved POs/committed vendor costs.
* **Change Orders**: For scope/budget/date changes, with financial impact recorded.

### 10.2 Monitoring & Alerts

* Budget vs Actuals vs Forecast at project/portfolio.
* Over-utilization, overdue deliverables, overdue invoices, expiring contracts.
* Exception reports: budget breaches, unbilled approved work, unapproved time/expense.

### 10.3 Reporting & KPIs (Executive Pack)

* Portfolio Margin, Cashflow Forecast, AR Aging, Utilization Heatmaps, Vendor Scorecards, Sales Pipeline.

---

## 11) State Models (Business)

* **Assignment.Status**: Planned → Active → (OnHold ↔ Active) → Completed/Cancelled.
* **Deliverable.Status**: Planned → InProgress → PendingApproval → Accepted/Rejected.
* **Invoice.Status**: Draft → Review → Submitted (external) → Paid/Overdue/Cancelled (synced).
* **Tender.Status**: Preparing → Submitted → Evaluation → (TechFail | FinFail | AwardInProgress) → Awarded → Contracting → ContractSigned.

**State Guards (examples)**

* Deliverable can move to *Accepted* only if acceptance criteria met + approver recorded.
* Invoice can move to *Submitted* only after approval and successful API sync.
* Assignment can move to *Active* only if pre-checks passed or justified + approved.

---

## 12) Validations & Business Rules (Checklist)

* **Dates**: All entity dates within project window.
* **Capacity**: Per slot/path per day Σ Allocation% ≤ AllowedAllocation%.
* **Utilization**: Per person per day Σ Allocation% ≤ 100% (soft).
* **Budget Caps**: Σ planned invoices ≤ ContractValue; Σ vendor rate-card ≤ NTE.
* **Dependencies**: Deliverable with unmet predecessor cannot be marked Accepted.
* **Approvals**: Guard state transitions with configured approvers.
* **Data Completeness**: Mandatory sets (owner + due date + acceptance criteria for deliverables; client & terms for invoices; contract for vendor work).
* **Integration Trust**: 3rd-party invoicing is source of truth for payment/GL status; local system cannot override.

---

## 13) Non-Functional Business Expectations

* **Timeliness**: Weekly status cadence for tasks/progress; daily utilization refresh; daily AR sync.
* **Accuracy**: No invoice generation without approved backing data.
* **Security**: Least-privilege access to finance/vendor/contract data.
* **Traceability**: Full audit for regulators and internal reviews.
* **Scalability**: Portfolio views for thousands of assignments/invoices.
* **Resilience**: Integration retries + reconciliation reports.

---

## 14) Acceptance Criteria (Module-Level)

* **Planning**: Cannot approve project plan with missing mandatory fields; auto dates computed; budget ≤ contract or has change order.
* **Assignments**: Hard blockers prevent activation; soft warnings require justification & approver tag.
* **Utilization**: Dashboards highlight over/under utilization; alerts triggered.
* **Deliverables**: Gate approvals enforced; dependency slips propagate forecasts.
* **Invoice Planning**: Sum of plan = contract; milestone gating works.
* **Invoice Mgmt**: Successful API sync; statuses mirror external; DSO and aging accurate.
* **Vendors**: No assignment or vendor invoice without active contract/PO.
* **CRM**: No stage advancement without owner, value, and expected close date.
* **Tenders**: Conversion preserves proposal data; statuses reflect reality.
* **Finance/Governance**: All approvals/audits present; exception reports available.

---

## 15) Glossary

* **AllowedAllocation%**: Max concurrent capacity for a slot/path (e.g., 100% = one FTE).
* **Utilization%**: Booked time / Available capacity.
* **NTE**: Not-To-Exceed cap on vendor spend.
* **DSO**: Days Sales Outstanding (avg days to collect).
* **AR**: Accounts Receivable; **GL**: General Ledger.
* **PO**: Purchase Order; **SLA**: Service Level Agreement.

---

## 16) Example Exception Justifications (Soft-Rule)

* “Temporary 120% utilization for 2 weeks due to milestone crunch; extra capacity covered by compensatory time later.”
* “Senior SME assigned despite role mismatch; higher cost approved by PMO due to critical skill need.”
* “Partial milestone billing (50%) agreed with client; contract addendum on file.”

---

### Addendum: 3rd-Party Invoicing/GL Integration (Business Contract)

* **Goals**:

  * Fast, accurate billing; Single source of truth for AR/GL; End-to-end traceability from deliverable → invoice → cash → GL.
* **Data Outbound**: Invoice (header/lines, tax, currency, client), Credit Notes, Attachments (deliverable acceptance, time exports).
* **Data Inbound**: Invoice states, Payments, AR Aging, GL journal posting refs, Customer balance.
* **Controls**:

  * Pre-flight checks before send;
  * Idempotent external keys;
  * Reconciliation jobs + exception reports;
  * Finance-only credentials and access logs.
* **Cadence**: Real-time on create/update; scheduled nightly reconciliation.

---

**End of Specification**
