namespace SmartBuy.Core.Common
{
    /// <summary>
    /// Arma deep links de carrito. VTEX expone una URL pública que carga varios
    /// productos de una vez: /checkout/cart/add?sku=A&amp;qty=2&amp;seller=1&amp;sku=B...
    /// El SKU es nuestro codigo_externo. Si no hay ítems válidos devuelve null:
    /// mejor sin botón que con un carrito roto.
    /// </summary>
    public static class CarritoLinkBuilder
    {
        public static string? ArmarVtex(string? baseUrl, IReadOnlyCollection<(string Sku, int Cantidad)> items)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return null;

            var validos = items
                .Where(i => !string.IsNullOrWhiteSpace(i.Sku) && i.Cantidad > 0)
                .ToList();

            if (validos.Count == 0)
                return null;

            var partes = validos.Select(i => $"sku={Uri.EscapeDataString(i.Sku)}&qty={i.Cantidad}&seller=1");

            return $"{baseUrl.TrimEnd('/')}/checkout/cart/add?{string.Join("&", partes)}";
        }
    }
}
