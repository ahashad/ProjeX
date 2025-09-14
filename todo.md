# Project Start Approval ToDo

## ToDo Tasks

1. **Display "Approved" action for planned projects**
   - In `ProjectList` component > Projects Grid > Actions column, render an "Approved" button in the first position.
   - Only show the button when the project status is **Planned**.
   - Restrict visibility to users with **Admin** or **Manager** roles.

2. **Approval flow & status updates**
   - On "Approved" click, open a component (modal or page) to confirm project start.
   - Change project status to **In-Progress** once the form is submitted.
   - If the calculated end date is already in the past when the form is submitted, set status directly to **Completed**.

3. **Create project start confirmation component**
   - **Start Date** (required): calendar input.
   - **End Date** (auto-calculated): `startDate + projectPeriodInMonths` (no manual input).
   - **Project Manager** (optional): dropdown populated with employees.
   - Submit button to trigger validation and update project data/status.

4. **Data handling & validation**
   - Validate presence of Start Date before submission.
   - Ensure End Date is recalculated if Start Date changes.
   - Persist all changes to the project (start date, end date, project manager, status) via API or store update.

5. **Role-based access & permissions**
   - Verify only Admin and Manager roles can:
     - See the "Approved" button.
     - Access and submit the confirmation component.
   - Ensure backend permission checks align with UI restrictions.

6. **Testing & QA**
   - Unit tests for the component (form validation, end date calculation).
   - Integration tests for approval flow (button visibility, status transitions).
   - Permission tests to confirm restricted access for non-admin/manager roles.

