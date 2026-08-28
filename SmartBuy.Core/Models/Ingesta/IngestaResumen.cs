namespace SmartBuy.Core.Models.Ingesta
{
    /// <summary>Resumen que recibe el bot al terminar la ingesta.</summary>
    public class IngestaResumen
    {
        public long CapturaId { get; set; }

        public int ItemsProcesados { get; set; }

        /// <summary>Publicaciones que quedaron vinculadas a un producto del catálogo (EAN).</summary>
        public int PublicacionesMatcheadas { get; set; }

        /// <summary>Publicaciones sin matching todavía: alimentan la cola de revisión.</summary>
        public int PublicacionesPendientes { get; set; }
    }
}
