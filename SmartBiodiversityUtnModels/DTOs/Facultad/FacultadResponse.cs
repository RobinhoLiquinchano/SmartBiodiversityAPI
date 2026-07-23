namespace SmartBiodiversityUtnModels.DTOs.Facultad
{
    public class FacultadResponse
    {
        public string IdFacultad { get; set; } = string.Empty;
        public int Numero { get; set; }           // ====== NUEVO ======
        public string Nombre { get; set; } = string.Empty;
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string? Descripcion { get; set; }
        public int TotalEspecies { get; set; }
    }
}
