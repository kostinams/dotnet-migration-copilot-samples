using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ContosoUniversity.Services
{
    /// <summary>
    /// Service for managing file uploads to Azure Blob Storage
    /// </summary>
    public class BlobStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly BlobContainerClient _containerClient;

        /// <summary>
        /// Initializes a new instance of the BlobStorageService
        /// </summary>
        public BlobStorageService()
        {
            _connectionString = ConfigurationManager.AppSettings["AzureBlobStorage:ConnectionString"];
            _containerName = ConfigurationManager.AppSettings["AzureBlobStorage:ContainerName"];

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Azure Blob Storage connection string is not configured. Please set 'AzureBlobStorage:ConnectionString' in Web.config.");
            }

            if (string.IsNullOrEmpty(_containerName))
            {
                throw new InvalidOperationException("Azure Blob Storage container name is not configured. Please set 'AzureBlobStorage:ContainerName' in Web.config.");
            }

            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                _containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize Azure Blob Storage client. Please verify the connection string is valid.", ex);
            }
        }

        /// <summary>
        /// Ensures the blob container exists, creating it if necessary
        /// </summary>
        private async Task EnsureContainerExistsAsync()
        {
            try
            {
                await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create or access blob container '{_containerName}'.", ex);
            }
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage
        /// </summary>
        /// <param name="fileStream">The file stream to upload</param>
        /// <param name="fileName">The name to give the file in blob storage</param>
        /// <param name="contentType">The MIME content type of the file</param>
        /// <returns>The absolute URL of the uploaded blob</returns>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            try
            {
                // Ensure container exists
                await EnsureContainerExistsAsync();

                // Get blob client for the file
                var blobClient = _containerClient.GetBlobClient(fileName);

                // Set blob HTTP headers
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType ?? "application/octet-stream"
                };

                // Upload the file
                fileStream.Position = 0; // Reset stream position
                await blobClient.UploadAsync(fileStream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });

                // Return the absolute URL of the blob
                return blobClient.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to upload file '{fileName}' to blob storage.", ex);
            }
        }

        /// <summary>
        /// Deletes a file from Azure Blob Storage
        /// </summary>
        /// <param name="blobUrl">The absolute URL or blob name to delete</param>
        /// <returns>True if the blob was deleted, false if it didn't exist</returns>
        public async Task<bool> DeleteFileAsync(string blobUrl)
        {
            if (string.IsNullOrEmpty(blobUrl))
            {
                return false; // Nothing to delete
            }

            try
            {
                // Extract blob name from URL if a full URL was provided
                string blobName = ExtractBlobNameFromUrl(blobUrl);

                // Get blob client
                var blobClient = _containerClient.GetBlobClient(blobName);

                // Delete the blob if it exists
                var response = await blobClient.DeleteIfExistsAsync();
                return response.Value;
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - deletion failures shouldn't block other operations
                System.Diagnostics.Debug.WriteLine($"Error deleting blob '{blobUrl}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the URL for a blob
        /// </summary>
        /// <param name="blobName">The name of the blob</param>
        /// <returns>The absolute URL of the blob</returns>
        public string GetBlobUrl(string blobName)
        {
            if (string.IsNullOrEmpty(blobName))
            {
                return null;
            }

            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                return blobClient.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting blob URL for '{blobName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extracts the blob name from a full URL or returns the input if it's already just a name
        /// </summary>
        /// <param name="blobUrlOrName">Either a full blob URL or just the blob name</param>
        /// <returns>The blob name</returns>
        private string ExtractBlobNameFromUrl(string blobUrlOrName)
        {
            if (string.IsNullOrEmpty(blobUrlOrName))
            {
                return blobUrlOrName;
            }

            // If it's a URL, extract the blob name
            if (blobUrlOrName.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                blobUrlOrName.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(blobUrlOrName);
                    // The blob name is the path without the leading slash and container name
                    var segments = uri.Segments;
                    if (segments.Length >= 2)
                    {
                        // segments[0] = "/", segments[1] = "container-name/", segments[2+] = blob path
                        return string.Join("", segments, 2, segments.Length - 2).TrimEnd('/');
                    }
                }
                catch
                {
                    // If URL parsing fails, return as-is
                }
            }

            return blobUrlOrName;
        }

        /// <summary>
        /// Checks if a given path/URL is a blob storage URL
        /// </summary>
        /// <param name="path">The path or URL to check</param>
        /// <returns>True if it's a blob storage URL, false otherwise</returns>
        public static bool IsBlobUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }
    }
}
