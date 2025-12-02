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
    public string Chaine { get; }
    public decimal? Valeur { get; }

    public Entree(TypeEntree type, string chaine, decimal? valeur = null)
    {
        Type = type;
        Chaine = chaine;
        Valeur = valeur;
    }
    public override string ToString()
    {
        return Valeur is null ? $"{Type}:{Chaine}" : $"{Type}:{Valeur}";
    }
}
