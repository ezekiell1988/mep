namespace AulaIA.Api.Shared.Persistence;

/// <summary>
/// GUIDs fijos y datos maestros para el seed inicial de la base de datos.
/// Los GUIDs son deterministas para que las migraciones sean reproducibles.
/// Fuente: Directorio de Oficinas MEP (mep.go.cr/oficinas) — mayo 2026.
/// </summary>
public static class SeedData
{
    // ── Códigos de Dirección Regional (DRE) ──────────────────────────────
    // Los 27 DREs oficiales del MEP de Costa Rica
    public static class RegionCodes
    {
        public const string Aguirre         = "AGU";
        public const string Alajuela        = "ALA";
        public const string Canas           = "CAN";
        public const string Cartago         = "CAR";
        public const string Coto            = "COT";
        public const string Desamparados    = "DES"; // ← zona beachhead (Aserrí)
        public const string GrandeTerraba   = "GTE";
        public const string Guapiles        = "GUA";
        public const string Heredia         = "HER";
        public const string Liberia         = "LIB";
        public const string Limon           = "LIM";
        public const string Nicoya          = "NIC";
        public const string NorteNorte      = "NNO";
        public const string Occidente       = "OCC";
        public const string Palmares        = "PAL";
        public const string Peninsular      = "PEN";
        public const string PerezZeledon    = "PZE";
        public const string Puntarenas      = "PUN";
        public const string Puriscal        = "PUR";
        public const string SanCarlos       = "SCA";
        public const string SanJoseCentral  = "SJC";
        public const string SanJoseNorte    = "SJN";
        public const string SanJoseOeste    = "SJO";
        public const string SantaCruz       = "SCR";
        public const string Sarapiqui       = "SAR";
        public const string Turrialba       = "TUR";
        public const string Tunialba        = "TUN";
    }

    // ── Usuarios semilla ────────────────────────────────────────────────
    public static class Users
    {
        /// <summary>Ezequiel Baltodano — Admin de plataforma</summary>
        public static readonly Guid Ezequiel = new("bb000001-0000-0000-0000-000000000001");

        /// <summary>Adriana Guido — Docente, Liceo de Aserrí, Artes Plásticas</summary>
        public static readonly Guid Adriana  = new("bb000001-0000-0000-0000-000000000002");
    }

    // ── Instituciones (colegios públicos MEP) ────────────────────────────
    public static class Institutions
    {
        // ── DRE Desamparados — Circuito 06 (zona beachhead: Aserrí) ──────
        /// <summary>Liceo de Aserrí — colegio público diurno de Aserrí, San José.</summary>
        public static readonly Guid LiceoAserri = new("aa000001-0000-0000-0000-000000000001");

        /// <summary>Colegio Técnico Profesional de Aserrí</summary>
        public static readonly Guid CtpAserri = new("aa000001-0000-0000-0000-000000000002");

        /// <summary>Colegio Nocturno de Aserrí</summary>
        public static readonly Guid NocturnoAserri = new("aa000001-0000-0000-0000-000000000003");

        /// <summary>Liceo San Gabriel — Aserrí, San José</summary>
        public static readonly Guid LiceoSanGabriel = new("aa000001-0000-0000-0000-000000000004");

        // ── DRE Desamparados — Circuito 01-05 (expansión cercana) ────────
        /// <summary>Colegio Técnico Profesional de Desamparados</summary>
        public static readonly Guid CtpDesamparados = new("aa000001-0000-0000-0000-000000000005");

        /// <summary>Liceo de Desamparados</summary>
        public static readonly Guid LiceoDesamparados = new("aa000001-0000-0000-0000-000000000006");

        /// <summary>Liceo Ing. Manuel Benavides — San José, DRE Desamparados</summary>
        public static readonly Guid LiceoManuelBenavides = new("aa000001-0000-0000-0000-000000000007");

        // ── DRE San José Central (expansión Fase 2) ───────────────────────
        /// <summary>Liceo de Costa Rica — San José; uno de los colegios más antiguos del país (1888)</summary>
        public static readonly Guid LiceoCostaRica = new("aa000001-0000-0000-0000-000000000008");

        /// <summary>Colegio de Señoritas — San José, DRE San José Central</summary>
        public static readonly Guid ColegioSenoritas = new("aa000001-0000-0000-0000-000000000009");

        /// <summary>Liceo Julio Fonseca — San José, DRE San José Central</summary>
        public static readonly Guid LiceoJulioFonseca = new("aa000001-0000-0000-0000-000000000010");

        /// <summary>Liceo José Joaquín Vargas Calvo — San José, DRE San José Central</summary>
        public static readonly Guid LiceoVargasCalvo = new("aa000001-0000-0000-0000-000000000011");

        // ── DRE San José Norte (expansión) ────────────────────────────────
        /// <summary>Liceo de Guadalupe — Goicoechea, San José Norte</summary>
        public static readonly Guid LiceoGuadalupe = new("aa000001-0000-0000-0000-000000000012");

        /// <summary>Colegio Técnico Profesional de Moravia</summary>
        public static readonly Guid CtpMoravia = new("aa000001-0000-0000-0000-000000000013");

        // ── DRE Heredia ───────────────────────────────────────────────────
        /// <summary>Liceo de Heredia — sede de la DRE Heredia</summary>
        public static readonly Guid LiceoHeredia = new("aa000001-0000-0000-0000-000000000014");

        /// <summary>Liceo Daniel Oduber Quirós — Heredia</summary>
        public static readonly Guid LiceoDanielOduber = new("aa000001-0000-0000-0000-000000000015");

        // ── DRE Cartago ───────────────────────────────────────────────────
        /// <summary>Liceo de Cartago — Cartago centro</summary>
        public static readonly Guid LiceoCartago = new("aa000001-0000-0000-0000-000000000016");

        /// <summary>Colegio Técnico Profesional de Cartago</summary>
        public static readonly Guid CtpCartago = new("aa000001-0000-0000-0000-000000000017");

        // ── DRE Alajuela ──────────────────────────────────────────────────
        /// <summary>Liceo de Alajuela — Alajuela centro</summary>
        public static readonly Guid LiceoAlajuela = new("aa000001-0000-0000-0000-000000000018");

        /// <summary>Colegio Técnico Profesional Jesús Ocaña Rojas — Alajuela</summary>
        public static readonly Guid CtpAlajuela = new("aa000001-0000-0000-0000-000000000019");

        // ── DRE San José Oeste ────────────────────────────────────────────
        /// <summary>Liceo de Escazú — San José Oeste</summary>
        public static readonly Guid LiceoEscazu = new("aa000001-0000-0000-0000-000000000020");
    }
}
