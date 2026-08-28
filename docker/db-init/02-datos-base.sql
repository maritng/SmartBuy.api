-- ============================================================================
-- SmartBuy - Datos base: cadenas a capturar y árbol inicial de categorías.
--
-- Idempotente: se puede re-ejecutar sin duplicar (on conflict do nothing).
-- Las cadenas son las grandes con tienda online en Argentina; se agregan o
-- quitan según qué bots se implementen.
-- El árbol de categorías es deliberadamente chico: crece con el catálogo,
-- no hace falta anticipar todos los rubros.
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Cadenas
-- ----------------------------------------------------------------------------
insert into cadena (nombre, sitio_web) values
    ('Carrefour', 'https://www.carrefour.com.ar'),
    ('Coto',      'https://www.cotodigital.com.ar'),
    ('Día',      'https://diaonline.supermercadosdia.com.ar'),
    ('Jumbo',     'https://www.jumbo.com.ar'),
    ('Disco',     'https://www.disco.com.ar'),
    ('Vea',       'https://www.vea.com.ar'),
    ('ChangoMás', 'https://www.masonline.com.ar')
on conflict (nombre) do nothing;

-- ----------------------------------------------------------------------------
-- Categorías raíz (rubros)
-- ----------------------------------------------------------------------------
insert into categoria (nombre, padre_id) values
    ('Almacén',    null),
    ('Bebidas',    null),
    ('Lácteos',   null),
    ('Frescos',    null),
    ('Congelados', null),
    ('Limpieza',   null),
    ('Perfumería', null)
on conflict (nombre, padre_id) do nothing;

-- ----------------------------------------------------------------------------
-- Subcategorías
-- ----------------------------------------------------------------------------
with raiz as (
    select id, nombre from categoria where padre_id is null
)
insert into categoria (nombre, padre_id)
select sub.nombre, raiz.id
from (values
    -- Almacén
    ('Aceites y vinagres',      'Almacén'),
    ('Arroz y legumbres',       'Almacén'),
    ('Fideos y pastas secas',   'Almacén'),
    ('Harinas y repostería',    'Almacén'),
    ('Conservas',               'Almacén'),
    ('Yerba, té y café',        'Almacén'),
    ('Azúcar y endulzantes',    'Almacén'),
    ('Galletitas y snacks',     'Almacén'),
    ('Desayuno y untables',     'Almacén'),
    -- Bebidas
    ('Gaseosas',                'Bebidas'),
    ('Aguas y saborizadas',     'Bebidas'),
    ('Jugos e isotónicas',      'Bebidas'),
    ('Cervezas',                'Bebidas'),
    ('Vinos y espumantes',      'Bebidas'),
    -- Lácteos
    ('Leches',                  'Lácteos'),
    ('Yogures y postres',       'Lácteos'),
    ('Quesos',                  'Lácteos'),
    ('Manteca y crema',         'Lácteos'),
    -- Frescos
    ('Carnes y pollo',          'Frescos'),
    ('Frutas y verduras',       'Frescos'),
    ('Fiambres',                'Frescos'),
    ('Panadería',               'Frescos'),
    ('Huevos',                  'Frescos'),
    -- Congelados
    ('Hamburguesas y rebozados','Congelados'),
    ('Vegetales congelados',    'Congelados'),
    ('Helados',                 'Congelados'),
    -- Limpieza
    ('Lavandinas y desinfectantes', 'Limpieza'),
    ('Detergentes y lavavajillas',  'Limpieza'),
    ('Cuidado de la ropa',      'Limpieza'),
    ('Papeles y descartables',  'Limpieza'),
    -- Perfumería
    ('Higiene personal',        'Perfumería'),
    ('Cuidado del cabello',     'Perfumería'),
    ('Cuidado bucal',           'Perfumería')
) as sub (nombre, padre)
join raiz on raiz.nombre = sub.padre
on conflict (nombre, padre_id) do nothing;
