using Orion.Domain.Attributes;

namespace SmartBuy.Data.OrionCatalog
{
    // Catálogo Orion del dominio "SmartBuy".
    // Las acciones están divididas por subdominio en archivos parciales:
    //   SmartBuyOrionCatalog.Cadena.cs
    //   SmartBuyOrionCatalog.Ingesta.cs
    //   SmartBuyOrionCatalog.Producto.cs
    //   SmartBuyOrionCatalog.Publicacion.cs
    //   SmartBuyOrionCatalog.Recomendacion.cs
    //   SmartBuyOrionCatalog.Usuario.cs
    //   SmartBuyOrionCatalog.Lista.cs
    [OrionCatalog("SmartBuy")]
    public sealed partial class SmartBuyOrionCatalog
    {
    }
}
