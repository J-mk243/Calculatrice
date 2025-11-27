using Calculatrice.Domaine.Services;

namespace Calculatrice.Infrastructure.Strategies;
public sealed class Division : IStrategieOperation
{
    public string Symbole => "/";
    public int Priorite => 2;
    public bool AssocDroite => false;
    public decimal Calculer(decimal gauche, decimal droite)
    {
        if (droite == 0) throw new DivideByZeroException();
        return gauche / droite;
    }
}
