# Teaching Material Image Upload Feature

This feature allows administrators to upload images for teaching materials (textbooks) associated with courses.

## Features

- **Image Upload**: Upload teaching material images when creating or editing courses
- **File Validation**: Supports JPG, JPEG, PNG, GIF, and BMP formats
- **Size Limits**: Maximum file size of 5MB per image
- **Cloud Storage**: Images are stored in Azure Blob Storage for scalability and reliability
- **Automatic Cleanup**: Images are automatically deleted when courses are removed
- **Unique Filenames**: Each uploaded image gets a unique filename to prevent conflicts
- **Direct Access**: Images are served directly from Azure Blob Storage via HTTPS

## Usage

### Creating a Course with Teaching Material Image

1. Navigate to the Courses section
2. Click "Create New" (Admin only)
3. Fill in the course details
4. In the "Teaching Material Image" section, click "Choose File"
5. Select an image file (JPG, JPEG, PNG, GIF, or BMP)
6. Click "Create" to save the course

### Editing a Course's Teaching Material Image

1. Navigate to the Courses section
2. Click "Edit" next to the course you want to modify
3. If a teaching material image already exists, it will be displayed
4. To change the image, click "Choose File" and select a new image
5. Click "Save" to update the course

### Viewing Teaching Material Images

- **Course List**: Small thumbnails (50x50px) are displayed in the courses index
- **Course Details**: Full-size images (max 300x300px) are displayed on the course details page

## Technical Details

### Azure Blob Storage
- Images are stored in Azure Blob Storage in the configured container (default: `teaching-materials`)
- Filenames follow the pattern: `course_{CourseID}_{GUID}.{extension}`
- Old images are automatically deleted from blob storage when replaced
- Each blob URL is stored in the database for direct access
- Blob container is configured with public read access for serving images

### Configuration
Required settings in `Web.config`:
```xml
<add key="AzureBlobStorage:ConnectionString" value="UseDevelopmentStorage=true"/>
<add key="AzureBlobStorage:ContainerName" value="teaching-materials"/>
```

**Development Environment:**
- Use `"UseDevelopmentStorage=true"` to connect to Azure Storage Emulator
- Install and run Azurite (Azure Storage Emulator) for local development

**Production Environment:**
- Replace with actual Azure Storage Account connection string:
  ```
  DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT_NAME;AccountKey=YOUR_ACCOUNT_KEY;EndpointSuffix=core.windows.net
  ```

### Database Schema
- Field: `TeachingMaterialImagePath` (VARCHAR(255)) in the Course table
- Stores the absolute blob URL (e.g., `https://storageaccount.blob.core.windows.net/teaching-materials/course_1045_guid.jpg`)
- URLs are publicly accessible for image display without authentication

### Security
- File type validation prevents uploading of non-image files
- File size validation prevents uploads larger than 5MB
- Only authenticated users with appropriate roles can upload images

### Authorization
- **Create/Upload**: Admin role required
- **Edit/Upload**: Admin or Teacher role required
- **View**: All authenticated users can view images
- **Delete**: Admin role required (deletes both course and associated image)

## Troubleshooting

### Common Issues

1. **"File too large" error**: Ensure your image is under 5MB
2. **"Invalid file type" error**: Only JPG, JPEG, PNG, GIF, and BMP files are supported
3. **Upload fails**: 
   - Verify Azure Blob Storage connection string is configured correctly
   - Check that the Azure Storage account is accessible
   - For development, ensure Azurite (Azure Storage Emulator) is running
   - Check container name matches configuration
4. **Images not displaying**:
   - Verify blob container has public read access enabled
   - Check blob URLs in database are valid
   - Verify CORS settings if accessing from different domain

### Configuration

**Web.config Upload Limits:**
- `maxRequestLength="10240"` (10MB in KB)
- `maxAllowedContentLength="10485760"` (10MB in bytes)
- `executionTimeout="3600"` (1 hour timeout for large uploads)

**Azure Storage Account Requirements:**
- Container must be created before first upload (automatic creation is enabled)
- Recommended: Set container public access level to "Blob" for public read access
- Configure CORS rules if needed for cross-origin access

## Deployment Considerations

### Azure Storage Account Setup
1. **Create Azure Storage Account** (if not exists):
   - Use Azure Portal, CLI, or ARM templates
   - Recommended: General-purpose v2 account
   - Choose appropriate replication (LRS, GRS, etc.)
   - Select performance tier (Standard or Premium)

2. **Create Blob Container**:
   - Container name: `teaching-materials` (or as configured)
   - Public access level: "Blob (anonymous read access for blobs only)"
   - Can be created automatically by the application on first upload

3. **Configure Connection String**:
   - Add to Web.config appSettings
   - Consider using Azure Key Vault for production secrets
   - For Azure App Service, use Application Settings instead of Web.config

### Development Environment Setup
1. **Install Azurite** (Azure Storage Emulator):
   ```bash
   npm install -g azurite
   azurite start
   ```
   Or use Docker:
   ```bash
   docker run -p 10000:10000 mcr.microsoft.com/azure-storage/azurite
   ```

2. **Use Development Connection String**:
   ```xml
   <add key="AzureBlobStorage:ConnectionString" value="UseDevelopmentStorage=true"/>
   ```

### Migration from Local File Storage
If migrating from existing local file storage:
1. Existing courses with local file paths (`~/Uploads/...`) will need migration
2. Create a migration utility to:
   - Read existing files from `/Uploads/TeachingMaterials/`
   - Upload each file to Azure Blob Storage
   - Update database with new blob URLs
3. The `BlobStorageService.IsBlobUrl()` method can distinguish blob URLs from local paths

### Security Best Practices
- **Never commit connection strings with secrets** to source control
- Use Azure Key Vault or App Service Application Settings for production
- Enable Azure Storage firewall rules to restrict access
- Consider using Managed Identity for authentication in Azure
- Regularly rotate storage account keys
- Monitor blob access patterns and costs

### Backup Strategy
- Azure Blob Storage provides built-in redundancy (LRS/GRS)
- Consider enabling soft delete for blob versioning
- Implement lifecycle management policies for cost optimization
- Document restore procedures for disaster recovery
- Consider geo-redundant storage (GRS) for critical data

## Future Enhancements

Potential improvements for this feature:
- Image resizing and optimization using Azure Functions
- Multiple image support per course
- Image gallery view
- Bulk upload functionality
- Image metadata support (alt text, captions)
- CDN integration for faster image delivery worldwide
- Thumbnail generation using Azure Cognitive Services
- Automatic image format optimization (WebP conversion)
