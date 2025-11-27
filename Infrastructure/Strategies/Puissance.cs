using Calculatrice.Domaine.Services;

namespace Calculatrice.Infrastructure.Strategies;
public sealed class Puissance : IStrategieOperation
{
    public string Symbole => "^";
    public int Priorite => 3;
    public bool AssocDroite => true;
    public decimal Calculer(decimal gauche, decimal droite)
    {
        return (decimal)Math.Pow((double) gauche, (double)droite); ;
    }
}
