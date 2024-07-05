using FlightDocsSystem_v3.Data;
using FlightDocsSystem_v3.Models;

namespace FlightDocsSystem_v3.Services
{
    public interface IDocumentService
    {
        Task<IEnumerable<Document>> GetAllDocuments();
        Task<Document> GetDocumentById(int documentId);
        Task<bool> CreateDocument(int flightId, DocumentCreateDto documentDto);
        Task<Document> UpdateDocument(int id, DocumentViewModel document, IFormFile newFile);
        Task<bool> DeleteDocument(int documentId);
        Task<(MemoryStream fileStream, string contentType, string fileName)> DownloadDocumentAsync(int documentId);
        Task<IEnumerable<Document>> SearchDocumentsByType(string documentType);
        Task<IEnumerable<Document>> SearchDocumentsByCreatedAt(DateTime createdAt);
        Task<IEnumerable<Document>> SearchDocumentsByFlightIdAndName(int flightId, string documentName);
        Task<bool> UpdateCanEditFlag(int documentId, bool canEdit);

    }
}
