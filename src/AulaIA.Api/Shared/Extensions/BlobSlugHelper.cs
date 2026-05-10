using System.Globalization;
using System.Text;

namespace AulaIA.Api.Shared.Extensions;

public static class BlobSlugHelper
{
    /// <summary>
    /// Convierte un nombre de asignatura en un slug ASCII seguro para blob names.
    /// Ej: "Educación Física" → "educacion-fisica"
    /// Ej: "Artes Plásticas"  → "artes-plasticas"
    /// </summary>
    public static string ToAsciiSlug(string value)
    {
        var normalized = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;   // eliminar diacríticos (tildes, etc.)
            sb.Append(c == ' ' ? '-' : c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
