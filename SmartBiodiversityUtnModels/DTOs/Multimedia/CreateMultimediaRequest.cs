

using Microsoft.AspNetCore.Http;

namespace SmartBiodiversityUtnModels.DTOs.Multimedia
{
    public class CreateMultimediaRequest
    {
        public string EspecieId { get; set; } = string.Empty;
        public IFormFile Archivo { get; set; } = null!;
        public string TipoArchivo { get; set; } = "Imagen";
    }
}
