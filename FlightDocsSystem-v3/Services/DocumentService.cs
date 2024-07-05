using FlightDocsSystem_v3.Data;
using FlightDocsSystem_v3.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace FlightDocsSystem_v3.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly FlightDocsSystemContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DocumentService(FlightDocsSystemContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Document>> GetAllDocuments()
        {
            var userRole = GetUserRole();
            if (userRole == "Admin")
                return await _dbContext.Documents
                                        .Include(d => d.CreatedByNavigation)
                                        .Include(d => d.Flight)
                                        .ToListAsync();
            else
            {
                var userId = GetUserId();
                var flightsUserIsIn = await _dbContext.UserFlights
                                                        .Where(uf => uf.UserId == userId)
                                                        .Select(uf => uf.FlightId)
                                                        .ToListAsync();
                return await _dbContext.Documents
                                        .Include(d => d.CreatedByNavigation)
                                        .Include(d => d.Flight)
                                        .Where(d => flightsUserIsIn.Contains(d.FlightId))
                                        .ToListAsync();
            }
        }
        public async Task<Document> GetDocumentById(int documentId)
        {
            var userRole = GetUserRole();
            var userId = GetUserId();
            var document = await _dbContext.Documents
                                            .Include(d => d.CreatedByNavigation)
                                            .Include(d => d.Flight)
                                            .FirstOrDefaultAsync(d => d.DocumentId == documentId);
            if (document == null)
                return null;
            if (userRole == "Admin" || document.Flight.UserFlights.Any(uf => uf.UserId == userId))
                return document;
            return null;
        }

        public async Task<bool> CreateDocument(int flightId, DocumentCreateDto documentDto)
        {
            var flight = await _dbContext.Flights.FindAsync(flightId);
            if (flight == null)
                throw new ArgumentException($"Flight with id {flightId} not found.");

            if (flight.Status == "Đã hạ cánh")
                throw new InvalidOperationException("Cannot create document for a flight that has landed.");

            string directory = Path.Combine("wwwroot", "document");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var userId = GetUserId();
            var document = new Document
            {
                FlightId = flightId,
                DocumentType = documentDto.DocumentType,
                Title = documentDto.Title,
                Content = documentDto.Content,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                CanEdit = false
            };

            var fileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(documentDto.File.FileName)}";
            string filePath = Path.Combine(directory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await documentDto.File.CopyToAsync(stream);
            }

            document.FilePath = filePath;

            _dbContext.Documents.Add(document);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<Document> UpdateDocument(int id, DocumentViewModel document, IFormFile newFile)
        {
            var existingDocument = await _dbContext.Documents.FindAsync(id);
            if (existingDocument == null)
                throw new Exception("Document not found");
            var userId = GetUserId();
            var userRole = GetUserRole();
            if (!CanUserEditDocument(existingDocument, userId, userRole))
                throw new InvalidOperationException("You do not have permission to edit this document.");
            existingDocument.DocumentType = document.DocumentType;
            existingDocument.Title = document.Title;
            existingDocument.Content = document.Content;
            existingDocument.UpdatedBy = userId;
            existingDocument.UpdatedAt = DateTime.UtcNow;
            if (newFile != null && newFile.Length > 0)
            {
                string directory = Path.Combine("wwwroot", "document");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(newFile.FileName);
                string filePath = Path.Combine(directory, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await newFile.CopyToAsync(stream);
                }
                existingDocument.FilePath = filePath;
            }
            _dbContext.Entry(existingDocument).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return existingDocument;
        }

        public async Task<bool> DeleteDocument(int documentId)
        {
            var document = await _dbContext.Documents.FindAsync(documentId);
            if (document == null) return false;
            _dbContext.Documents.Remove(document);
            return true;
        }

        public async Task<(MemoryStream fileStream, string contentType, string fileName)> DownloadDocumentAsync(int documentId)
        {
            var document = await _dbContext.Documents.FindAsync(documentId);
            if (document == null)
                throw new FileNotFoundException("Document not found.");
            var filePath = document.FilePath;
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                throw new FileNotFoundException("File not found.");
            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            var contentType = GetContentType(filePath);
            var fileName = Path.GetFileName(filePath);
            return (memory, contentType, fileName);
        }
        public async Task<IEnumerable<Document>> SearchDocumentsByType(string documentType)
        {
            var documents = await _dbContext.Documents
                                            .Include(d => d.CreatedByNavigation)
                                            .Include(d => d.Flight)
                                            .Where(d => d.DocumentType.Contains(documentType))
                                            .ToListAsync();
            return documents;
        }
        public async Task<IEnumerable<Document>> SearchDocumentsByCreatedAt(DateTime createdAt)
        {
            var documents = await _dbContext.Documents
                                            .Include(d => d.CreatedByNavigation)
                                            .Include(d => d.Flight)
                                            .Where(d => d.CreatedAt.Date == createdAt.Date)
                                            .ToListAsync();
            return documents;
        }

        public async Task<IEnumerable<Document>> SearchDocumentsByFlightIdAndName(int flightId, string documentName)
        {
            var documents = await _dbContext.Documents
                                            .Include(d => d.CreatedByNavigation)
                                            .Include(d => d.Flight)
                                            .Where(d => d.FlightId == flightId && d.Title.Contains(documentName))
                                            .ToListAsync();
            return documents;
        }

        public async Task<bool> UpdateCanEditFlag(int documentId, bool canEdit)
        {
            var document = await _dbContext.Documents.FindAsync(documentId);
            if (document == null)
                throw new ArgumentException($"Document with ID {documentId} not found.");
            var userId = GetUserId();
            var userRole = GetUserRole();
            if (userRole != "Admin" && document.CreatedBy != userId)
                throw new InvalidOperationException("You do not have permission to update CanEdit flag for this document.");
            document.CanEdit = canEdit;
            _dbContext.Entry(document).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
        {
            { ".txt", "text/plain" },
            { ".pdf", "application/pdf" },
            { ".doc", "application/vnd.ms-word" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".gif", "image/gif" },
            { ".csv", "text/csv" }
        };
        }

        private string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
        }

        private int GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new InvalidOperationException("User not authenticated or invalid user ID.");
            return userId;
        }

        private bool CanUserEditDocument(Document document, int userId, string userRole)
        {
            if (userRole == "Admin" || document.CreatedBy == userId || document.CanEdit)
                return true;
            var userInFlight = _dbContext.UserFlights.Any(uf => uf.UserId == userId && uf.FlightId == document.FlightId);
            return userInFlight;
        }
    }
}
