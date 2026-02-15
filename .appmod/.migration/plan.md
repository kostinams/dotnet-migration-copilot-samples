# Azure Blob Storage Migration Plan

## Project Overview
**Project:** ContosoUniversity - .NET Framework 4.8 ASP.NET MVC Application  
**Migration Type:** Local File I/O to Azure Blob Storage  
**Migration Date:** 2024  
**Target:** Replace local file system storage with Azure Blob Storage for teaching material images

## Current State Analysis

### Current Implementation
- **Framework:** .NET Framework 4.8
- **Application Type:** ASP.NET MVC 5
- **File Storage Location:** `~/Uploads/TeachingMaterials/` (local file system)
- **Affected Controller:** `Controllers/CoursesController.cs`
- **Storage Field:** `Course.TeachingMaterialImagePath` (stores relative paths like `~/Uploads/TeachingMaterials/{fileName}`)

### Current File Operations
1. **Upload (Create):** Lines 52-96 in CoursesController.cs
   - Uses `Server.MapPath()` to resolve physical path
   - Creates directory if not exists
   - Saves file using `HttpPostedFileBase.SaveAs()`
   - Stores relative path in database
   
2. **Edit (Replace):** Lines 134-189 in CoursesController.cs
   - Deletes old file from file system
   - Uploads new file
   - Updates database path
   
3. **Delete:** Lines 226-243 in CoursesController.cs
   - Deletes file from file system using `File.Delete()`
   - Removes database record

### File Validation
- **Allowed Extensions:** .jpg, .jpeg, .png, .gif, .bmp
- **Max File Size:** 5MB (5 * 1024 * 1024 bytes)
- **Naming Convention:** `course_{CourseID}_{Guid}{extension}`

## Migration Phases

### Phase 1: Analysis ✓
**Status:** Complete  
**Tasks:**
- [x] Analyze current file I/O implementation
- [x] Review CoursesController.cs code
- [x] Document file operations (upload, edit, delete)
- [x] Identify validation rules and constraints

### Phase 2: Dependencies
**Status:** Pending  
**Tasks:**
- [ ] Add Azure.Storage.Blobs NuGet package (latest stable version)
- [ ] Check for CVE vulnerabilities in new package
- [ ] Verify package compatibility with .NET Framework 4.8
- [ ] Update packages.config

**Expected Changes:**
- Add `Azure.Storage.Blobs` package reference
- May require additional Azure dependencies (Azure.Core, Azure.Identity, etc.)

### Phase 3: Configuration
**Status:** Pending  
**Tasks:**
- [ ] Add Azure Blob Storage connection string to Web.config
- [ ] Add blob container name configuration setting
- [ ] Create configuration keys in appSettings section
- [ ] Document configuration setup for deployment

**Configuration Keys:**
- `AzureBlobStorage:ConnectionString` - Azure Storage account connection string
- `AzureBlobStorage:ContainerName` - Blob container name for teaching materials (e.g., "teaching-materials")

### Phase 4: Code Implementation
**Status:** Pending  
**Tasks:**
- [ ] Create BlobStorageService class in Services folder
  - [ ] Implement UploadFileAsync method
  - [ ] Implement DeleteFileAsync method
  - [ ] Implement GetBlobUrlAsync method
  - [ ] Add proper error handling and logging
  
- [ ] Update CoursesController.cs
  - [ ] Inject BlobStorageService
  - [ ] Replace Create action file upload logic
  - [ ] Replace Edit action file upload/replace logic
  - [ ] Replace Delete action file deletion logic
  - [ ] Update path storage to use blob URL instead of relative path
  
- [ ] Update Course model if needed
  - [ ] Consider renaming TeachingMaterialImagePath to TeachingMaterialImageUrl
  - [ ] Update display attributes

- [ ] Update Views if needed
  - [ ] Verify image display works with blob URLs
  - [ ] Update any path resolution logic

**Key Design Decisions:**
1. **Service Pattern:** Create a dedicated BlobStorageService for reusability
2. **Async Operations:** Use async/await for blob operations (better scalability)
3. **Error Handling:** Graceful degradation if blob storage is unavailable
4. **URL Storage:** Store full blob URL instead of relative path for direct access
5. **Container Structure:** Use single container with file naming convention for organization

### Phase 5: Verification (MANDATORY)
**Status:** Pending  
**Tasks:**
- [ ] Build verification (`dotnet msbuild`)
- [ ] CVE vulnerability check
- [ ] Migration consistency check
- [ ] Migration completeness check
- [ ] Manual testing (if possible in CI environment)
  - [ ] Test file upload (Create course with image)
  - [ ] Test file replace (Edit course and change image)
  - [ ] Test file delete (Delete course with image)
  - [ ] Test validation rules still work
  - [ ] Test display of images from blob storage

### Phase 6: Documentation
**Status:** Pending  
**Tasks:**
- [ ] Update TEACHING_MATERIAL_UPLOAD.md with Azure Blob Storage instructions
- [ ] Document Azure Storage account setup requirements
- [ ] Add deployment configuration guide
- [ ] Update setup/troubleshooting sections

## Migration Constraints

### Must Preserve
- File validation rules (extensions, size limits)
- File naming convention (for consistency)
- Error handling behavior
- User experience (no visible changes to UI)

### Azure Blob Storage Requirements
1. **Azure Storage Account** must be created before deployment
2. **Blob Container** must be created and accessible
3. **Connection String** must be configured in Web.config
4. **Public Access** Container should allow blob-level public read access for image display
5. **CORS** May need to configure CORS if images served from different domain

### Backward Compatibility Considerations
- Existing courses with local file paths will need migration strategy
- Consider hybrid approach during transition:
  - Check if path starts with `http://` or `https://` (blob URL)
  - Otherwise treat as legacy local file path
  - Provide migration utility to move existing files to blob storage

## Risk Assessment

### High Risk Items
1. **Configuration Errors:** Missing or invalid connection string will break file uploads
   - Mitigation: Add configuration validation on app startup
   
2. **Data Migration:** Existing files need to be migrated to blob storage
   - Mitigation: Create separate migration utility script

### Medium Risk Items
1. **Performance:** Network calls to Azure may be slower than local file I/O
   - Mitigation: Use async operations, consider caching strategies
   
2. **Cost:** Azure Blob Storage has usage costs
   - Mitigation: Document cost considerations, use appropriate storage tier

### Low Risk Items
1. **File Size Limits:** Azure Blob Storage supports much larger files
2. **Concurrent Access:** Azure handles concurrent operations better than local file system

## Success Criteria
- [ ] All file upload operations use Azure Blob Storage
- [ ] All file delete operations use Azure Blob Storage
- [ ] Build succeeds without errors
- [ ] No CVE vulnerabilities introduced
- [ ] All verification checks pass
- [ ] Configuration documented for deployment
- [ ] Existing validation rules still enforced

## Rollback Plan
If migration fails:
1. Revert code changes via Git
2. Keep Web.config settings for future retry
3. Local file storage code remains in Git history

## Post-Migration Tasks
1. Create data migration utility for existing files
2. Set up Azure Storage account monitoring
3. Configure backup/disaster recovery for blob storage
4. Review and optimize storage costs
5. Consider CDN integration for better performance
