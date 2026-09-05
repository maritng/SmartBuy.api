namespace SmartBuy.Core.Common
{
    /// <summary>
    /// Sanea el par (precio de lista, precio de venta) que publican los sitios.
    /// Origen: algunas capturas VTEX de Cencosud traen ListPrice corrupto con un
    /// factor ~×82 sobre el precio real (detectado 06/09/2026), mientras que la
    /// promo legítima más agresiva (~70% off) da lista ≈ 3× venta. Regla: si la
    /// lista supera FactorMaximo × venta, el ListPrice se considera inválido y
    /// el precio de venta pasa a ser la lista (sin oferta: no hay descuento real
    /// que mostrar). Sin precio de venta no hay referencia para juzgar y el par
    /// pasa intacto. También centraliza la regla de oferta (venta < lista).
    /// </summary>
    public static class SaneadorPrecios
    {
        public const decimal FactorMaximo = 10m;

        public static ParSaneado Sanear(decimal precioLista, decimal? precioVenta)
        {
            if (precioVenta is null or <= 0)
                return new ParSaneado { PrecioLista = precioLista };

            if (precioLista > precioVenta.Value * FactorMaximo)
                return new ParSaneado { PrecioLista = precioVenta.Value, Saneado = true };

            return new ParSaneado
            {
                PrecioLista = precioLista,
                PrecioOferta = precioVenta < precioLista ? precioVenta : null
            };
        }
    }

    /// <summary>El par lista/oferta listo para la ingesta, con la marca de saneo.</summary>
    public sealed class ParSaneado
    {
        public decimal PrecioLista { get; init; }

        public decimal? PrecioOferta { get; init; }

        /// <summary>true si el ListPrice original era inválido y fue reemplazado.</summary>
        public bool Saneado { get; init; }
    }
}
