---
name: smartbuy-equivalencias
description: "Idea futura para SmartBuy pedida por el usuario (01/09/2026): productos equivalentes/sustitutos para comparar más allá del EAN idéntico."
metadata: 
  node_type: memory
  type: project
  originSessionId: efb1dcac-1782-40ac-88f0-439166f64119
  modified: 2026-08-31T22:46:29.979Z
---

Idea post-MVP para [[proyecto-smartbuy]], surgida al analizar la cola de matching: los ~374 pendientes de una sola cadena son mayormente **marcas propias** (Carrefour Classic, Cuisine & Co, etc.) — productos que genuinamente existen en un solo súper, sin correlación automática posible por EAN (EANs distintos = productos distintos; no es limitación del sistema, es la realidad del retail; ojo además con los códigos que empiezan en 28 de Coto: internos de balanza, no EANs globales).

**La feature "equivalencias":** hoy la recomendación compara productos *idénticos* (mismo EAN). La evolución es comparar *sustitutos*: "quiero soda sifón 2L, no me importa la marca" → la Soda Carrefour Classic compite contra la Cuisine & Co de Cencosud. Requiere:
- Concepto nuevo en el modelo (ej. `producto_equivalencia` o "producto genérico" que agrupa sustitutos).
- Decisión de producto: ¿la recomendación compara lo idéntico, lo equivalente, o ambos con un toggle del usuario?
- Es el caso de uso ideal para el LLM/Flowise: proponer grupos de sustitutos ("estas 5 sodas de marca propia son equivalentes") con revisión humana, igual que el matching.

**Mientras tanto ya operativo:** GenerarDesdePendientes?minCadenas=1 incorpora los single-chain como productos de una cadena (histórico útil, sin comparación), y la cola de matching permite curar/descartar a mano.

Relacionada con [[smartbuy-precio-conveniencia]] (el otro pendiente grande del backlog de ideas).
