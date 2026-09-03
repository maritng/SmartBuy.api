# SmartBuy — Artifacts y skills de Claude

> Los recursos de Claude Code creados y usados en este proyecto: los artifacts publicados
> (documentos vivos, privados salvo que los compartas) y los skills que guían cómo se trabaja.
> Actualizado el 03/09/2026.

## Artifacts del proyecto

Los artifacts son páginas publicadas desde las sesiones de Claude. Se acceden desde estos links,
con `/artifacts` en la terminal de Claude Code, o en la galería web (claude.ai/code/artifacts).
Son privados hasta que se compartan desde el menú de la propia página.

| Artifact | Qué es | Link |
|---|---|---|
| 🛒 **SmartBuy** | La visión del producto: la idea macro, el roadmap post-MVP y su evolución. Fue el primer artifact del proyecto y se actualizó a medida que el roadmap avanzaba. | https://claude.ai/code/artifact/34072c2c-53b4-4f0b-b214-214047e3157e |
| ⚙️ **SmartBuy por dentro** | La guía del código: backend capa por capa (Orion, flujos de captura/ofertas/recomendación), frontend, Docker, y el review de mejoras priorizado que originó los tests y varios pendientes del backlog. | https://claude.ai/code/artifact/509ceb90-da87-4ce5-9707-986fb2dba870 |
| 💰 **Monetizar SmartBuy** | El análisis de negocio: tesis B2C/B2B (la app vs. el dataset), qué falta antes de cobrar, y el camino en 4 etapas. | https://claude.ai/code/artifact/de3b83cb-f6b6-47fe-953d-abc5718eb7ef |

> Nota: la cuenta tiene más artifacts de otros proyectos (Empleos 360, etc.); acá se listan solo
> los de SmartBuy.

## Skills usados en el proyecto

Los skills son instrucciones empaquetadas que viven en `C:\Users\margrandi\.claude\skills\` y que
Claude invoca según la tarea (las reglas de cuándo usar cada uno están en el `CLAUDE.md` global).

| Skill | Para qué sirve | Cómo se usó en SmartBuy |
|---|---|---|
| **token-efficient-engineering** | Trabajar con el mínimo contexto necesario: lecturas puntuales, cambios chicos, verificación dirigida. Se invoca antes de analizar, debuggear, implementar o revisar. | El modo de trabajo por defecto de todo el proyecto: leer solo los archivos a tocar, verificar con el test/curl más chico posible. |
| **dotnet-orion-backend** | Backend .NET con arquitectura Orion: endpoints, services, repositories, DTOs, validación, SQL, patrones seguros. | Toda la API: la estructura Api/Core/Data, el catálogo de acciones Orion, los patrones de StandarResponse y los CTE de ingesta. |
| **angular21-ux-ui-expert** | Frontend Angular 21 con criterio UX/UI: standalone, signals, rutas lazy, features, estados de pantalla, accesibilidad, responsive. | Todo el front: la arquitectura core/features, la convención de 4 estados por pantalla, los stores con signals y el mobile-first. |
| **secure-software-review** | Revisión de seguridad antes de tocar código sensible: auth, autorización, datos personales, configuración. | Los diseños de auth (PBKDF2, JWT, anti-IDOR por claims y en SQL), la API key de ingesta y el manejo de secretos. |
| **greenfield-dotnet-angular-orion** | Crear proyectos nuevos desde cero con este stack. | El arranque del proyecto: solución .NET, scaffold Angular, decisiones de estructura iniciales. |
| **existing-project-modernization** | Migraciones y modernización de proyectos existentes con riesgo mínimo. | Disponible para futuras actualizaciones de Angular/.NET; no fue necesario todavía. |

## La memoria del proyecto

Además de artifacts y skills, Claude mantiene memoria persistente del proyecto entre sesiones
(decisiones, reglas de trabajo, backlog, lecciones aprendidas) en:

- `C:\Users\margrandi\.claude\projects\C--Users-margrandi-Desktop-Proyectos-SmartBuy\memory\`
  (memorias nuevas: ideas de categorías/Excel, monetización)
- `C:\Users\margrandi\.claude\projects\C--Users-margrandi-Desktop-Proyectos-Empleos\memory\proyecto-smartbuy.md`
  (el histórico largo del proyecto, porque las primeras sesiones corrieron desde esa carpeta)

Reglas de trabajo acordadas que viven ahí: explicar cada paso antes de codear, pedir autorización
por archivo, los commits los hace el usuario (Claude sugiere mensaje y momento), y todo deploy va
al stack Docker para verlo funcionando.

---
*Los tres artifacts + los dos docs de esta carpeta (`historia-del-proyecto.md`,
`crecimiento-base-datos.md`) forman la documentación completa del proyecto a la fecha.*
