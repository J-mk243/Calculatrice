using System.Globalization;

namespace Calculatrice.Infrastructure.Evaluation;

public class Equation
{
    private const string OperateursSupportes = "+-*/^";

    /// <summary>
    /// Transforme une expression mathématique en une liste d'entrées (tokens).
    /// </summary>
    public IReadOnlyList<Entree> ExtraireEntrees(string expression)
    {
        var entrees = new List<Entree>();
        string expr = expression.Trim();

        for (int index = 0; index < expr.Length; index++)
        {
            char courant = expr[index];

            if (char.IsWhiteSpace(courant)) continue;

            if (courant == '(') { entrees.Add(new Entree(TypeEntree.ParentheseGauche, "(")); continue; }
            if (courant == ')') { entrees.Add(new Entree(TypeEntree.ParentheseDroite, ")")); continue; }

            if (OperateursSupportes.Contains(courant))
            {
                if (EstUnMoinsUnaire(courant, entrees))
                {
                    index++;
                    while (index < expr.Length && char.IsWhiteSpace(expr[index])) index++;

                    var (nombreNegatif, nouvelIndex) = LireNombre(expr, index);
                    index = nouvelIndex - 1;

                    if (nombreNegatif is null)
                        throw new ArgumentException("Un nombre négatif est mal formé.");

                    decimal valeur = -nombreNegatif.Value;
                    entrees.Add(new Entree(TypeEntree.Nombre, valeur.ToString(CultureInfo.InvariantCulture), valeur));
                    continue;
                }

                entrees.Add(new Entree(TypeEntree.Operateur, courant.ToString()));
                continue;
            }

            if (char.IsDigit(courant) || courant == '.')
            {
                var (nombre, nouvelIndex) = LireNombre(expr, index);
                index = nouvelIndex - 1;

                if (nombre is null) throw new ArgumentException("Nombre mal formé.");
                entrees.Add(new Entree(TypeEntree.Nombre, nombre.Value.ToString(CultureInfo.InvariantCulture), nombre));
                continue;
            }

            if (char.IsLetter(courant))
            {
                var (identifiant, nouvelIndex) = LireIdentifiant(expr, index);
                index = nouvelIndex - 1;

                entrees.Add(new Entree(TypeEntree.Fonction, identifiant));
                continue;
            }

            throw new ArgumentException($"Caractère invalide: '{courant}'.");
        }

        return entrees;
    }

    /// <summary>
    /// Vérifie si le caractère '-' doit être interprété comme un signe négatif (unaire).
    /// </summary>
    private static bool EstUnMoinsUnaire(char c, List<Entree> entrees)
    {
        return c == '-' &&
               (entrees.Count == 0 ||
                entrees[^1].Type == TypeEntree.Operateur ||
                entrees[^1].Type == TypeEntree.ParentheseGauche ||
                entrees[^1].Type == TypeEntree.Fonction);
    }

    /// <summary>
    /// Lit un nombre (entier ou décimal) et retourne sa valeur et la nouvelle position.
    /// </summary>
    private static (decimal? valeur, int nouvelIndex) LireNombre(string s, int index)
    {
        int debut = index;
        bool pointVu = false;

        while (index < s.Length)
        {
            char c = s[index];
            if (char.IsDigit(c)) { index++; continue; }
            if (c == '.' && !pointVu) { pointVu = true; index++; continue; }
            break;
        }

        ReadOnlySpan<char> span = s.AsSpan(debut, index - debut);
        if (span.Length == 0) return (null, index);

        return decimal.TryParse(span, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal valeur)
            ? (valeur, index)
            : (null, index);
    }

    /// <summary>
    /// Lit un identifiant de fonction (ex: sqrt) et retourne son nom et la nouvelle position.
    /// </summary>
    private static (string identifiant, int nouvelIndex) LireIdentifiant(string s, int index)
    {
        int debut = index;
        while (index < s.Length && char.IsLetter(s[index])) index++;
        string identifiant = s.Substring(debut, index - debut).ToLowerInvariant();
        return (identifiant, index);
    }
}
