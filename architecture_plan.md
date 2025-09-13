# ProjeX Blazor Architecture Plan

## 1. Overall Architecture

The application will follow a **component-based architecture** using Blazor Server, with a clear separation of concerns. The existing Clean Architecture with Domain, Application, and Infrastructure layers will be leveraged. The Blazor UI will interact with the Application layer through dependency injection of services.

## 2. Component Structure

Components will be organized by feature into the following structure:

```
/Components
├── /Layout         # Main layout, nav menu, login display
├── /Shared         # Reusable components (e.g., confirmation dialogs, loading spinners)
└── /Pages          # Feature-specific pages
    ├── /Account
    ├── /Assignments
    ├── /Clients
    ├── /Dashboard
    ├── /Deliverables
    ├── /Employees
    ├── /Invoices
    ├── /Overheads
    ├── /Payments
    ├── /Projects
    ├── /Reports
    ├── /Roles
    ├── /TeamPlanning
    └── /TimeEntries
```

Each feature folder will contain:
- **`[Feature]List.razor`**: Main page with a grid to list all items.
- **`[Feature]Form.razor`**: A component for creating and editing items.
- **`[Feature]Details.razor`**: A page to display the details of a single item.

## 3. Routing

Routing will be defined using the `@page` directive in each page component. The routing scheme will be as follows:

- `/` - Home page (redirects to dashboard if logged in)
- `/dashboard` - Main dashboard
- `/clients` - Client list
- `/clients/create` - Create new client
- `/clients/edit/{Id}` - Edit client
- `/clients/details/{Id}` - Client details
- (Similar routing for all other features)

## 4. State Management

For a Blazor Server application, component state is managed on the server. For simple cases, component parameters and `@code` block variables will be sufficient. For more complex state sharing between components, we will use a cascaded parameter or a scoped service.

## 5. API Endpoints

While most data will be accessed through the injected services, some functionalities will require dedicated API controllers:

- **File Upload/Download**: An API controller will handle file uploads and downloads to/from the server.
- **Report Generation**: An API controller will be used to generate and download reports in PDF or Excel format.
- **Real-time Notifications**: A SignalR hub will be used for real-time notifications.

## 6. Authentication and Authorization

Authentication will be handled by ASP.NET Core Identity. Authorization will be implemented using:
- **`[Authorize]` attribute**: On pages and components to restrict access to authenticated users.
- **`AuthorizeView` component**: To show/hide UI elements based on roles.
- **Policies**: `AdminOnly` and `ManagerOrAdmin` policies will be used to secure features.

## 7. UI/UX

- **Syncfusion Blazor Components**: Will be used for grids, dialogs, charts, and other complex UI elements.
- **Bootstrap 5**: For layout and styling.
- **Font Awesome**: For icons.
- **Custom CSS**: For application-specific styling.

## 8. Error Handling

- **Error Boundary**: A global error boundary will be used to catch and handle unhandled exceptions.
- **Toasts/Notifications**: For displaying user-friendly error messages.
- **Logging**: Serilog will be used for structured logging of errors and events.

## 9. Testing

- **Unit Tests**: xUnit will be used to test the Application and Infrastructure layers.
- **Component Tests**: bUnit will be used to test Blazor components in isolation.
- **End-to-End Tests**: Selenium or Playwright can be used for end-to-end testing of the application.


