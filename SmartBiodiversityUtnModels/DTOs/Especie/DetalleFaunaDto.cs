using System;

namespace SmartBiodiversityUtnModels.DTOs.Especie
{
    /// <summary>
    /// Detalle zoológico opcional de una especie de FAUNA.
    /// Se usa tanto para recibir datos (crear/actualizar) como para exponerlos en el detalle.
    /// </summary>
    public class DetalleFaunaDto
    {
        public decimal? LongitudPromedioCm { get; set; }
        public decimal? EnvergaduraCm { get; set; }
        public decimal? PesoPromedioGramos { get; set; }
        public string? TipoPelajePlumaje { get; set; }
        public string? DimorfismoSexual { get; set; }
        public string? Dieta { get; set; }
        public string? PatronActividad { get; set; }
    }
}
