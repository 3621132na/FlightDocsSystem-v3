using FlightDocsSystem_v3.Data;
using FlightDocsSystem_v3.Models;
using FlightDocsSystem_v3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightDocsSystem_v3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }
        [Authorize(Roles = "Admin, GO")]
        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var documents = await _documentService.GetAllDocuments();
                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{documentId}")]
        public async Task<IActionResult> GetDocumentById(int documentId)
        {
            try
            {
                var document = await _documentService.GetDocumentById(documentId);
                if (document == null)
                    return NotFound();
                return Ok(document);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [Authorize(Roles = "Admin, GO")]
        [HttpPost("create/{flightId}")]
        public async Task<IActionResult> CreateDocument(int flightId, DocumentCreateDto documentDto)
        {
            try
            {
                var createdDocument = await _documentService.CreateDocument(flightId, documentDto);
                return Ok(createdDocument);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, DocumentViewModel document, IFormFile newFile)
        {
            try
            {
                var updatedDocument = await _documentService.UpdateDocument(id, document, newFile);
                if (updatedDocument == null)
                    return NotFound();
                return Ok(updatedDocument);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [Authorize(Roles = "Admin, GO")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                var success = await _documentService.DeleteDocument(id);
                if (!success)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("download/{documentId}")]
        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            try
            {
                var (fileStream, contentType, fileName) = await _documentService.DownloadDocumentAsync(documentId);
                return File(fileStream, contentType, fileName);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("search-by-type")]
        public async Task<IActionResult> SearchDocumentsByType(string documentType)
        {
            try
            {
                var documents = await _documentService.SearchDocumentsByType(documentType);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("search-by-created-at")]
        public async Task<IActionResult> SearchDocumentsByCreatedAt(DateTime createdAt)
        {
            try
            {
                var documents = await _documentService.SearchDocumentsByCreatedAt(createdAt);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("search-by-flight-id-and-name")]
        public async Task<IActionResult> SearchDocumentsByFlightIdAndName(int flightId, string documentName)
        {
            try
            {
                var documents = await _documentService.SearchDocumentsByFlightIdAndName(flightId, documentName);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("update-can-edit/{documentId}")]
        public async Task<IActionResult> UpdateCanEditFlag(int documentId, bool canEdit)
        {
            try
            {
                var success = await _documentService.UpdateCanEditFlag(documentId, canEdit);
                if (!success)
                    return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
