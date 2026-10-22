# Volunteer Management & Assistance Coordination System
**Windows Desktop Application | C# · .NET 8 · WPF · XAML**

An end-to-end academic desktop application designed to coordinate volunteer organizations, manage assistance requests, and match volunteers with nearby opportunities. Built with a layered architecture, the project combines operational workflows with geographic services, automated email communication, analytical dashboards, concurrency controls, and a configurable simulation engine.

**Developed by Toiby Glazer** as a software engineering academic project.

> **Academic data notice:** All volunteers, accounts, names, email addresses, and other records provided as project data are fictional or simulated for educational/testing purposes. They do not represent real registered users. They are included to illustrate the application workflow.

## Features at a Glance

| Area | Capabilities |
| --- | --- |
| Authentication | ID/password login, role-based screens, PBKDF2 password hashing, password policy and changes |
| Volunteers | Registration, profile editing, activity and contact details, distance preferences, volunteer history |
| Requests | Creating, updating, browsing, filtering, assigning, completing and canceling assistance calls |
| Maps and matching | Address autocomplete, geocoding, route and aerial distance calculation, nearby opportunities, interactive maps |
| Email automation | Welcome emails, notifications of relevant new requests, cancellations, styled HTML emails and map attachments |
| Analytics | Volunteer activity statistics, charts for call statuses and types, activity timeline |
| Administration | Manage organization data, set simulated clock, configure risk time, initialize/reset data |
| Simulation | Timed activity simulation, assignment changes, event observation, concurrency safeguards |
| Persistence | Swappable in-memory and XML data access implementations |

## User Experience & Core Workflows

### Login and Roles
Users sign in with their numeric identifier and password. The business layer looks up the volunteer account and verifies the password hash. The UI then opens a role-specific workspace:

- **Manager:** administrative dashboard with access to volunteer and request management, system clock, configuration and simulation controls.
- **Volunteer:** volunteer-specific workspace for browsing eligible opportunities, managing details and reviewing activity.

The project implements role-based UI navigation; production authorization would additionally require explicit checks for every privileged action.

### Volunteer Management
- Add and edit volunteer accounts with contact details, address and role.
- Configure active status, maximum volunteering distance and distance-calculation method.
- Manage profile data and images.
- Search/filter/sort volunteer lists and inspect work statistics.
- Maintain assignment history and protect records from inappropriate deletion when related activity exists.
- Validate identifiers, addresses, email format, required fields and password complexity.

### Assistance Requests & Assignments
Managers can create categorized assistance requests with an address, geographical location, description and deadlines. The system:
- Tracks open, in-progress, closed and expired calls.
- Assigns calls to volunteers, including an active assignment lifecycle.
- Records assignment start, completion and cancellation details.
- Supports volunteer and manager cancellation flows.
- Applies business rules to prevent invalid state transitions and deletion of assigned calls.
- Displays call and assignment history and detailed call information.

### Location-Aware Matching & Maps
Geography is an important part of the system:
- **Nominatim / OpenStreetMap:** Address suggestions and geocoding (addresses to latitude/longitude).
- **OpenStreetMap routing services:** Route-distance calculations for supported travel modes.
- **Aerial distance:** Direct great-circle distance calculations as an alternative to route distance.
- **Leaflet.js + OpenStreetMap tiles:** Interactive geographical maps, markers and request locations in the desktop interface.
- **LocationIQ Static Maps API:** Produces shareable map images associated with assistance requests.

The application considers each volunteer's maximum preferred distance when displaying opportunities and choosing recipients for new-request notifications.

### Automated Email Integration
The project uses **Gmail SMTP** through .NET mail APIs with asynchronous operations and custom HTML templates.

Supported communication includes:
1. **Welcome messages:** Email onboarding information to newly registered volunteers.
2. **Nearby request alerts:** Notify volunteers within their configured geographical range when a relevant call is created.
3. **Map-rich notifications:** Include generated map imagery and contextual request information.
4. **Cancellation alerts:** Notify affected volunteers about canceled assignments or calls.

SMTP host configuration uses `smtp.gmail.com`, port `587`, and SSL/TLS-enabled mail transfer. The password is now read from the `VOLUNTEER_APP_GMAIL_PASSWORD` environment variable rather than being embedded in the current source code.

### Statistics and Visual Reporting
A dedicated volunteer statistics window shows:
- Completed/treated, canceled and expired assistance calls.
- Breakdowns of request types and statuses.
- Activity over time.
- Chart visualizations generated with **QuickChart**.

### Administration and Simulation
The management dashboard can advance the simulated system clock (minute, hour, day, month or year), configure risk time ranges, initialize/reset the data, and start/stop a background simulator.

The simulator models volunteer and call activity as time progresses, with timed decisions and assignment updates. Observer notifications keep relevant views current. Guards and locks protect certain state-changing operations while simulation runs.

## Architecture

The solution is organized as a multi-project layered application:

```text
PL — Presentation Layer (WPF / XAML)
    Login, manager & volunteer views, maps, statistics
                     |
                     v
BL — Business Logic Layer
    BlApi interfaces + implementations + helpers
    Validation, authentication, matching, notifications,
    simulator and assignment rules
                     |
                     v
DalFacade — Data Access Contracts
    Entities (DO), interfaces, factory/configuration
                 /       \
                v         v
           DalList       DalXml
           In-memory     XML files
```

| Project / folder | Responsibility |
| --- | --- |
| `PL/` | Desktop UI, data binding, maps, forms, login and dashboard windows |
| `BL/BO/` | Business objects, enums and domain exceptions |
| `BL/BlApi/` | Business service contracts and factory |
| `BL/BlImplementation/` | Business use cases for volunteers, calls and administration |
| `BL/Helpers/` | Password handling, validation, routing, email, observers, simulated clock and simulator |
| `DalFacade/` | Persistent data objects and DAL interfaces |
| `DalList/` | In-memory repository implementation |
| `DalXml/` | XML-backed persistence |
| `BlTest/`, `DalTest/` | Testing tools and data-initialization programs |
| `xml/` | Educational test data and XML storage/configuration |

Factories and interfaces separate UI and business rules from concrete data-storage implementations. Business entities are converted to and from data objects as needed.

## Security Features — Additional Work

Security-oriented components were implemented as additional engineering features beyond straightforward desktop CRUD:

**Password hashing and verification**
- PBKDF2-based derived password hashes with a cryptographically generated **16-byte salt**.
- **32-byte** derived key stored with the salt as Base64-separated fields.
- **10,000 iterations** in the educational implementation.
- **Constant-time comparison** using `CryptographicOperations.FixedTimeEquals`.
- Verification of the old password before accepting a password change.
- Password complexity checks (length, upper/lowercase, digits and special-character requirements).

**Workflow protections**
- Separation between manager and volunteer presentation flows.
- Explicit validations and domain-specific exceptions.
- Synchronization locks for important mutations and simulation-related protections.
- Background asynchronous tasks for UI responsiveness when accessing data and external services.

**Security limitations:** This is an academic application, not a fully audited production identity system. Its current PBKDF2 parameters should be reviewed for production. Login uses direct desktop business-layer calls rather than JWT or web sessions. Role-specific screens alone do not prove server-enforced authorization.

## External Services & Technologies

| Technology | Purpose |
| --- | --- |
| C#, .NET 8 | Application and business logic |
| WPF / XAML | Windows desktop UI |
| XML and in-memory collections | Data storage implementations |
| Nominatim / OpenStreetMap | Address autocomplete and geocoding |
| OSM routing endpoints | Road/walking route-based distance calculations |
| Leaflet.js | Interactive maps |
| LocationIQ | Static map images for notifications |
| Gmail SMTP | Automated HTML email delivery |
| QuickChart | Charts and activity reporting |
| Microsoft.Web.WebView2 | Embedded web content support |
| Newtonsoft.Json | JSON processing |
| async/await, Observer pattern, locks | Responsiveness, updates and synchronization |

## Example Request Flow

```text
Manager creates assistance request
    ↓
BL validates request and resolves location
    ↓
DAL persists request
    ↓
BL checks volunteers' positions and distance preferences
    ↓
Eligible volunteers receive HTML email/map notification
    ↓
Volunteer opens a nearby request and accepts an assignment
    ↓
Assignment moves through active / completed / canceled states
    ↓
History, volunteer statistics and manager views update
```

## Running the Application

1. Use **Windows** with Visual Studio and the **.NET 8 SDK**, including desktop/WPF development support.
2. Clone this repository and open `dotNet2024_4524_2763.sln`.
3. Restore NuGet dependencies and build.
4. Select/configure the intended DAL implementation and initialize educational records as needed.
5. To enable Gmail notification features, define the environment variable `VOLUNTEER_APP_GMAIL_PASSWORD` with the Gmail application password on the machine running the app. Restart Visual Studio/the application after adding it.
6. Run the **PL** (WPF) project.

### Email environment variable on Windows

For a persistent **user-level** variable, execute this PowerShell command locally, substituting your password **on your own machine only**:

```powershell
[Environment]::SetEnvironmentVariable("VOLUNTEER_APP_GMAIL_PASSWORD", "<your-existing-gmail-app-password>", "User")
```

Do **not** paste real passwords into GitHub or commit scripts containing their values. The code no longer includes the SMTP password in its latest revision. If the environment variable is missing, features that initialize SMTP configuration will fail until the variable is configured.

### Educational Data and Public Repository Considerations

All user records included with the project are **fictional educational/test records**, not real volunteers. The project retains this sample data to demonstrate application behavior.

Although the Gmail password has been moved out of the *current* source, **earlier Git commits may still contain the previous hardcoded value**. A separate LocationIQ API key is also present in the source as originally implemented; it has **not** been modified as part of this README/security change. Before making the repository public or reusing real service credentials, review historical secrets, service-account access and third-party API settings.

## Engineering Highlights

- Layered architecture with interchangeable DAL providers and factory-based dependencies.
- Role-sensitive login and volunteer lifecycle management.
- PBKDF2 password protection and account-validation rules.
- Geographic request matching with several distance-calculation strategies.
- Multiple external integrations across maps, routing, charts and email.
- Automatic, location-aware email delivery with map attachments.
- Time-dependent simulation and concurrent operations.
- Reactive view updates using observers.
- Operational analytics and volunteer history.

---

**Author:** [Toiby Glazer](https://github.com/toibyglazer)  
*Academic software engineering project — volunteer operations and assistance coordination.*
