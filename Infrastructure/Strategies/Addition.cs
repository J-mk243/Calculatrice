using Calculatrice.Domaine.Services;

namespace Calculatrice.Infrastructure.Strategies;
public sealed class Addition : IStrategieOperation
{
    public string Symbole => "+";
    public int Priorite => 1;
    public bool AssocDroite => false;
    public decimal Calculer(decimal gauche, decimal droite) => gauche + droite;
}
