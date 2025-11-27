using Calculatrice.Domaine.Policies;

namespace Calculatrice.Infrastructure.Policies;
public sealed class DivisionParZeroPolicy : IPolicy
{
    public void Verifier(string operateur, decimal gauche, decimal droite)
    {
        if (operateur == "/" && droite == 0)
            throw new DivideByZeroException("Division par zéro interdite.");
    }
}
