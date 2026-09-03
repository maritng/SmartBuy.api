---
name: smartbuy-precio-conveniencia
description: "Idea futura para SmartBuy que le interesó al usuario (31/08/2026): medir el \"precio de la conveniencia\" y comparar precios dentro del mismo grupo empresario."
metadata: 
  node_type: memory
  type: project
  originSessionId: efb1dcac-1782-40ac-88f0-439166f64119
  modified: 2026-08-31T12:06:51.953Z
---

Idea a encarar más adelante en [[proyecto-smartbuy]], surgida al probar el filtro `cadenasIds` de ResolverLista. El usuario pidió explícitamente guardarla.

**1. El "precio de la conveniencia":** correr la misma lista sin filtro (óptimo teórico) y con las cadenas accesibles del usuario, y mostrar la diferencia `totalFiltrado - totalOptimo`: cuánto pagás de más por comprar solo donde te queda cómodo. Hoy se puede calcular a mano con dos requests; la feature sería que la app lo devuelva/muestre sola (posible campo extra en la respuesta o pantalla comparativa del FE). Extensión natural: gráfico/histórico de ese costo.

**2. Comparación intra-grupo Cencosud:** Jumbo, Disco y Vea (cadenas 5, 6 y 7) son del mismo grupo y comparten plataforma VTEX, pero pueden cobrar distinto el mismo producto. Detectar y visibilizar esas diferencias (mismo EAN, distinto precio, mismo dueño) es un análisis diferencial que la base ya soporta con precio_vigente + publicaciones matcheadas.

**How to apply:** cuando el usuario retome "lo del precio de la conveniencia" o "lo de Cencosud", esto es lo que quiso decir; proponer el diseño (campo en totales vs. endpoint comparativo) antes de codear, como siempre.
