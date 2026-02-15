# ContosoUniversity Migration Plan
## .NET Framework 4.8 to .NET 8.0

### Project Information
- **Project Name**: ContosoUniversity
- **Location**: `/home/runner/work/dotnet-migration-copilot-samples/dotnet-migration-copilot-samples/ContosoUniversity`
- **Source Framework**: .NET Framework 4.8 (v4.8)
- **Target Framework**: .NET 8.0 (Long Term Support)
- **Application Type**: ASP.NET MVC 5 → ASP.NET Core MVC
- **Database**: Entity Framework Core 3.1 → Entity Framework Core 8.0
- **Package Management**: packages.config → PackageReference

### Current Technology Stack
- ASP.NET MVC 5.2.9
- Entity Framework Core 3.1.32
- Web.config configuration
- packages.config for package management
- System.Web dependencies
- Traditional ASP.NET MVC routing and filters
- IIS-based hosting model
- MSMQ for notifications

### Target Technology Stack
- ASP.NET Core MVC 8.0
- Entity Framework Core 8.0
- appsettings.json configuration
- PackageReference (SDK-style project)
- ASP.NET Core middleware pipeline
- Kestrel web server with IIS integration option
- Modern dependency injection
- Async/await patterns throughout
- Azure-ready configuration

---

## Migration Phases

### Phase 1: Analysis and Planning ✓
**Goal**: Understand current application structure and dependencies

#### Tasks:
1. ✓ Analyze project structure and organization
   - Controllers: BaseController, HomeController, StudentsController, CoursesController, InstructorsController, DepartmentsController, NotificationsController
   - Models: Course, Enrollment, Student, Instructor, Department, OfficeAssignment, CourseAssignment, Notification, Person (TPH inheritance)
   - Data: SchoolContext (EF Core 3.1), DbInitializer, SchoolContextFactory
   - Services: NotificationService
   - Views: Razor views for all entities

2. ✓ Document dependencies and packages
   - ASP.NET MVC 5.2.9 and related packages
   - Entity Framework Core 3.1.32 (already using EF Core!)
   - Microsoft.Extensions.* packages (3.1.32)
   - Bootstrap 5.3.3, jQuery 3.7.1
   - Microsoft.Identity.Client 4.21.1
   - WebGrease, Antlr for bundling/minification

3. ✓ Identify configuration requirements
   - Connection strings in Web.config
   - AppSettings: webpages configuration, notification queue path
   - System.web settings: compilation, httpRuntime
   - Assembly binding redirects
   - IIS configuration

4. ✓ Map feature compatibility
   - EF Core already in use ✓
   - MVC controllers need conversion to ASP.NET Core MVC
   - Razor views mostly compatible (minor syntax changes)
   - Bundling needs migration to ASP.NET Core approach
   - MSMQ notification system needs review for Azure

**Deliverables**: 
- ✓ Current state documentation
- ✓ Dependency inventory
- ✓ Migration plan (this document)

---

### Phase 2: Project Structure Migration
**Goal**: Convert project file to SDK-style and update structure

#### Tasks:
1. **Backup current project**
   - Create Git branch: `feature/migration-to-net8`
   - Commit current state as baseline

2. **Create new SDK-style project file**
   - Convert from old .csproj format to SDK-style
   - Set `<TargetFramework>net8.0</TargetFramework>`
   - Set `<OutputType>Exe</OutputType>` for web applications
   - Remove explicit file listings (use implicit includes)
   - Configure Razor compilation: `<AddRazorSupportForMvc>true</AddRazorSupportForMvc>`

3. **Update directory structure**
   - Keep existing MVC structure (Controllers, Views, Models)
   - Add `wwwroot/` for static files (move Content, Scripts)
   - Create `Properties/launchSettings.json` for development settings
   - Move Web.config content to appsettings.json

4. **Remove legacy files**
   - Remove packages.config
   - Remove Web.Debug.config and Web.Release.config
   - Remove Global.asax and Global.asax.cs
   - Remove App_Start folder (BundleConfig, FilterConfig, RouteConfig)

**Verification**:
- Project loads in Visual Studio/VS Code
- Solution builds without errors (may have warnings about missing dependencies)

---

### Phase 3: Dependencies Migration
**Goal**: Update all NuGet packages to .NET 8.0 compatible versions

#### Tasks:
1. **Remove .NET Framework-specific packages**
   - Remove Microsoft.AspNet.Mvc (5.2.9)
   - Remove Microsoft.AspNet.Razor (3.2.9)
   - Remove Microsoft.AspNet.WebPages (3.2.9)
   - Remove Microsoft.AspNet.Web.Optimization (1.1.3)
   - Remove Microsoft.CodeDom.Providers.DotNetCompilerPlatform
   - Remove Microsoft.Web.Infrastructure
   - Remove System.Web.* references
   - Remove WebGrease, Antlr

2. **Add ASP.NET Core packages**
   - Add Microsoft.AspNetCore.App (framework reference - implicit in .NET 8)
   - Add Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation (for development)
   - Add Microsoft.AspNetCore.StaticFiles (implicit, but verify)

3. **Update Entity Framework Core**
   - Update Microsoft.EntityFrameworkCore from 3.1.32 to 8.0.x
   - Update Microsoft.EntityFrameworkCore.SqlServer from 3.1.32 to 8.0.x
   - Update Microsoft.EntityFrameworkCore.Tools from 3.1.32 to 8.0.x
   - Update Microsoft.EntityFrameworkCore.Design to 8.0.x (if needed)

4. **Update Microsoft.Extensions packages**
   - Update from 3.1.32 to 8.0.x:
     - Microsoft.Extensions.Configuration
     - Microsoft.Extensions.DependencyInjection
     - Microsoft.Extensions.Logging
     - Microsoft.Extensions.Options
   - These are typically included with ASP.NET Core

5. **Update other dependencies**
   - Update Newtonsoft.Json to latest (or consider System.Text.Json)
   - Update Microsoft.Data.SqlClient to latest stable
   - Update Microsoft.Identity.Client to latest (if used)
   - Update jQuery, Bootstrap (already latest)

6. **Add new .NET 8 packages as needed**
   - Microsoft.VisualStudio.Web.CodeGeneration.Design (for scaffolding)

**Verification**:
- Run `dotnet restore`
- Check for package compatibility issues
- Verify no deprecated packages remain

---

### Phase 4: Configuration Migration
**Goal**: Migrate from Web.config to appsettings.json

#### Tasks:
1. **Create appsettings.json**
   - Create `appsettings.json` in project root
   - Create `appsettings.Development.json` for dev settings

2. **Migrate connection strings**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=ContosoUniversityNoAuthEFCore;Integrated Security=True;MultipleActiveResultSets=True"
     }
   }
   ```

3. **Migrate app settings**
   ```json
   {
     "NotificationSettings": {
       "QueuePath": ".\\Private$\\ContosoUniversityNotifications"
     },
     "Kestrel": {
       "Limits": {
         "MaxRequestBodySize": 10485760
       }
     }
   }
   ```

4. **Configure logging**
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning",
         "Microsoft.EntityFrameworkCore": "Warning"
       }
     }
   }
   ```

5. **Update or remove Web.config**
   - Keep minimal Web.config for IIS deployment (if needed)
   - Remove all app settings and connection strings
   - Keep only IIS-specific handlers if deploying to IIS

**Verification**:
- Configuration values load correctly
- Connection string accessible
- Logging configuration works

---

### Phase 5: Application Startup Migration
**Goal**: Replace Global.asax with Program.cs and Startup pattern

#### Tasks:
1. **Create Program.cs**
   - Create minimal hosting model with WebApplicationBuilder
   - Configure services (DI container)
   - Configure middleware pipeline
   - Set up database initialization

   ```csharp
   var builder = WebApplication.CreateBuilder(args);
   
   // Add services
   builder.Services.AddControllersWithViews();
   builder.Services.AddDbContext<SchoolContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
   builder.Services.AddScoped<NotificationService>();
   
   var app = builder.Build();
   
   // Configure middleware pipeline
   if (!app.Environment.IsDevelopment())
   {
       app.UseExceptionHandler("/Home/Error");
       app.UseHsts();
   }
   
   app.UseHttpsRedirection();
   app.UseStaticFiles();
   app.UseRouting();
   app.UseAuthorization();
   
   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}");
   
   // Initialize database
   using (var scope = app.Services.CreateScope())
   {
       var context = scope.ServiceProvider.GetRequiredService<SchoolContext>();
       DbInitializer.Initialize(context);
   }
   
   app.Run();
   ```

2. **Remove Global.asax startup code**
   - Delete Global.asax and Global.asax.cs
   - Migrate initialization logic to Program.cs

3. **Remove App_Start classes**
   - BundleConfig → Move to Link Tag Helper or bundling library
   - FilterConfig → Use middleware or filters in Program.cs
   - RouteConfig → Use endpoint routing in Program.cs

4. **Create Properties/launchSettings.json**
   ```json
   {
     "profiles": {
       "ContosoUniversity": {
         "commandName": "Project",
         "launchBrowser": true,
         "environmentVariables": {
           "ASPNETCORE_ENVIRONMENT": "Development"
         },
         "applicationUrl": "https://localhost:5001;http://localhost:5000"
       }
     }
   }
   ```

**Verification**:
- Application starts without errors
- Home page loads
- Database initializes correctly
- Dependency injection works

---

### Phase 6: Controllers Migration
**Goal**: Update controllers to ASP.NET Core MVC patterns

#### Tasks:
1. **Update base controller class references**
   - Change from `System.Web.Mvc.Controller` to `Microsoft.AspNetCore.Mvc.Controller`
   - Update all using statements

2. **Update BaseController.cs**
   - Remove System.Web.Mvc references
   - Update to use Microsoft.AspNetCore.Mvc
   - Update HttpContext access patterns

3. **Update all controllers**
   - HomeController.cs
   - StudentsController.cs
   - CoursesController.cs
   - InstructorsController.cs
   - DepartmentsController.cs
   - NotificationsController.cs

4. **Update controller-specific patterns**
   - `HttpContext.User` → remains the same
   - `Server.MapPath()` → Use `IWebHostEnvironment.WebRootPath`
   - `Request.Files` → `IFormFile` parameters
   - Action filters: Update to ASP.NET Core filters
   - Model binding: Generally compatible, verify edge cases

5. **Update dependency injection**
   - Constructor injection for DbContext
   - Constructor injection for services
   - Remove manual instantiation

6. **Update file upload handling**
   - Change from `HttpPostedFileBase` to `IFormFile`
   - Update file saving logic to use `IWebHostEnvironment`

7. **Update async patterns**
   - Ensure all async methods properly use async/await
   - Update ActionResult to ActionResult<T> where appropriate

**Verification**:
- All controllers compile
- No System.Web references remain
- Dependency injection works
- Actions return correct results

---

### Phase 7: Views Migration
**Goal**: Update Razor views for ASP.NET Core

#### Tasks:
1. **Update _ViewStart.cshtml**
   - Verify layout path is correct

2. **Update _Layout.cshtml**
   - Change `@Styles.Render()` to `<link>` tags or Tag Helpers
   - Change `@Scripts.Render()` to `<script>` tags or Tag Helpers
   - Update bundling approach (use LibMan, npm, or Tag Helpers)
   - Update paths to use `~/wwwroot/`

3. **Update Views/Web.config**
   - Remove or convert to _ViewImports.cshtml
   - Add common namespace imports
   - Add Tag Helper support: `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`

4. **Update individual views**
   - Update Html helpers that changed
   - `Html.BeginForm()` → remains the same or use Tag Helpers
   - `Url.Action()` → remains the same
   - Validation helpers → remains mostly the same

5. **Update static file references**
   - Update paths to Content folder → ~/wwwroot/css/
   - Update paths to Scripts folder → ~/wwwroot/js/
   - Update image paths

6. **Add _ViewImports.cshtml**
   ```cshtml
   @using ContosoUniversity
   @using ContosoUniversity.Models
   @using ContosoUniversity.Models.SchoolViewModels
   @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
   ```

**Verification**:
- All views render correctly
- CSS and JavaScript load properly
- Forms submit correctly
- Validation works

---

### Phase 8: Static Files Migration
**Goal**: Move static content to wwwroot

#### Tasks:
1. **Create wwwroot folder structure**
   ```
   wwwroot/
   ├── css/
   ├── js/
   ├── lib/
   └── images/
   ```

2. **Move Content folder**
   - Move Content/*.css → wwwroot/css/
   - Move Content/*.map → wwwroot/css/
   - Keep or move Content to wwwroot/content if needed

3. **Move Scripts folder**
   - Move Scripts/*.js → wwwroot/js/
   - Consider using LibMan or npm for client libraries

4. **Move Uploads folder**
   - Move Uploads/ → wwwroot/uploads/
   - Update file paths in controllers

5. **Update bundling and minification**
   - Option 1: Use BuildBundlerMinifier NuGet package
   - Option 2: Use npm with webpack/gulp
   - Option 3: Use Tag Helpers with environment tags
   - Remove Microsoft.AspNet.Web.Optimization references

**Verification**:
- Static files serve correctly
- CSS styling works
- JavaScript functions work
- File uploads work

---

### Phase 9: Data Access Migration
**Goal**: Update Entity Framework Core to 8.0

#### Tasks:
1. **Update SchoolContext.cs**
   - Verify DbContext constructor accepts DbContextOptions<SchoolContext>
   - Update any deprecated APIs
   - Review OnModelCreating for breaking changes
   - Verify TPH inheritance configuration

2. **Update DbInitializer.cs**
   - Verify seeding logic works with EF Core 8.0
   - Update any changed APIs

3. **Update SchoolContextFactory.cs**
   - Update for EF Core 8.0 design-time factory pattern
   - Ensure configuration reading works

4. **Review model classes**
   - Verify annotations are compatible
   - Update any navigation properties if needed
   - Check for EF Core 8.0 specific optimizations

5. **Update migrations**
   - Review existing migrations compatibility
   - Run `dotnet ef migrations list`
   - Create new migration if schema changes needed
   - Test migration apply/revert

6. **Update LINQ queries**
   - Review for EF Core 8.0 translation changes
   - Update client evaluation patterns
   - Use async methods throughout

**Verification**:
- Database context initializes
- Migrations run successfully
- CRUD operations work
- Queries execute correctly
- No N+1 query issues

---

### Phase 10: Services and Dependencies Migration
**Goal**: Update services to use ASP.NET Core DI

#### Tasks:
1. **Update NotificationService.cs**
   - Review MSMQ usage (System.Messaging)
   - Consider Azure Service Bus for cloud migration
   - Update for constructor injection
   - Add interface INotificationService
   - Register in DI container

2. **Create service interfaces**
   - Create interfaces for testability
   - Follow dependency inversion principle

3. **Register services in Program.cs**
   ```csharp
   builder.Services.AddScoped<INotificationService, NotificationService>();
   ```

4. **Update PaginatedList.cs**
   - Verify compatibility with EF Core 8.0
   - Update async patterns if needed

5. **Remove System.Web dependencies**
   - Replace HttpContext.Current with IHttpContextAccessor
   - Update file path resolution
   - Update configuration access

**Verification**:
- Services inject correctly
- Notification system works (or is properly abstracted)
- No System.Web references remain
- Scoped services work correctly per request

---

### Phase 11: Authentication and Authorization
**Goal**: Migrate authentication to ASP.NET Core Identity or modern auth

#### Tasks:
1. **Review current authentication**
   - Check if Windows Authentication is used (from Web.config)
   - Review IIS settings

2. **Configure authentication in Program.cs**
   ```csharp
   builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
   app.UseAuthentication();
   app.UseAuthorization();
   ```

3. **Update authorization attributes**
   - Verify [Authorize] attributes work
   - Update any custom authorization filters

4. **Update user access patterns**
   - `HttpContext.User` → `User` property (same)
   - Update any Windows identity specific code

**Verification**:
- Authentication works
- Authorization policies apply correctly
- User identity accessible in controllers

---

### Phase 12: Error Handling and Logging
**Goal**: Implement ASP.NET Core error handling and logging

#### Tasks:
1. **Configure error handling middleware**
   ```csharp
   if (!app.Environment.IsDevelopment())
   {
       app.UseExceptionHandler("/Home/Error");
       app.UseHsts();
   }
   else
   {
       app.UseDeveloperExceptionPage();
   }
   ```

2. **Update ErrorViewModel.cs**
   - Verify it works with new error handling

3. **Update Error.cshtml view**
   - Ensure it displays correctly

4. **Add logging**
   - Configure logging in appsettings.json
   - Inject ILogger<T> in controllers
   - Add logging statements for key operations
   - Consider Application Insights for Azure

5. **Remove custom error pages from Web.config**
   - Handled by middleware now

**Verification**:
- Error pages display correctly
- Development error page shows details
- Production error page is user-friendly
- Logging captures events

---

### Phase 13: Azure Preparation
**Goal**: Prepare application for Azure deployment

#### Tasks:
1. **Add Azure-specific packages (optional)**
   - Microsoft.ApplicationInsights.AspNetCore
   - Azure.Identity for managed identity
   - Microsoft.Extensions.Configuration.AzureKeyVault

2. **Update configuration for Azure**
   - Add environment-specific appsettings
   - Create appsettings.Production.json
   - Use Azure App Configuration or Key Vault
   - Update connection strings for Azure SQL

3. **Configure Application Insights**
   ```csharp
   builder.Services.AddApplicationInsightsTelemetry();
   ```

4. **Review MSMQ replacement**
   - Plan migration to Azure Service Bus
   - Or Azure Queue Storage
   - Update NotificationService abstraction

5. **Configure for App Service**
   - Ensure HTTPS redirection configured
   - Configure forwarded headers if behind proxy
   - Set up health checks

6. **Add deployment files**
   - Consider adding .github/workflows for CI/CD
   - Add azure-pipelines.yml if using Azure DevOps

**Verification**:
- Application runs in Azure App Service
- Logging works with Application Insights
- Database connections work
- Configuration sources work

---

### Phase 14: Code Modernization
**Goal**: Apply .NET 8.0 best practices and patterns

#### Tasks:
1. **Update to C# 12 features**
   - Use primary constructors where appropriate
   - Use collection expressions
   - Use namespaced file scoped declarations
   - Use nullable reference types

2. **Enable nullable reference types**
   - Add `<Nullable>enable</Nullable>` to project file
   - Update code to handle nullability

3. **Use minimal APIs patterns where beneficial**
   - Consider for simple endpoints

4. **Update async patterns**
   - Use ValueTask where appropriate
   - Use ConfigureAwait properly
   - Ensure all I/O is async

5. **Apply performance improvements**
   - Use Span<T> and Memory<T> where beneficial
   - Review string allocations
   - Use StringComparison.Ordinal

6. **Add XML documentation**
   - Document public APIs
   - Enable XML documentation generation

**Verification**:
- Code compiles with all new features
- Performance improves or remains same
- Code analysis passes
- No warnings

---

### Phase 15: Testing
**Goal**: Ensure application works correctly after migration

#### Tasks:
1. **Build verification**
   - Run `dotnet build`
   - Resolve all build errors
   - Resolve all warnings
   - Verify debug and release configurations

2. **Unit tests (if exist)**
   - Update test projects to .NET 8.0
   - Update test dependencies
   - Run all tests
   - Fix failing tests

3. **Integration tests**
   - Test database operations
   - Test all CRUD operations
   - Test file uploads
   - Test notifications

4. **Manual testing**
   - Test all pages load
   - Test navigation
   - Test forms submission
   - Test validation
   - Test error scenarios

5. **Performance testing**
   - Compare startup time
   - Compare page load times
   - Check memory usage
   - Profile application

**Verification**:
- All builds succeed
- All tests pass
- Manual tests complete successfully
- Performance is acceptable

---

### Phase 16: Security and Compliance
**Goal**: Ensure security best practices

#### Tasks:
1. **Run CVE vulnerability check**
   - Use `dotnet list package --vulnerable`
   - Update vulnerable packages
   - Document any risks

2. **Security headers**
   - Add security headers middleware
   - Configure CORS if needed
   - Set up CSP (Content Security Policy)

3. **Data protection**
   - Configure data protection if needed
   - Review sensitive data handling

4. **Input validation**
   - Review all input validation
   - Ensure anti-forgery tokens used
   - Check for XSS vulnerabilities

5. **Dependency scanning**
   - Run dependency check tools
   - Review licenses
   - Update outdated packages

**Verification**:
- No known vulnerabilities
- Security headers present
- Input validation works
- CSRF protection enabled

---

### Phase 17: Documentation
**Goal**: Document migration and new setup

#### Tasks:
1. **Update README.md**
   - Document .NET 8.0 requirements
   - Update setup instructions
   - Update run instructions
   - Document dependencies

2. **Document configuration**
   - Explain appsettings.json structure
   - Document connection strings
   - Document environment variables

3. **Document deployment**
   - Azure deployment steps
   - IIS deployment steps (if supported)
   - Docker containerization (if planned)

4. **Update developer documentation**
   - Update build instructions
   - Update debugging steps
   - Document new features

5. **Document breaking changes**
   - List what changed from .NET Framework
   - Migration notes for future reference

**Verification**:
- README is accurate
- Documentation is complete
- New developers can set up project

---

### Phase 18: Final Verification and Cleanup
**Goal**: Final checks before completion

#### Tasks:
1. **Build verification**
   - Clean build succeeds
   - No warnings
   - Both Debug and Release configurations work

2. **Run all verification checks**
   - ✅ Build verification: `dotnet build`
   - ✅ CVE check: `dotnet list package --vulnerable`
   - ✅ Consistency check: Verify all files migrated
   - ✅ Completeness check: All features work
   - ✅ Unit tests: `dotnet test` (if exist)

3. **Code cleanup**
   - Remove commented code
   - Remove unused files
   - Remove unused packages
   - Format code consistently

4. **Final code review**
   - Review all changes
   - Check for System.Web references
   - Verify best practices applied

5. **Create migration summary**
   - Document what was migrated
   - Document challenges and solutions
   - Document test results
   - Create final report

**Verification**:
- ✅ dotnet build succeeds
- ✅ No CVE vulnerabilities
- ✅ All consistency checks pass
- ✅ All completeness checks pass
- ✅ Tests pass
- ✅ Application runs correctly

---

## Success Criteria

### Functional Requirements
- ✅ Application builds without errors
- ✅ Application runs on .NET 8.0
- ✅ All pages load correctly
- ✅ All CRUD operations work
- ✅ Database operations function properly
- ✅ File uploads work
- ✅ Validation works
- ✅ Error handling works

### Technical Requirements
- ✅ Uses .NET 8.0 SDK-style project
- ✅ Uses Entity Framework Core 8.0
- ✅ Uses ASP.NET Core MVC 8.0
- ✅ Uses appsettings.json configuration
- ✅ Uses dependency injection throughout
- ✅ No System.Web dependencies
- ✅ No .NET Framework packages
- ✅ Async/await patterns used

### Quality Requirements
- ✅ No build warnings
- ✅ No security vulnerabilities (CVE check)
- ✅ Code follows .NET 8.0 best practices
- ✅ Performance equal or better than original
- ✅ All tests pass
- ✅ Documentation updated

### Azure Readiness
- ✅ Can deploy to Azure App Service
- ✅ Configuration supports multiple environments
- ✅ Logging configured for cloud
- ✅ Connection strings externalized
- ✅ Ready for CI/CD

---

## Risk Assessment

### High Risk Items
1. **MSMQ Notification System**
   - Risk: MSMQ not available in Azure
   - Mitigation: Abstract notification interface, plan Azure Service Bus migration
   - Status: Needs review

2. **Windows Authentication**
   - Risk: May need reconfiguration for Azure
   - Mitigation: Test Azure AD integration
   - Status: Needs review

3. **File Upload Path Changes**
   - Risk: Hard-coded paths may break
   - Mitigation: Use IWebHostEnvironment
   - Status: Will address in Phase 6

### Medium Risk Items
1. **EF Core 3.1 to 8.0 Breaking Changes**
   - Risk: Query translation changes
   - Mitigation: Test all database operations thoroughly
   - Status: Will monitor during migration

2. **Bundling and Minification**
   - Risk: No direct replacement for System.Web.Optimization
   - Mitigation: Use Tag Helpers or build tools
   - Status: Will address in Phase 8

### Low Risk Items
1. **View Compatibility**
   - Risk: Minor Razor syntax changes
   - Mitigation: Update as needed
   - Status: Low impact

---

## Timeline Estimate

- **Phase 1-2**: Analysis and Project Structure (1-2 hours)
- **Phase 3-4**: Dependencies and Configuration (2-3 hours)
- **Phase 5**: Application Startup (1-2 hours)
- **Phase 6**: Controllers Migration (2-3 hours)
- **Phase 7-8**: Views and Static Files (2-3 hours)
- **Phase 9-10**: Data Access and Services (2-3 hours)
- **Phase 11-12**: Auth and Error Handling (1-2 hours)
- **Phase 13-14**: Azure Prep and Modernization (2-3 hours)
- **Phase 15-18**: Testing and Verification (3-4 hours)

**Total Estimated Time**: 16-25 hours

---

## Next Steps

1. ✅ Review and approve this migration plan
2. Create Git branch for migration work
3. Begin Phase 2: Project Structure Migration
4. Follow phases sequentially
5. Update progress.md after each task
6. Run verification checks at each phase
7. Complete final verification in Phase 18

---

## Notes

- This migration plan is comprehensive and covers all aspects of migrating from .NET Framework 4.8 to .NET 8.0
- The application is already using Entity Framework Core 3.1, which makes the migration easier than if it were using EF 6.x
- Each phase includes verification steps to ensure progress is correct
- The plan follows the CRITICAL migration workflow requirements
- All verification steps (build, CVE, consistency, completeness, tests) are mandatory and cannot be skipped
- Progress must be tracked in progress.md throughout the migration

---

*Migration Plan Created: 2025*
*Last Updated: 2025*
