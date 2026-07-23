using System;

namespace SmartBiodiversityUtnModels.DTOs.Especie
{
    /// <summary>
    /// Detalle botánico opcional de una especie de FLORA.
    /// Se usa tanto para recibir datos (crear/actualizar) como para exponerlos en el detalle.
    /// </summary>
    public class DetalleFloraDto
    {
        public decimal? AlturaPromedioM { get; set; }
        public decimal? AlturaMaximaM { get; set; }
        public decimal? DiametroTroncoCm { get; set; }
        public string? TipoCortezaTronco { get; set; }
        public string? FormaCopa { get; set; }
        public string? TipoHoja { get; set; }
        public string? ColorFlorFruto { get; set; }
        public string? HabitoCrecimiento { get; set; }
    }
}
