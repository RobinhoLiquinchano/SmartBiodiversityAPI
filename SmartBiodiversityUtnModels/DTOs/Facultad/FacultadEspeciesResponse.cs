namespace SmartBiodiversityUtnModels.DTOs.Facultad
{
    /// <summary>
    /// DTO que devuelve el endpoint GET /api/Facultades/{id}/especies
    /// Contiene las listas separadas de Flora y Fauna con sus conteos.
    /// </summary>
    public class FacultadEspeciesResponse
    {
        public string IdFacultad { get; set; } = string.Empty;
        public string NombreFacultad { get; set; } = string.Empty;
        public double Latitud { get; set; }
        public double Longitud { get; set; }

        public List<EspecieResumenDto> Flora { get; set; } = new();
        public List<EspecieResumenDto> Fauna { get; set; } = new();

        public int TotalFlora { get; set; }
        public int TotalFauna { get; set; }
        public int TotalGeneral { get; set; }
    }

    public class EspecieResumenDto
    {
        public string IdEspecie { get; set; } = string.Empty;
        public string NombreComun { get; set; } = string.Empty;
        public string NombreCientifico { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
    }
}
