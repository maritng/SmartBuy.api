using SmartBuy.Core.Common;
using Xunit;

namespace SmartBuy.Tests
{
    /// <summary>
    /// La ventana vigente es el último horario programado (hora argentina,
    /// UTC-3 fijo) que ya pasó — puede ser de ayer. El orquestador captura una
    /// vez por ventana, así que este cálculo define CUÁNDO corren los bots.
    /// </summary>
    public class VentanaCapturaTests
    {
        private static readonly int[] MananaYNoche = { 7, 19 };

        /// <summary>Construye un instante en hora argentina y lo pasa a UTC.</summary>
        private static DateTimeOffset ArtUtc(int dia, int hora, int minuto = 0)
            => new DateTimeOffset(2026, 9, dia, hora, minuto, 0, VentanaCaptura.OffsetArgentina).ToUniversalTime();

        private static DateTimeOffset InicioArt(DateTimeOffset resultadoUtc)
            => resultadoUtc.ToOffset(VentanaCaptura.OffsetArgentina);

        [Fact]
        public void A_media_manana_rige_la_ventana_de_las_7()
        {
            var inicio = InicioArt(VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 10), MananaYNoche));

            Assert.Equal(new DateTimeOffset(2026, 9, 3, 7, 0, 0, VentanaCaptura.OffsetArgentina), inicio);
        }

        [Fact]
        public void A_la_noche_rige_la_ventana_de_las_19()
        {
            var inicio = InicioArt(VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 20, 30), MananaYNoche));

            Assert.Equal(new DateTimeOffset(2026, 9, 3, 19, 0, 0, VentanaCaptura.OffsetArgentina), inicio);
        }

        [Fact]
        public void De_madrugada_sigue_vigente_la_ventana_de_ayer_a_las_19()
        {
            var inicio = InicioArt(VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 3), MananaYNoche));

            Assert.Equal(new DateTimeOffset(2026, 9, 2, 19, 0, 0, VentanaCaptura.OffsetArgentina), inicio);
        }

        [Fact]
        public void Justo_a_la_hora_programada_abre_la_ventana_nueva()
        {
            var inicio = InicioArt(VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 7), MananaYNoche));

            Assert.Equal(new DateTimeOffset(2026, 9, 3, 7, 0, 0, VentanaCaptura.OffsetArgentina), inicio);
        }

        [Fact]
        public void Un_minuto_antes_de_las_7_todavia_rige_ayer_19()
        {
            var inicio = InicioArt(VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 6, 59), MananaYNoche));

            Assert.Equal(new DateTimeOffset(2026, 9, 2, 19, 0, 0, VentanaCaptura.OffsetArgentina), inicio);
        }

        [Fact]
        public void Con_un_solo_horario_la_ventana_dura_todo_el_dia()
        {
            var inicio = InicioArt(VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 23, 59), new[] { 7 }));

            Assert.Equal(new DateTimeOffset(2026, 9, 3, 7, 0, 0, VentanaCaptura.OffsetArgentina), inicio);
        }

        [Fact]
        public void Horarios_desordenados_o_repetidos_no_cambian_el_resultado()
        {
            var esperado = InicioArt(VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 10), MananaYNoche));
            var desordenado = InicioArt(VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 10), new[] { 19, 7, 19, 7 }));

            Assert.Equal(esperado, desordenado);
        }

        [Fact]
        public void Sin_horarios_validos_rige_el_default_de_las_7()
        {
            var vacio = InicioArt(VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 10), Array.Empty<int>()));
            var nulo = InicioArt(VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 10), null));
            var invalidos = InicioArt(VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 10), new[] { -1, 24, 99 }));

            var esperado = new DateTimeOffset(2026, 9, 3, 7, 0, 0, VentanaCaptura.OffsetArgentina);
            Assert.Equal(esperado, vacio);
            Assert.Equal(esperado, nulo);
            Assert.Equal(esperado, invalidos);
        }

        [Fact]
        public void El_resultado_viene_en_utc_con_el_offset_correcto()
        {
            // 10:00 ART = 13:00 UTC; la ventana de las 7 ART = 10:00 UTC.
            var inicioUtc = VentanaCaptura.InicioVentanaActualUtc(ArtUtc(3, 10), MananaYNoche);

            Assert.Equal(TimeSpan.Zero, inicioUtc.Offset);
            Assert.Equal(new DateTime(2026, 9, 3, 10, 0, 0), inicioUtc.DateTime);
        }
    }
}
