using AulaIA.Api.Shared.Domain;

namespace AulaIA.Api.Shared.Persistence;

/// <summary>
/// GUIDs fijos y datos maestros para el seed inicial de la base de datos.
/// Los GUIDs son deterministas para que las migraciones sean reproducibles.
/// Fuente: Directorio de Oficinas MEP (mep.go.cr/oficinas) — mayo 2026.
/// </summary>
public static class SeedData
{
    public static readonly DateTime CreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
        public const string EzequielAuth0Sub = "auth0|69fae47c268da9d7e46c6d4b";
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

    public static class DemoPlaneamiento
    {
        public static readonly Guid GrupoArtesPlasticas7_1 = new("cc000002-0000-0000-0000-000000000001");

        public static Group GrupoArtesPlasticas => new()
        {
            Id = GrupoArtesPlasticas7_1,
            Name = "7-1 Artes Plásticas",
            Level = "7° Año",
            Subject = "Artes Plásticas",
            SchoolYear = 2026,
            TeacherId = Users.Ezequiel,
            TeacherSub = Users.EzequielAuth0Sub,
            InstitutionId = Institutions.LiceoAserri,
            IsActive = true,
            CreatedAt = CreatedAt,
            PctCotidiano = 20m,
            PctPruebas = 45m,
            PctExtraclase = 20m,
            PctOtros = 15m
        };

        public static Student[] EstudiantesArtesPlasticas7_1 => CreateStudents();

        public static CalendarEvent[] EventosArtesPlasticas7_1 =>
        [
            CreateEvent(1, new DateOnly(2026, 2, 23), null, "Acto cívico institucional", CalendarEventType.Civic),
            CreateEvent(2, new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 20), "Semana de exámenes I Trimestre", CalendarEventType.Exam),
            CreateEvent(3, new DateOnly(2026, 3, 27), null, "Consejo de profesores", CalendarEventType.TeacherMeeting),
            CreateEvent(4, new DateOnly(2026, 4, 13), new DateOnly(2026, 4, 17), "FEA institucional", CalendarEventType.SportWeek),
            CreateEvent(5, new DateOnly(2026, 4, 24), null, "Capacitación institucional", CalendarEventType.Institutional)
        ];

        private static Student[] CreateStudents()
        {
            string[] names =
            [
                "Valeria Arias Montero",
                "Sebastián Barrantes Solís",
                "María José Brenes Castro",
                "Diego Calderón Vargas",
                "Camila Campos Rojas",
                "Andrés Cordero Jiménez",
                "Sofía Delgado Mora",
                "Daniela Díaz Hernández",
                "Samuel Fernández Chaves",
                "Lucía Flores Quesada",
                "Mateo García López",
                "Isabella Gómez Arce",
                "Emiliano Gutiérrez Salazar",
                "Natalia Herrera Ulate",
                "Gabriel Jiménez Rodríguez",
                "Paula León Sánchez",
                "Julián López Núñez",
                "Mariana Martínez Vega",
                "Esteban Méndez Castro",
                "Ana Lucía Molina Rojas",
                "José Pablo Mora Alfaro",
                "Daniel Navarro Céspedes",
                "Fernanda Núñez Chacón",
                "Adrián Pacheco Soto",
                "Nicole Pérez Bonilla",
                "Luis Diego Quesada Ramírez",
                "Laura Ramírez Fallas",
                "Fabián Rojas Hidalgo",
                "Daniela Salazar Picado",
                "Christopher Sánchez Vargas",
                "Michelle Segura Campos",
                "Kevin Solano Aguilar",
                "Ariana Soto Morales",
                "Brandon Torres Rivas",
                "Melissa Ureña Castro",
                "Anthony Valverde Mora",
                "Karla Vargas Esquivel",
                "Jeremy Vega Delgado",
                "Tatiana Villalobos Cordero",
                "Santiago Zamora Ruiz"
            ];

            return names.Select((name, index) =>
            {
                var number = index + 1;
                var id = StudentId(number);
                return new Student
                {
                    Id = id,
                    FullName = name,
                    StudentCode = $"AP7-2026-{number:000}",
                    QrCode = id.ToString("N"),
                    GroupId = GrupoArtesPlasticas7_1,
                    IsActive = true,
                    CreatedAt = CreatedAt
                };
            }).ToArray();
        }

        private static Guid StudentId(int number) =>
            Guid.Parse($"dd000002-0000-0000-0000-{number:000000000000}");

        private static CalendarEvent CreateEvent(
            int number,
            DateOnly date,
            DateOnly? endDate,
            string title,
            CalendarEventType type) => new()
            {
                Id = Guid.Parse($"ff000002-0000-0000-0000-{number:000000000000}"),
                GroupId = GrupoArtesPlasticas7_1,
                Date = date,
                EndDate = endDate,
                Title = title,
                Type = type,
                SchoolYear = 2026,
                CreatedByAuth0Sub = Users.EzequielAuth0Sub,
                CreatedAt = CreatedAt
            };
    }
}
