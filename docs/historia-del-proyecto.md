# SmartBuy — La historia del proyecto

> De la idea al sistema autónomo: qué es, cómo está construido, las etapas que recorrimos
> y lo que viene. Escrito el 03/09/2026, con el roadmap post-MVP completo.

## 1. La idea

Armás tu lista de compras y SmartBuy te dice **dónde comprar cada producto para gastar lo menos
posible**, comparando los precios reales de los supermercados argentinos. Los precios no los carga
nadie: **bots propios los capturan solos, dos veces por día**, de los sitios públicos de las cadenas.

De esa idea simple se desprendió todo lo demás: si capturás precios todos los días, acumulás un
**histórico** que nadie más tiene — y el histórico habilita saber si conviene comprar hoy, medir tu
inflación personal, y ver las tendencias de precios por rubro. La visión completa incluye llegar
hasta el carrito armado del súper con un click (hecho) y, a futuro, la compra automática.

**Cadenas cubiertas (7):** Carrefour, Coto, Día, Jumbo, Disco, Vea y ChangoMás.
**Categorías (5):** bebidas, almacén, lácteos, limpieza y perfumería.

## 2. Lo técnico

### Arquitectura

Tres piezas en un stack Docker que corre 24/7 (`deploy.bat` publica versiones nuevas):

| Pieza | Tecnología | Puerto |
|---|---|---|
| **SmartBuy.api** | Monolito .NET 9 — los bots viven adentro como BackgroundService | 5100 (docker) / 5080 (dev) |
| **SmartBuy.UI** | Angular 21 zoneless + signals, PWA instalable, servida por nginx | 4300 (docker) / 4200 (dev) |
| **PostgreSQL 17** | Base `SmartBuy`, volumen persistente | 5432 |

El backend espeja la arquitectura de Empleos 360: **Api / Core / Data** con controllers → services
→ repositories + interfaces, **Orion** como gateway de datos (catálogo de acciones + SQL en
archivos versionados) y `StandarResponse<T>` como contrato. El frontend es arquitectura por
features (`core/` + `features/`), SCSS propio con tokens, mobile-first, y la convención de los
4 estados obligatorios por pantalla (cargando / error / vacío / ok).

### Las decisiones de diseño que sostienen todo

- **Precios append-only, particionados por mes**: nunca se pisa un precio → el histórico es gratis
  y las consultas del "último precio" siguen rápidas a cualquier escala.
- **Catálogo maestro + matching por EAN**: el EAN es el ancla de identidad entre cadenas.
  Matching automático al ingerir, retro-matching al crear productos, cola manual para el resto,
  y generación masiva de catálogo desde pendientes (marcados `curado=false` para revisión).
- **Ingesta atómica por ítem** (un CTE: upsert de publicación + matching + precio) detrás de API key.
- **Ofertas computadas con honestidad**: solo promos por cantidad (3x2, "2do al 70%") se convierten
  a precio efectivo — los "% off" planos ya vienen aplicados en el precio publicado (computarlos
  sería descontar dos veces) y las promos de tarjeta dependen del medio de pago.
- **Precio por unidad** ($/L, $/kg) parseado del nombre publicado, para que el "más barato" no sea
  el envase más chico.
- **Lógica pura en `Core/Common`, siempre testeada**: OfertaCalculator, ContenidoParser,
  PasswordHasher, SenalCompra, InflacionCanasta, VentanaCaptura, CarritoLinkBuilder,
  IndiceCategoria — **119 tests** que corren en milisegundos sin base ni HTTP.
- **Seguridad**: JWT + PBKDF2 para usuarios (el usuarioId sale SIEMPRE del token — anti-IDOR
  reforzado en el propio SQL), API key con comparación de tiempo constante para la ingesta.
- **Bots configurables por appsettings**: cadena nueva o categoría nueva = un bloque de config,
  sin código. Horarios de captura en hora argentina (7:00 y 19:00), una captura OK por ventana,
  robusto a PC apagada y reinicios, con auto-saneo de capturas huérfanas.

## 3. Las etapas que recorrimos

### El MVP (agosto 2026)
1. Docker + PostgreSQL portable; modelo de datos del catálogo y DDL.
2. Backend .NET con Orion; ingesta segura con API key.
3. ABM de producto + cola de matching de publicaciones pendientes.
4. **ResolverLista**: la consulta estrella — reparto óptimo, mejor cadena única, ahorro medido
   (4-10% en listas reales), filtro de "mis cadenas".
5. Bots reales: VTEX (6 cadenas con la misma plataforma) y Coto (Constructor.io, con EAN y precio
   por sucursal). Diarco explorado y postergado (no publica precios web).
6. Retro-matching por EAN y generación masiva de catálogo (235 productos en un click).
7. Frontend Angular en 4 etapas: scaffold, lista + resultado, catálogo, matching.

### El roadmap post-MVP (elegido y completado en orden)
1. **Usuarios + listas guardadas + PWA** — cuentas con JWT, listas en el servidor, app instalable.
2. **Precio por unidad + ofertas computadas** — ContenidoParser (98,7% de cobertura retro) y
   OfertaCalculator; la recomendación de la Coca cambió de ganador cuando entró el "2do al 70%".
3. **El histórico como producto** — 3a: historia de precios por producto + señal "¿conviene
   comprar hoy?"; 3b: inflación personal de cada canasta guardada (variación solo entre días
   completos — honestidad estadística).
4. **Deep links al carrito** — un click arma el carrito real del súper con todos los productos y
   cantidades (verificado contra Carrefour; Cencosud exige sesión + sucursal previa).

### Los extras de esta semana
- **Dockerización completa + deploy con doble click** → el sistema es autónomo de verdad.
- **5 categorías por cadena** (eran 2) con slugs verificados sitio por sitio.
- **Panel de capturas** (`/capturas`): la bitácora de los bots sin abrir SQL.
- **SmartBuy.Tests**: la red de 119 tests sobre toda la lógica pura.
- **Horarios argentinos**: ventanas 7:00/19:00, tick de 15 minutos.
- **`categoria_captura`**: los bots etiquetan cada publicación con la categoría del propio súper,
  normalizada por config — la categorización gruesa gratis que destrabó las tendencias
  (y de paso Coto ganó la ruta Almacén que le faltaba).
- **Tendencias** (`/tendencias`): índice de precios encadenado base 100 por categoría, con
  canasta común día a día — un IPC personal por rubro, dos veces por día.

## 4. El estado hoy (03/09/2026)

- ~3.500 publicaciones, ~700 productos, ~15.000 precios históricos creciendo a ~6.400/día.
- 91% del catálogo etiquetado por categoría; capturas 7:00 y 19:00 sin intervención humana.
- Pantallas: lista + resultado con carritos, catálogo, matching, histórico por producto,
  inflación por lista, tendencias por rubro, panel de capturas.
- Crecimiento de la base: un no-problema (ver `crecimiento-base-datos.md`).

## 5. Lo que pensamos para el futuro

### Backlog de producto
- **Export a Excel/CSV** (histórico, inflación, resultado) para la versión desktop.
- **Promos afinadas**: aplicar el precio efectivo solo si la cantidad de la lista alcanza la promo
  (cantidad ≥ N), y revisar las diferencias contra cómo arma el carrito cada súper.
- **Equivalencias/sustitutos**: comparar "soda 2L de cualquier marca" — marcas propias que solo
  existen en un súper; caso ideal para curación asistida por LLM.
- **Precio de la conveniencia**: cuánto pagás de más por comprar solo donde te queda cómodo.
- **Intra-Cencosud**: Jumbo, Disco y Vea son del mismo dueño y cobran distinto el mismo EAN.
- **Carrito de Coto** (sin deep link público conocido) y **Diarco** vía app/folletos.
- **Curación con LLM/Flowise**: completar marca/categoría/contenido de los `curado=false`.

### Backlog técnico (pre-producción)
- Secretos por variables de entorno + password real (hoy mínimo 6, decisión de MVP).
- Migraciones de base versionadas; healthcheck de la API en compose.
- Rate limiting y CORS estrictos; Swagger apagado en producción.
- Particiones de `precio` posteriores a 2027-12; backups; ingesta por lote si la cobertura crece 10×.
- CI/CD con GitHub Actions; VPS cuando salga de la PC.

### Monetización (análisis del 03/09)
La tesis: SmartBuy son **dos productos** — la app (B2C freemium: alertas de baja, histórico
ilimitado, Excel) y el **dataset** (B2B: dashboards de pricing para marcas, API de precios por EAN,
el índice de tendencias como IPC de alta frecuencia). El dataset probablemente vale más, y cada día
de captura agranda una barrera de entrada que ningún competidor nuevo puede recuperar. Camino en
4 etapas: validar gratis → el índice como marketing → primer cliente B2B → premium B2C.
Prerequisito innegociable antes de cobrar: consulta legal sobre el uso comercial de los datos.

---
*El circuito de la visión original está cerrado: los bots capturan solos → el catálogo se matchea →
la lista se resuelve al mejor precio real → mirás si conviene comprar hoy y cuánto subió tu
canasta → y un click te deja el carrito armado. De acá en adelante, todo es profundizar.*
