namespace Calculatrice.Infrastructure.Evaluation;
public enum TypeEntree
{
    Nombre,
    Operateur,
    ParentheseGauche,
    ParentheseDroite,
    Fonction
}
public class Entree
{
    public TypeEntree Type { get; }
    public string Caractere { get; }
    public decimal? Valeur { get; }

    public Entree(TypeEntree type, string caractere, decimal? valeur = null)
    {
        Type = type;
        Caractere = caractere;
        Valeur = valeur;
    }
    public override string ToString()
    {
        return Valeur is null ? $"{Type}:{Caractere}" : $"{Type}:{Valeur}";
    }
}
