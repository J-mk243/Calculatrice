using Calculatrice.Domaine.Services;

namespace Calculatrice.Application;
public sealed class CalculerExpression
{
    private readonly ICalculerEquation _evaluateur;
    public CalculerExpression(ICalculerEquation evaluateur)
    {
        _evaluateur = evaluateur;
    }
    public decimal Executer(string expression)
    {
        return _evaluateur.Evaluer(expression);
    }
}
