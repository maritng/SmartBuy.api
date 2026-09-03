# SmartBuy — Crecimiento de la base de datos y proyección

> Medición real del 03/09/2026 (día 4 de captura autónoma) y proyección a 2 meses y a un año.
> Contexto: 7 cadenas, 5-6 categorías por cadena, 2 corridas diarias (7:00 y 19:00 hora argentina).

## Situación al 03/09/2026

| Métrica | Valor |
|---|---|
| Base completa | **13 MB** (≈8 MB son overhead fijo de PostgreSQL) |
| `precio` (la tabla que crece) | 14.969 filas ≈ 3 MB con índices → **~200 bytes/fila** |
| `publicacion` | 3.508 filas ≈ 1,5 MB (crece lento: upsert por cadena+codigo_externo, solo entra surtido nuevo) |
| `captura` | ~14 filas/día — despreciable |
| Ritmo de `precio` | ~3.000/día con 1 corrida → **~6.400/día** desde las 2 ventanas (2 × ~3.200 ítems) |

Filas de `precio` por día (histórico completo):

| Fecha | Filas |
|---|---|
| 31/08 | 2.223 |
| 01/09 | 3.016 |
| 02/09 | 3.007 |
| 03/09 | 6.721 (primer día con 2 ventanas) |

## Proyección a 2 meses (03/11/2026)

- **`precio`**: 6.400 filas/día × 60 días = **~384.000 filas nuevas** ≈ 77 MB.
  Total acumulado: **~400.000 filas, ~80 MB**.
- **`publicacion`**: rotación de surtido, +2.000-3.000 publicaciones ≈ +1,5 MB.
- **`captura`**: ~850 filas — despreciable.
- **Total de la base: ~95-100 MB.**

## Proyección a 1 año

~2,3 millones de filas de `precio` ≈ **medio GB**. Terreno trivial para PostgreSQL; el
**particionado mensual** de `precio` (diseñado desde el día 1) mantiene rápidas las consultas
de "último precio" para siempre: solo tocan la partición del mes vigente.

## Escenario B2B (si se amplía cobertura)

Con categorías completas (~5× más páginas por cadena): ~32.000 precios/día ≈ **6-7 GB/año**.
Sigue siendo cómodo para un VPS chico con disco decente. Lo que cambia en ese escenario:
los backups dejan de ser opcionales y conviene pasar la ingesta a lotes.

## Veredicto

**El crecimiento es un no-problema a la escala actual.** Si algún día la base come mucho,
va a ser porque la cobertura (y el negocio) lo justifica.

## Recordatorios asociados al crecimiento

1. **Particiones de `precio` creadas hasta 2027-12** (+ una default que ataja lo demás).
   Antes de esa fecha: script o job anual que cree las particiones del año siguiente.
   Es el clásico problema que explota (suave) dos años después de olvidado.
2. **Backups de PostgreSQL** cuando la base valga plata — hoy el volumen Docker
   (`smartbuy-pgdata`) es la única copia.
3. **Ingesta por lote** (hoy es 1 llamada a la base por ítem, ~500 por captura): recién
   dolería con cobertura ~10×; el rediseño con `unnest` de arrays está anotado en el backlog.

---
*Generado el 03/09/2026 a partir de mediciones reales (`pg_database_size`,
`pg_total_relation_size`, conteos por fecha) sobre la base viva.*
