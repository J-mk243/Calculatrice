using System.Globalization;

namespace Calculatrice.Infrastructure.Evaluation;
public sealed class Equation
{
    public IReadOnlyList<Entree> Recuperer(string expression)
    {
        var elements = new List<Entree>();
        string e = expression.Trim();
        int i = 0;

        while (i < e.Length)
        {
            char c = e[i];

            if (char.IsWhiteSpace(c)) { i++; continue; }

            if (c == '(') { elements.Add(new Entree(TypeEntree.ParentheseGauche, "(")); i++; continue; }
            if (c == ')') { elements.Add(new Entree(TypeEntree.ParentheseDroite, ")")); i++; continue; }

            if ("+-*/^".Contains(c))
            {
                if (EstMoinsUnaire(c, elements))
                {
                    i++;
                    while (i < e.Length && char.IsWhiteSpace(e[i])) i++;

                    decimal? nombreNegatif = LireNombre(e, ref i);
                    if (nombreNegatif is null)
                        throw new ArgumentException("Nombre négatif mal formé après '-'.");

                    decimal valeur = -nombreNegatif.Value;
                    elements.Add(new Entree(TypeEntree.Nombre, valeur.ToString(CultureInfo.InvariantCulture), valeur));
                    continue;
                }

                elements.Add(new Entree(TypeEntree.Operateur, c.ToString()));
                i++;
                continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                decimal? nombre = LireNombre(e, ref i);
                if (nombre is null) throw new ArgumentException("Nombre mal formé.");
                elements.Add(new Entree(TypeEntree.Nombre, nombre.Value.ToString(CultureInfo.InvariantCulture), nombre));
                continue;
            }

            if (char.IsLetter(c))
            {
                string ident = LireIdentifiant(e, ref i);
                elements.Add(new Entree(TypeEntree.Fonction, ident));
                continue;
            }

            throw new ArgumentException($"Caractère invalide: '{c}'.");
        }

        return elements;
    }

    private static bool EstMoinsUnaire(char c, List<Entree> elements)
    {
        return c == '-' &&
               (elements.Count == 0 ||
                elements[^1].Type == TypeEntree.Operateur ||
                elements[^1].Type == TypeEntree.ParentheseGauche ||
                elements[^1].Type == TypeEntree.Fonction);
    }

    private static decimal? LireNombre(string s, ref int i)
    {
        int start = i;
        bool pointVu = false;

        while (i < s.Length)
        {
            char c = s[i];
            if (char.IsDigit(c)) { i++; continue; }
            if (c == '.' && !pointVu) { pointVu = true; i++; continue; }
            break;
        }

        ReadOnlySpan<char> span = s.AsSpan(start, i - start);
        if (span.Length == 0) return null;

        return decimal.TryParse(span, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal val)
            ? val
            : null;
    }
    private static string LireIdentifiant(string s, ref int i)
    {
        int start = i;
        while (i < s.Length && char.IsLetter(s[i])) i++;
        return s.Substring(start, i - start).ToLowerInvariant();
    }
}
